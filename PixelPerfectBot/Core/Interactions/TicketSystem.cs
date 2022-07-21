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
            if (user.GeneralSupportCooldown > DateTime.Now)
            {
                await RespondAsync("You are on cooldown for this ticket. Please try again in 24 hours", ephemeral: true);
                return;
            }
            if (!Database.DBData.Tickets.Exists(x => x.UserId == Context.User.Id && x.Type == 0))
            {
                await DeferAsync();
                var channel = await Context.Guild.CreateTextChannelAsync(Context.User.Username, x => { x.CategoryId = Config.BotConfiguration.GeneralSupportCategory; });
                await channel.AddPermissionOverwriteAsync(Context.User, new OverwritePermissions().Modify(viewChannel: PermValue.Allow, sendMessages: PermValue.Allow));
                DB.CreateTicket(0, Context.User.Id);
                await channel.SendMessageAsync(Context.User.Mention, components: new ComponentBuilder().WithButton("Close Ticket", $"CloseTicket:{Context.User.Id},{0}", ButtonStyle.Danger).Build());
                user.GeneralSupportCooldown = DateTime.Now.AddDays(1);
                DB.UpdateUser(user);
            }
            else
                await RespondAsync("Could not create ticket. You already have a duplicate ticket open.", ephemeral: true);
        }

        [ComponentInteraction("ContentSupport")]
        public async Task CreateContentSupportTicket()
        {
            var user = DB.GetUser(Context.User.Id);
            if (user.ContentSupportCooldown > DateTime.Now)
            {
                await RespondAsync("You are on cooldown for this ticket. Please try again in 24 hours", ephemeral: true);
                return;
            }
            if (!Database.DBData.Tickets.Exists(x => x.UserId == Context.User.Id && x.Type == 1))
            {
                await DeferAsync();
                var channel = await Context.Guild.CreateTextChannelAsync(Context.User.Username, x => { x.CategoryId = Config.BotConfiguration.ContentSupportCategory; });
                await channel.AddPermissionOverwriteAsync(Context.User, new OverwritePermissions().Modify(viewChannel: PermValue.Allow, sendMessages: PermValue.Allow));
                DB.CreateTicket(1, Context.User.Id);
                await channel.SendMessageAsync(Context.User.Mention, components: new ComponentBuilder().WithButton("Close Ticket", $"CloseTicket:{Context.User.Id},{1}", ButtonStyle.Danger).Build());
                user.ContentSupportCooldown = DateTime.Now.AddDays(1);
                DB.UpdateUser(user);
            }
            else
                await RespondAsync("Could not create ticket. You already have a duplicate ticket open.", ephemeral: true);
        }

        [ComponentInteraction("ServerSupport")]
        public async Task CreateServerSupportTicket()
        {
            var user = DB.GetUser(Context.User.Id);
            if (user.ServerSupportCooldown > DateTime.Now)
            {
                await RespondAsync("You are on cooldown for this ticket. Please try again in 24 hours", ephemeral: true);
                return;
            }
            if (!Database.DBData.Tickets.Exists(x => x.UserId == Context.User.Id && x.Type == 2))
            {
                await DeferAsync();
                var channel = await Context.Guild.CreateTextChannelAsync(Context.User.Username, x => { x.CategoryId = Config.BotConfiguration.MCBEServerSupportCategory; });
                await channel.AddPermissionOverwriteAsync(Context.User, new OverwritePermissions().Modify(viewChannel: PermValue.Allow, sendMessages: PermValue.Allow));
                DB.CreateTicket(2, Context.User.Id);
                await channel.SendMessageAsync(Context.User.Mention, components: new ComponentBuilder().WithButton("Close Ticket", $"CloseTicket:{Context.User.Id},{2}", ButtonStyle.Danger).Build());
                user.ServerSupportCooldown = DateTime.Now.AddDays(1);
                DB.UpdateUser(user);
            }
            else
                await RespondAsync("Could not create ticket. You already have a duplicate ticket open.", ephemeral: true);
        }

        [ComponentInteraction("CloseTicket:*,*")]
        public async Task CloseTicket(string UserId, string TicketType)
        {
            var user = Context.User as SocketGuildUser;
            if(UserId == Context.User.Id.ToString() || user.GuildPermissions.Administrator)
            {
                await DeferAsync();
                var channel = Context.Channel as SocketTextChannel;
                if (channel != null)
                {
                    await channel.DeleteAsync();
                    DB.DeleteTicket(Convert.ToInt16(TicketType), Convert.ToUInt64(UserId));
                }
            }
        }
    }
}
