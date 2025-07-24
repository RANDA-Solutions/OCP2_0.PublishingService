using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OpenCredentialPublisher.PublishingService.Api
{
    public class OcpDynamicClientRegistrationResult
    {
        [JsonPropertyName("client_id"), JsonProperty(PropertyName = "client_id", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore, Required = Required.Default)]
        public string ClientId { get; set; }

        [JsonPropertyName("client_secret"), JsonProperty(PropertyName = "client_secret", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore, Required = Required.Default)]
        public string ClientSecret { get; set; }

        [JsonPropertyName("client_id_issued_at"), JsonProperty(PropertyName = "client_id_issued_at", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore, Required = Required.Default)]
        public long ClientIdIssuedAt { get; set; }

        [JsonPropertyName("client_secret_expires_at"), JsonProperty(PropertyName = "client_secret_expires_at", DefaultValueHandling = DefaultValueHandling.Include, NullValueHandling = NullValueHandling.Ignore, Required = Required.Default)]
        public long ClientSecretExpiresAt { get; set; }

        [JsonPropertyName("client_name"), JsonProperty(PropertyName = "client_name", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore, Required = Required.Default)]
        public string ClientName { get; set; }

        [JsonPropertyName("client_uri"), JsonProperty(PropertyName = "client_uri", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore, Required = Required.Default)]
        public string ClientUri { get; set; }

        [JsonPropertyName("token_endpoint_auth_method"), JsonProperty(PropertyName = "token_endpoint_auth_method", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore, Required = Required.Default)]
        public string TokenEndpointAuthenticationMethod { get; set; }

        [JsonPropertyName("scope"), JsonProperty(PropertyName = "scope", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore, Required = Required.Default)]
        public string Scope { get; set; }

        [JsonPropertyName("grant_types"), JsonProperty(PropertyName = "grant_types", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore, Required = Required.Default)]
        public ICollection<string> GrantTypes { get; set; }
    }

}
