using System.Data;
using Core.Entities;
using Core.Contracts.Repositories;
using Dapper;
using Microsoft.Extensions.Logging;

namespace Data.Repositories;

internal class CapabilityTypeRepository : ICapabilityTypeRepository, IRepository
{
    private readonly ILogger<CapabilityTypeRepository> logger;
    private readonly IDbConnection connection;
    private record CapabilityTypeIconRow(int CapabilityTypeId, string Name, string? ActiveColor, string? InactiveColor);

    public CapabilityTypeRepository(ILogger<CapabilityTypeRepository> logger, IDbConnection connection)
    {
        this.logger = logger;
        this.connection = connection;
    }

    public async Task CreateAsync(CapabilityType capabilityType)
    {
        connection.Open();
        using var transaction = connection.BeginTransaction();
        try
        {
            var newId = await connection.ExecuteScalarAsync<int>(CapabilityTypeQuery.Insert, capabilityType, transaction);
            capabilityType.Id = newId;

            if (capabilityType.Icons is not null && capabilityType.Icons.Any())
            {
                var iconParams = capabilityType.Icons.Select(i => new
                {
                    CapabilityTypeId = newId,
                    i.Name,
                    i.ActiveColor,
                    i.InactiveColor
                });

                await connection.ExecuteAsync(CapabilityTypeQuery.InsertIcon, iconParams, transaction);
            }

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
        finally
        {
            connection.Close();
        }
    }

    public async Task<IEnumerable<CapabilityType>> GetAllAsync(string? name)
    {
        var types = (await connection.QueryAsync<CapabilityType>(CapabilityTypeQuery.GetAll, new { name = $"%{name}%" })).ToList();

        if (types.Count == 0)
            return types;

        var ids = types.Select(t => t.Id).ToArray();
        var iconRows = await connection.QueryAsync<CapabilityTypeIconRow>(CapabilityTypeQuery.SelectIconsByTypeIds, new { ids });
        var grouped = iconRows.GroupBy(r => r.CapabilityTypeId)
                              .ToDictionary(g => g.Key, g => g.Select(r => new CapabilityIcon(r.Name, r.ActiveColor, r.InactiveColor)).ToList());

        foreach (var t in types)
        {
            if (grouped.TryGetValue(t.Id, out var icons))
                t.Icons = icons;
            else
                t.Icons = new List<CapabilityIcon>();
        }

        return types;
    }

    public async Task<CapabilityType?> GetByNameAsync(string name)
    {
        var type = await connection.QueryFirstOrDefaultAsync<CapabilityType>(new CommandDefinition(CapabilityTypeQuery.GetByName, new { name }, cancellationToken: default));
        if (type is null) return null;

        var icons = await connection.QueryAsync<CapabilityTypeIconRow>(CapabilityTypeQuery.SelectIconsByTypeId, new { id = type.Id });
        type.Icons = icons.Select(r => new CapabilityIcon(r.Name, r.ActiveColor, r.InactiveColor)).ToList();
        return type;
    }

    public async Task UpdateAsync(string currentName, string? newName, string? actuatorMode, string? dataType, bool? computedValue, IEnumerable<CapabilityIcon>? icons)
    {
        var existing = await GetByNameAsync(currentName);
        if (existing is null)
            throw new KeyNotFoundException($"CapabilityType '{currentName}' not found");

        connection.Open();
        using var transaction = connection.BeginTransaction();
        try
        {
            var updateParams = new
            {
                id = existing.Id,
                Name = newName,
                ActuatorMode = actuatorMode,
                DataType = dataType,
                ComputedValue = computedValue
            };
            await connection.ExecuteAsync(new CommandDefinition(CapabilityTypeQuery.UpdatePartialById, updateParams, transaction));

            if (icons is not null)
            {
                await connection.ExecuteAsync(new CommandDefinition(CapabilityTypeQuery.DeleteIconsByTypeId, new { id = existing.Id }, transaction));

                if (icons.Any())
                {
                    var iconParams = icons.Select(i => new
                    {
                        CapabilityTypeId = existing.Id,
                        i.Name,
                        i.ActiveColor,
                        i.InactiveColor
                    });

                    await connection.ExecuteAsync(new CommandDefinition(CapabilityTypeQuery.InsertIcon, iconParams, transaction));
                }
            }

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
        finally
        {
            connection.Close();
        }
    }

    public async Task DeleteAsync(string name)
    {
        var existing = await GetByNameAsync(name);
        if (existing is null)
            return;

        connection.Open();
        using var transaction = connection.BeginTransaction();
        try
        {
            await connection.ExecuteAsync(new CommandDefinition(CapabilityTypeQuery.DeleteIconsByTypeId, new { id = existing.Id }, transaction));
            await connection.ExecuteAsync(new CommandDefinition(CapabilityTypeQuery.DeleteTypeById, new { id = existing.Id }, transaction));
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
        finally
        {
            connection.Close();
        }
    }

    public async Task UpdateAsync(CapabilityType capabilityType)
    {
        connection.Open();
        using var transaction = connection.BeginTransaction();
        try
        {
            var updateParams = new
            {
                id = capabilityType.Id,
                capabilityType.Name,
                capabilityType.ActuatorMode,
                capabilityType.DataType,
                ComputedValue = capabilityType.ComputedValue
            };
            await connection.ExecuteAsync(new CommandDefinition(CapabilityTypeQuery.UpdatePartialById, updateParams, transaction));

            // Replace icons set
            await connection.ExecuteAsync(new CommandDefinition(CapabilityTypeQuery.DeleteIconsByTypeId, new { id = capabilityType.Id }, transaction));
            if (capabilityType.Icons is not null && capabilityType.Icons.Any())
            {
                var iconParams = capabilityType.Icons.Select(i => new
                {
                    CapabilityTypeId = capabilityType.Id,
                    i.Name,
                    i.ActiveColor,
                    i.InactiveColor
                });
                await connection.ExecuteAsync(new CommandDefinition(CapabilityTypeQuery.InsertIcon, iconParams, transaction));
            }

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
        finally
        {
            connection.Close();
        }
    }
}
