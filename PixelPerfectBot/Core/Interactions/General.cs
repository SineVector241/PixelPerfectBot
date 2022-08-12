using Discord;
using Discord.WebSocket;
using Discord.Interactions;
using Fergun.Interactive;

namespace PixelPerfectBot.Core.Interactions
{
    public class General : InteractionModuleBase<SocketInteractionContext<SocketMessageComponent>>
    {
        public InteractiveService Interactive { get; set; }

        [ComponentInteraction("SetRRTitle:*")]
        public async Task SetRRTitle(string UserId)
        {
            if(Context.User.Id.ToString() != UserId)
            {
                await RespondAsync("This is not your dashboard", ephemeral: true);
                return;
            }
            var embed = EmbedBuilderExtensions.ToEmbedBuilder(Context.Interaction.Message.Embeds.First());
            await DeferAsync();
            var prompt = await FollowupAsync("Send a message below this message to set the title");
            var msg = await Interactive.NextMessageAsync(x => x.Author.Id == Context.User.Id && x.Channel.Id == Context.Channel.Id, timeout: TimeSpan.FromMinutes(5));
            if (msg.IsSuccess)
            {
                embed.WithTitle(msg.Value.Content);
                await ModifyOriginalResponseAsync(x => x.Embed = embed.Build());
                await msg.Value.DeleteAsync();
                await prompt.DeleteAsync();
            }
            else
            {
                await prompt.DeleteAsync();
            }
        }

        [ComponentInteraction("SetRRDescription:*")]
        public async Task SetRRDescription(string UserId)
        {
            if (Context.User.Id.ToString() != UserId)
            {
                await RespondAsync("This is not your dashboard", ephemeral: true);
                return;
            }
            var embed = EmbedBuilderExtensions.ToEmbedBuilder(Context.Interaction.Message.Embeds.First());
            await DeferAsync();
            var prompt = await FollowupAsync("Send a message below this message to set the description");
            var msg = await Interactive.NextMessageAsync(x => x.Author.Id == Context.User.Id && x.Channel.Id == Context.Channel.Id, timeout: TimeSpan.FromMinutes(5));
            if (msg.IsSuccess)
            {
                embed.WithDescription(msg.Value.Content);
                await ModifyOriginalResponseAsync(x => x.Embed = embed.Build());
                await msg.Value.DeleteAsync();
                await prompt.DeleteAsync();
            }
            else
            {
                await prompt.DeleteAsync();
            }
        }

        [ComponentInteraction("AddRRRole:*")]
        public async Task AddRRRole(string UserId)
        {
            if (Context.User.Id.ToString() != UserId)
            {
                await RespondAsync("This is not your dashboard", ephemeral: true);
                return;
            }
            var embed = EmbedBuilderExtensions.ToEmbedBuilder(Context.Interaction.Message.Embeds.First());
            var channel = (SocketTextChannel)Context.Interaction.Channel;
            await DeferAsync();
            var messages = new List<IMessage>();
            var buttonname = await FollowupAsync("Name of button");
            messages.Add(buttonname);

            var name = await Interactive.NextMessageAsync(x => x.Author.Id == Context.User.Id && x.Channel.Id == Context.Channel.Id, timeout: TimeSpan.FromMinutes(5));
            if (name.IsSuccess && !embed.Fields.Exists(x => x.Name == name.Value.Content))
            {
                messages.Add(name.Value);

                var rolemsg = await ReplyAsync("Role to set to");
                messages.Add(rolemsg);

                var role = await Interactive.NextMessageAsync(x => x.Author.Id == Context.User.Id && x.Channel.Id == Context.Channel.Id, timeout: TimeSpan.FromMinutes(5));
                try
                {
                    if (role.IsSuccess && Context.Guild.GetRole(Convert.ToUInt64(role.Value.Content.Replace("<@&", "").Replace(">", ""))) != null)
                    {
                        messages.Add(role.Value);
                        embed.AddField(name.Value.Content, $"<@&{role.Value.Content}>");
                        await Context.Interaction.Message.ModifyAsync(x => x.Embed = embed.Build());
                        await channel.DeleteMessagesAsync(messages);
                    }
                    else
                        await channel.DeleteMessagesAsync(messages);
                }
                catch {
                    if(role.IsSuccess) messages.Add(role.Value);
                    await channel.DeleteMessagesAsync(messages);
                }
            }
            else
            {
                await buttonname.DeleteAsync();
                if(name.IsSuccess) await name.Value.DeleteAsync();
                await ReplyAsync("Error. You have either been timed out or that name exists in the reaction role list");
            }
        }

