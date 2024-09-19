using MediatR;

namespace OpenCredentialPublisher.PublishingService.Shared
{
    public class Publish2_0ProcessRequestCommand : ICommand, INotification
    {
        public string RequestId { get; }
        public bool PushPackage { get; }
        public string PushUri { get; }

        public Publish2_0ProcessRequestCommand(string requestId, bool pushPackage = false, string pushUri = null)
        {
            RequestId = requestId;
            PushPackage = pushPackage;
            PushUri = pushUri;
        }
    }

}
