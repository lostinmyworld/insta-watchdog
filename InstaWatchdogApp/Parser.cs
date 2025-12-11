using InstaWatchdogApp.Abstractions;
using InstaWatchdogApp.Models;
using Social.Models.Discord;
using Social.Models.Gist;
using Social.Models.Instagram;
using Social.Oversharers.Abstractions;

namespace InstaWatchdogApp;

public class Parser : IParser
{
    private readonly EnvironmentVariables _environmentVars;

    private static readonly string[] _headers =
    [
        "**Το έχεις δει αυτό;**",
        "**Μήπως σου ξέφυγε αυτό;**",
        "**Κάτι νέο ανέβηκε 👀**",
        "**Νέο post στον αέρα 🚀**",
        "**Ρίξε μια ματιά σε αυτό 👇**",
        "**Τσι τσι ρι πο τσι ρι**",
        "**Notification; Δεν ήρθε; Σου το φέρνω εγώ.**",
        "**Αυτό αξίζει ένα κλικ.**"
    ];

    public Parser(IEnvironmentLoader environmentLoader)
    {
        var gistOptionsToRetrieve = new GistOptions()
        {
            GistId = "GIST_ID",
            GistToken = "GIST_TOKEN",
            GistStateFileName = "GIST_STATE_FILE_NAME",
        };
        var gistOptions = environmentLoader.LoadGistOptions(gistOptionsToRetrieve);

        _environmentVars = RetrieveEnvironmentVariables(gistOptions!);
    }

    public EnvironmentVariables GetEnvironmentVariables()
    {
        return _environmentVars;
    }

    public InstagramPost? GetOlderNotSharedPost(List<InstagramPost> posts, LastState? state)
    {
        // no posts retrieved => nothing to do
        if (posts is null || posts.Count == 0)
        {
            return null;
        }

        var lastPostId = state?.LastPostId;

        var lastIndex = string.IsNullOrWhiteSpace(lastPostId)
            ? -1
            : posts.FindIndex(p => p.Id == lastPostId);

        // no previous post or not found => return oldest post
        if (lastIndex < 0)
        {
            return posts[^1];
        }

        // no new posts, all are shared => nothing to do
        if (lastIndex == 0)
        {
            return null;
        }

        return posts[lastIndex - 1];
    }

    public DiscordRequest BuildDiscordRequest(InstagramPost post)
    {
        var lines = new List<string>
        {
            GetRandomHeader()
        };

        if (!string.IsNullOrWhiteSpace(post.Permalink))
        {
            lines.Add(post.Permalink);
        }

        return new()
        {
            Content = string.Join("\n", lines)
        };
    }

    private static EnvironmentVariables RetrieveEnvironmentVariables(GistOptions gistOptions)
    {
        var instagramToken = Environment.GetEnvironmentVariable("IG_ACCESS_TOKEN");
        if (string.IsNullOrWhiteSpace(instagramToken))
        {
            Console.Error.WriteLine("Missing environment variable: IG_ACCESS_TOKEN.");
        }
        var instagramGraphApiToken = Environment.GetEnvironmentVariable("INSTAGRAM_GRAPH_API_MEDIA_URI");
        if (string.IsNullOrWhiteSpace(instagramGraphApiToken))
        {
            Console.Error.WriteLine("Missing environment variable: INSTAGRAM_GRAPH_API_MEDIA_URI.");
        }
        var discordWebhookUrl = Environment.GetEnvironmentVariable("DISCORD_WEBHOOK_URL");
        if (string.IsNullOrWhiteSpace(discordWebhookUrl))
        {
            Console.Error.WriteLine("Missing environment variable: DISCORD_WEBHOOK_URL.");
        }

        return new EnvironmentVariables(
            instagramToken!,
            instagramGraphApiToken!,
            discordWebhookUrl!,
            gistOptions);
    }

    private static string GetRandomHeader()
    {
        var index = Random.Shared.Next(_headers.Length);
        return _headers[index];
    }
}
