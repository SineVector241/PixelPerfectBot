using Discord;
using Discord.WebSocket;
using Discord.Interactions;

namespace PixelPerfectBot.Core.SlashCommands
{
    [Group("admin","Admin Commands")]
    [RequireOwner]
    public class AdminCommands : InteractionModuleBase<SocketInteractionContext<SocketInteraction>>
    {
        private Database DB = new Database();

        [SlashCommand("sendsupportembed", "Sends the support embed with buttons")]
        public async Task SendSupportEmbed(SocketTextChannel channel)
        {
            try
            {
                await DeferAsync();
                var embed = new EmbedBuilder()
                    .WithTitle("Support")
                    .WithDescription("Welcome to our support channel!\n\nIf you would like support regarding Pixel Perfect services and content feel free to ask for advice or a question! 💜\n\nDo not hesitate— We would love to get in touch with you!\n\n—*The Pixel Perfect Team* <:PixelPlay:998441350691307520>")
                    .WithColor(Color.Green);
                var builder = new ComponentBuilder()
                    .WithButton("Pixel Perfect | General Support", "GeneralSupport", ButtonStyle.Secondary, Emoji.Parse("👋"))
                    .WithButton("Pixel Perfect Content | Support", "ContentSupport", ButtonStyle.Success, Emoji.Parse("💬"), row: 1)
                    .WithButton("Pixel Perfect MCBE Server | Support", "ServerSupport", ButtonStyle.Success, Emoji.Parse("💬"), row: 1)
                    .WithButton("Claim VIP Role", "VIP", ButtonStyle.Primary, Emoji.Parse("⭐"), row: 2)
                    .WithButton("Apply For Content Creator", "ContentCreator", ButtonStyle.Danger, Emoji.Parse("✅"), row: 2)
                    .WithButton("Contact Us", style: ButtonStyle.Link, url: "https://www.pixelsperfect.net/contact-us", row: 2);

                await channel.SendMessageAsync(embed: embed.Build(), components: builder.Build());
                await FollowupAsync("Successfully sent");
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

        [SlashCommand("emojiinfo", "Emoji Information")]
        public async Task EmojiInfo(string emoji)
        {
            await DeferAsync();
            var emote = Emote.Parse(emoji);
            await FollowupAsync(emote.Id.ToString());
        }
        
        [SlashCommand("sendmessage", "Sends a message in a specific channel")]
        public async Task SendMessage(string Message, SocketTextChannel? channel = null)
        {
            try
            {
                if (channel == null)
                {
                    await Context.Channel.SendMessageAsync(Message);
                    await RespondAsync("Sent Message", ephemeral: true);
                }
                else
                {
                    await channel.SendMessageAsync(Message);
                    await RespondAsync("Sent Message", ephemeral: true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                var embed = new EmbedBuilder()
                    .WithTitle("An error has occured")
                    .WithDescription($"Error Message: {ex.Message}")
                    .WithColor(Color.DarkRed);
                await RespondAsync(embed: embed.Build());
            }
        }
    }
}
