using Core.Entities;

namespace Core.Contracts.Repositories;

public interface ICapabilityTypeRepository : IRepository
{
    Task CreateAsync(CapabilityType capabilityType);
    Task<IEnumerable<CapabilityType>> GetAllAsync(string? name);
    Task<CapabilityType?> GetByNameAsync(string name);
    Task UpdateAsync(string currentName, string? newName, string? actuatorMode, string? dataType, bool? computedValue, string? valueSymbol, IEnumerable<CapabilityIcon>? icons);
    Task UpdateAsync(CapabilityType capabilityType);
    Task DeleteAsync(string name);
}
