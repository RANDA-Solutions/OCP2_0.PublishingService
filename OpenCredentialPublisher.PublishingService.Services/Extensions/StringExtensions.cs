using OpenCredentialPublisher.Credentials.Clrs.v2_0;
using OpenCredentialPublisher.PublishingService.Data;
using System;

namespace OpenCredentialPublisher.PublishingService.Services.Extensions
{
    public static class StringExtensions
    {
        public static Image ToImage(this string image)
        {
            if (String.IsNullOrWhiteSpace(image)) 
                return default;
            return new Image { Id = image, Type = "Image" };
        } 

        public static string ToDateTimeZString(this string? dateTime, DateTime? now = null)
        {
            if (String.IsNullOrWhiteSpace(dateTime) && now == null)
                return DateTime.UtcNow.ToString(Formats.DateTimeZFormat);
            if (String.IsNullOrWhiteSpace(dateTime) && now != null)
                return now.Value.ToUniversalTime().ToString(Formats.DateTimeZFormat);
            return DateTime.Parse(dateTime).ToUniversalTime().ToString(Formats.DateTimeZFormat);
        }
    }
}