        [ComponentInteraction("RemoveRRRole:*")]
        public async Task RemoveRRRole(string UserId)
        {
            if (Context.User.Id.ToString() != UserId)
            {
                await RespondAsync("This is not your dashboard", ephemeral: true);
                return;
            }
            var embed = EmbedBuilderExtensions.ToEmbedBuilder(Context.Interaction.Message.Embeds.First());
            var channel = (SocketTextChannel)Context.Interaction.Channel;
            await DeferAsync();
            var messages = new List<IMessage>();
            var buttonname = await FollowupAsync("Name of button to remove");
            messages.Add(buttonname);

            var name = await Interactive.NextMessageAsync(x => x.Author.Id == Context.User.Id && x.Channel.Id == Context.Channel.Id, timeout: TimeSpan.FromMinutes(5));
            if (name.IsSuccess && embed.Fields.Exists(x => x.Name == name.Value.Content))
            {
                messages.Add(name.Value);
                embed.Fields.RemoveAll(x => x.Name == name.Value.Content);
                await Context.Interaction.Message.ModifyAsync(x => x.Embed = embed.Build());
                await channel.DeleteMessagesAsync(messages);
            }
            else
            {
                if (name.IsSuccess) messages.Add(name.Value);
                await ReplyAsync("Could not remove. Either you have been timed out or that button name does not exist");
                await channel.DeleteMessagesAsync(messages);
            }
        }

        [ComponentInteraction("PublishRR:*,*")]
        public async Task PublishRR(string UserId, string ChannelId)
        {
            var setupembed = Context.Interaction.Message.Embeds.First();
            if (Context.User.Id.ToString() != UserId)
            {
                await RespondAsync("This is not your dashboard", ephemeral: true);
                return;
            }
            await DeferAsync();
            var channel = Context.Guild.GetTextChannel(Convert.ToUInt64(ChannelId));
            var embed = new EmbedBuilder()
                .WithTitle(setupembed.Title)
                .WithDescription(setupembed.Description)
                .WithColor(Color.Blue);

            var builder = new ComponentBuilder();

            foreach(var field in setupembed.Fields)
            {
                var role = Context.Guild.GetRole(Convert.ToUInt64(field.Value.Replace("<@&", "").Replace(">", "")));
                if (role != null)
                {
                    builder.WithButton(field.Name, $"RR:{role.Id}");
                }
            }

            await channel.SendMessageAsync(embed: embed.Build(), components: builder.Build());
            await FollowupAsync("Sent Reaction Roles", ephemeral: true);
        }

        [ComponentInteraction("RR:*")]
        public async Task RR(string RoleId)
        {
            var role = Context.Guild.GetRole(Convert.ToUInt64(RoleId));
            var user = Context.User as SocketGuildUser;
            if(user.Roles.Contains(role))
            {
                await user.RemoveRoleAsync(role);
                await RespondAsync("Successfully removed role!", ephemeral: true);
            }
            else
            {
                await user.AddRoleAsync(role);
                await RespondAsync("Successfully added role!", ephemeral: true);
            }
        }

