﻿using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CountingBot.Databases.CountingDatabaseTables
{
    public class UserCountsTable : ITable
    {
        private readonly SqliteConnection connection;

        public UserCountsTable(SqliteConnection connection) => this.connection = connection;

        public Task InitAsync()
        {
            using SqliteCommand cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS UserCounts (guild_id TEXT NOT NULL, user_id TEXT NOT NULL, count INTEGER NOT NULL, last_num INTEGER NOT NULL);", connection);
            return cmd.ExecuteNonQueryAsync();
        }

        public async Task<int> GetUserCountAsync(SocketGuildUser u)
        {
            int count = 0;

            string getUserCount = "SELECT count FROM UserCounts WHERE guild_id = @guild_id AND user_id = @user_id;";
            using (SqliteCommand cmd = new SqliteCommand(getUserCount, connection))
            {
                cmd.Parameters.AddWithValue("@guild_id", u.Guild.Id.ToString());
                cmd.Parameters.AddWithValue("@user_id", u.Id.ToString());

                SqliteDataReader reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    _ = int.TryParse(reader["count"].ToString(), out count);
                }
                reader.Close();
            }

            return count;
        }

        public async Task<List<(SocketGuildUser user, int count)>> GetAllUserCountsAsync(SocketGuild g)
        {
            List<(SocketGuildUser user, int count)> userCounts = new List<(SocketGuildUser user, int count)>();

            string getUserCounts = "SELECT user_id, count FROM UserCounts WHERE guild_id = @guild_id;";
            using (SqliteCommand cmd = new SqliteCommand(getUserCounts, connection))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

                SqliteDataReader reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    _ = ulong.TryParse(reader["user_id"].ToString(), out ulong userId);
                    _ = int.TryParse(reader["count"].ToString(), out int count);

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
            using (SqliteCommand cmd = new SqliteCommand(getUserNum, connection))
            {
                cmd.Parameters.AddWithValue("@guild_id", u.Guild.Id.ToString());
                cmd.Parameters.AddWithValue("@user_id", u.Id.ToString());

                SqliteDataReader reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    _ = int.TryParse(reader["last_num"].ToString(), out num);
                }
                reader.Close();
            }

            return num;
        }

        public async Task IncrementUserCountAsync(SocketGuildUser u, int num)
        {
            string update = "UPDATE UserCounts SET count = count + 1, last_num = @num WHERE guild_id = @guild_id AND user_id = @user_id;";
            string insert = "INSERT INTO UserCounts (guild_id, user_id, count, last_num) SELECT @guild_id, @user_id, 1, @num WHERE (SELECT Changes() = 0);";

            using (SqliteCommand cmd = new SqliteCommand(update + insert, connection))
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
            using (SqliteCommand cmd = new SqliteCommand(delete, connection))
            {
                cmd.Parameters.AddWithValue("@guild_id", u.Guild.Id.ToString());
                cmd.Parameters.AddWithValue("@user_id", u.Id.ToString());
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}
