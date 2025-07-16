using Core.Entities;

namespace Core.Contracts.Repositories;

public interface IMonitoredPlaceRepository : IRepository
{
    Task<IEnumerable<MonitoredPlace>> GetAllAsync();
}