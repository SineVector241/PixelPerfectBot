using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Net;
using BinaryIO;
using System.Net.Sockets;

namespace PixelPerfectBot.Core.SlashCommands
{
    public class GeneralCommands : InteractionModuleBase<SocketInteractionContext<SocketInteraction>>
    {
        private Database DB = new Database();
        [SlashCommand("suggest", "Submit a suggestion")]
        public async Task Suggestion()
        {
#if DEBUG
            await RespondAsync("BOT IS IN DEV MODE. This is currently disabled!", ephemeral: true);
            return;
#endif
            var user = DB.GetUser(Context.User.Id);;
            if (user.SuggestionCooldown > DateTime.UtcNow)
            {
                await RespondAsync($"You are on cooldown for this. Please try again <t:{DateTimeOffset.FromFileTime(user.SuggestionCooldown.ToFileTime()).ToUnixTimeSeconds()}:R>", ephemeral: true);
                return;
            }
            await RespondWithModalAsync<Interactions.SuggestionApplication>("SubmitSuggestion");
        }

        [SlashCommand("pingserver", "Pings a minecraft bedrock server")]
        public async Task PingServerMCBE(string IP, int Port)
        {
            try
            {
                await RespondAsync("Coming soon", ephemeral: true);
            }
            catch (Exception ex)
            {
                await RespondAsync(ex.Message);
            }
        }

        [AutocompleteCommand("query", "mcdoc")]
        public async Task MCDocAutocomplete()
        {
            string userInput = (Context.Interaction as SocketAutocompleteInteraction).Data.Current.Value.ToString();
            List<AutocompleteResult> results = new List<AutocompleteResult>();
            foreach(var MCDocData in Database.MCDocsData.Molang)
            {
                if(MCDocData.FunctionName.Contains(userInput, StringComparison.InvariantCultureIgnoreCase))
                    results.Add(new AutocompleteResult(MCDocData.FunctionName, MCDocData.FunctionName));
            }; // only send suggestions that starts with user's input; use case insensitive matching
            Console.WriteLine(userInput);


            // max - 25 suggestions at a time
            await (Context.Interaction as SocketAutocompleteInteraction).RespondAsync(results.Take(25));
        }

        [SlashCommand("mcdoc", "Minecraft documentation searcher/query's")]
        public async Task MCDocs([Summary("query"), Autocomplete] string query)
        {
            if(query.StartsWith("Molang:"))
            {
                var data = Database.MCDocsData.Molang.FirstOrDefault(x => x.FunctionName == query);
                var embed = new EmbedBuilder()
                    .WithTitle(data.FunctionName.Replace("Molang:", ""))
                    .WithDescription(data.Description)
                    .AddField("Examples", "No Examples")
                    .AddField("Link Resources", "No Resources")
                    .WithTimestamp(DateTime.UtcNow)
                    .WithColor(Color.Blue);
                await RespondAsync(embed: embed.Build());
            }
        }
    }
}
