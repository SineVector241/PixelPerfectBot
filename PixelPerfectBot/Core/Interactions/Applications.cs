using Discord;
using Discord.WebSocket;
using Discord.Interactions;
using Fergun.Interactive;

namespace PixelPerfectBot.Core.Interactions
{
    public class Applications : InteractionModuleBase<SocketInteractionContext<SocketMessageComponent>>
    {
        public InteractiveService Interactive { get; set; }
        private Database DB = new Database();

        [ComponentInteraction("VIP")]
        public async Task ClaimVIP()
        {
            await RespondAsync("This is still currently under development. Try again later in a couple of days.", ephemeral: true);
            return;

            var user = DB.GetUser(Context.User.Id);
            if (user.ClaimVIPCooldown > DateTime.UtcNow || user.SentClaimVIP)
            {
                await RespondAsync("You either have already sent an application or you are on cooldown for this application", ephemeral: true);
                return;
            }
            var channel = Context.Guild.GetTextChannel(Config.BotConfiguration.ApplicationChannel);
            var embed = new EmbedBuilder()
                .WithTitle("Claim VIP Application")
                .AddField("User", Context.User.Mention)
                .WithColor(Color.Gold)
                .WithCurrentTimestamp();
            var builder = new ComponentBuilder()
                .WithButton("Accept", $"AcceptVIP:{Context.User.Id}", ButtonStyle.Success)
                .WithButton("Deny", $"DenyVIP:{Context.User.Id}", ButtonStyle.Danger);
            await RespondAsync("Sent claim", ephemeral: true);
            await channel.SendMessageAsync(embed: embed.Build(), components: builder.Build());
        }

        [ComponentInteraction("ContentCreator")]
        public async Task ContentCreatorApplication()
        {
            try
            {
                var user = DB.GetUser(Context.User.Id);
                if (user.ContentCreatorCooldown > DateTime.UtcNow || user.SentContentCreator)
                {
                    await RespondAsync("You either have already sent an application or you are on cooldown for this application", ephemeral: true);
                    return;
                }
                var embed = new EmbedBuilder()
                    .WithTitle("Content Creator Application")
                    .WithColor(Color.DarkOrange)
                    .AddField("Q1: What kind of content do you develop for MCBE? (i.e. maps, addons, etc)", "Not Answered")
                    .AddField("Q2: Why would you like to become a content creator at Pixel Perfect?", "Not Answered")
                    .AddField("Q3: Which of your projects are you most proud of? Why?", "Not Answered")
                    .AddField("Q4: How often would you post on our website? (i.e. weekly, fortnightly, monthly)", "Not Answered")
                    .AddField("Q5: Please attach 3 or more pictures showcasing your content. LINKS ONLY!", "Not Answered")
                    .AddField("Q6: Please send through any relevant links in regards to your content. (i.e. MCPEDL, github, YouTube videos)", "Not Answered")
                    .AddField("Q7: Anything else that you’d like to add?", "Not Answered")
                    .WithAuthor(Context.User);
                var selectMenu = new SelectMenuBuilder()
                    .WithCustomId("AnswerCCQuestion")
                    .WithPlaceholder("Answer a Question")
                    .AddOption("Question 1", "Q1", "What kind of content do you develop for MCBE? (i.e. maps, addons, etc)")
                    .AddOption("Question 2", "Q2", "Why would you like to become a content creator at Pixel Perfect?")
                    .AddOption("Question 3", "Q3", "Which of your projects are you most proud of? Why?")
                    .AddOption("Question 4", "Q4", "How often would you post on our website? (i.e. weekly, fortnightly, monthly)")
                    .AddOption("Question 5", "Q5", "Please attach 3 or more pictures showcasing your content. LINKS ONLY!")
                    .AddOption("Question 6", "Q6", "Please send through any relevant links in regards to your content.")
                    .AddOption("Question 7", "Q7", "Anything else that you’d like to add?");
                var builder = new ComponentBuilder()
                    .WithSelectMenu(selectMenu)
                    .WithButton("Submit", $"SubmitCCApplication:{Context.Guild.Id}", ButtonStyle.Success, new Emoji("✅"), row: 1);

                var msg = await Context.User.SendMessageAsync(embed: embed.Build(), components: builder.Build());
                await RespondAsync($"Sent Application. Please check your DM's from Pixel Perfect Bot\nOr Click Here: {msg.GetJumpUrl()}", ephemeral: true);
            }
            catch (Exception ex)
            {
                if (ex.Message == "The server responded with error 50007: Cannot send messages to this user")
                {
                    await RespondAsync("Could not send application. Please allow Pixel Perfect Bot to DM you in your discord settings.", ephemeral: true);
                }
                else
                {
                    Console.WriteLine(ex);
                }
            }
        }

