using System.Text.Json.Serialization;

namespace InstaWatchdogApp.Models;

public class GistResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = default!;

    [JsonPropertyName("files")]
    public Dictionary<string, GistFile> Files { get; set; } = [];
}

public class GistFile
{
    [JsonPropertyName("filename")]
    public string FileName { get; set; } = default!;

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
}
