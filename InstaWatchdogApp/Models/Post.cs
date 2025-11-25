using System.Text.Json.Serialization;

namespace InstaWatchdogApp.Models;

public class Post
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = default!;

    [JsonPropertyName("caption")]
    public string? Caption { get; set; }

    [JsonPropertyName("media_type")]
    public string? MediaType { get; set; }

    [JsonPropertyName("media_url")]
    public string? MediaUrl { get; set; }

    [JsonPropertyName("permalink")]
    public string? Permalink { get; set; }

    [JsonPropertyName("timestamp")]
    public string? Timestamp { get; set; }

    [JsonIgnore]
    public DateTimeOffset? TimeOffset { get; set; }
}
