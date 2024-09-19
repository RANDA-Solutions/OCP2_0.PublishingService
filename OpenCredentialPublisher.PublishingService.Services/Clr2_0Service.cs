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
        public Clr2_0Service() { }

        public async Task<ClrCredential> Transform(string appBaseUri, PublishRequest publishRequest, ClrCredential source)
        {
            var baseUri = new System.Uri(appBaseUri);

            (var requestId, var clientId, var revocationListId) = publishRequest;

            var now = DateTime.UtcNow;
            var issuanceDate = now.ToString(Formats.DateTimeZFormat);
            var issuerId = UriUtility.Combine(baseUri, "api", "issuers", clientId);
            source.Id = UriUtility.Combine(baseUri, "api", "credentials", requestId);
            if (String.IsNullOrEmpty(source.IssuanceDate))
                source.IssuanceDate = issuanceDate;
            if (String.IsNullOrEmpty(source.AwardedDate))
                source.AwardedDate = now.ToString(Formats.DateTimeFormat);
            if (source.Issuer == null)
            {
                source.Issuer = new Profile
                {
                    Id = issuerId,
                    Type = new[] { "Profile" }
                };
            }

            source.CredentialStatus = new BasicProperties { Id = UriUtility.Combine(baseUri, "api", "status", revocationListId), Type = nameof(RevocationList) };
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
                            await Sign(issuerId, baseUri, publishRequest, endorsement);
                        }
                    }
                    await Sign(issuerId, baseUri, publishRequest, achievement);
                    credentials.Add(achievement);
                }
            }
            source.CredentialSubject.VerifiableCredential = credentials.ToArray();
            if (source.Endorsement != null)
            {
                foreach (var endorsement in source.Endorsement)
                {
                    await Sign(issuerId, baseUri, publishRequest, endorsement);
                }
            }
            await Sign(issuerId, baseUri, publishRequest, source);
            return source;
        }

        public (SigningKey signingKey, byte[] privateKey, string verificationMethod) GetKey(string issuerId, Uri baseUri)
        {
            (var publicKey, var privateKey) = CryptoMethods.GenerateEd25519Keys();
            var publicKeyString = CryptoMethods.Base58EncodeBytes(publicKey);
            var privateKeyString = CryptoMethods.Base58EncodeBytes(privateKey);

            var key = SigningKey.Create(issuerId);
            key.KeyType = CryptoSuites.Ed25519Signature2020;
            key.StoredInKeyVault = false;

            key.PublicKey = publicKeyString;
            key.PrivateKey = privateKeyString;
            var base58KeyString = CryptoMethods.Base58EncodeEd25519PublicKey(publicKey);
            var verificationMethod = $"did:key:{base58KeyString}";
            //var verificationMethod = new Uri(UriUtility.Combine(baseUri, "api", "keys", key.IssuerId, key.KeyName));
            return (key, privateKey, verificationMethod);
        }

        public async Task Sign<T>(string issuerId, Uri verificationMethod, PublishRequest publishRequest, T credential) where T: IVerifiableCredential
        {
            var json = System.Text.Json.JsonSerializer.Serialize(credential, 
                new JsonSerializerOptions { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
            credential.Proof = new[] { await CreateProofAsync(json, issuerId, verificationMethod, publishRequest) };
        }

        //public async Task Sign(string issuerId, Uri verificationMethod, PublishRequest publishRequest, AchievementCredential credential)
        //{
        //    var json = JsonConvert.SerializeObject(credential);
        //    credential.Proof = new[] { await CreateProofAsync(json, issuerId, verificationMethod, publishRequest) };
        //}

        //public async Task Sign(string issuerId, Uri verificationMethod, PublishRequest publishRequest, ClrCredential credential)
        //{
        //    var json = JsonConvert.SerializeObject(credential);
        //    credential.Proof = new[] { await CreateProofAsync(json, issuerId, verificationMethod, publishRequest) };
        //}

        public async Task<Proof> CreateProofAsync(string json, string issuerId, Uri verificationMethod, PublishRequest publishRequest)
        {
            var key = GetKey(issuerId, verificationMethod);
            publishRequest.SigningKeys.Add(key.signingKey);
            var proofService = new ProofService();
            return await proofService.CreateProof(json, key.verificationMethod, key.privateKey);
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
    }
}
