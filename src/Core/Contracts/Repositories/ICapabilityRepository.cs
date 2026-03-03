using Core.Entities;

namespace Core.Contracts.Repositories;

public interface ICapabilityRepository : IRepository
{
    Task<IEnumerable<Capability>> GetAllCapabilitiesAsync(CapabilityFind? capabilityFind, CancellationToken cancellationToken);
    Task AddAsync(string device_id, IEnumerable<Capability> enumerable);
    Task DeleteAsync(int id);
    Task UpdateAsync(Capability capability, CancellationToken cancellationToken);
    Task<Capability?> GetByNameAsync(string device_id, string capability_name, CancellationToken cancellationToken);
    Task<Capability?> GetByReferenceIdAsync(string referenceId, CancellationToken cancellationToken);
    Task<Capability?> GetByUidAsync(string uid, CancellationToken cancellationToken);
    Task<Capability?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<bool> UpdateValueAsync(string device_id, string capability_name, string value, CancellationToken cancellationToken);
}
public record class CapabilityFind(string? name = null, string? type = null, string? owner = null, string? value = null, bool? active = null, string? reference_id = null, string? smart_home_id = null, string? uid = null, string? group_name = null, string? device_id = null)
{
    public CapabilityFind() : this(null, null, null, null, null, null, null, null, null, null) { }
}
public class CapabilityHistory
{
    public DateTime UpdatedAt { get; set; }
    public string Value { get; set; } = string.Empty;
}
public record class CapabilityHistoryFind(int? last_hours, string? date_start, string? date_end)
{
    public CapabilityHistoryFind() : this(null, null, null) { }
}

