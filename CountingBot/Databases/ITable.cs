using System.Threading.Tasks;

namespace CountingBot.Databases
{
    interface ITable
    {
        public Task InitAsync();
    }
}
