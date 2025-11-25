using InstaWatchdogApp;

var parser = new Parser();

var environmentVars = parser.GetEnvironmentVariables();

var apiClient = new ApiClient(environmentVars);

try
{
    var state = await apiClient.LoadPreviousStateAsync();
    Console.WriteLine($"LastInstagramId from state: {state.LastPostId ?? "<null>"}");

    // Retrieve the latest Instagram posts
    var instagramPosts = await apiClient.RetrievePostsAsync();
    if (instagramPosts.Count == 0)
    {
        await Console.Error.WriteLineAsync("No posts returned from Instagram.");
        return;
    }

    // Find 1 not shared in Discord (its post id will be located in state if it is)
    var postToShare = parser.GetNewPost(instagramPosts, state);
    if (postToShare is null)
    {
        return;
    }

    // Share Instagram link to Discord
    var discordRequest = parser.BuildDiscordRequest(postToShare);
    await apiClient.SendToDiscordAsync(discordRequest);

    // Saved shared post Id to Gist
    state.LastPostId = postToShare.Id;
    await apiClient.SaveCurrentStateAsync(state);

    Console.WriteLine("Finished.");
}
catch (Exception ex)
{
    await Console.Error.WriteLineAsync("Error in InstaWatchdog:");
    Console.Error.WriteLine(ex);
}
