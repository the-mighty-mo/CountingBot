using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CountingBot.Databases
{
    public class CountingDatabase
    {
        private readonly SqliteConnection cnCounting = new SqliteConnection("Filename=Counting.db");

        public readonly ChannelsTable Channels;
        public readonly UserCountsTable UserCounts;

        public CountingDatabase()
        {
            Channels = new ChannelsTable(cnCounting);
            UserCounts = new UserCountsTable(cnCounting);
        }

        public async Task InitAsync()
        {
            await cnCounting.OpenAsync();

            List<Task> cmds = new List<Task>();
            using (SqliteCommand cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS Channels (guild_id TEXT PRIMARY KEY, channel_id TEXT NOT NULL, count INTEGER NOT NULL);", cnCounting))
            {
                cmds.Add(cmd.ExecuteNonQueryAsync());
            }
            using (SqliteCommand cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS UserCounts (guild_id TEXT NOT NULL, user_id TEXT NOT NULL, count INTEGER NOT NULL, last_num INTEGER NOT NULL);", cnCounting))
            {
                cmds.Add(cmd.ExecuteNonQueryAsync());
            }

            await Task.WhenAll(cmds);
        }

        public async Task CloseAsync() => await cnCounting.CloseAsync();

        public class ChannelsTable
        {
            private readonly SqliteConnection cnCounting;

            public ChannelsTable(SqliteConnection cnCounting) => this.cnCounting = cnCounting;

            public async Task<SocketTextChannel> GetCountingChannelAsync(SocketGuild g)
            {
                SocketTextChannel channel = null;

                string getChannel = "SELECT channel_id FROM Channels WHERE guild_id = @guild_id;";
                using (SqliteCommand cmd = new SqliteCommand(getChannel, cnCounting))
                {
                    cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

                    SqliteDataReader reader = await cmd.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        ulong.TryParse(reader["channel_id"].ToString(), out ulong channelID);
                        channel = g.GetTextChannel(channelID);
                    }
                    reader.Close();
                }

                return channel;
            }

            public async Task SetCountingChannelAsync(SocketTextChannel channel)
            {
                string update = "UPDATE Channels SET channel_id = @channel_id WHERE guild_id = @guild_id;";
                string insert = "INSERT INTO Channels (guild_id, channel_id, count) SELECT @guild_id, @channel_id, 0 WHERE (SELECT Changes() = 0);";

                using (SqliteCommand cmd = new SqliteCommand(update + insert, cnCounting))
                {
                    cmd.Parameters.AddWithValue("@guild_id", channel.Guild.Id.ToString());
                    cmd.Parameters.AddWithValue("@channel_id", channel.Id.ToString());
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            public async Task RemoveCountingChannelAsync(SocketGuild g)
            {
                string delete = "DELETE FROM Channels WHERE guild_id = @guild_id;";
                using (SqliteCommand cmd = new SqliteCommand(delete, cnCounting))
                {
                    cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            public async Task<int> GetCountAsync(SocketGuild g)
            {
                int count = 0;

                string getCount = "SELECT count FROM Channels WHERE guild_id = @guild_id;";
                using (SqliteCommand cmd = new SqliteCommand(getCount, cnCounting))
                {
                    cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

                    SqliteDataReader reader = await cmd.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        int.TryParse(reader["count"].ToString(), out count);
                    }
                    reader.Close();
                }

                return count;
            }

            public async Task IncrementCountAsync(SocketGuild g)
            {
                string update = "UPDATE Channels SET count = count + 1 WHERE guild_id = @guild_id;";
                string insert = "INSERT INTO Channels (guild_id, channel_id, count) SELECT @guild_id, @channel_id, 1 WHERE (SELECT Changes() = 0);";

                using (SqliteCommand cmd = new SqliteCommand(update + insert, cnCounting))
                {
                    cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public class UserCountsTable
        {
            private readonly SqliteConnection cnCounting;

            public UserCountsTable(SqliteConnection cnCounting) => this.cnCounting = cnCounting;

            public async Task<int> GetUserCountAsync(SocketGuildUser u)
            {
                int count = 0;

                string getUserCount = "SELECT count FROM UserCounts WHERE guild_id = @guild_id AND user_id = @user_id;";
                using (SqliteCommand cmd = new SqliteCommand(getUserCount, cnCounting))
                {
                    cmd.Parameters.AddWithValue("@guild_id", u.Guild.Id.ToString());
                    cmd.Parameters.AddWithValue("@user_id", u.Id.ToString());

                    SqliteDataReader reader = await cmd.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        int.TryParse(reader["count"].ToString(), out count);
                    }
                    reader.Close();
                }

                return count;
            }

            public async Task<List<(SocketGuildUser user, int count)>> GetAllUserCountsAsync(SocketGuild g)
            {
                List<(SocketGuildUser user, int count)> userCounts = new List<(SocketGuildUser user, int count)>();

                string getUserCounts = "SELECT user_id, count FROM UserCounts WHERE guild_id = @guild_id;";
                using (SqliteCommand cmd = new SqliteCommand(getUserCounts, cnCounting))
                {
                    cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

                    SqliteDataReader reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        ulong.TryParse(reader["user_id"].ToString(), out ulong userId);
                        int.TryParse(reader["count"].ToString(), out int count);

                        SocketGuildUser user = g.GetUser(userId);
                        if (user != null)
                        {
                            userCounts.Add((user, count));
                        }
                    }
                    reader.Close();
                }

                userCounts.Sort(Comparer<(SocketGuildUser user, int count)>.Create((x, y) => y.count.CompareTo(x.count)));
                return userCounts;
            }

            public async Task<int> GetLastUserNumAsync(SocketGuildUser u)
            {
                int num = -1;

                string getUserNum = "SELECT last_num FROM UserCounts WHERE guild_id = @guild_id AND user_id = @user_id;";
                using (SqliteCommand cmd = new SqliteCommand(getUserNum, cnCounting))
                {
                    cmd.Parameters.AddWithValue("@guild_id", u.Guild.Id.ToString());
                    cmd.Parameters.AddWithValue("@user_id", u.Id.ToString());

                    SqliteDataReader reader = await cmd.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        int.TryParse(reader["last_num"].ToString(), out num);
                    }
                    reader.Close();
                }

                return num;
            }

            public async Task IncrementUserCountAsync(SocketGuildUser u, int num)
            {
                string update = "UPDATE UserCounts SET count = count + 1, last_num = @num WHERE guild_id = @guild_id AND user_id = @user_id;";
                string insert = "INSERT INTO UserCounts (guild_id, user_id, count, last_num) SELECT @guild_id, @user_id, 1, @num WHERE (SELECT Changes() = 0);";

                using (SqliteCommand cmd = new SqliteCommand(update + insert, cnCounting))
                {
                    cmd.Parameters.AddWithValue("@guild_id", u.Guild.Id.ToString());
                    cmd.Parameters.AddWithValue("@user_id", u.Id.ToString());
                    cmd.Parameters.AddWithValue("@num", num);
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            public async Task ResetUserCountAsync(SocketGuildUser u)
            {
                string delete = "DELETE FROM UserCounts WHERE guild_id = @guild_id AND user_id = @user_id;";
                using (SqliteCommand cmd = new SqliteCommand(delete, cnCounting))
                {
                    cmd.Parameters.AddWithValue("@guild_id", u.Guild.Id.ToString());
                    cmd.Parameters.AddWithValue("@user_id", u.Id.ToString());
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }
    }
}
