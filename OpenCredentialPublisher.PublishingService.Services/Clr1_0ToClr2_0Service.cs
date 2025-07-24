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
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;

namespace OpenCredentialPublisher.PublishingService.Services
{
    public class Clr1_0ToClr2_0Service : ITransformationService<ClrDType, ClrCredential>
    {
        private readonly Clr2_0Service _signingService;
        public Clr1_0ToClr2_0Service(Clr2_0Service signingService) 
        {
            _signingService = signingService ?? throw new ArgumentNullException(nameof(signingService));
        }

        public async Task<ClrCredential> Transform(string appBaseUri, PublishRequest publishRequest, ClrDType source)
        {
            (var requestId, var clientId, var revocationListId) = publishRequest;            

            var issuanceDate = DateTime.UtcNow.ToString(Formats.DateTimeFormat);
            var credential = new ClrCredential
            {
                Issuer = new Profile
                {
                    Id = clientId,
                    Type = new[] { "Profile" }
                },
                ValidFrom = issuanceDate,
                AwardedDate = source.IssuedOn.ToString(Formats.DateTimeFormat),
                Name = source.Name,
                Partial = source.Partial ?? false,
            };

            if (!credential.Context.Contains("https://purl.imsglobal.org/spec/ob/v3p0/extensions.json"))
                credential.Context.Add("https://purl.imsglobal.org/spec/ob/v3p0/extensions.json");

            var subject = new Credentials.Clrs.v2_0.ClrSubject();
            credential.CredentialSubject = subject;

            subject.Id = GetId();
            var assocations = new List<Association>();

            if (source.Achievements != null && source.Achievements.Any())
            {
                var achievements = new List<Achievement>();

                foreach (var a in source.Achievements)
                {
                    achievements.Add(a.ToAchievement());
                }
                subject.Achievement = achievements.ToArray();
            }

            if (source.Assertions != null && source.Assertions.Any())
            {
                var credentials = new List<VerifiableCredential2_0>();
                foreach (var assertion in source.Assertions)
                {
                    var achievementCredential = new AchievementCredential();
                    achievementCredential.Id = assertion.Id;
                    achievementCredential.Issuer = new Profile
                    {
                        Id = assertion.Achievement.Issuer.Id,
                        Type = new[] { "Profile" },
                        Image = String.IsNullOrEmpty(assertion.Achievement.Issuer.Image) ? default :
                        new Image
                        {
                            Id = assertion.Achievement.Issuer.Image,
                            Type = "Image"
                        },
                        Name = assertion.Achievement.Issuer.Name
                    };

                    achievementCredential.ValidFrom = issuanceDate;
                    achievementCredential.Name = assertion.Achievement.Name;
                    achievementCredential.Description = assertion.Achievement.Description;
                    achievementCredential.Endorsement = assertion.Endorsements.ToEndorsementCredentials();

                    var achievementSubject = new AchievementSubject
                    {
                        Id = GetId(),
                        Type = new[] { nameof(AchievementSubject) },
                        ActivityStartDate = assertion.ActivityStartDate,
                        ActivityEndDate = assertion.ActivityEndDate,
                        LicenseNumber = assertion.LicenseNumber,
                        Narrative = assertion.Narrative,
                        CreditsEarned = assertion.CreditsEarned,
                        Image = assertion.Image.ToImage(),
                        Result = assertion.Results?.ToResult(),
                        Achievement = assertion.Achievement.ToAchievement(),
                        Role = assertion.Role,
                        Source = assertion.Achievement.Issuer.ToProfile(),
                        Term = assertion.Term,

                    };
                    achievementCredential.CredentialSubject = achievementSubject;

                    if (assertion.Recipient != null)
                        achievementSubject.Identifier = new[] { assertion.Recipient.ToIdentityObject() };

                    if (assertion.Image != null)
                    {
                        achievementCredential.Image = new Image
                        {
                            Id = assertion.Image
                        };
                    }

                    if (assertion.Evidence != null && assertion.Evidence.Any())
                    {
                        achievementCredential.Evidence = assertion.Evidence.ToEvidence();
                    }

                    if (assertion.Achievement?.Associations != null &&
                            assertion.Achievement.Associations.Any())
                    {
                        assocations.AddRange(assertion.Achievement.Associations.Select(a => new Association
                        {
                            AssociationType = GetAssociationType(a.AssociationType),
                            TargetId = a.TargetId,
                            SourceId = assertion.Achievement.Id,
                            Type = "Association"
                        }));
                    }
                    
                    credentials.Add(achievementCredential);
                }
                subject.VerifiableCredential = credentials.ToArray();
            }

            subject.Association = assocations.ToArray();
            _ = await _signingService.Transform(appBaseUri, publishRequest, credential);
            return credential;
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
