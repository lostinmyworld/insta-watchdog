using InstaWatchdogApp.Abstractions;
using InstaWatchdogApp.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Social.Models.Instagram;
using Social.Oversharers.Abstractions;

var services = new ServiceCollection();

services.AddDependencies();

var serviceProvider = services.BuildServiceProvider();

var parser = serviceProvider.GetRequiredService<IParser>();

var environmentVars = parser.GetEnvironmentVariables();

var instagramConsumer = serviceProvider.GetRequiredService<IInstagramConsumer>();
var discordSharer = serviceProvider.GetRequiredService<IDiscordSharer>();
var gistConsumer = serviceProvider.GetRequiredService<IGistConsumer>();

var state = await gistConsumer.LoadPreviousState(environmentVars.GistOptions);
Console.WriteLine($"LastInstagramId from state: {state.LastPostId ?? "null"}.");

var instaRequest = new InstagramRequest(environmentVars.InstagramToken, "insta-watchdog/1.0", HowManyPostsToFetch: 20);
// Retrieve the latest Instagram posts
var instagramPosts = await instagramConsumer.RetrievePostsAsync(instaRequest);
if (instagramPosts.Count == 0)
{
    await Console.Error.WriteLineAsync("No posts returned from Instagram.");
    return;
}

// Find 1 not shared in Discord (its post id will be located in state if it is)
var postToShare = parser.GetOlderNotSharedPost(instagramPosts, state);
if (postToShare is null)
{
    Console.WriteLine("No new post to share.");
    return;
}

// Share Instagram link to Discord
var discordRequest = parser.BuildDiscordRequest(postToShare);
await discordSharer.SendToDiscord(discordRequest, environmentVars.DiscordWebHook);

// Saved shared post Id to Gist
state.LastPostId = postToShare.Id;
await gistConsumer.SaveCurrentState(state, environmentVars.GistOptions);

Console.WriteLine("Finished.");
