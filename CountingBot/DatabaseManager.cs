using System.Threading.Tasks;
using CountingBot.Databases;

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
