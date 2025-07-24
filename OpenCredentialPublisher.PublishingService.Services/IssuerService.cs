using Microsoft.EntityFrameworkCore;
using OpenCredentialPublisher.Credentials.Clrs.v1_0.Interfaces;
using OpenCredentialPublisher.PublishingService.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCredentialPublisher.PublishingService.Services
{
    public class IssuerService
    {
        private readonly OcpDbContext _dbContext;

        public IssuerService(OcpDbContext dbContext, IKeyStore keyStore)
        {
            _dbContext = dbContext;
        }

        public async Task<Issuer> GetIssuerAsync(string issuerId, string clientId, bool createIfNotFound =  true)
        {
            var issuer = await _dbContext.Issuers
                .Include(i => i.SigningKeys)
                .FirstOrDefaultAsync(i => i.IssuerUuid == issuerId);
            
            if (issuer == null)
            {
                if (createIfNotFound)
                    return await CreateIssuerAsync(issuerId, clientId);
                throw new KeyNotFoundException($"Issuer with ID {issuerId} not found.");
            }
            
            return issuer;
        }

        public async Task<Issuer> GetIssuerAsync(int issuerId)
        {
            var issuer = await _dbContext.Issuers
                .Include(i => i.SigningKeys)
                .FirstOrDefaultAsync(i => i.Id == issuerId);

            return issuer;
        }

        public async Task<Issuer> CreateIssuerAsync(string issuerId, string clientId)
        {
            var issuer = new Issuer
            {
                IssuerUuid = issuerId,
                ClientId = clientId,
                SigningKeys = new List<SigningKey>()
            };
            _dbContext.Issuers.Add(issuer);
            await _dbContext.SaveChangesAsync();
            return issuer;
        }
    }
}
