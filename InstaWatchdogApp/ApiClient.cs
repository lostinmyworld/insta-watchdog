using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using InstaWatchdogApp.Models;
using Microsoft.AspNetCore.WebUtilities;

namespace InstaWatchdogApp;

public class ApiClient
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
    };

    private readonly static string _instagramFields = "id,caption,media_type,media_url,permalink,timestamp";

    private const int FetchLimit = 20; // how many latest posts to check

    private readonly EnvironmentVariables _environmentVariables;
    private readonly HttpClient _httpClient;

    public ApiClient(EnvironmentVariables environmentVariables)
    {
        _environmentVariables = environmentVariables;
        _httpClient = new HttpClient();
    }

    public async Task<List<Post>> RetrievePostsAsync()
    {
        var queryParams = new Dictionary<string, string>
        {
            ["fields"] = _instagramFields,
            ["limit"] = FetchLimit.ToString(),
            ["access_token"] = _environmentVariables.InstagramToken
        };

        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("insta-watchdog/1.0");

        var uriToCall = QueryHelpers.AddQueryString(Endpoints.InstagramGraphApiUri, queryParams!);

        var json = await _httpClient.GetStringAsync(uriToCall);
        var postResponse = JsonSerializer.Deserialize<PostResponse>(json, _jsonOptions);

        var instagramPosts = postResponse?.Data ?? [];

        foreach (var post in instagramPosts)
        {
            post.TimeOffset = ParseTimestamp(post.Timestamp);
        }

        return instagramPosts.OrderByDescending(post => post.TimeOffset)
           .ToList();
    }

    public async Task<InstaState> LoadPreviousStateAsync()
    {
        var requestUri = Endpoints.GithubGistApiUri.Replace("{gistId}", _environmentVariables.GistId);
        using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        request.Headers.Authorization = new("Bearer", _environmentVariables.GistToken);
        request.Headers.UserAgent.ParseAdd("insta-watchdog/1.0");

        using var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var gistResponse = JsonSerializer.Deserialize<GistResponse>(json, _jsonOptions);

        if (gistResponse?.Files is null
            || gistResponse.Files.Count == 0
            || !gistResponse.Files.TryGetValue(_environmentVariables.GistStateFileName, out var stateFile)
            || string.IsNullOrWhiteSpace(stateFile.Content))
        {
            return new();
        }

        try
        {
            return JsonSerializer.Deserialize<InstaState>(stateFile.Content, _jsonOptions)
                ?? new();
        }
        catch (JsonException ex)
        {
            await Console.Error.WriteLineAsync($"Failed to deserialize InstaState from Gist with message {ex.Message}.");

            return new();
        }
    }

    public async Task SaveCurrentStateAsync(InstaState state)
    {
        var stateJson = JsonSerializer.Serialize(state, _jsonOptions);

        var payload = new GistResponse
        {
            Files = new()
            {
                [_environmentVariables.GistStateFileName] = new GistFile
                {
                    Content = stateJson
                }
            }
        };

        var json = JsonSerializer.Serialize(payload, _jsonOptions);

        var requestUri = Endpoints.GithubGistApiUri.Replace("{gistId}", _environmentVariables.GistId);
        using var request = new HttpRequestMessage(HttpMethod.Patch, requestUri)
        {
            Content = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _environmentVariables.GistToken);
        request.Headers.UserAgent.ParseAdd("insta-watchdog/1.0");

        using var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    private static DateTimeOffset ParseTimestamp(string? timeStamp)
    {
        if (string.IsNullOrWhiteSpace(timeStamp))
        {
            return DateTimeOffset.MinValue;
        }

        if (timeStamp.EndsWith("+0000"))
        {
            timeStamp = timeStamp[..^5] + "+00:00";
        }

        if (DateTimeOffset.TryParse(timeStamp, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dto))
        {
            return dto;
        }

        return DateTimeOffset.MinValue;
    }

    public async Task SendToDiscordAsync(DiscordRequest discordRequest)
    {
        var jsonPayload = JsonSerializer.Serialize(discordRequest);
        var request = new HttpRequestMessage(HttpMethod.Post, _environmentVariables.DiscordWebHook)
        {
            Content = new StringContent(jsonPayload, Encoding.UTF8, MediaTypeNames.Application.Json)
        };

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        Console.WriteLine($"Discord response: {(int)response.StatusCode} {response.StatusCode}");
    }
}
