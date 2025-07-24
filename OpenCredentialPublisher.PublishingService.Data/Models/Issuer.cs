using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenCredentialPublisher.PublishingService.Data
{
    public class Issuer
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string IssuerUuid { get; set; }
        public string ClientId { get; set; }
        public List<SigningKey> SigningKeys { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
    }
}