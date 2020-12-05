using CountingBot.Databases;
using System.Threading.Tasks;

namespace CountingBot
{
    public static class DatabaseManager
    {
        public static readonly CountingDatabase countingDatabase = new CountingDatabase();

        public static async Task InitAsync()
        {
            await Task.WhenAll(
                countingDatabase.InitAsync()
            );
        }

        public static async Task CloseAsync()
        {
            await Task.WhenAll(
                countingDatabase.CloseAsync()
            );
        }
    }
}