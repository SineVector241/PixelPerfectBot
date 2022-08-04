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
            var user = DB.GetUser(Context.User.Id);
            var guilduser = Context.User as SocketGuildUser;
            if (user.ClaimVIPCooldown > DateTime.UtcNow || user.SentClaimVIP || guilduser.Roles.FirstOrDefault(x => x.Id == Config.BotConfiguration.VIPRoleId) != null)
            {
                await RespondAsync("You either have already sent an application, Have the role or you are on cooldown for this application", ephemeral: true);
                return;
            }
            await RespondWithModalAsync<VIPRoleApplication>("VIPApplication");
        }

        [ComponentInteraction("Suggest")]
        public async Task SendSuggestModal()
        {
            var user = DB.GetUser(Context.User.Id);
            if (user.SuggestionCooldown > DateTime.UtcNow)
            {
                await RespondAsync($"You are on cooldown for this. Please try again <t:{DateTimeOffset.FromFileTime(user.SuggestionCooldown.ToFileTime()).ToUnixTimeSeconds()}:R>", ephemeral: true);
                return;
            }
            await RespondWithModalAsync<SuggestionApplication>("SubmitSuggestion");
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
                    if (answer.IsSuccess)
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
                .WithButton("Deny", $"DenyCCApp:{Context.User.Id}", ButtonStyle.Secondary, Emoji.Parse("❌"));
            var embed = Context.Interaction.Message.Embeds.First().ToEmbedBuilder();
            foreach (var field in embed.Fields)
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
                    await Context.Interaction.Message.ModifyAsync(x => { x.Content = $"Accepted Application: Accepted by {Context.User.Mention}"; x.Components = null; });
                    return;
                }
                await RespondAsync("Accepted User", ephemeral: true);
                await Context.Interaction.Message.ModifyAsync(x => { x.Content = $"Accepted Application: Accepted by {Context.User.Mention}"; x.Components = null; });
            }
            catch(Exception ex)
            {
                await Context.Interaction.Message.ModifyAsync(x => { x.Content = $"Application could not be accepted\nError: {ex.Message}";});
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
            catch
            {
                await RespondAsync("Could not send DM or the user does not exist. Denied Application Successfully", ephemeral: true);
                await Context.Interaction.Message.ModifyAsync(x => { x.Content = $"Denied Application: Denied by {Context.User.Mention}"; x.Components = null; });
            }
        }

        [ComponentInteraction("AcceptVIPApp:*")]
        public async Task AcceptVIPApplication(string UserId)
        {
            var UserData = DB.GetUser(Context.User.Id);
            try
            {
                var user = Context.Guild.GetUser(Convert.ToUInt64(UserId));
                await user.AddRoleAsync(Config.BotConfiguration.VIPRoleId);
                try
                {
                    UserData.SentClaimVIP = false;
                    UserData.RemoveVIPRole = DateTime.UtcNow.AddDays(31);
                    DB.UpdateUser(UserData);
                    await user.SendMessageAsync("You have successfully recieved your VIP Role for 31 days. You will need to reapply again for the VIP role if you still have VIP");
                }
                catch
                {
                    await RespondAsync("Could not send the user the DM message but accepted the application", ephemeral: true);
                    await Context.Interaction.Message.ModifyAsync(x => { x.Content = $"Accepted Application: Accepted by {Context.User.Mention}"; x.Components = null; });
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

        [ComponentInteraction("DenyVIPApp:*")]
        public async Task DenyVIPApp(string UserId)
        {
            try
            {
                var user = Context.Guild.GetUser(Convert.ToUInt64(UserId));
                var UserData = DB.GetUser(Convert.ToUInt64(UserId));
                UserData.SentClaimVIP = false;
                DB.UpdateUser(UserData);
                await Context.Interaction.Message.ModifyAsync(x => { x.Content = $"Denied Application: Denied by {Context.User.Mention}"; x.Components = null; });
                await user.SendMessageAsync($"Unfortunately your VIP application has been denied. You will be able to try again <t:{DateTimeOffset.FromFileTime(UserData.ClaimVIPCooldown.ToFileTime()).ToUnixTimeSeconds()}:R>");
                await RespondAsync("Denied Application", ephemeral: true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                await RespondAsync("Could not send DM or the user does not exist. Denied Application Successfully", ephemeral: true);
                await Context.Interaction.Message.ModifyAsync(x => { x.Content = $"Denied Application: Denied by {Context.User.Mention}"; x.Components = null; });
            }
        }

        //========================= Upvote and Downvote actions below =========================
        [ComponentInteraction("UpvoteSuggestion")]
        public async Task UpvoteSuggestion()
        {
            var suggestion = DB.GetSuggestion(Context.Interaction.Message.Id);
            if (suggestion.DownvoteUserIds.Exists(x => x == Context.User.Id))
            {
                await RespondAsync("You have downvoted on this suggestion. Press downvote button again to upvote!", ephemeral: true);
                return;
            }

            if (suggestion.UpvoteUserIds.Exists(x => x == Context.User.Id))
            {
                suggestion.UpvoteUserIds.Remove(Context.User.Id);
                suggestion.UpvotesDownvotes -= 1;
            }
            else
            {
                suggestion.UpvoteUserIds.Add(Context.User.Id);
                suggestion.UpvotesDownvotes += 1;
            }
            var embed = EmbedBuilderExtensions.ToEmbedBuilder(Context.Interaction.Message.Embeds.First());
            embed.Fields.First().WithValue(suggestion.UpvotesDownvotes.ToString());

            if (suggestion.UpvotesDownvotes == 8 && !suggestion.TopSuggestion)
            {
                await Context.Guild.GetTextChannel(Config.BotConfiguration.TopSuggestionChannel).SendMessageAsync();
                suggestion.TopSuggestion = true;
            }
            DB.UpdateSuggestion(suggestion);
            await Context.Interaction.Message.ModifyAsync(x => x.Embed = embed.Build());
            await DeferAsync();
        }

        [ComponentInteraction("DownvoteSuggestion")]
        public async Task DownvoteSuggestion()
        {
            var suggestion = DB.GetSuggestion(Context.Interaction.Message.Id);
            if (suggestion.UpvoteUserIds.Exists(x => x == Context.User.Id))
            {
                await RespondAsync("You have upvoted on this suggestion. Press upvote button again to downvote!", ephemeral: true);
                return;
            }
            if (suggestion.DownvoteUserIds.Exists(x => x == Context.User.Id))
            {
                suggestion.DownvoteUserIds.Remove(Context.User.Id);
                suggestion.UpvotesDownvotes += 1;
            }
            else
            {
                suggestion.DownvoteUserIds.Add(Context.User.Id);
                suggestion.UpvotesDownvotes -= 1;
            }
            DB.UpdateSuggestion(suggestion);
            var embed = EmbedBuilderExtensions.ToEmbedBuilder(Context.Interaction.Message.Embeds.First());
            embed.Fields.First().WithValue(suggestion.UpvotesDownvotes.ToString());
            await Context.Interaction.Message.ModifyAsync(x => x.Embed = embed.Build());
            await DeferAsync();
        }

        //========================= Poll actions below =========================

        [ComponentInteraction("Option:*")]
        public async Task UpvoteOption(string Option)
        {
            var poll = DB.GetPoll(Context.Interaction.Message.Id);
            var VotedOther = false;
            var VotedNotOther = false;
            try
            {
                if (poll.OptionUserIds.ElementAt(Convert.ToInt16(Option)).Contains(Context.User.Id))
                {
                    VotedNotOther = true;
                }
            }
            catch
            { }
            foreach (var users in poll.OptionUserIds)
            {
                if (!VotedNotOther && users.Contains(Context.User.Id))
                {
                    users.Remove(Context.User.Id);
                    VotedOther = true;
                }
            }
            if (!VotedOther && !VotedNotOther)
            {
                poll.OptionUserIds.ElementAt(Convert.ToInt16(Option)).Add(Context.User.Id);
                await RespondAsync($"Added your poll vote!", ephemeral: true);
            }
            else if (VotedNotOther)
            {
                poll.OptionUserIds.ElementAt(Convert.ToInt16(Option)).Remove(Context.User.Id);
                await RespondAsync($"Removed your poll submission", ephemeral: true);
            }
            else if (VotedOther)
            {
                poll.OptionUserIds.ElementAt(Convert.ToInt16(Option)).Add(Context.User.Id);
                await RespondAsync($"Moved your vote!", ephemeral: true);
            }
            DB.UpdatePoll(poll);
            var builder2 = new ComponentBuilder();
            var olderbuilder = ComponentBuilder.FromMessage(Context.Interaction.Message);
            var rows = olderbuilder.ActionRows;
            var counter = 0;
            for (int j = 0; j < rows.Count; j++)
            {
                foreach (var component in rows[j].Components)
                {
                    switch (component)
                    {
                        case ButtonComponent button:
                            if(Convert.ToInt16(button.Label.Replace(": ", "")) != poll.OptionUserIds.ElementAt(counter).Count)
                            {
                                builder2.WithButton(button.ToBuilder().WithLabel($": {poll.OptionUserIds.ElementAt(counter).Count}"), j);
                            }
                            else
                            {
                                builder2.WithButton(button.ToBuilder(), j);
                            }
                            break;
                    }
                    counter++;
                }
            }
            await Context.Interaction.Message.ModifyAsync(x => x.Components = builder2.Build());
        }
    }

    public class ModalApplications : InteractionModuleBase<SocketInteractionContext<SocketInteraction>>
    {
        private Database DB = new Database();

        [ModalInteraction("SendEmbed")]
        public async Task SendEmbed(SlashCommands.EmbedSendModal modal)
        {
            var embed = new EmbedBuilder()
                .WithTitle(modal.ETitle)
                .WithDescription(modal.Description)
                .WithAuthor(Context.User)
                .WithColor(Color.Green)
                .WithCurrentTimestamp();
            await Context.Channel.SendMessageAsync(embed: embed.Build());
            await RespondAsync("Successfully sent embed", ephemeral: true);
        }

        [ModalInteraction("VIPApplication")]
        public async Task SubmitVIPApplication(VIPRoleApplication modal)
        {
            var user = DB.GetUser(Context.User.Id);
            var channel = Context.Guild.GetTextChannel(Config.BotConfiguration.VIPApplicationChannel);
            var embed = new EmbedBuilder()
                .WithTitle($"New VIP Application: {Context.User.Username}")
                .AddField("Profile Link", modal.ProfileLink)
                .AddField("Email", modal.Email)
                .AddField("Proof Of Purchase", modal.ProofOfPurchase)
                .WithAuthor(Context.User)
                .WithTimestamp(DateTime.UtcNow)
                .WithColor(Color.Green);
            var builder = new ComponentBuilder()
                .WithButton("Accept", $"AcceptVIPApp:{Context.User.Id}", ButtonStyle.Success, Emoji.Parse("✅"))
                .WithButton("Deny", $"DenyVIPApp:{Context.User.Id}", ButtonStyle.Secondary, Emoji.Parse("❌"));
            await channel.SendMessageAsync(embed: embed.Build(), components: builder.Build());
            user.SentClaimVIP = true;
            user.ClaimVIPCooldown = DateTime.UtcNow.AddDays(7);
            DB.UpdateUser(user);
            await RespondAsync("Successfully sent application", ephemeral: true);
        }

        [ModalInteraction("SubmitSuggestion")]
        public async Task SubmitSuggestionModal(SuggestionApplication modal)
        {
            var user = DB.GetUser(Context.User.Id);
            var channel = Context.Guild.GetTextChannel(Config.BotConfiguration.SuggestionChannel);
            var embed = new EmbedBuilder()
                .WithTitle(modal.SuggestionTitle)
                .WithDescription(modal.Description)
                .AddField("Upvotes - Downvotes", "0")
                .WithColor(Color.LightOrange)
                .WithAuthor(Context.User)
                .WithTimestamp(DateTime.UtcNow);
            var builder = new ComponentBuilder()
                .WithButton("Upvote", "UpvoteSuggestion", ButtonStyle.Success, Emoji.Parse("👍"))
                .WithButton("Downvote", "DownvoteSuggestion", ButtonStyle.Danger, Emoji.Parse("👎"));
            var msg = await channel.SendMessageAsync($"Suggestion from {Context.User.Mention}", embed: embed.Build(), components: builder.Build());
            user.SuggestionCooldown = DateTime.UtcNow.AddMinutes(15);
            DB.UpdateUser(user);
            DB.AddSuggestion(new Database.Suggestion() { MessageId = msg.Id, UpvotesDownvotes = 0 });
            DB.TrimFirstSuggestion();
            await RespondAsync("Successfully sent suggestion", ephemeral: true);
        }
    }

    public class VIPRoleApplication : IModal
    {
        public string Title => "VIP Role Application";

        [InputLabel("Pixels Perfect Profile")]
        [ModalTextInput("PPLink", placeholder: "Link to Pixels Perfect Profile")]
        public string ProfileLink { get; set; }

        [InputLabel("Email")]
        [ModalTextInput("Email")]
        public string Email { get; set; }

        [InputLabel("Proof Of Purchase(Image)")]
        [ModalTextInput("IMG", placeholder: "LINK ONLY!")]
        public string ProofOfPurchase { get; set; }
    }

    public class SuggestionApplication : IModal
    {
        public string Title => "Suggestion";

        [InputLabel("Title of suggestion")]
        [ModalTextInput("Title", maxLength: 125)]
        public string SuggestionTitle { get; set; }

        [InputLabel("Description of suggestion")]
        [ModalTextInput("Description", TextInputStyle.Paragraph)]
        public string Description { get; set; }
    }
}
