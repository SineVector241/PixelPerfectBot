using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace PixelPerfectBot.Core.Interactions
{
    public class TicketSystem : InteractionModuleBase<SocketInteractionContext<SocketMessageComponent>>
    {
        private Database DB = new Database();

        [ComponentInteraction("GeneralSupport")]
        public async Task CreateGeneralSupportTicket()
        {
            var user = DB.GetUser(Context.User.Id);
            if (user.GeneralSupportCooldown > DateTime.UtcNow)
            {
                await RespondAsync($"You are on cooldown for this ticket. Please try again <t:{DateTimeOffset.FromFileTime(user.GeneralSupportCooldown.ToFileTime()).ToUnixTimeSeconds()}:R>", ephemeral: true);
                return;
            }
            if (!Database.DBData.Tickets.Exists(x => x.UserId == Context.User.Id && x.Type == 0))
            {
                await DeferAsync();
                var channel = await Context.Guild.CreateTextChannelAsync(Context.User.Username, x => { x.CategoryId = Config.BotConfiguration.GeneralSupportCategory; });
                await channel.AddPermissionOverwriteAsync(Context.User, new OverwritePermissions().Modify(viewChannel: PermValue.Allow, sendMessages: PermValue.Allow));
                DB.CreateTicket(0, Context.User.Id);
                await channel.SendMessageAsync($"{Context.User.Mention}, <@&{Config.BotConfiguration.ChatModRoleId}>", embed: new EmbedBuilder().WithTitle("General Support Ticket").WithDescription("Please specify what your problem/question is and we will respond back to you shortly.").WithColor(Color.LightOrange).Build(), components: new ComponentBuilder().WithButton("Close Ticket", $"CloseTicket:{Context.User.Id},{0}", ButtonStyle.Danger).Build());
                user.GeneralSupportCooldown = DateTime.UtcNow.AddDays(1);
                DB.UpdateUser(user);
                await new Utils().DiscordLog(Context.Guild, "Ticket Created", $"Ticket created by {Context.User.Mention}\nType: General Support", Color.Green);
            }
            else
                await RespondAsync("Could not create ticket. You already have a duplicate ticket open.", ephemeral: true);
        }

        [ComponentInteraction("ContentSupport")]
        public async Task CreateContentSupportTicket()
        {
            var user = DB.GetUser(Context.User.Id);
            if (user.ContentSupportCooldown > DateTime.UtcNow)
            {
                await RespondAsync($"You are on cooldown for this ticket. Please try again <t:{DateTimeOffset.FromFileTime(user.ContentSupportCooldown.ToFileTime()).ToUnixTimeSeconds()}:R>", ephemeral: true);
                return;
            }
            if (!Database.DBData.Tickets.Exists(x => x.UserId == Context.User.Id && x.Type == 1))
            {
                await DeferAsync();
                var channel = await Context.Guild.CreateTextChannelAsync(Context.User.Username, x => { x.CategoryId = Config.BotConfiguration.ContentSupportCategory; });
                await channel.AddPermissionOverwriteAsync(Context.User, new OverwritePermissions().Modify(viewChannel: PermValue.Allow, sendMessages: PermValue.Allow));
                DB.CreateTicket(1, Context.User.Id);
                await channel.SendMessageAsync($"{Context.User.Mention}, <@&{Config.BotConfiguration.ContentCreatorRoleId}>", embed: new EmbedBuilder().WithTitle("Content Support Ticket").WithDescription("Please specify what your problem/question is and we will respond back to you shortly.").WithColor(Color.LightOrange).Build(), components: new ComponentBuilder().WithButton("Close Ticket", $"CloseTicket:{Context.User.Id},{1}", ButtonStyle.Danger).Build());
                user.ContentSupportCooldown = DateTime.UtcNow.AddDays(1);
                DB.UpdateUser(user);
                await new Utils().DiscordLog(Context.Guild, "Ticket Created", $"Ticket created by {Context.User.Mention}\nType: Content Support", Color.Green);
            }
            else
                await RespondAsync("Could not create ticket. You already have a duplicate ticket open.", ephemeral: true);
        }

        [ComponentInteraction("ServerSupport")]
        public async Task CreateServerSupportTicket()
        {
            var user = DB.GetUser(Context.User.Id);
            if (user.ServerSupportCooldown > DateTime.UtcNow)
            {
                await RespondAsync($"You are on cooldown for this ticket. Please try again <t:{DateTimeOffset.FromFileTime(user.ServerSupportCooldown.ToFileTime()).ToUnixTimeSeconds()}:R>", ephemeral: true);
                return;
            }
            if (!Database.DBData.Tickets.Exists(x => x.UserId == Context.User.Id && x.Type == 2))
            {
                await DeferAsync();
                var channel = await Context.Guild.CreateTextChannelAsync(Context.User.Username, x => { x.CategoryId = Config.BotConfiguration.MCBEServerSupportCategory; });
                await channel.AddPermissionOverwriteAsync(Context.User, new OverwritePermissions().Modify(viewChannel: PermValue.Allow, sendMessages: PermValue.Allow));
                DB.CreateTicket(2, Context.User.Id);
                await channel.SendMessageAsync($"{Context.User.Mention}, <@&{Config.BotConfiguration.ChatModRoleId}>", embed: new EmbedBuilder().WithTitle("Server Support Ticket").WithDescription("Please specify what your problem/question is and we will respond back to you shortly.").WithColor(Color.LightOrange).Build(), components: new ComponentBuilder().WithButton("Close Ticket", $"CloseTicket:{Context.User.Id},{2}", ButtonStyle.Danger).Build());
                user.ServerSupportCooldown = DateTime.UtcNow.AddDays(1);
                DB.UpdateUser(user);
                await new Utils().DiscordLog(Context.Guild, "Ticket Created", $"Ticket created by {Context.User.Mention}\nType: Server Support", Color.Green);
            }
            else
                await RespondAsync("Could not create ticket. You already have a duplicate ticket open.", ephemeral: true);
        }

        [ComponentInteraction("CloseTicket:*,*")]
        public async Task CloseTicket(string UserId, string TicketType)
        {
            var user = Context.User as SocketGuildUser;
            string ticketType = "";
            switch(TicketType)
            {
                case "0": ticketType = "General Support"; break;
                case "1": ticketType = "Content Support"; break;
                case "2":  ticketType = "Server Support"; break;
            }
            if(UserId == Context.User.Id.ToString() || user.GuildPermissions.Administrator || user.Roles.FirstOrDefault(x => x.Id == Config.BotConfiguration.TicketManagerRoleId) != null)
            {
                await DeferAsync();
                var channel = Context.Channel as SocketTextChannel;
                if (channel != null)
                {
                    await channel.DeleteAsync();
                    DB.DeleteTicket(Convert.ToInt16(TicketType), Convert.ToUInt64(UserId));
                    await new Utils().DiscordLog(Context.Guild, "Ticket Deleted", $"Ticket deleted by {Context.User.Mention}\nType: {ticketType}", Color.DarkRed);
                }
            }
            else
            {
                await RespondAsync("Cannot close ticket. You do not have the permissions to close this ticket.", ephemeral: true);
            }
        }
    }
}
