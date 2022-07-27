using CountingBot.Databases;
using System.Threading.Tasks;

namespace CountingBot
{
    public static class DatabaseManager
    {
        public static readonly CountingDatabase countingDatabase = new();

        public static Task InitAsync() =>
            Task.WhenAll(
                countingDatabase.InitAsync()
            );

        public static Task CloseAsync() =>
            Task.WhenAll(
                countingDatabase.CloseAsync()
            );
    }
}