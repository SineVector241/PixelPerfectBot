using Discord;
using Discord.WebSocket;
using Discord.Interactions;
using Microsoft.Extensions.DependencyInjection;

namespace PixelPerfectBot
{
    public class EventManager
    {
        private Core.Database DB = new Core.Database();
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
            BotClient.MessageReceived += MessageRecieved;
            IService.InteractionExecuted += InteractionExecuted;
            return Task.CompletedTask;
        }

        private async Task InteractionExecuted(ICommandInfo arg1, IInteractionContext arg2, IResult arg3)
        {
            if(arg2.Interaction.HasResponded && !arg3.IsSuccess)
            {
                await arg2.Interaction.FollowupAsync($"An error occured: {arg3.ErrorReason}");
            }
            else if(!arg3.IsSuccess)
            {
                await arg2.Interaction.RespondAsync($"An error occured: {arg3.ErrorReason}");
            }
        }

        private async Task MessageRecieved(SocketMessage msg)
        {
            var User = msg.Author as SocketGuildUser;
            if (User != null && !User.IsBot)
            {
                DB.CreateUserIfNotExists(User.Id);
                if (msg.Channel.Id == Config.BotConfiguration.SuggestionChannel)
                {
                    await msg.DeleteAsync();
                    var user = DB.GetUser(msg.Author.Id);
                    if (user.SuggestionCooldown > DateTime.UtcNow || user.SentContentCreator)
                    {
                        return;
                    }
                    var channel = User.Guild.GetTextChannel(Config.BotConfiguration.SuggestionChannel);
                    var embed = new EmbedBuilder()
                        .WithTitle("Suggestion")
                        .WithDescription(msg.Content)
                        .AddField("Upvotes - Downvotes", "0")
                        .WithColor(Color.LightOrange)
                        .WithAuthor(msg.Author)
                        .WithTimestamp(DateTime.UtcNow);
                    var builder = new ComponentBuilder()
                        .WithButton("Upvote", "UpvoteSuggestion", ButtonStyle.Success, Emoji.Parse("👍"))
                        .WithButton("Downvote", "DownvoteSuggestion", ButtonStyle.Danger, Emoji.Parse("👎"));
                    var Message = await channel.SendMessageAsync($"Suggestion from {msg.Author.Mention}", embed: embed.Build(), components: builder.Build());
                    user.SuggestionCooldown = DateTime.UtcNow.AddMinutes(15);
                    DB.UpdateUser(user);
                    DB.AddSuggestion(new Core.Database.Suggestion() { MessageId = Message.Id, UpvotesDownvotes = 0 });
                    DB.TrimFirstSuggestion();
                }
            }
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
