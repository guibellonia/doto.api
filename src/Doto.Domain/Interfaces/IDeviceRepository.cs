using Doto.Domain.Entities;

namespace Doto.Domain.Interfaces;

public interface IDeviceRepository
{
    Task<Device?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Device?> GetByPushTokenAsync(string pushToken, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Device>> GetActiveByUserAsync(Guid userId, CancellationToken cancellationToken = default);

    Task AddAsync(Device device, CancellationToken cancellationToken = default);

    Task UpdateAsync(Device device, CancellationToken cancellationToken = default);
}
