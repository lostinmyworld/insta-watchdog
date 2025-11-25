using System.Text.Json.Serialization;

namespace InstaWatchdogApp.Models;

public class PostResponse
{
    [JsonPropertyName("data")]
    public List<Post> Data { get; set; } = [];
}
