using Core.Entities;

namespace Core.Contracts.Repositories;

public interface IPlatformRepository : IRepository
{
    Task<IEnumerable<Platform>> GetAllAsync();
}