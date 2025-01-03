using System.Text.Json.Serialization;

namespace FerrexWeb.Models
{
    public class JSResult
    {
        [JsonPropertyName("distanceKm")]
        public double DistanceKm { get; set; }

        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }
    }
}
