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
            }
            catch (Exception ex)
            {
                await RespondAsync(ex.Message);
            }
        }
    }
}
