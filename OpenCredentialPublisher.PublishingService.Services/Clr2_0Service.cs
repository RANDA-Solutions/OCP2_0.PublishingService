using LevelData.Credentials.DIDForge.Helpers;
using MediatR;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using OpenCredentialPublisher.Credentials.Clrs.v1_0.Clr;
using OpenCredentialPublisher.Credentials.Clrs.v1_0.Interfaces;
using OpenCredentialPublisher.Credentials.Clrs.v1_0.Utilities;
using OpenCredentialPublisher.Credentials.Clrs.v2_0;
using OpenCredentialPublisher.Credentials.Cryptography;
using OpenCredentialPublisher.Credentials.VerifiableCredentials;
using OpenCredentialPublisher.PublishingService.Data;
using OpenCredentialPublisher.PublishingService.Services.Abstractions;
using OpenCredentialPublisher.PublishingService.Services.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;

namespace OpenCredentialPublisher.PublishingService.Services
{
    public class Clr2_0Service : ITransformationService<ClrCredential, ClrCredential>
    {
        private readonly IssuerService _issuerService;
        private readonly ProofService _proofService;
        public Clr2_0Service(ProofService proofService, IssuerService issuerService) 
        {
            _issuerService = issuerService;
            _proofService = proofService;
        }

        public async Task<ClrCredential> Transform(string appBaseUri, PublishRequest publishRequest, ClrCredential source)
        {
            var baseUri = new System.Uri(appBaseUri);

            (var requestId, var clientId, var revocationListId) = publishRequest;

            if (!source.Context.Contains("https://purl.imsglobal.org/spec/ob/v3p0/extensions.json"))
                source.Context.Add("https://purl.imsglobal.org/spec/ob/v3p0/extensions.json");

            var now = DateTime.UtcNow;
            source.Id = UriUtility.Combine(baseUri, "api", "credentials", requestId);
            source.AwardedDate = source.AwardedDate.ToDateTimeZString(now);
            source.ValidFrom = source.ValidFrom.ToDateTimeZString(now);

            var issuerId = source.Issuer?.Id ?? Guid.NewGuid().ToString();
            var sourceIssuer = await AssignIssuerAsync(source.Issuer, appBaseUri, issuerId, publishRequest.ClientId);
            source.Issuer = sourceIssuer.profile;

            source.CredentialStatus = new BasicProperties { Id = UriUtility.Combine(baseUri, "api", "status", revocationListId), Type = "1EdTechRevocationList" };
            var proofService = new ProofService();
            var credentials = new List<AchievementCredential>();
            foreach (var credential in source.CredentialSubject.VerifiableCredential)
            {
                var json = System.Text.Json.JsonSerializer.SerializeToDocument(credential, new JsonSerializerOptions { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
                json.RootElement.TryGetProperty("type", out var type);
                var types = type.EnumerateArray().Select(t => t.GetString()).ToList();
                if (types.Any(t => t == "AchievementCredential" || t == "OpenBadgeCredential"))
                {
                    var achievement = System.Text.Json.JsonSerializer.Deserialize<AchievementCredential>(json.RootElement.GetRawText());
                    if (achievement.Endorsement != null)
                    {
                        foreach (var endorsement in achievement.Endorsement)
                        {
                            endorsement.AwardedDate = endorsement.AwardedDate.ToDateTimeZString(now);
                            endorsement.ValidFrom = endorsement.ValidFrom.ToDateTimeZString(now);
                            await SignEndorsement(appBaseUri, publishRequest, issuerId, endorsement);
                        }
                    }
                    achievement.AwardedDate = achievement.AwardedDate.ToDateTimeZString(now);
                    achievement.ValidFrom = achievement.ValidFrom.ToDateTimeZString(now);
                    var achievementIssuer = await AssignIssuerAsync(achievement.Issuer, appBaseUri, issuerId, publishRequest.ClientId);
                    achievement.Issuer = achievementIssuer.profile;

                    await Sign(achievementIssuer.issuer.Id, achievement.Issuer.Id, publishRequest, achievement);
                    credentials.Add(achievement);
                }
            }
            source.CredentialSubject.VerifiableCredential = credentials.ToArray();
            if (source.Endorsement != null)
            {
                foreach (var endorsement in source.Endorsement)
                {
                    await SignEndorsement(appBaseUri, publishRequest, issuerId, endorsement);
                }
            }
            await Sign(sourceIssuer.issuer.Id, source.Issuer.Id, publishRequest, source);
            return source;
        }

        private async Task SignEndorsement(string appBaseUri, PublishRequest publishRequest, string issuerId, EndorsementCredential endorsement)
        {
            var endorsementIssuer = await AssignIssuerAsync(endorsement.Issuer, appBaseUri, issuerId, publishRequest.ClientId);
            endorsement.Issuer = endorsementIssuer.profile;
            await Sign(endorsementIssuer.issuer.Id, endorsement.Issuer.Id, publishRequest, endorsement);
        }

        private async Task<(Profile profile, Data.Issuer issuer)> AssignIssuerAsync(Profile issuerProfile, string appBaseUri, string issuerId, string clientId)
        {
            var keyController = GetDidWeb(appBaseUri, issuerProfile?.Id ?? issuerId);
            var issuer = await _issuerService.GetIssuerAsync(issuerProfile?.Id ?? issuerId, clientId);
            if (issuerProfile == null)
            {
                return (new Profile
                {
                    Id = keyController,
                    Type = new[] { "Profile" }
                }, issuer);
            }
            else
            {
                issuerProfile.Id = keyController;
                return (issuerProfile, issuer);
            }
        }

        public async Task<(SigningKey signingKey, byte[] privateKey)> GetKeyAsync(int issuerId)
        {
            var issuer = await _issuerService.GetIssuerAsync(issuerId);
            if (issuer == null)
                throw new KeyNotFoundException($"Issuer with ID {issuerId} not found.");
            if (issuer.SigningKeys == null || !issuer.SigningKeys.Any(x => !(x.Revoked || x.Expired)))
            {
                (var publicKey, var privateKey) = CryptoMethods.GenerateEd25519Keys();

                var key = SigningKey.Create(issuerId);
                key.KeyType = CryptoSuites.Ed25519Signature2020;
                key.StoredInKeyVault = false;

                key.PublicKey = CryptoMethods.Base58EncodeEd25519PublicKey(publicKey);
                key.PrivateKey = CryptoMethods.Base58EncodeBytes(privateKey);
                key.KeyFragment = $"key-{Guid.NewGuid(),8:N}";
                if (issuer.SigningKeys == null)
                    issuer.SigningKeys = new List<SigningKey>();

                issuer.SigningKeys.Add(key);
            }

            var signingKey = issuer.SigningKeys.FirstOrDefault();
            var privateKeyString = CryptoMethods.Base58DecodeString(signingKey.PrivateKey);

            return (signingKey, privateKeyString);
        }

        public async Task Sign<T>(int issuerId, string keyController, PublishRequest publishRequest, T credential) where T: IVerifiableCredential2_0
        {
            var json = System.Text.Json.JsonSerializer.Serialize(credential, 
                new JsonSerializerOptions { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
            credential.Proof = new[] { await CreateProofAsync(json, issuerId, keyController, publishRequest) };
        }

        public async Task<Proof> CreateProofAsync(string json, int issuerId, string keyController, PublishRequest publishRequest)
        {
            var key = await GetKeyAsync(issuerId);
            if (publishRequest.SigningKeys.All(x => x.PrivateKey != key.signingKey.PrivateKey))
            {
                publishRequest.SigningKeys.Add(key.signingKey);
            }
            return await _proofService.CreateProof(json, $"{keyController}#{key.signingKey.KeyFragment}", key.privateKey);
        }

        public Credentials.Clrs.v2_0.AssociationTypeEnum GetAssociationType(Credentials.Clrs.v1_0.Clr.AssociationTypeEnum associationType) =>
            associationType switch
            {
                Credentials.Clrs.v1_0.Clr.AssociationTypeEnum.exactMatchOf => Credentials.Clrs.v2_0.AssociationTypeEnum.exactMatchOf,
                Credentials.Clrs.v1_0.Clr.AssociationTypeEnum.isChildOf => Credentials.Clrs.v2_0.AssociationTypeEnum.isChildOf,
                Credentials.Clrs.v1_0.Clr.AssociationTypeEnum.isParentOf => Credentials.Clrs.v2_0.AssociationTypeEnum.isParentOf,
                Credentials.Clrs.v1_0.Clr.AssociationTypeEnum.isPartOf => Credentials.Clrs.v2_0.AssociationTypeEnum.isPartOf,
                Credentials.Clrs.v1_0.Clr.AssociationTypeEnum.isPeerOf => Credentials.Clrs.v2_0.AssociationTypeEnum.isPeerOf,
                Credentials.Clrs.v1_0.Clr.AssociationTypeEnum.isRelatedTo => Credentials.Clrs.v2_0.AssociationTypeEnum.isRelatedTo,
                Credentials.Clrs.v1_0.Clr.AssociationTypeEnum.precedes => Credentials.Clrs.v2_0.AssociationTypeEnum.precedes,
                Credentials.Clrs.v1_0.Clr.AssociationTypeEnum.replacedBy => Credentials.Clrs.v2_0.AssociationTypeEnum.replacedBy,
                _ => Credentials.Clrs.v2_0.AssociationTypeEnum.isRelatedTo
            };

        private string GetId()
        {
            return $"urn:uuid:{Guid.NewGuid():D}";
        }

        private string GetDidWeb(string appBaseUri, string issuerId)
        {
            var docUri = $"{appBaseUri}/issuers/{issuerId}";
            return UriToDidWebHelper.ConvertUriToDidWeb(docUri);
        }
    }
}
