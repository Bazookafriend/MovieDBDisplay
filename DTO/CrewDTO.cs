using System.Text.Json.Serialization;

namespace DTO
{
    public class CrewDTO : CreditDTO
    {
        [JsonPropertyName("department")]
        public string Department { get; set; } = "";

        [JsonPropertyName("job")]
        public string Job { get; set; } = "";
    }
}