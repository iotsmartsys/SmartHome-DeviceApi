using Core.Entities;

namespace Core.Contracts.Repositories;

public interface IPropertyRepository : IRepository
{
    Task<IEnumerable<Property>> GetByDeviceAsync(string device_id, Criteria<Property>? criteria, CancellationToken cancellationToken);
    Task AddAsync(string device_id, Property property, CancellationToken cancellationToken);
    Task UpdateAsync(string device_id, Property property, CancellationToken cancellationToken);
    Task RemoveAsync(string device_id, Property property, CancellationToken cancellationToken);
    Task<Property?> GetByIdAsync(string device_id, int id, CancellationToken cancellationToken);
}