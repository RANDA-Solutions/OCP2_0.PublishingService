using Newtonsoft.Json;
using OpenCredentialPublisher.Credentials.Clrs.v2_0;

namespace OpenCredentialPublisher.PublishingService.Api
{
    public class Clr2_0PublishRequest
    {
        /// <summary>
        /// This can be used by the client to track the request
        /// Otherwise, a GUID will be generated
        /// </summary>
        [JsonProperty("id", Required = Required.AllowNull)]
        public string Id { get; set; }

        [JsonProperty("clr", Required = Required.Always)]
        public ClrCredential Clr { get; set; }
    }
}
