using System.Net;
using Discord.WebSocket;
using Discord;

namespace PixelPerfectBot.Core
{
    public class Utils
    {
        public async Task<string> GetRequest(string url)
        {
            HttpResponseMessage request = await new HttpClient().GetAsync(url);
            using Stream webStream = request.Content.ReadAsStream();

            using StreamReader reader = new StreamReader(webStream);
            string data = reader.ReadToEnd();
            return data;
        }

        public async Task DiscordLog(SocketGuild guild, string Title, string Description, Color? color = null)
        {
            if(color == null)
            {
                color = Color.DarkGrey;
            }

            var embed = new EmbedBuilder()
                .WithTitle(Title)
                .WithDescription(Description)
                .WithTimestamp(DateTime.Now)
                .WithColor((Color)color);
            await guild.GetTextChannel(Config.BotConfiguration.LoggingChannel).SendMessageAsync(embed: embed.Build());
        }
    }
}
