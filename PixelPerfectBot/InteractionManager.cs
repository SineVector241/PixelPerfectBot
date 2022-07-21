using Discord;
using Discord.WebSocket;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Discord.Interactions;

namespace PixelPerfectBot
{
    public class InteractionManager
    {
        private readonly IServiceProvider SProvider;
        private readonly InteractionService IService;
        private readonly DiscordSocketClient BotClient;

        public InteractionManager(IServiceProvider Services)
        {
            SProvider = Services;
            IService = SProvider.GetRequiredService<InteractionService>();
            BotClient = SProvider.GetRequiredService<DiscordSocketClient>();
        }

        public async Task Initialize()
        {
            try
            {
                await IService.AddModulesAsync(Assembly.GetEntryAssembly(), SProvider);

                foreach (ModuleInfo module in IService.Modules)
                {
                    Console.WriteLine($"\u001b[97m[{DateTime.Now}]: [\u001b[93mMODULES\u001b[97m] => {module.Name} \u001b[92mInitialized\u001b[97m");
                    IService.Log += InteractionServiceLog;

                    foreach(SlashCommandInfo command in module.SlashCommands)
                    {
                        Console.WriteLine($"\u001b[97m[{DateTime.Now}]: [\u001b[93mCOMMAND\u001b[97m] => {module.Name}: \u001b[34m{command.Name}\u001b[97m \u001b[92mLoaded\u001b[97m");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\u001b[97m[{DateTime.Now}]: [\u001b[31mERROR\u001b[97m] => An error occured in InteractionManager.cs \nError Info:\n{ex}");
            }
        }

        private Task InteractionServiceLog(LogMessage msg)
        {
            Console.WriteLine(msg.Message);
            return Task.CompletedTask;
        }
    }
}