            //Color Roles
        [ComponentInteraction("SelectColorRole:*")]
        public async Task SelectColorRole(string Selection)
        {
            var user = Context.User as SocketGuildUser;
            if(user.Roles.FirstOrDefault(x => x.Id == Config.BotConfiguration.VIPRoleId) == null && user.Roles.FirstOrDefault(x => x.Id == Config.BotConfiguration.BoosterRoleId) == null)
            {
                await RespondAsync("You must be a VIP or a server booster to get a custom color!", ephemeral: true);
                return;
            }
            switch(Convert.ToInt16(Selection))
            {
                case 0:
                    if (user.Roles.FirstOrDefault(x => x.Id == Config.BotConfiguration.ColorRoles.Blue) == null)
                        await user.AddRoleAsync(Config.BotConfiguration.ColorRoles.Blue);
                    else
                        await user.RemoveRoleAsync(Config.BotConfiguration.ColorRoles.Blue);
                    break;
                case 1:
                    if (user.Roles.FirstOrDefault(x => x.Id == Config.BotConfiguration.ColorRoles.Gold) == null)
                        await user.AddRoleAsync(Config.BotConfiguration.ColorRoles.Gold);
                    else
                        await user.RemoveRoleAsync(Config.BotConfiguration.ColorRoles.Gold);
                    break;
                case 2:
                    if (user.Roles.FirstOrDefault(x => x.Id == Config.BotConfiguration.ColorRoles.Green) == null)
                        await user.AddRoleAsync(Config.BotConfiguration.ColorRoles.Green);
                    else
                        await user.RemoveRoleAsync(Config.BotConfiguration.ColorRoles.Green);
                    break;
                case 3:
                    if (user.Roles.FirstOrDefault(x => x.Id == Config.BotConfiguration.ColorRoles.Magenta) == null)
                        await user.AddRoleAsync(Config.BotConfiguration.ColorRoles.Magenta);
                    else
                        await user.RemoveRoleAsync(Config.BotConfiguration.ColorRoles.Magenta);
                    break;
                case 4:
                    if (user.Roles.FirstOrDefault(x => x.Id == Config.BotConfiguration.ColorRoles.Orange) == null)
                        await user.AddRoleAsync(Config.BotConfiguration.ColorRoles.Orange);
                    else
                        await user.RemoveRoleAsync(Config.BotConfiguration.ColorRoles.Orange);
                    break;
                case 5:
                    if (user.Roles.FirstOrDefault(x => x.Id == Config.BotConfiguration.ColorRoles.Purple) == null)
                        await user.AddRoleAsync(Config.BotConfiguration.ColorRoles.Purple);
                    else
                        await user.RemoveRoleAsync(Config.BotConfiguration.ColorRoles.Purple);
                    break;
                case 6:
                    if (user.Roles.FirstOrDefault(x => x.Id == Config.BotConfiguration.ColorRoles.Red) == null)
                        await user.AddRoleAsync(Config.BotConfiguration.ColorRoles.Red);
                    else
                        await user.RemoveRoleAsync(Config.BotConfiguration.ColorRoles.Red);
                    break;
                case 7:
                    if (user.Roles.FirstOrDefault(x => x.Id == Config.BotConfiguration.ColorRoles.Teal) == null)
                        await user.AddRoleAsync(Config.BotConfiguration.ColorRoles.Teal);
                    else
                        await user.RemoveRoleAsync(Config.BotConfiguration.ColorRoles.Teal);
                    break;
                case 8:
                    if (user.Roles.FirstOrDefault(x => x.Id == Config.BotConfiguration.ColorRoles.DarkBlue) == null)
                        await user.AddRoleAsync(Config.BotConfiguration.ColorRoles.DarkBlue);
                    else
                        await user.RemoveRoleAsync(Config.BotConfiguration.ColorRoles.DarkBlue);
                    break;
                case 9:
                    if (user.Roles.FirstOrDefault(x => x.Id == Config.BotConfiguration.ColorRoles.DarkGreen) == null)
                        await user.AddRoleAsync(Config.BotConfiguration.ColorRoles.DarkGreen);
                    else
                        await user.RemoveRoleAsync(Config.BotConfiguration.ColorRoles.DarkGreen);
                    break;
                case 10:
                    if (user.Roles.FirstOrDefault(x => x.Id == Config.BotConfiguration.ColorRoles.DarkMagenta) == null)
                        await user.AddRoleAsync(Config.BotConfiguration.ColorRoles.DarkMagenta);
                    else
                        await user.RemoveRoleAsync(Config.BotConfiguration.ColorRoles.DarkMagenta);
                    break;
                case 11:
                    if (user.Roles.FirstOrDefault(x => x.Id == Config.BotConfiguration.ColorRoles.DarkOrange) == null)
                        await user.AddRoleAsync(Config.BotConfiguration.ColorRoles.DarkOrange);
                    else
                        await user.RemoveRoleAsync(Config.BotConfiguration.ColorRoles.DarkOrange);
                    break;
                case 12:
                    if (user.Roles.FirstOrDefault(x => x.Id == Config.BotConfiguration.ColorRoles.DarkPurple) == null)
                        await user.AddRoleAsync(Config.BotConfiguration.ColorRoles.DarkPurple);
                    else
                        await user.RemoveRoleAsync(Config.BotConfiguration.ColorRoles.DarkPurple);
                    break;
                case 13:
                    if (user.Roles.FirstOrDefault(x => x.Id == Config.BotConfiguration.ColorRoles.DarkRed) == null)
                        await user.AddRoleAsync(Config.BotConfiguration.ColorRoles.DarkRed);
                    else
                        await user.RemoveRoleAsync(Config.BotConfiguration.ColorRoles.DarkRed);
                    break;
                case 14:
                    if (user.Roles.FirstOrDefault(x => x.Id == Config.BotConfiguration.ColorRoles.DarkTeal) == null)
                        await user.AddRoleAsync(Config.BotConfiguration.ColorRoles.DarkTeal);
                    else
                        await user.RemoveRoleAsync(Config.BotConfiguration.ColorRoles.DarkTeal);
                    break;
                case 15:
                    if (user.Roles.FirstOrDefault(x => x.Id == Config.BotConfiguration.ColorRoles.LightOrange) == null)
                        await user.AddRoleAsync(Config.BotConfiguration.ColorRoles.LightOrange);
                    else
                        await user.RemoveRoleAsync(Config.BotConfiguration.ColorRoles.LightOrange);
                    break;
            }
            await DeferAsync();

        }
    }
}
