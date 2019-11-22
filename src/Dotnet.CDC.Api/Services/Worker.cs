using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace Dotnet.CDC.Api.Services
{
    public class Worker : BackgroundService
    {
        private readonly ICacheMySql _cacheMySql;

        public Worker(ICacheMySql cacheMySql)
        {
            _cacheMySql = cacheMySql;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Run(() => _cacheMySql.Consume(false, out _, stoppingToken), stoppingToken);
            }
        }
    }
}
