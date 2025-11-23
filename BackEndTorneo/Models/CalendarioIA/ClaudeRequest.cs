using System.Text.Json.Serialization; 

namespace BackEndTorneo.Models.CalendarioIA
{
    public class ClaudeRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = null!;

        [JsonPropertyName("max_tokens")] 
        public int Max_Tokens { get; set; }

        [JsonPropertyName("messages")]
        public List<ClaudeMessage> Messages { get; set; } = new();
    }

    public class ClaudeMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = null!;

        [JsonPropertyName("content")]
        public string Content { get; set; } = null!;
    }
}