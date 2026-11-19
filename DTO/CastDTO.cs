using System.Text.Json.Serialization;

namespace DTO
{
    public class CastDTO : CreditDTO
    {
        [JsonPropertyName("cast_id")]
        public int CastId { get; set; }

        [JsonPropertyName("character")]
        public string Character { get; set; } = "";

        [JsonPropertyName("order")]
        public int Order { get; set; }
    }
}