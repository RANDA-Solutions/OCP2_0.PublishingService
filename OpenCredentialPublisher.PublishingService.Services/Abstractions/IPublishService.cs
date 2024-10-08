﻿using OpenCredentialPublisher.Credentials.Clrs.v1_0.Clr;
using OpenCredentialPublisher.Credentials.Clrs.v2_0;
using OpenCredentialPublisher.Credentials.VerifiableCredentials;
using System.Threading.Tasks;

namespace OpenCredentialPublisher.PublishingService.Services
{
    public interface IPublishService
    {
        Task<string> ProcessRequestAsync(string id, ClrDType clr, string clientId, string pathway, bool pushAfterPublish = false, string pushUri = null);
        Task<string> ProcessRequestAsync(string id, ClrCredential clr, string clientId, bool pushAfterPublish = false, string pushUri = null);

        Task<PublishStatusResult> GetAsync(string requestId, string clientId, string walletBaseUri, string scope, string endpoint, string method);

        Task RevokeAsync(string requestId, string clientId);

        Task<string> GetCredentialsAsync(string requestId);
        Task SetRequestStateAsync(string accessKey, string state);
    }

    

}
