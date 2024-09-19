using MediatR;
using Newtonsoft.Json;
using OpenCredentialPublisher.PublishingService.Data;
using OpenCredentialPublisher.PublishingService.Services;
using OpenCredentialPublisher.PublishingService.Shared;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenCredentialPublisher.PublishingService.Api
{
    public class PublishProcessRequestHandler : INotificationHandler<PublishProcessRequestCommand>, INotificationHandler<Publish2_0ProcessRequestCommand>
    {
        private readonly IQueueService _queueService;

        public PublishProcessRequestHandler(IQueueService queueService)
        {
            _queueService = queueService;
        }
        public async Task Handle(PublishProcessRequestCommand notification, CancellationToken cancellationToken)
        {
#if DEBUG
            await _queueService.SendMessageAsync(PublishQueues.PublishRequest, JsonConvert.SerializeObject(notification), TimeSpan.FromSeconds(2));
#else
            await _queueService.SendMessageAsync(PublishQueues.PublishRequest, JsonConvert.SerializeObject(notification));
#endif
        }

        public async Task Handle(Publish2_0ProcessRequestCommand notification, CancellationToken cancellationToken)
        {
#if DEBUG
            await _queueService.SendMessageAsync(PublishQueues.Publish2_0Request, JsonConvert.SerializeObject(notification), TimeSpan.FromSeconds(2));
#else
            await _queueService.SendMessageAsync(PublishQueues.Publish2_0Request, JsonConvert.SerializeObject(notification));
#endif
        }
    }

}
