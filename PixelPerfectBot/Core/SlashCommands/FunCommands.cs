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
        [SlashCommand("kill", "Kills someone")]
        public async Task Kill(SocketGuildUser Target)
        {
            if (Context.User.Id != Target.Id)
            {
                string[] KillList = { 
                    "[1] stabbed [2] to death", 
                    "[1] chopped up [2] into thin slices", 
                    "[2] was flattened by [1] who was driving a truck",
                    "[2] was consumed by [1]",
                    "[1] blew up [2]",
                    "[1] shot down [2] in a tank",
                    "[2] was sniped by [1]",
                    "[1] amogus killed [2]",
                    "[1] trapped [2] in a bedrock box who later starved to death",
                    "[1] had a bad day and took it out on [2]",
                    "[1] shook [2] hand too hard that it tore off their arm and bled to death",
                    "[1] pushed [2] into the void",
                    "[1] threw [2] to the moon who didn't survive 1/1000000000000000000000000000 of the journey before they died.",
                    "[1] force fed [2] too many burgers before they exploded",
                    "[1] stuck a sticky bomb onto [2]'s back",
                    "[1] force fed [2] explodable candy",
                    "[1] snowball fought [2] too hard that they turned to ice",
                    "[1] alt+f4'ed [2] out of existence",
                    "[1] gave [2] an unlucky block",

                    "[1] was flattened by an iron golem while trying to kill [2]",
                    "[1] was flattened by a random tree while trying to kill [2]",
                    "[1] stepped on a mine while trying to sneak up on [2]",
                    "[1] tried to kill [2] but realised they were a ghost",
                    "[1] made a trap too powerful that it created a black hole"
                    };
                string death = KillList.ElementAt(new Random().Next(0, KillList.Length));
                var embed = new EmbedBuilder()
                    .WithTitle("Kill")
                    .WithDescription(death.Replace("[1]", Context.User.Mention).Replace("[2]", Target.Mention))
                    .WithAuthor(Context.User)
                    .WithColor(Color.Red);
                await RespondAsync(embed: embed.Build());
            }
            else
                await RespondAsync("Ok you suicided. Select someone else next time.", ephemeral: true);
        }

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
