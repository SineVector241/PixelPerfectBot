using Discord;
using Discord.WebSocket;
using Discord.Interactions;
using Microsoft.Extensions.DependencyInjection;

namespace PixelPerfectBot
{
    public class EventManager
    {
        private DiscordSocketClient BotClient;
        private readonly InteractionService IService;
        private readonly IServiceProvider SProvider;

        public EventManager(IServiceProvider Services)
        {
            SProvider = Services;
            IService = Services.GetRequiredService<InteractionService>();
            BotClient = Services.GetRequiredService<DiscordSocketClient>();
        }

        public Task Initialize()
        {
            BotClient.Ready += Ready;
            BotClient.InteractionCreated += InteractionCreated;
            return Task.CompletedTask;
        }

        private async Task InteractionCreated(SocketInteraction interaction)
        {
            try
            {
                var DB = new Core.Database();
                DB.CreateUserIfNotExists(interaction.User.Id);
                if (interaction is SocketMessageComponent)
                {
                    var ctx = new SocketInteractionContext<SocketMessageComponent>(BotClient, (SocketMessageComponent)interaction);
                    await IService.ExecuteCommandAsync(ctx, SProvider);
                }
                else
                {
                    var ctx = new SocketInteractionContext<SocketInteraction>(BotClient, interaction);
                    await IService.ExecuteCommandAsync(ctx, SProvider);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private async Task Ready()
        {
            try
            {
                Console.WriteLine($"\u001b[97m[{DateTime.Now}]: [\u001b[92mREADY\u001b[97m] => {BotClient.CurrentUser.Username} is ready!");
#if DEBUG
                await BotClient.SetGameAsync("In Development Phase");
#else
                await BotClient.SetGameAsync("Helping Pixel Perfect Members");
#endif
                await BotClient.SetStatusAsync(UserStatus.Online);
                await IService.RegisterCommandsToGuildAsync(669289013328216074);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\u001b[97m[{DateTime.Now}]: [\u001b[31mERROR\u001b[97m] => An error occured in EventManager.cs \nError Info:\n{ex}");
            }
        }
    }
}
