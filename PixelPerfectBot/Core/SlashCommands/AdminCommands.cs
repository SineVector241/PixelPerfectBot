using Discord;
using Discord.WebSocket;
using Discord.Interactions;

namespace PixelPerfectBot.Core.SlashCommands
{
    [Group("dev", "Developer Commands")]
    [RequireOwner]
    public class DeveloperCommands : InteractionModuleBase<SocketInteractionContext<SocketInteraction>>
    {
        private Database DB = new Database();

        [SlashCommand("deletemsg", "Deletes a message in a channel")]
        public async Task DeleteMessage(string MsgId, SocketTextChannel channel)
        {
            await DeferAsync();
            var Id = Convert.ToUInt64(MsgId);
            var msg = await channel.GetMessageAsync(Id);
            await msg.DeleteAsync();
            await FollowupAsync("Deleted Message");
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

    [Group("staff", "Staff Commands")]
    [RequireUserPermission(GuildPermission.ManageMessages)]
    public class AdminCommands : InteractionModuleBase<SocketInteractionContext<SocketInteraction>>
    {
        private Database DB = new Database();

        [SlashCommand("serverinfo", "Displays server information")]
        public async Task ServerInformation()
        {
            var embed = new EmbedBuilder()
                .WithTitle($"Server Information: {Context.Guild.Name}")
                .AddField("Created At", $"{Context.Guild.CreatedAt.DateTime.ToLongDateString()} {Context.Guild.CreatedAt.DateTime.ToLongTimeString()}")
                .AddField("Members", $"{Context.Guild.MemberCount}/{Context.Guild.MaxMembers}")
                .AddField("Categories", Context.Guild.CategoryChannels.Count)
                .AddField("Text Channels", Context.Guild.TextChannels.Count)
                .AddField("Voice Channels", Context.Guild.VoiceChannels.Count)
                .AddField("Stage Channels", Context.Guild.StageChannels.Count)
                .AddField("Roles", Context.Guild.Roles.Count)
                .AddField("Emotes", Context.Guild.Emotes.Count)
                .AddField("Features", Context.Guild.Features.Value)
                .WithThumbnailUrl(Context.Guild.IconUrl)
                .WithAuthor(Context.User)
                .WithColor(Color.Blue);
            await RespondAsync(embed: embed.Build());
        }

        [SlashCommand("userinfo", "Displays the selected user information")]
        public async Task UserInformation(SocketGuildUser? User = null)
        {
            try
            {
                if (User == null)
                    User = Context.User as SocketGuildUser;
                var embed = new EmbedBuilder()
                    .WithTitle($"User Information: {User.Username}")
                    .AddField("Display Name", User.DisplayName)
                    .AddField("Discriminator", User.Discriminator)
                    .AddField("Created At", $"{User.CreatedAt.DateTime.ToLongDateString()} {User.CreatedAt.DateTime.ToLongTimeString()}")
                    .AddField("Joined At", $"{User.JoinedAt.Value.DateTime.ToLongDateString()} {User.JoinedAt.Value.DateTime.ToLongTimeString()}")
                    .AddField("Permissions",
                    $"**Administrator:** {User.GuildPermissions.Administrator}\n**Manage Messages:** {User.GuildPermissions.ManageMessages}\n**Manage Channels:** {User.GuildPermissions.ManageChannels}\n**Manage Roles:** {User.GuildPermissions.ManageRoles}\n**Manage Emojis/Stickers:** {User.GuildPermissions.ManageEmojisAndStickers}\n**Manage Events:** {User.GuildPermissions.ManageEvents}".Replace("True", "✅").Replace("False", "❎"))
                    .AddField("Bot", User.IsBot)
                    .WithThumbnailUrl(User.GetAvatarUrl())
                    .WithAuthor(Context.User)
                    .WithColor(Color.Blue);
                await RespondAsync(embed: embed.Build());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        [SlashCommand("embed", "Sends a quick embed")]
        public async Task SendEmbed(SocketTextChannel? channel = null)
        {
            if (channel == null)
                await RespondWithModalAsync<EmbedSendModal>("SendEmbed");
            else
                await RespondWithModalAsync<EmbedSendModal>("SendEmbed");
        }

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

        [Group("settings", "Configure Settings")]
        public class Settings : InteractionModuleBase<SocketInteractionContext<SocketInteraction>>
        {
            private Config config = new Config();

            [SlashCommand("set-category-setting", "Sets a category setting in the bots configuration.")]
            public async Task SetCategorySetting([Choice("GeneralSupportCategory", 0), Choice("ContentSupportCategory", 1), Choice("MCBEServerSupportCategory", 2)] int Setting, SocketCategoryChannel Category)
            {
                switch (Setting)
                {
                    case 0:
                        Config.BotConfiguration.GeneralSupportCategory = Category.Id;
                        break;
                    case 1:
                        Config.BotConfiguration.ContentSupportCategory = Category.Id;
                        break;
                    case 2:
                        Config.BotConfiguration.MCBEServerSupportCategory = Category.Id;
                        break;
                    default:
                        await RespondAsync("That setting does not exist", ephemeral: true);
                        break;
                }
                config.UpdateConfig();
                await RespondAsync("Successfully set setting", ephemeral: true);
            }

            [SlashCommand("view", "View settings of the bot")]
            public async Task ViewSettings()
            {
                var embed = new EmbedBuilder()
                    .WithTitle("Bot Settings")
                    .WithDescription("**@deleted-role = Not Set/Configured/Removed**\n**#deleted-channel = Not Set/Configured/Removed**")
                    .AddField("General Support Category", $"<#{Config.BotConfiguration.GeneralSupportCategory}>")
                    .AddField("Content Support Category", $"<#{Config.BotConfiguration.ContentSupportCategory}>")
                    .AddField("MCBEServer Support Category", $"<#{Config.BotConfiguration.MCBEServerSupportCategory}>")
                    .AddField("Application Channel", $"<#{Config.BotConfiguration.ApplicationChannel}>")
                    .AddField("Ticket Logging Channel", $"<#{Config.BotConfiguration.TicketLoggingChannel}>")
                    .AddField("VIP Application Channel", $"<#{Config.BotConfiguration.VIPApplicationChannel}>")
                    .AddField("Suggestion Channel", $"<#{Config.BotConfiguration.SuggestionChannel}>")
                    .AddField("Top Suggestion Channel", $"<#{Config.BotConfiguration.TopSuggestionChannel}>")
                    .AddField("Poll Channel", $"<#{Config.BotConfiguration.PollChannel}>")
                    .AddField("Content Creator Role", $"<@&{Config.BotConfiguration.ContentCreatorRoleId}>")
                    .AddField("Chat Mod Role", $"<@&{Config.BotConfiguration.ChatModRoleId}>")
                    .AddField("Ticket Manager Role", $"<@&{Config.BotConfiguration.TicketManagerRoleId}>")
                    .AddField("VIP Role", $"<@&{Config.BotConfiguration.VIPRoleId}>")
                    .WithColor(Color.Orange);
                await RespondAsync(embed: embed.Build());
            }

            [SlashCommand("set-channel-setting", "Sets a channel setting in the bots configuration")]
            public async Task SetChannelSetting([Choice("ApplicationChannel", 0), Choice("TicketLoggingChannel", 1), Choice("VIPApplicationChannel", 2), Choice("SuggestionChannel", 3), Choice("TopSuggestionChannel", 4), Choice("PollChannel", 5)] int Setting, SocketTextChannel Channel)
            {
                switch (Setting)
                {
                    case 0:
                        Config.BotConfiguration.ApplicationChannel = Channel.Id;
                        break;
                    case 1:
                        Config.BotConfiguration.TicketLoggingChannel = Channel.Id;
                        break;
                    case 2:
                        Config.BotConfiguration.VIPApplicationChannel = Channel.Id;
                        break;
                    case 3:
                        Config.BotConfiguration.SuggestionChannel = Channel.Id;
                        break;
                    case 4:
                        Config.BotConfiguration.TopSuggestionChannel = Channel.Id;
                        break;
                    case 5:
                        Config.BotConfiguration.PollChannel = Channel.Id;
                        break;
                }
                config.UpdateConfig();
                await RespondAsync("Successfully set setting", ephemeral: true);
            }

            [SlashCommand("set-role-setting", "Sets a role setting in the bots configuration")]
            public async Task SetRoleSetting([Choice("ContentCreatorRole", 0), Choice("ChatModRole", 1), Choice("TicketManagerRole", 2), Choice("VIPRole", 3)] int Setting, SocketRole Role)
            {
                switch (Setting)
                {
                    case 0:
                        Config.BotConfiguration.ContentCreatorRoleId = Role.Id;
                        break;
                    case 1:
                        Config.BotConfiguration.ChatModRoleId = Role.Id;
                        break;
                    case 2:
                        Config.BotConfiguration.TicketManagerRoleId = Role.Id;
                        break;
                    case 3:
                        Config.BotConfiguration.VIPRoleId = Role.Id;
                        break;
                }
                config.UpdateConfig();
                await RespondAsync("Successfully set setting", ephemeral: true);
            }
        }

        [SlashCommand("poll", "Sends a poll in a channel")]
        public async Task StaffPoll(string Title, string PollQuestion, SocketRole PingRole,
            string Option1 = "Yes",
            string Option2 = "No",
            string? Option3 = null,
            string? Option4 = null,
            string? Option5 = null,
            string? Option6 = null,
            string? Option7 = null,
            string? Option8 = null,
            string? Option9 = null,
            string? Option10 = null)
        {
            await DeferAsync(true);
            var Channel = Context.Guild.GetTextChannel(Config.BotConfiguration.PollChannel);
            var embed = new EmbedBuilder()
                .WithAuthor(Context.User)
                .WithTimestamp(DateTime.UtcNow)
                .WithColor(Color.Purple)
                .WithTitle(Title)
                .WithDescription(PollQuestion);
            var builder = new ComponentBuilder();
            if (Option1 == "Yes" && Option2 == "No" && Option3 == null && Option4 == null && Option5 == null && Option6 == null && Option7 == null && Option8 == null && Option9 == null && Option10 == null)
            {
                builder.WithButton("Yes: 0", "Option:1", ButtonStyle.Success, Emoji.Parse("✅"));
                builder.WithButton("No: 0", "Option:2", ButtonStyle.Danger, Emoji.Parse("❎"));
                var ynmsg = await Channel.SendMessageAsync($"Poll: {PingRole.Mention}", embed: embed.Build(), components: builder.Build());
                DB.AddPoll(new Database.Poll() { MessageId = ynmsg.Id });
                await FollowupAsync("Successfully sent poll", ephemeral: true);
                return;
            }
            try
            {
                builder.WithButton(": 0", "Option:0", emote: Emoji.Parse("1️⃣"));
                builder.WithButton(": 0", "Option:1", emote: Emoji.Parse("2️⃣"));
                embed.AddField("Option: 1️⃣", Option1);
                embed.AddField("Option: 2️⃣", Option2);
                if (Option3 != null)
                {
                    builder.WithButton(": 0", "Option:2", emote: Emoji.Parse("3️⃣"));
                    embed.AddField("Option: 3️⃣", Option3);
                }
                if (Option4 != null)
                {
                    builder.WithButton(": 0", "Option:3", emote: Emoji.Parse("4️⃣"));
                    embed.AddField("Option: 4️⃣", Option4);
                }
                if (Option5 != null)
                {
                    builder.WithButton(": 0", "Option:4", emote: Emoji.Parse("5️⃣"));
                    embed.AddField("Option: 5️⃣", Option5);
                }
                if (Option6 != null)
                {
                    builder.WithButton(": 0", "Option:5", emote: Emoji.Parse("6️⃣"), row: 1);
                    embed.AddField("Option: 6️⃣", Option6);
                }
                if (Option7 != null)
                {
                    builder.WithButton(": 0", "Option:6", emote: Emoji.Parse("7️⃣"), row: 1);
                    embed.AddField("Option: 7️⃣", Option7);
                }
                if (Option8 != null)
                {
                    builder.WithButton(": 0", "Option:7", emote: Emoji.Parse("8️⃣"), row: 1);
                    embed.AddField("Option: 8️⃣", Option8);
                }
                if (Option9 != null)
                {
                    builder.WithButton(": 0", "Option:8", emote: Emoji.Parse("9️⃣"), row: 1);
                    embed.AddField("Option: 9️⃣", Option9);
                }
                if (Option10 != null)
                {
                    builder.WithButton(": 0", "Option:9", emote: Emoji.Parse("🔟"), row: 1);
                    embed.AddField("Option: 🔟", Option10);
                }
                var msg = await Channel.SendMessageAsync($"Poll: {PingRole.Mention}", embed: embed.Build(), components: builder.Build());
                var polldata = new Database.Poll() { MessageId = msg.Id };
                for (int i = 0; i < 10; i++)
                    polldata.OptionUserIds.Add(new List<ulong>());
                DB.AddPoll(polldata);
                await FollowupAsync("Successfully sent poll", ephemeral: true);
            }
            catch (Exception ex)
            {
                await FollowupAsync($"Error: {ex.Message}", ephemeral: true);
            }
        }
    }

    public class EmbedSendModal : IModal
    {
        public string Title => "Embed";

        [InputLabel("Title of embed")]
        [ModalTextInput("Title")]
        public string ETitle { get; set; }

        [InputLabel("Description")]
        [ModalTextInput("Description", TextInputStyle.Paragraph)]
        public string Description { get; set; }
    }
}