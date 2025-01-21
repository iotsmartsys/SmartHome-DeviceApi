using Core.Entities;

namespace Core.Contracts.Repositories;

public interface ICapabilityTypeRepository : IRepository
{
    Task CreateAsync(CapabilityType capabilityType);
    Task<IEnumerable<CapabilityType>> GetAllAsync(string? name);
    Task<CapabilityType?> GetByNameAsync(string name);
}