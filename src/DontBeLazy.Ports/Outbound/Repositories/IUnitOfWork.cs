using System.Threading;
using System.Threading.Tasks;

namespace DontBeLazy.Ports.Outbound.Repositories;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
