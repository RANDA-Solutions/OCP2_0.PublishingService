﻿using MediatR;
using Newtonsoft.Json;
using OpenCredentialPublisher.PublishingService.Data;
using OpenCredentialPublisher.PublishingService.Services;
using OpenCredentialPublisher.PublishingService.Shared;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenCredentialPublisher.PublishingService.Functions
{
    public class PublishSignClrNotificationHandler : INotificationHandler<PublishSignClrCommand>, INotificationHandler<Publish2_0SignClrCommand>
    {
        private readonly IQueueService _queueService;

        public PublishSignClrNotificationHandler(IQueueService queueService)
        {
            _queueService = queueService;
        }
        public async Task Handle(PublishSignClrCommand notification, CancellationToken cancellationToken)
        {
#if DEBUG
            await _queueService.SendMessageAsync(PublishQueues.PublishSignClr, JsonConvert.SerializeObject(notification), TimeSpan.FromSeconds(Constants.DebugDelay));
#else
            await _queueService.SendMessageAsync(PublishQueues.PublishSignClr, JsonConvert.SerializeObject(notification));
#endif
        }

        public async Task Handle(Publish2_0SignClrCommand notification, CancellationToken cancellationToken)
        {
#if DEBUG
            await _queueService.SendMessageAsync(PublishQueues.Publish2_0SignClr, JsonConvert.SerializeObject(notification), TimeSpan.FromSeconds(Constants.DebugDelay));
#else
            await _queueService.SendMessageAsync(PublishQueues.Publish2_0SignClr, JsonConvert.SerializeObject(notification));
#endif
        }

    }

}
