using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Fergun.Interactive;

namespace PixelPerfectBot
{
    public class Bot
    {
        private DiscordSocketClient BotClient;
        private ServiceProvider SProvider;
        private InteractionService IService;
        public Bot()
        {
            BotClient = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Debug,
                MessageCacheSize = 100,
                UseInteractionSnowflakeDate = false,
                AlwaysDownloadUsers = true
            });

            IService = new InteractionService(BotClient, new InteractionServiceConfig
            {
                LogLevel = LogSeverity.Debug
            });

            SProvider = BuildServiceProvider();
        }

        public async Task Run()
        {

            await new EventManager(SProvider).Initialize();
            await new InteractionManager(SProvider).Initialize();
            //Log all events to Log() Function
            BotClient.Log += Log;

            //Test if there is no token inputted
            if (string.IsNullOrWhiteSpace(Config.BotConfiguration.Token))
            {
                Console.WriteLine("\u001b[41mBOT CONFIGURATION TOKEN IS BLANK\u001b[40m");
                return;
            }

            //Login and start the bot
            await BotClient.LoginAsync(TokenType.Bot, Config.BotConfiguration.Token);
            await BotClient.StartAsync();

            //Keeps Bot Running
            await Task.Delay(-1);
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine($"\u001b[97m[{DateTime.Now}]: [\u001b[93m{msg.Source}\u001b[97m] => {msg.Message}");
            return Task.CompletedTask;
        }

        public ServiceProvider BuildServiceProvider()
        {
            return new ServiceCollection()
                .AddSingleton(BotClient)
                .AddSingleton(IService)
                .AddSingleton<InteractiveService>()
                .BuildServiceProvider();
        }
    }
}
