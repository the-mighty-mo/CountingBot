using CountingBot.Modules;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace CountingBot
{
    public class CommandHandler
    {
        public const string prefix = "\\";
        public static int argPos = 0;

        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly IServiceProvider _services;

        public CommandHandler(DiscordSocketClient client, IServiceProvider services)
        {
            _client = client;
            _services = services;

            CommandServiceConfig config = new CommandServiceConfig()
            {
                DefaultRunMode = RunMode.Async
            };
            _commands = new CommandService(config);
        }

        public async Task InitCommandsAsync()
        {
            _client.Connected += SendConnectMessage;
            _client.Disconnected += SendDisconnectError;
            _client.MessageReceived += HandleCommandAsync;
            _client.MessageReceived += HandleCountingAsync;

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
            _commands.CommandExecuted += SendErrorAsync;
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
            if (Program.isConsole)
            {
                await Console.Out.WriteLineAsync($"{SecurityInfo.botName} is online");
            }
        }

        private async Task SendDisconnectError(Exception e)
        {
            if (Program.isConsole)
            {
                await Console.Out.WriteLineAsync(e.Message);
            }
        }

        private async Task HandleCountingAsync(SocketMessage m)
        {
            if (!(m is SocketUserMessage msg) || !(msg.Channel is SocketTextChannel channel) || !(m.Author is SocketGuildUser user))
            {
                return;
            }

            if (channel.Id == (await SetChannel.GetCountingChannelAsync(channel.Guild)).Id)
            {
                Task<int> nextCount = SetChannel.GetCountAsync(channel.Guild).ContinueWith(x => x.Result + 1);
                Task<int> lastUserNum = GetUserCount.GetLastUserNumAsync(user);
                if (m.Content != (await nextCount).ToString() || await lastUserNum + 1 == await nextCount)
                {
                    await msg.DeleteAsync();
                }
                else
                {
                    await Task.WhenAll(
                        SetChannel.IncrementCountAsync(user.Guild),
                        GetUserCount.IncrementUserCountAsync(user, await nextCount)
                    );
                } 
            }
        }

        private async Task HandleCommandAsync(SocketMessage m)
        {
            if (!(m is SocketUserMessage msg) || (msg.Author.IsBot && msg.Author.Id != _client.CurrentUser.Id))
            {
                return;
            }

            SocketCommandContext Context = new SocketCommandContext(_client, msg);
            bool isCommand = msg.HasMentionPrefix(_client.CurrentUser, ref argPos) || msg.HasStringPrefix(prefix, ref argPos);

            if (isCommand)
            {
                var result = await _commands.ExecuteAsync(Context, argPos, _services);

                List<Task> cmds = new List<Task>();
                if (msg.Author.IsBot)
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
