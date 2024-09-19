using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using OpenCredentialPublisher.Credentials.Clrs.v1_0.Clr;
using OpenCredentialPublisher.Credentials.Clrs.v1_0.Interfaces;
using OpenCredentialPublisher.Credentials.Clrs.v1_0.Utilities;
using OpenCredentialPublisher.Credentials.Clrs.v2_0;
using OpenCredentialPublisher.Credentials.Cryptography;
using OpenCredentialPublisher.Credentials.VerifiableCredentials;
using OpenCredentialPublisher.PublishingService.Data;
using OpenCredentialPublisher.PublishingService.Services;
using OpenCredentialPublisher.PublishingService.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;

namespace OpenCredentialPublisher.PublishingService.Functions
{
    public class Publish2_0SignClrHandler : PublishMessageHandlerBase, ICommandHandler<Publish2_0SignClrCommand>
    {
        private readonly IMediator _mediator;
        private readonly IFileStoreService _fileService;
        private readonly IKeyStore _keyStore;

        private readonly string _appBaseUri;

        private readonly string readyState = PublishProcessingStates.Publish2_0SignClrReady;
        private readonly string processingState = PublishProcessingStates.Publish2_0SignClrProcessing;
        private readonly string failureState = PublishProcessingStates.Publish2_0SignClrFailure;

        public Publish2_0SignClrHandler(IConfiguration configuration, IOptions<AzureBlobOptions> blobOptions, OcpDbContext context, ILogger<PublishMessageHandlerBase> log, 
                            IMediator mediator, IFileStoreService fileService, IKeyStore keyStore) : base(blobOptions, context, log)
        {
            _mediator = mediator;
            _fileService = fileService;
            _appBaseUri = configuration["AppBaseUri"];
            _keyStore = keyStore;
        }

        public async Task HandleAsync(Publish2_0SignClrCommand command)
        {
            var publishRequest = await PreProcessAsync(command);

            if (publishRequest == null) return;

            try
            {
                var leaseId = await AcquireLockAsync("pub", publishRequest.RequestId, TimeSpan.FromSeconds(30));

                try
                {
                    await ProcessAsync(publishRequest);

                    await ReleaseLockAsync();

                    await PostProcessAsync(publishRequest);
                }
                catch (Exception)
                {
                    publishRequest.ProcessingState = failureState;
                    await SaveChangesAsync();

                    throw;
                }
            }
            catch (Exception)
            {
                await ReleaseLockAsync();

                throw;
            }
        }


        private async Task<PublishRequest> PreProcessAsync(Publish2_0SignClrCommand command)
        {
            var publishRequest = await GetPublishRequestAsync(command.RequestId);

            if (publishRequest == null)
            {
                throw new Exception($"RequestId '{command.RequestId}' not found");
            }

            var validProcessingStates = new string[] { readyState, processingState, failureState };

            if (!validProcessingStates.Contains(publishRequest.ProcessingState))
            {
                Log.LogWarning($"'{publishRequest.ProcessingState}' is not a valid state for Processing");
                return null;
            }

            return publishRequest;
        }

        private async Task ProcessAsync(PublishRequest publishRequest)
        {
            publishRequest.ProcessingState = processingState;
            await SaveChangesAsync();

            // Read File
            //var latestFile = publishRequest.ContainsPdf.Value ? publishRequest.GetQrCodeImprintedClr() : publishRequest.GetOriginalClr();
            var latestFile = publishRequest.GetClr2_0();
            // Download PdfQrCodeClrFilePath or OriginalClrFilePath
            var contents = await _fileService.DownloadAsStringAsync(latestFile.FileName);

            var baseUri = new System.Uri(_appBaseUri);

            var clr = System.Text.Json.JsonSerializer.Deserialize<ClrCredential>(contents);
            var transformService = new Clr2_0Service();
            var clrCredential = await transformService.Transform(_appBaseUri, publishRequest, clr);
            await SaveChangesAsync();

            var filename = ClrWithSignatureFilename(publishRequest.RequestId);

            // Upload CLR to Blob
            await _fileService.StoreAsync(filename, System.Text.Json.JsonSerializer.Serialize(clrCredential, 
                new JsonSerializerOptions { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping }));

            publishRequest.Files.Add(File.Create(filename, ClrFileTypes.SignedClr2_0));
            if (publishRequest.PushAfterPublish)
            {
                publishRequest.ProcessingState = PublishProcessingStates.PublishPushReady;
                publishRequest.PublishState = PublishStates.Pushing;
            }
            else
            {
                publishRequest.ProcessingState = PublishProcessingStates.PublishNotifyReady;
                publishRequest.PublishState = PublishStates.Complete;
            }
            publishRequest.PackageSignedTimestamp = DateTimeOffset.UtcNow;
            Log.LogInformation($"Clr 2.0 Signed File Added: {filename}");
            Log.LogInformation($"Next PublishState: '{publishRequest.PublishState}, Next ProcessingState: '{publishRequest.ProcessingState}'");

            await SaveChangesAsync();
        }

        private async Task PostProcessAsync(PublishRequest publishRequest)
        {
            switch (publishRequest.ProcessingState)
            {

                case PublishProcessingStates.PublishNotifyReady:
                    await _mediator.Publish(new PublishNotifyCommand(publishRequest.RequestId));
                    break;
                case PublishProcessingStates.PublishPushReady:
                    await _mediator.Publish(new PublishPushCommand(publishRequest.RequestId));
                    break;

                default:
                    throw new Exception("Invalid State");
            }
        }

        private string ClrWithSignatureFilename(string requestId)
        {
            return string.Format("{0:yyyy}/{0:MM}/{0:dd}/{0:HH}/clr2_0signed_{1}_{0:mmssffff}.json", DateTime.UtcNow, requestId);
        }

        private string VcWrappedClrFilename(string requestId)
        {
            return string.Format("{0:yyyy}/{0:MM}/{0:dd}/{0:HH}/clrvcwrap_{1}_{0:mmssffff}.json", DateTime.UtcNow, requestId);
        }

    }

}
