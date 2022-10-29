using CountingBot.Databases.CountingDatabaseTables;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CountingBot.Databases
{
    public class CountingDatabase
    {
        private readonly SqliteConnection connection = new("Filename=Counting.db");
        private readonly Dictionary<System.Type, ITable> tables = new();

        public ChannelsTable Channels => (tables[typeof(ChannelsTable)] as ChannelsTable)!;
        public UserCountsTable UserCounts => (tables[typeof(UserCountsTable)] as UserCountsTable)!;

        public CountingDatabase()
        {
            tables.Add(typeof(ChannelsTable), new ChannelsTable(connection));
            tables.Add(typeof(UserCountsTable), new UserCountsTable(connection));
        }

        public async Task InitAsync()
        {
            IEnumerable<Task> GetTableInits()
            {
                foreach (var table in tables.Values)
                {
                    yield return table.InitAsync();
                }
            }
            await connection.OpenAsync().ConfigureAwait(false);
            await Task.WhenAll(GetTableInits()).ConfigureAwait(false);
        }

        public Task CloseAsync() => connection.CloseAsync();
    }
}