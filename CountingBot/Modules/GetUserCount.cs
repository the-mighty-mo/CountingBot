﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CountingBot.Modules
{
    public class GetUserCount : ModuleBase<SocketCommandContext>
    {
        [Command("getcount")]
        [Alias("get-count")]
        public async Task GetCountAsync()
        {
            if (Context.User is SocketGuildUser user)
            {
                await GetCountAsync(user);
            }
        }

        [Command("getcount")]
        [Alias("get-count")]
        public async Task GetCountAsync(SocketGuildUser user)
        {
            Task<int> count = GetUserCountAsync(user);

            List<(SocketGuildUser user, int count)> userCounts = await GetAllUserCountsAsync(Context.Guild);
            int userRank = 1 + userCounts.IndexOf((user, await count));

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithDescription($"{user.Mention} has sent {await count} messages in the counting channel.\n" +
                    $"Rank: {userRank}");

            await Context.Channel.SendMessageAsync(embed: embed.Build());
        }

        public static async Task<int> GetUserCountAsync(SocketGuildUser u)
        {
            int count = 0;

            string getUserCount = "SELECT count FROM UserCounts WHERE guild_id = @guild_id AND user_id = @user_id;";
            using (SqliteCommand cmd = new SqliteCommand(getUserCount, Program.cnCounting))
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

        public static async Task<List<(SocketGuildUser user, int count)>> GetAllUserCountsAsync(SocketGuild g)
        {
            List<(SocketGuildUser user, int count)> userCounts = new List<(SocketGuildUser user, int count)>();

            string getUserCounts = "SELECT user_id, count FROM UserCounts WHERE guild_id = @guild_id;";
            using (SqliteCommand cmd = new SqliteCommand(getUserCounts, Program.cnCounting))
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

        public static async Task<int> GetLastUserNumAsync(SocketGuildUser u)
        {
            int num = -1;

            string getUserNum = "SELECT last_num FROM UserCounts WHERE guild_id = @guild_id AND user_id = @user_id;";
            using (SqliteCommand cmd = new SqliteCommand(getUserNum, Program.cnCounting))
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

        public static async Task IncrementUserCountAsync(SocketGuildUser u, int num)
        {
            string update = "UPDATE UserCounts SET count = count + 1, last_num = @num WHERE guild_id = @guild_id AND user_id = @user_id;";
            string insert = "INSERT INTO UserCounts (guild_id, user_id, count, last_num) SELECT @guild_id, @user_id, 1, @num WHERE (SELECT Changes() = 0);";

            using (SqliteCommand cmd = new SqliteCommand(update + insert, Program.cnCounting))
            {
                cmd.Parameters.AddWithValue("@guild_id", u.Guild.Id.ToString());
                cmd.Parameters.AddWithValue("@user_id", u.Id.ToString());
                cmd.Parameters.AddWithValue("@num", num);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public static async Task ResetUserCountAsync(SocketGuildUser u)
        {
            string delete = "DELETE FROM UserCounts WHERE guild_id = @guild_id AND user_id = @user_id;";
            using (SqliteCommand cmd = new SqliteCommand(delete, Program.cnCounting))
            {
                cmd.Parameters.AddWithValue("@guild_id", u.Guild.Id.ToString());
                cmd.Parameters.AddWithValue("@user_id", u.Id.ToString());
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}
