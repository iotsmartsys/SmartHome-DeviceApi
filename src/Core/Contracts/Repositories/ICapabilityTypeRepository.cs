using Core.Entities;

namespace Core.Contracts.Repositories;

public interface ICapabilityTypeRepository : IRepository
{
    Task<IEnumerable<CapabilityType>> GetAllAsync(string? name);
    Task<CapabilityType?> GetByNameAsync(string name);
}