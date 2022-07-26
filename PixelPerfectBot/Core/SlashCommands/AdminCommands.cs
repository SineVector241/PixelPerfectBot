﻿using Discord;
using Discord.WebSocket;
using Discord.Interactions;

namespace PixelPerfectBot.Core.SlashCommands
{
    [Group("dev", "Developer Commands")]
    [RequireOwner]
    public class DeveloperCommands : InteractionModuleBase<SocketInteractionContext<SocketInteraction>>
    {
        private Database DB = new Database();

        [SlashCommand("add-molang-doc", "Adds a new molang doc to the moland documentation")]
        public async Task AddMolangDoc(string FunctionName, string Description)
        {
            await DeferAsync();
            Database.MCDocsData.Molang.Add(new Database.MolangData() { FunctionName = $"Molang:{FunctionName}", Description = Description });
            DB.UpdateDocs();
            await FollowupAsync("Successfully added");
        }

        [SlashCommand("delete-msg", "Deletes a message in a channel")]
        public async Task DeleteMessage(string MsgId, SocketTextChannel channel)
        {
            await DeferAsync();
            var Id = Convert.ToUInt64(MsgId);
            var msg = await channel.GetMessageAsync(Id);
            await msg.DeleteAsync();
            await FollowupAsync("Deleted Message");
        }

        [SlashCommand("emoji-info", "Emoji Information")]
        public async Task EmojiInfo(string emoji)
        {
            await DeferAsync();
            var emote = Emote.Parse(emoji);
            await FollowupAsync(emote.Id.ToString());
        }

        [SlashCommand("send-message", "Sends a message in a specific channel")]
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

        [SlashCommand("create-reactionrole", "Creates a new dashboard to create a reaction role")]
        public async Task ReactionRoleDash(SocketTextChannel Channel)
        {
            var embed = new EmbedBuilder()
                .WithTitle("New Reaction Role Setup")
                .WithDescription("Enter description")
                .WithFooter($"Set to channel: {Channel.Name}")
                .WithColor(Color.Blue);
            var builder = new ComponentBuilder()
                .WithButton("Add Role", $"AddRRRole:{Context.User.Id}", ButtonStyle.Success, Emoji.Parse("➕"))
                .WithButton("Remove Role", $"RemoveRRRole:{Context.User.Id}", ButtonStyle.Danger, Emoji.Parse("➖"))
                .WithButton("Set Title", $"SetRRTitle:{Context.User.Id}", ButtonStyle.Primary, Emoji.Parse("✏️"))
                .WithButton("Set Description", $"SetRRDescription:{Context.User.Id}", ButtonStyle.Primary, Emoji.Parse("✏️"))
                .WithButton("Send/Publish", $"PublishRR:{Context.User.Id},{Channel.Id}", ButtonStyle.Secondary, Emoji.Parse("✅"));
            await RespondAsync(embed: embed.Build(), components: builder.Build());
        }

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
                    .AddField("Flags", User.PublicFlags)
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

