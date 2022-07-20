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
        private static int argPos = 0;

        private readonly DiscordSocketClient client;
        private readonly CommandService commands;
        private readonly IServiceProvider services;

        public CommandHandler(DiscordSocketClient client, IServiceProvider services)
        {
            this.client = client;
            this.services = services;

            CommandServiceConfig config = new()
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
            if (!result.IsSuccess && info.GetValueOrDefault()?.RunMode == RunMode.Async && result.Error is not (CommandError.UnknownCommand or CommandError.UnmetPrecondition))
            {
                await context.Channel.SendMessageAsync($"Error: {result.ErrorReason}");
            }
        }

        private Task SendConnectMessage() =>
            Console.Out.WriteLineAsync($"{SecurityInfo.botName} is online");

        private Task SendDisconnectError(Exception e) =>
            Console.Out.WriteLineAsync(e.Message);

        private async Task HandleCountingAsync(SocketMessage m)
        {
            if (m is not SocketUserMessage msg || msg.Channel is not SocketTextChannel channel || m.Author is not SocketGuildUser user)
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

        private Task<bool> CanBotRunCommandsAsync(SocketUserMessage msg) => Task.Run(() => msg.Author.Id == client.CurrentUser.Id);

        private Task<bool> ShouldDeleteBotCommands() => Task.Run(() => true);

        private async Task HandleCommandAsync(SocketMessage m)
        {
            if (m is not SocketUserMessage msg || (msg.Author.IsBot && !await CanBotRunCommandsAsync(msg)))
            {
                return;
            }

            SocketCommandContext Context = new(client, msg);
            bool isCommand = msg.HasMentionPrefix(client.CurrentUser, ref argPos) || msg.HasStringPrefix(prefix, ref argPos);

            if (isCommand)
            {
                var result = await commands.ExecuteAsync(Context, argPos, services);

                List<Task> cmds = new();
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