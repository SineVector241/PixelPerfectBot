using Discord;
using Discord.WebSocket;
using Discord.Interactions;

namespace PixelPerfectBot.Core.Interactions
{
    public class General : InteractionModuleBase<SocketInteractionContext<SocketMessageComponent>>
    {
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