        [SlashCommand("send-color-role-embed", "Sends the color role embed in the specified channel")]
        public async Task SendColorRoleEmbed(SocketTextChannel channel)
        {
            var embed = new EmbedBuilder()
                .WithTitle("Color Role Settings")
                .WithDescription("Color Roles")
                .AddField("Blue", $"<@&{Config.BotConfiguration.ColorRoles.Blue}>")
                .AddField("Gold", $"<@&{Config.BotConfiguration.ColorRoles.Gold}>")
                .AddField("Green", $"<@&{Config.BotConfiguration.ColorRoles.Green}>")
                .AddField("Magenta", $"<@&{Config.BotConfiguration.ColorRoles.Magenta}>")
                .AddField("Orange", $"<@&{Config.BotConfiguration.ColorRoles.Orange}>")
                .AddField("Purple", $"<@&{Config.BotConfiguration.ColorRoles.Purple}>")
                .AddField("Red", $"<@&{Config.BotConfiguration.ColorRoles.Red}>")
                .AddField("Teal", $"<@&{Config.BotConfiguration.ColorRoles.Teal}>")
                .AddField("Dark Blue", $"<@&{Config.BotConfiguration.ColorRoles.DarkBlue}>")
                .AddField("Dark Green", $"<@&{Config.BotConfiguration.ColorRoles.DarkGreen}>")
                .AddField("Dark Magenta", $"<@&{Config.BotConfiguration.ColorRoles.DarkMagenta}>")
                .AddField("Dark Orange", $"<@&{Config.BotConfiguration.ColorRoles.DarkOrange}>")
                .AddField("Dark Purple", $"<@&{Config.BotConfiguration.ColorRoles.DarkPurple}>")
                .AddField("Dark Red", $"<@&{Config.BotConfiguration.ColorRoles.DarkRed}>")
                .AddField("Dark Teal", $"<@&{Config.BotConfiguration.ColorRoles.DarkTeal}>")
                .AddField("Light Orange", $"<@&{Config.BotConfiguration.ColorRoles.LightOrange}>")
                .WithColor(Color.Orange);
            var builder = new ComponentBuilder()
                .WithButton("Blue", "SelectColorRole:0")
                .WithButton("Gold", "SelectColorRole:1")
                .WithButton("Green", "SelectColorRole:2")
                .WithButton("Magenta", "SelectColorRole:3")
                .WithButton("Orange", "SelectColorRole:4")
                .WithButton("Purple", "SelectColorRole:5")
                .WithButton("Red", "SelectColorRole:6")
                .WithButton("Teal", "SelectColorRole:7")
                .WithButton("Dark Blue", "SelectColorRole:8")
                .WithButton("Dark Green", "SelectColorRole:9")
                .WithButton("Dark Magenta", "SelectColorRole:10")
                .WithButton("Dark Orange", "SelectColorRole:11")
                .WithButton("Dark Purple", "SelectColorRole:12")
                .WithButton("Dark Red", "SelectColorRole:13")
                .WithButton("Dark Teal", "SelectColorRole:14")
                .WithButton("Light Orange", "SelectColorRole:15");
            await channel.SendMessageAsync(embed: embed.Build(), components: builder.Build());
            await RespondAsync("Successfully sent");
        }

        [SlashCommand("send-support-embed", "Sends the support embed with buttons")]
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

