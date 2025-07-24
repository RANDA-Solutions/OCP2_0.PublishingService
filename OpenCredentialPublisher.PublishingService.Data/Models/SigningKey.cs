using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenCredentialPublisher.PublishingService.Data
{
    public class SigningKey
    {
        [Key]
        public int Id { get; set; }

        [Column(TypeName = "nvarchar(128)")]
        public string RequestId { get; set; }

        [Column(TypeName = "nvarchar(128)"), Required]
        public string KeyName { get; set; }

        [Column(TypeName = "nvarchar(32)")]
        public string KeyType { get; set; }

        [Column(TypeName = "nvarchar(256)")]
        public string VaultKeyIdentifier { get; set; }

        public int IssuerId { get; set; }
        [Column(TypeName = "nvarchar(128)")]
        public string KeyFragment { get; set; }

        [Column(TypeName = "nvarchar(2048)")]
        public string PrivateKey { get; set; }

        [Column(TypeName = "nvarchar(256)")]
        public string PublicKey { get; set; }

        [Column(TypeName = "datetimeoffset(7)"), Required]
        public DateTimeOffset CreateTimestamp { get; set; }

        public bool Expired { get; set; }
        public bool Revoked { get; set; }

        [Required]
        public bool StoredInKeyVault { get; set; } = false;

        [ForeignKey("RequestId")]
        public virtual PublishRequest PublishRequest { get; set; }

        [ForeignKey("IssuerId")]
        public virtual Issuer Issuer { get; set; }

        public static SigningKey Create(int issuerId)
        {
            var key = new SigningKey()
            {
                CreateTimestamp = DateTimeOffset.UtcNow,
                KeyName = Guid.NewGuid().ToString("d"),
                IssuerId = issuerId
            };

            return key;
        }

    }

}