        [ComponentInteraction("AnswerCCQuestion")]
        public async Task AnswerContentCreateQuestion(string[] Q)
        {
            await DeferAsync();
            var embed = Context.Interaction.Message.Embeds.First().ToEmbedBuilder();
            foreach (var field in embed.Fields)
            {
                if (field.Name.StartsWith(Q.First().ToString()))
                {
                    var msg = await FollowupAsync($"{field.Name}\n**Answer Below This Message**");
                    var answer = await Interactive.NextMessageAsync(x => x.Author.Id == Context.User.Id && x.Channel.Id == Context.Channel.Id, timeout: TimeSpan.FromMinutes(5));
                    if(answer.IsSuccess)
                    {
                        field.Value = answer.Value;
                        await ModifyOriginalResponseAsync(x => x.Embed = embed.Build());
                        await msg.DeleteAsync();
                    }
                    else
                    {
                        await msg.DeleteAsync();
                    }
                }
            }
        }

        [ComponentInteraction("SubmitCCApplication:*")]
        public async Task SubmitCCApp(string GuildId)
        {
            var user = DB.GetUser(Context.User.Id);
            if (user.ContentCreatorCooldown > DateTime.UtcNow || user.SentContentCreator)
            {
                await RespondAsync("You either have already sent an application or you are on cooldown for this application", ephemeral: true);
                return;
            }
            var builder = new ComponentBuilder()
                .WithButton("Accept", $"AcceptCCApp:{Context.User.Id}", ButtonStyle.Success, Emoji.Parse("✅"))
                .WithButton("Deny", $"DenyCCApp:{Context.User.Id}", ButtonStyle.Success, Emoji.Parse("❌"));
            var embed = Context.Interaction.Message.Embeds.First().ToEmbedBuilder();
            foreach(var field in embed.Fields)
            {
                Console.WriteLine(field.Value);
                if (field.Value.ToString() == "Not Answered" && !field.Name.StartsWith("Q7"))
                {
                    await RespondAsync("Cannot submit application. You still have unanswered questions!\nTip: You do not need to answer Question 7", ephemeral: true);
                    return;
                }
            }
            await DeferAsync();
            embed.WithTitle($"Content Creator Application: {Context.User.Username}");
            await Context.Client.GetGuild(Convert.ToUInt64(GuildId)).GetTextChannel(Config.BotConfiguration.ApplicationChannel).SendMessageAsync(embed: embed.Build(), components: builder.Build());
            await FollowupAsync("Submitted Application");
            await DeleteOriginalResponseAsync();
            user.ContentCreatorCooldown = DateTime.UtcNow.AddDays(7);
            user.SentContentCreator = true;
            DB.UpdateUser(user);
        }

        //========================= Accept and deny actions below =========================

        [ComponentInteraction("AcceptCCApp:*")]
        public async Task AcceptCCApp(string UserId)
        {
            try
            {
                var user = Context.Guild.GetUser(Convert.ToUInt64(UserId));
                await user.AddRoleAsync(Config.BotConfiguration.ContentCreatorRoleId);
                try
                {
                    await user.SendMessageAsync("**Your application has been successful!** 💜\n\nWelcome to the community as a Pixel Perfect Content Creator! Please refer the the links below for further information. 😄\nPost Guidelines\nhttps://pixelsperfect.net/post-guidelines\nAbout Us\nhttps://pixelsperfect.net/about-us\nPrivate Discord\nhttps://discord.gg/cHc7DsEgHh\n\nPlease note: as a content creator you are required to join our private discord server and abide by our post guidelines. — Thank you! <:PixelPlay:998441350691307520>");
                }
                catch
                {
                    await RespondAsync("Could not send the user the DM message but accepted the application", ephemeral: true);
                    return;
                }
                await RespondAsync("Accepted User", ephemeral: true);
                await Context.Interaction.Message.ModifyAsync(x => { x.Content = $"Accepted Application: Accepted by {Context.User.Mention}"; x.Components = null; });
            }
            catch
            {
                await Context.Interaction.Message.ModifyAsync(x => { x.Content = "Application could not be accepted"; x.Components = null; });
                await RespondAsync("Could not accept user as this user may not exist in the server.", ephemeral: true);
            }
        }

        [ComponentInteraction("DenyCCApp:*")]
        public async Task DenyCCApp(string UserId)
        {
            try
            {
                var user = Context.Guild.GetUser(Convert.ToUInt64(UserId));
                var UserData = DB.GetUser(Convert.ToUInt64(UserId));
                UserData.SentContentCreator = false;
                DB.UpdateUser(UserData);
                await Context.Interaction.Message.ModifyAsync(x => { x.Content = $"Denied Application: Denied by {Context.User.Mention}"; x.Components = null; });
                await user.SendMessageAsync($"Unfortunately your Content Creator application has been denied. You will be able to try again <t:{DateTimeOffset.FromFileTime(UserData.ContentCreatorCooldown.ToFileTime()).ToUnixTimeSeconds()}:R>");
                await RespondAsync("Denied Application", ephemeral: true);
            }
            catch (Exception ex)
            {
                await RespondAsync("Could not send DM or the user does not exist. Denied Application Successfully", ephemeral: true);
            }
        }
    }
}