        [SlashCommand("accept-application", "Accepts a user into the content team")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task AcceptUserToContent(SocketGuildUser user)
        {
            await user.SendMessageAsync("**Your application has been successful!** 💜\n\nWelcome to the community as a Pixel Perfect Content Creator! Please refer the the links below for further information. 😄\nPost Guidelines\nhttps://pixelsperfect.net/post-guidelines\nAbout Us\nhttps://pixelsperfect.net/about-us\nPrivate Discord\nhttps://discord.gg/cHc7DsEgHh\n\nPlease note: as a content creator you are required to join our private discord server and abide by our post guidelines. — Thank you! <:PixelPlay:998441350691307520>");
            await user.AddRoleAsync(Config.BotConfiguration.ContentCreatorRoleId);
            await RespondAsync("Accepted/Added user to content moderation team!");
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
            public async Task ViewSettings([Choice("General Settings", 0), Choice("Color Roles", 1)] int type)
            {
                switch (type)
                {
                    case 0:
                        var general = new EmbedBuilder()
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
                            .AddField("Server Booster Role", $"<@&{Config.BotConfiguration.BoosterRoleId}>")
                            .WithColor(Color.Orange);
                        await RespondAsync(embed: general.Build());
                        break;
                    case 1:
                        var colorrole = new EmbedBuilder()
                            .WithTitle("Color Role Settings")
                            .WithDescription("**@deleted-role = Not Set/Configured/Removed**")
                            .AddField("Blue", $"<@&{Config.BotConfiguration.ColorRoles.Blue}>")
                            .AddField("Gold", $"<@&{Config.BotConfiguration.ColorRoles.Gold}>")
                            .AddField("Green", $"<@&{Config.BotConfiguration.ColorRoles.Green}>")
                            .AddField("Magenta", $"<@&{Config.BotConfiguration.ColorRoles.Magenta}>")
                            .AddField("Orange", $"<@&{Config.BotConfiguration.ColorRoles.Orange}>")
                            .AddField("Purple", $"<@&{Config.BotConfiguration.ColorRoles.Purple}>")
                            .AddField("Red", $"<@&{Config.BotConfiguration.ColorRoles.Red}>")
                            .AddField("Teal", $"<@&{Config.BotConfiguration.ColorRoles.Teal}>")
                            .AddField("Dark Blue", $"<@&{Config.BotConfiguration.ColorRoles.DarkBlue}>")
                            .AddField("Dark Green", $"<@&{Config.BotConfiguration.ColorRoles.DarkGreen}>")
                            .AddField("Dark Magenta", $"<@&{Config.BotConfiguration.ColorRoles.DarkMagenta}>")
                            .AddField("Dark Orange", $"<@&{Config.BotConfiguration.ColorRoles.DarkOrange}>")
                            .AddField("Dark Purple", $"<@&{Config.BotConfiguration.ColorRoles.DarkPurple}>")
                            .AddField("Dark Red", $"<@&{Config.BotConfiguration.ColorRoles.DarkRed}>")
                            .AddField("Dark Teal", $"<@&{Config.BotConfiguration.ColorRoles.DarkTeal}>")
                            .AddField("Light Orange", $"<@&{Config.BotConfiguration.ColorRoles.LightOrange}>")
                            .WithColor(Color.Orange);
                        await RespondAsync(embed: colorrole.Build());
                        break;
                }
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
            public async Task SetRoleSetting([Choice("ContentCreatorRole", 0), Choice("ChatModRole", 1), Choice("TicketManagerRole", 2), Choice("VIPRole", 3), Choice("BoosterRole", 4)] int Setting, SocketRole Role)
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
                    case 4:
                        Config.BotConfiguration.BoosterRoleId = Role.Id;
                        break;
                }
                config.UpdateConfig();
                await RespondAsync("Successfully set setting", ephemeral: true);
            }

            [SlashCommand("set-color-role-setting", "Sets a color role setting in the bots configuration")]
            public async Task SetColorRoleSetting([
                Choice("Blue", 0),
                Choice("Gold", 1),
                Choice("Green", 2),
                Choice("Magenta", 3),
                Choice("Orange", 4),
                Choice("Purple", 5),
                Choice("Red", 6),
                Choice("Teal", 7),
                Choice("Dark Blue", 8),
                Choice("Dark Green", 9),
                Choice("Dark Magenta", 10),
                Choice("Dark Orange", 11),
                Choice("Dark Purple", 12),
                Choice("Dark Red", 13),
                Choice("Dark Teal", 14),
                Choice("Light Orange", 15),]
            int ColorRole, SocketRole Role)
            {
                switch(ColorRole)
                {
                    case 0:
                        Config.BotConfiguration.ColorRoles.Blue = Role.Id;
                        break;
                    case 1:
                        Config.BotConfiguration.ColorRoles.Gold = Role.Id;
                        break;
                    case 2:
                        Config.BotConfiguration.ColorRoles.Green = Role.Id;
                        break;
                    case 3:
                        Config.BotConfiguration.ColorRoles.Magenta = Role.Id;
                        break;
                    case 4:
                        Config.BotConfiguration.ColorRoles.Orange = Role.Id;
                        break;
                    case 5:
                        Config.BotConfiguration.ColorRoles.Purple = Role.Id;
                        break;
                    case 6:
                        Config.BotConfiguration.ColorRoles.Red = Role.Id;
                        break;
                    case 7:
                        Config.BotConfiguration.ColorRoles.Teal = Role.Id;
                        break;
                    case 8:
                        Config.BotConfiguration.ColorRoles.DarkBlue = Role.Id;
                        break;
                    case 9:
                        Config.BotConfiguration.ColorRoles.DarkGreen = Role.Id;
                        break;
                    case 10:
                        Config.BotConfiguration.ColorRoles.DarkMagenta = Role.Id;
                        break;
                    case 11:
                        Config.BotConfiguration.ColorRoles.DarkOrange = Role.Id;
                        break;
                    case 12:
                        Config.BotConfiguration.ColorRoles.DarkPurple = Role.Id;
                        break;
                    case 13:
                        Config.BotConfiguration.ColorRoles.DarkRed = Role.Id;
                        break;
                    case 14:
                        Config.BotConfiguration.ColorRoles.DarkTeal = Role.Id;
                        break;
                    case 15:
                        Config.BotConfiguration.ColorRoles.LightOrange = Role.Id;
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