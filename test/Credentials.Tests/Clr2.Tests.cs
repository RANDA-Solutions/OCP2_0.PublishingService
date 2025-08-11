#if !NETCOREAPP3_1
#error This test project must target netcoreapp3.1 for Microsoft.EntityFrameworkCore.InMemory 3.1.0
#endif
using Microsoft.EntityFrameworkCore;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using OpenCredentialPublisher.Credentials.Clrs.v1_0.Clr;
using OpenCredentialPublisher.Credentials.Clrs.v1_0.Interfaces;
using OpenCredentialPublisher.Credentials.Clrs.v1_0.KeyStorage;
using OpenCredentialPublisher.Credentials.Clrs.v1_0.Utilities;
using OpenCredentialPublisher.Credentials.Clrs.v2_0;
using OpenCredentialPublisher.Credentials.Drawing;
using OpenCredentialPublisher.PublishingService.Data;
using OpenCredentialPublisher.PublishingService.Services;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OpenCredentialPublisher.Credentials.Tests
{
    public class Clr2Tests
    {
        [Test]
        public async Task TransformClr1ToClr2_ContainsEmailAddressIdentityObject()
        {
            var options = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<OcpDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;
            using var dbContext = new OcpDbContext(options);

            var keyStoreMock = new Moq.Mock<IKeyStore>();
            var issuerService = new OpenCredentialPublisher.PublishingService.Services.IssuerService(dbContext, keyStoreMock.Object);

            var proofService = new ProofService();
            var clr2Service = new Clr2_0Service(proofService, issuerService);
            var transformService = new Clr1_0ToClr2_0Service(clr2Service);

            string clr1Json;
            using (var stream = new StreamReader(typeof(Clr2Tests).Assembly.GetManifestResourceStream($"{typeof(Clr2Tests).Namespace}.Files.nd-clr-transcript.json")))
            {
                clr1Json = stream.ReadToEnd();
            }
            var clr1 = JsonConvert.DeserializeObject<ClrDType>(clr1Json);
            Assert.IsNotNull(clr1, "Failed to deserialize nd-clr-transcript.json");

            var requestId = Guid.NewGuid().ToString();
            var clientId = Guid.NewGuid().ToString();
            var appBaseUri = "https://localhost";
            var publishRequest = new PublishRequest { RequestId = requestId, ClientId = clientId, RevocationListId = 1 };

            var clr2 = await transformService.Transform(appBaseUri, publishRequest, clr1);
            Assert.IsNotNull(clr2, "Transform returned null");
            Assert.IsNotNull(clr2.CredentialSubject, "CredentialSubject is null");
            Assert.IsNotNull(clr2.CredentialSubject.Identifier, "CredentialSubject.Identifier is null");

            bool hasEmail = false;
            foreach (var idObj in clr2.CredentialSubject.Identifier)
            {
                if (idObj != null && string.Equals(idObj.IdentityType, "emailAddress", StringComparison.OrdinalIgnoreCase))
                {
                    hasEmail = true;
                    break;
                }
            }
            Assert.IsTrue(hasEmail, "No IdentityObject with IdentityType 'emailAddress' found in CredentialSubject.Identifier");
        }
    }
}