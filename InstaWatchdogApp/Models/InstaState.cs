using System.Text.Json.Serialization;

namespace InstaWatchdogApp.Models;

public class InstaState
{
    [JsonPropertyName("lastInstagramId")]
    public string? LastPostId { get; set; }
}