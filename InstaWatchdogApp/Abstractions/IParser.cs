using InstaWatchdogApp.Models;
using Social.Models.Discord;
using Social.Models.Gist;
using Social.Models.Instagram;

namespace InstaWatchdogApp.Abstractions;

public interface IParser
{
    EnvironmentVariables GetEnvironmentVariables();
    InstagramPost? GetOlderNotSharedPost(List<InstagramPost> posts, LastState? state);
    DiscordRequest BuildDiscordRequest(InstagramPost post);
}
