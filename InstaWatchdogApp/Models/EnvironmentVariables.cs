namespace InstaWatchdogApp.Models;

public record EnvironmentVariables(
    string InstagramToken,
    string InstagramGraphApiUri,
    string DiscordWebHook,
    string GistId,
    string GistToken,
    string GistStateFileName);
