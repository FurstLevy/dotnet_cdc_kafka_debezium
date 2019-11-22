using System.Threading;

namespace Dotnet.CDC.Api.Services
{
    public interface ICacheMySql
    {
        void Consume(bool returnOnLastOffset, out bool finished, CancellationToken cancellationToken);
    }
}
