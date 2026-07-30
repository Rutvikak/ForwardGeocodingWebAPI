using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace GeocodingAPI.Models
{
    public class GeoCodeResponse
    {
        [Key]
        [JsonPropertyName("place_id")]
        public long PlaceId { get; set; }

        [JsonPropertyName("licence")]
        public string Licence { get; set; } = string.Empty;
        [JsonPropertyName("osm_type")]
        public string OSMType { get; set; } = string.Empty;
        [JsonPropertyName("osm_id")]

        public long OSMId { get; set; }
        [JsonPropertyName("lat")]

        public string Latitude { get; set; } = string.Empty;
        [JsonPropertyName("lon")]

        public string Longitude { get; set; } = string.Empty;
        [JsonPropertyName("class")]

        public string ClassName { get; set; } = string.Empty;
        [JsonPropertyName("type")]

        public string Type { get; set; } = string.Empty;
        [JsonPropertyName("place_rank")]
        public int PlaceRank { get; set; }
        [JsonPropertyName("importance")]
        public decimal Importance { get; set; }
        [JsonPropertyName("addresstype")]
        public string AddressType { get; set; } = string.Empty;
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; } = string.Empty;
        [JsonPropertyName("address")]
        public GeoCodeResponseAddress? address { get; set; } = null;
        [JsonPropertyName("boundingbox")]
        public List<string>? BoundingBox { get; set; } = null;
    }

    public class GeoCodeResponseAddress
    {
        [JsonPropertyName("road")]
        public string Road { get; set; } = string.Empty;
        [JsonPropertyName("neighbourhood")]
        public string Neighbourhood { get; set; } = string.Empty;
        [JsonPropertyName("city_district")]
        public string CityDistrict { get; set; } = string.Empty;
        [JsonPropertyName("city")]
        public string City { get; set; } = string.Empty;
        [JsonPropertyName("county")]
        public string County { get; set; } = string.Empty;
        [JsonPropertyName("state")]
        public string State { get; set; } = string.Empty;
        [JsonPropertyName("ISO3166-2-lvl4")]
        public string ISO3166 { get; set; } = string.Empty;
        [JsonPropertyName("postcode")]
        public string Postcode { get; set; } = string.Empty;
        [JsonPropertyName("country")]
        public string Country { get; set; } = string.Empty;
        [JsonPropertyName("country_code")]
        public string CountryCode { get; set; } = string.Empty;
    }

   public class UserResponse
    {
        public string UserQuerryAddress { get; set; }

        public string UserResponseString { get; set; } = "Data Found";
        public GeoCodeResponse? GeoCodeResponse { get; set; }
    }
}
