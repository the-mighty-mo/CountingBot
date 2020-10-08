using Discord;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CountingBot
{
    public class Program
    {
        private DiscordSocketConfig _config;
        private DiscordSocketClient _client;
        private CommandHandler _handler;

        public static readonly Random rng = new Random();

        public static readonly SqliteConnection cnChannels = new SqliteConnection("Filename=Channels.db");

        public static readonly bool isConsole = Console.OpenStandardInput(1) != Stream.Null;

        static void Main() => new Program().StartAsync().GetAwaiter().GetResult();

        public async Task StartAsync()
        {
            if (isConsole)
            {
                Console.Title = SecurityInfo.botName;
            }

            bool isRunning = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Count() > 1;
            if (isRunning)
            {
                await Task.Delay(1000);

                isRunning = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Count() > 1;
                if (isRunning)
                {
                    if (isConsole)
                    {
                        Console.WriteLine("Program is already running");
                        await Task.WhenAny(
                            Task.Run(() => Console.ReadLine()),
                            Task.Delay(5000)
                        );
                    }
                    return;
                }
            }

            List<Task> initSqlite = new List<Task>()
            {
                InitChannelsSqlite()
            };

            _config = new DiscordSocketConfig
            {
                AlwaysDownloadUsers = false
            };
            _client = new DiscordSocketClient(_config);

            await _client.LoginAsync(TokenType.Bot, SecurityInfo.token);
            await _client.StartAsync();
            await _client.SetGameAsync($"@{SecurityInfo.botName} help", null, ActivityType.Listening);

            IServiceProvider _services = new ServiceCollection().BuildServiceProvider();
            _handler = new CommandHandler(_client, _services);
            Task initCmd = _handler.InitCommandsAsync();

            await Task.WhenAll(initSqlite);
            if (isConsole)
            {
                Console.WriteLine($"{SecurityInfo.botName} has finished loading");
            }

            await initCmd;
            await Task.Delay(-1);
        }

        static async Task InitChannelsSqlite()
        {
            await cnChannels.OpenAsync();

            List<Task> cmds = new List<Task>();
            using (SqliteCommand cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS Channels (guild_id TEXT PRIMARY KEY, channel_id TEXT NOT NULL);", cnChannels))
            {
                cmds.Add(cmd.ExecuteNonQueryAsync());
            }

            await Task.WhenAll(cmds);
        }
    }
}