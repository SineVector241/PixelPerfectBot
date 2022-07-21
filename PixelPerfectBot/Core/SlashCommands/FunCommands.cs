using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace PixelPerfectBot.Core.SlashCommands
{
    [Group("fun", "Fun Commands")]
    public class FunCommands : InteractionModuleBase<SocketInteractionContext<SocketInteraction>>
    {
        private Utils utils = new Utils();
        [SlashCommand("joke", "Sends a random joke")]
        [RequireContext(ContextType.Guild)]
        public async Task Joke()
        {
            try
            {
                await DeferAsync();
                var data = await utils.GetRequest("https://v2.jokeapi.dev/joke/Any?blacklistFlags=nsfw,religious,political,racist,sexist");
                dynamic Data = JsonConvert.DeserializeObject(data);
                EmbedBuilder embed = new EmbedBuilder();

                if (Data["type"] == "twopart")
                {
                    embed.Title = Data["setup"];
                    embed.Description = $"||{Data["delivery"]}||";
                    embed.Color = Color.Blue;
                    await FollowupAsync(embed: embed.Build());
                }
                else if (Data["type"] == "single")
                {
                    embed.Title = "Single Joke";
                    embed.Description = $"{Data["joke"]}";
                    embed.Color = Color.Blue;
                    await FollowupAsync(embed: embed.Build());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                var embed = new EmbedBuilder()
                    .WithTitle("An error has occured")
                    .WithDescription($"Error Message: {ex.Message}")
                    .WithColor(Color.DarkRed);
                await FollowupAsync(embed: embed.Build());
            }
        }
    }
}
