using Core.Entities;

namespace Core.Contracts.Repositories;

public interface ICapabilityRepository : IRepository
{
    Task<IEnumerable<Capability>> GetAllCapabilitiesAsync(CapabilityFind? capabilityFind, CancellationToken cancellationToken);
    Task AddAsync(string device_id, IEnumerable<Capability> enumerable);
    Task DeleteAsync(int id);
    Task UpdateAsync(Capability capability, CancellationToken cancellationToken);
    Task<Capability?> GetByNameAsync(CancellationToken cancellationToken, string capability_name);
    Task<Capability?> GetByReferenceIdAsync(CancellationToken cancellationToken, string referenceId);
    Task<Capability?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<bool> UpdateValueAsync(string capability_name, string value, CancellationToken cancellationToken);
}
public record class CapabilityFind(string? name, string? type, string? owner, string? value,
    bool? active, string? reference_id, string? smart_home_id = null)
{
    public CapabilityFind() : this(null, null, null, null, null, null, null) { }
}
public class CapabilityHistory
{
    public DateTime UpdatedAt { get; set; }
    public string Value { get; set; } = string.Empty;
}
public record class CapabilityHistoryFind(int? last_hours, string? date_start, string? date_end )
{
    public CapabilityHistoryFind() : this(null, null, null) { }
}
 
