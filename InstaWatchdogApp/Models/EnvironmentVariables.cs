using Social.Models.Gist;

namespace InstaWatchdogApp.Models;

public record EnvironmentVariables(
    string InstagramToken,
    string InstagramGraphApiUri,
    string DiscordWebHook,
    GistOptions GistOptions);
