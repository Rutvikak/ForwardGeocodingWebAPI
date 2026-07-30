using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GeocodingAPI.Models
{
    public class UserRequest
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ID { get; set; }
        public string OriginalAddress { get; set; } = string.Empty;

        public byte[] HashValue { get; set; } = Array.Empty<byte>();
    }

    public class UserRequestEachAddress
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ID { get; set; }

        public string EachAddress { get; set; }

        public long? GeoCodeRequestID { get; set; }

        public GeoCodeRequest? GeoCodeRequests { get; set; }

        public byte[] HashValue { get; set; } = Array.Empty<byte>();
        public string? ErrorInAddress { get; set; }

    }

    public class GeoCodeRequest
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ID { get; set; }

        public long UserRequestID { get; set; }

        public long? CanadianAddressID { get; set; }
        public CanadianAddress? CanadianAddress { get; set; }

        public long? GeoCodeResponseID { get; set; }
        public GeoCodeResponse? GeoCodeResponse { get; set; }

    }

    public class CanadianAddress
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ID { get; set; }
        public string Unit { get; set; } = string.Empty;
        public string BuildingNumber { get; set; } = string.Empty;
        public string StreetName { get; set; } = string.Empty;
        public string Direction { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string ProvinceName { get; set; } = string.Empty;
        public int ProvinceNumber { get; set; }
        public string PostalCode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;

        public byte[] HashValue { get; set; } = Array.Empty<byte>();

        public string JoinAddress()
        {
            string[] members = { this.Unit, this.BuildingNumber, this.StreetName, this.Direction, this.City, this.ProvinceNumber.ToString(), this.PostalCode, this.Country };
            return string.Join("", members.Where(x => !string.IsNullOrEmpty(x)));
        }

        public string GetUrl()
        {
            string url = string.Empty;

            // Appending address components to URL 
            url = $"street={this.BuildingNumber ?? string.Empty} {this.StreetName}&city={this.City}&state={this.ProvinceName}";
            if (url != string.Empty)
            {
                url += $"&country={this.Country}";
                url = url.Replace(" ", "+");
                url += $"&postalcode={this.PostalCode}";
            }

            return url;
        }

    }

}