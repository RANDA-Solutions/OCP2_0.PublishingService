using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace OpenCredentialPublisher.PublishingService.Api
{
    public class OcpDynamicClientRegistrationRequest 
    {
        [JsonPropertyName("client_name"), JsonProperty(PropertyName = "client_name", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore, Required = Required.Default)]
        public string ClientName { get; set; }

        [JsonPropertyName("client_uri"), JsonProperty(PropertyName = "client_uri", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore, Required = Required.Default)]
        public string ClientUri { get; set; }

        [JsonPropertyName("token_endpoint_auth_method"), JsonProperty(PropertyName = "token_endpoint_auth_method", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore, Required = Required.Default)]
        public string TokenEndpointAuthenticationMethod { get; set; }

        [JsonPropertyName("scope"), JsonProperty(PropertyName = "scope", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore, Required = Required.Default)]
        public string Scope { get; set; }

    }

}
