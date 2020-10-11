using CountingBot.Modules;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using static CountingBot.DatabaseManager;

namespace CountingBot
{
    public class CommandHandler
    {
        public const string prefix = "\\";
        public static int argPos = 0;

        private readonly DiscordSocketClient client;
        private readonly CommandService commands;
        private readonly IServiceProvider services;

        public CommandHandler(DiscordSocketClient client, IServiceProvider services)
        {
            this.client = client;
            this.services = services;

            CommandServiceConfig config = new CommandServiceConfig()
            {
                DefaultRunMode = RunMode.Async
            };
            commands = new CommandService(config);
        }

        public async Task InitCommandsAsync()
        {
            client.Connected += SendConnectMessage;
            client.Disconnected += SendDisconnectError;
            client.MessageReceived += HandleCommandAsync;
            client.MessageReceived += HandleCountingAsync;

            await commands.AddModulesAsync(Assembly.GetEntryAssembly(), services);
            commands.CommandExecuted += SendErrorAsync;
        }

        private async Task SendErrorAsync(Optional<CommandInfo> info, ICommandContext context, IResult result)
        {
            if (!result.IsSuccess && info.Value.RunMode == RunMode.Async && result.Error != CommandError.UnknownCommand && result.Error != CommandError.UnmetPrecondition)
            {
                await context.Channel.SendMessageAsync($"Error: {result.ErrorReason}");
            }
        }

        private async Task SendConnectMessage()
        {
            await Console.Out.WriteLineAsync($"{SecurityInfo.botName} is online");
        }

        private async Task SendDisconnectError(Exception e)
        {
            await Console.Out.WriteLineAsync(e.Message);
        }

        private async Task HandleCountingAsync(SocketMessage m)
        {
            if (!(m is SocketUserMessage msg) || !(msg.Channel is SocketTextChannel channel) || !(m.Author is SocketGuildUser user))
            {
                return;
            }

            if (channel.Id == (await countingDatabase.Channels.GetCountingChannelAsync(channel.Guild)).Id)
            {
                Task<int> nextCount = countingDatabase.Channels.GetCountAsync(channel.Guild).ContinueWith(x => x.Result + 1);
                Task<int> lastUserNum = countingDatabase.UserCounts.GetLastUserNumAsync(user);
                if (m.Content != (await nextCount).ToString() || await lastUserNum + 1 == await nextCount)
                {
                    await msg.DeleteAsync();
                }
                else
                {
                    await Task.WhenAll(
                        countingDatabase.Channels.IncrementCountAsync(user.Guild),
                        countingDatabase.UserCounts.IncrementUserCountAsync(user, await nextCount)
                    );
                } 
            }
        }

        private async Task<bool> CanBotRunCommandsAsync(SocketUserMessage msg) => await Task.Run(() => msg.Author.Id == client.CurrentUser.Id);
        private async Task<bool> ShouldDeleteBotCommands() => await Task.Run(() => true);

        private async Task HandleCommandAsync(SocketMessage m)
        {
            if (!(m is SocketUserMessage msg) || (msg.Author.IsBot && !await CanBotRunCommandsAsync(msg)))
            {
                return;
            }

            SocketCommandContext Context = new SocketCommandContext(client, msg);
            bool isCommand = msg.HasMentionPrefix(client.CurrentUser, ref argPos) || msg.HasStringPrefix(prefix, ref argPos);

            if (isCommand)
            {
                var result = await commands.ExecuteAsync(Context, argPos, services);

                List<Task> cmds = new List<Task>();
                if (msg.Author.IsBot && await ShouldDeleteBotCommands())
                {
                    cmds.Add(msg.DeleteAsync());
                }
                else if (!result.IsSuccess && result.Error == CommandError.UnmetPrecondition)
                {
                    cmds.Add(Context.Channel.SendMessageAsync(result.ErrorReason));
                }

                await Task.WhenAll(cmds);
            }
        }
    }
}
