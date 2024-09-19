using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OpenCredentialPublisher.Credentials.Clrs.v1_0.Clr;
using OpenCredentialPublisher.PublishingService.Data;
using OpenCredentialPublisher.PublishingService.Services;
using OpenCredentialPublisher.PublishingService.Shared;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OpenCredentialPublisher.PublishingService.Functions
{

    public class Publish2_0ProcessRequestHandler : PublishMessageHandlerBase, ICommandHandler<Publish2_0ProcessRequestCommand>
    {
        private readonly IMediator _mediator;
        private readonly IFileStoreService _fileService;

        private readonly string readyState = PublishProcessingStates.PublishRequestReady;
        private readonly string processingState = PublishProcessingStates.PublishRequestProcessing;
        private readonly string failureState = PublishProcessingStates.PublishRequestFailure;

        public Publish2_0ProcessRequestHandler(IOptions<AzureBlobOptions> blobOptions, OcpDbContext context, ILogger<PublishMessageHandlerBase> log, 
                            IMediator mediator, IFileStoreService fileService) : base(blobOptions, context, log)
        {
            _mediator = mediator;
            _fileService = fileService;
        }

        public async Task HandleAsync(Publish2_0ProcessRequestCommand command)
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


        private async Task<PublishRequest> PreProcessAsync(Publish2_0ProcessRequestCommand command)
        {
            var publishRequest = await GetPublishRequestAsync(command.RequestId);

            if (publishRequest == null)
            {
                throw new Exception($"RequestId '{command.RequestId}' not found");
            }

            var validProcessingStates = new string[] { null, readyState, processingState, failureState };
            
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

            publishRequest.AccessKeys.Add(AccessKey.Create());
            publishRequest.ProcessingState = PublishProcessingStates.Publish2_0SignClrReady;
            publishRequest.PublishState = PublishStates.SignClr;

            Log.LogInformation($"Next PublishState: '{publishRequest.PublishState}, Next ProcessingState: '{publishRequest.ProcessingState}'");

            await SaveChangesAsync();
        }

        private async Task PostProcessAsync(PublishRequest publishRequest)
        {
            switch (publishRequest.ProcessingState)
            {
                case PublishProcessingStates.Publish2_0SignClrReady:
                    await _mediator.Publish(new Publish2_0SignClrCommand(publishRequest.RequestId));
                    break;

                default:
                    throw new Exception("Invalid State");
            }
        }

    }

}
