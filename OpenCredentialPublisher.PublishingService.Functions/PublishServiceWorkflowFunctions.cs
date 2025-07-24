using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OpenCredentialPublisher.PublishingService.Data;
using OpenCredentialPublisher.PublishingService.Shared;
using System;
using System.Threading.Tasks;

namespace OpenCredentialPublisher.PublishingService.Functions
{
    public class PublishServiceWorkflowFunctions
    {
        private readonly ICommandDispatcher _commandDispatcher;
        private readonly ILogger<PublishServiceWorkflowFunctions> log;

        public PublishServiceWorkflowFunctions(ICommandDispatcher commandDispatcher, ILogger<PublishServiceWorkflowFunctions> log)
        {
            _commandDispatcher = commandDispatcher;
            this.log = log;
        }

        [FunctionName("PublishProcessRequestQueueTrigger")]
        public async Task RunPublishProcessRequest([QueueTrigger(PublishQueues.PublishRequest, Connection = "QueueConnectionString")] PublishProcessRequestCommand command)
        {
            log.LogInformation($"PublishProcessRequestQueueTrigger function processed: {JsonConvert.SerializeObject(command)}");

            try
            {
                await _commandDispatcher.HandleAsync(command);
            }
            catch (Exception ex)
            {
                log.LogError(ex, ex.Message);
                throw;
            }
        }

        [FunctionName("Publish2_0ProcessRequestQueueTrigger")]
        public async Task RunPublish2_0ProcessRequest([QueueTrigger(PublishQueues.Publish2_0Request, Connection = "QueueConnectionString")] Publish2_0ProcessRequestCommand command)
        {
            log.LogInformation($"Publish2_0ProcessRequestQueueTrigger function processed: {JsonConvert.SerializeObject(command)}");

            try
            {
                await _commandDispatcher.HandleAsync(command);
            }
            catch (Exception ex)
            {
                log.LogError(ex, ex.Message);
                throw;
            }
        }


        [FunctionName("PublishPackageClrQueueTrigger")]
        public async Task RunPublishPackageClr([QueueTrigger(PublishQueues.PublishPackageClr, Connection = "QueueConnectionString")] PublishPackageClrCommand command)
        {
            log.LogInformation($"PublishPackageClrQueueTrigger function processed: {JsonConvert.SerializeObject(command)}");

            try
            {
                await _commandDispatcher.HandleAsync(command);
            }
            catch (Exception ex)
            {
                log.LogError(ex, ex.Message);
                throw;
            }
        }

        [FunctionName("Publish2_0SignClrQueueTrigger")]
        public async Task Publish2_0SignPackage([QueueTrigger(PublishQueues.Publish2_0SignClr, Connection = "QueueConnectionString")] Publish2_0SignClrCommand command)
        {
            log.LogInformation($"Publish2_0SignClrQueueTrigger function processed: {JsonConvert.SerializeObject(command)}");

            try
            {
                await _commandDispatcher.HandleAsync(command);
            }
            catch (Exception ex)
            {
                log.LogError(ex, ex.Message);
                throw;
            }
        }

        [FunctionName("PublishSignClrQueueTrigger")]
        public async Task PublishSignPackage([QueueTrigger(PublishQueues.PublishSignClr, Connection = "QueueConnectionString")] PublishSignClrCommand command)
        {
            log.LogInformation($"PublishSignClrQueueTrigger function processed: {JsonConvert.SerializeObject(command)}");

            try
            {
                await _commandDispatcher.HandleAsync(command);
            }
            catch (Exception ex)
            {
                log.LogError(ex, ex.Message);
                throw;
            }
        }


        [FunctionName("PublishNotifyQueueTrigger")]
        public async Task PublishNotifyPackage([QueueTrigger(PublishQueues.PublishNotify, Connection = "QueueConnectionString")] PublishNotifyCommand command, 
            int dequeueCount)
        {
            log.LogInformation($"PublishNotifyQueueTrigger function processed: {JsonConvert.SerializeObject(command)}");

            try
            {
                await _commandDispatcher.HandleAsync(command);
            }
            catch (Exception ex)
            {
                log.LogError(ex, ex.Message);
                throw;
            }
        }

        [FunctionName("PublishPushQueueTrigger")]
        public async Task PublishPush([QueueTrigger(PublishQueues.PublishPush, Connection = "QueueConnectionString")] PublishPushCommand command,
            int dequeueCount)
        {
            log.LogInformation($"PublishPushQueueTrigger function processed: {JsonConvert.SerializeObject(command)}");

            try
            {
                await _commandDispatcher.HandleAsync(command);
            }
            catch (Exception ex)
            {
                log.LogError(ex, ex.Message);
                throw;
            }
        }

    }

}
