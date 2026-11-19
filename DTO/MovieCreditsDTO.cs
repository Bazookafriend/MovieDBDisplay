using System.Text.Json.Serialization;

namespace DTO
{
    public class MovieCreditsDTO
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("cast")]
        public List<CastDTO> Cast { get; set; } = [];

        [JsonPropertyName("crew")]
        public List<CrewDTO> Crew { get; set; } = [];
    }
}