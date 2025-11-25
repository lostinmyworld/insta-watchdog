using InstaWatchdogApp.Models;

namespace InstaWatchdogApp;

internal class Parser
{
    private readonly EnvironmentVariables _environmentVars;

    private static readonly string[] _headers =
    {
        "**Το έχεις δει αυτό;**",
        "**Μήπως σου ξέφυγε αυτό;**",
        "**Κάτι νέο ανέβηκε 👀**",
        "**Νέο post στον αέρα 🚀**",
        "**Ρίξε μια ματιά σε αυτό 👇**",
        "**Τσι τσι ρι πο τσι ρι**",
        "**Notification; Δεν ήρθε; Σου το φέρνω εγώ.**",
        "**Αυτό αξίζει ένα κλικ.**"
    };

    public Parser()
    {
        LoadLocalEnv(".env.local");

        var instagramToken = Environment.GetEnvironmentVariable("IG_ACCESS_TOKEN");
        var instagramGraphApiToken = Environment.GetEnvironmentVariable("INSTAGRAM_GRAPH_API_MEDIA_URI");
        var discordWebhookUrl = Environment.GetEnvironmentVariable("DISCORD_WEBHOOK_URL");
        var gistId = Environment.GetEnvironmentVariable("GIST_ID");
        var gistToken = Environment.GetEnvironmentVariable("GIST_TOKEN");
        var gistStateFileName = Environment.GetEnvironmentVariable("GIST_STATE_FILE_NAME");

        if (string.IsNullOrWhiteSpace(instagramToken)
            || string.IsNullOrWhiteSpace(instagramGraphApiToken)
            || string.IsNullOrWhiteSpace(discordWebhookUrl)
            || string.IsNullOrWhiteSpace(gistId)
            || string.IsNullOrWhiteSpace(gistToken)
            || string.IsNullOrWhiteSpace(gistStateFileName))
        {
            Console.Error.WriteLine("Missing one or more required env vars: IG_ACCESS_TOKEN, DISCORD_WEBHOOK_URL, GIST_ID, GIST_TOKEN.");
        }

        _environmentVars = new EnvironmentVariables(
            instagramToken!,
            instagramGraphApiToken!,
            discordWebhookUrl!,
            gistId!,
            gistToken!,
            gistStateFileName!);
    }

    public EnvironmentVariables GetEnvironmentVariables()
    {
        return _environmentVars;
    }

    public Post? GetNewPost(List<Post> posts, InstaState? state)
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

    public DiscordRequest BuildDiscordRequest(Post post)
    {
        var lines = new List<string>
        {
            GetRandomHeader()
        };

        if (!string.IsNullOrWhiteSpace(post.Permalink))
        {
            lines.Add(post.Permalink);
        }

        var embed = new Dictionary<string, object?>
        {
            ["title"] = post.MediaType ?? "MEDIA",
            ["url"] = post.Permalink,
            ["timestamp"] = post.Timestamp
        };

        if (!string.IsNullOrWhiteSpace(post.MediaUrl))
        {
            embed["image"] = new { url = post.MediaUrl };
        }

        return new()
        {
            Content = string.Join("\n", lines),
            Embeds = [embed]
        };
    }

    private static void LoadLocalEnv(string path)
    {
        try
        {
            if (!File.Exists(path))
            {
                return;
            }

            Console.WriteLine($"Loading local env from {path}...");
            var lines = File.ReadAllLines(path);

            foreach (var raw in lines)
            {
                var line = raw.Trim();
                if (string.IsNullOrWhiteSpace(line)
                    || line.StartsWith('#'))
                {
                    continue;
                }

                var index = line.IndexOf('=', StringComparison.Ordinal);
                if (index <= 0)
                {
                    continue;
                }

                var key = line[..index].Trim();
                var value = line[(index + 1)..].Trim();

                Environment.SetEnvironmentVariable(key, value);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Failed to load .env.local: " + ex.Message);
        }
    }

    private static string GetRandomHeader()
    {
        var index = Random.Shared.Next(_headers.Length);
        return _headers[index];
    }
}
