using MediatR;

namespace OpenCredentialPublisher.PublishingService.Shared
{
    public class Publish2_0SignClrCommand : ICommand, INotification
    {
        public string RequestId { get; }

        public Publish2_0SignClrCommand(string requestId)
        {
            RequestId = requestId;
        }
    }

}
