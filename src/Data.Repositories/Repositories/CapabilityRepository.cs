using System.Data;
using Core.Contracts.Repositories;
using Core.Entities;
using Dapper;
using Microsoft.Extensions.Logging;

namespace Data.Repositories;

internal class CapabilityRepository(ILogger<CapabilityRepository> logger, IDbConnection connection) : ICapabilityRepository, IRepository
{
    public async Task AddAsync(string device_id, IEnumerable<Capability> capabilities, CancellationToken cancellationToken = default)
    {
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            int idDevice = await connection.ExecuteScalarAsync<int>(new CommandDefinition("SELECT Id FROM Devices WHERE DeviceId = @device_id", new { device_id }, transaction, cancellationToken: cancellationToken));
            if (idDevice == 0)
            {
                logger.LogWarning("Device {deviceId} not found", device_id);
                throw new NotFoundExceptionDomain($"Device {device_id} not found");
            }

            foreach (var capability in capabilities)
            {
                logger.LogInformation("Adicionando capability {capabilityName} para o device {deviceId}", capability.Name, device_id);
                var dataType = await connection.QuerySingleOrDefaultAsync<string>(new CommandDefinition(
                    CapabilityQuery.GetDataTypeByName, new { type = capability.Type }, transaction,
                    cancellationToken: cancellationToken));
                if (AirConditionerState.IsDataType(dataType))
                    capability.Value = AirConditionerState.Initialize(capability.Value);
                const string sql = CapabilityQuery.InsertCapability;
                await connection.ExecuteAsync(new CommandDefinition(sql, new
                {
                    DeviceId = idDevice,
                    capability.Name,
                    capability.Description,
                    capability.Type,
                    capability.Value,
                    capability.Owner
                }, transaction, cancellationToken: cancellationToken));

                logger.LogInformation("Capability {capabilityName} adicionada para o device {deviceId}", capability.Name, device_id);
            }
            transaction.Commit();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error ao adicionar capabilities para o device {deviceId}", device_id);
            transaction.Rollback();
            throw;
        }
        finally
        {
            connection.Close();
        }
    }

    public async Task<Capability?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var command = new FindCapabilityQueryBuilder()
            .WithId(id)
            .WithCancellationToken(cancellationToken)
            .Build();

        return (await GetAllAsync(command)).FirstOrDefault();
    }

    public async Task<Capability?> GetByNameAsync(string device_id, string capability_name, CancellationToken cancellationToken)
    {
        var command = new FindCapabilityQueryBuilder()
            .WithName(capability_name)
            .WithDeviceId(device_id)
            .WithCancellationToken(cancellationToken)
            .Build();

        return (await GetAllAsync(command)).FirstOrDefault();
    }

    public async Task<IEnumerable<Capability>> GetAllCapabilitiesAsync(CapabilityFind? capabilityFind, CancellationToken cancellationToken)
    {
        try
        {
            var command = new FindCapabilityQueryBuilder()
            .WithFind(capabilityFind)
            .WithCancellationToken(cancellationToken)
            .Build();

            return await GetAllAsync(command);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error ao buscar capabilities");
            throw;
        }
        finally
        {
            connection.Close();
        }
    }

    public async Task<Capability?> GetByReferenceIdAsync(string referenceId, CancellationToken cancellationToken)
    {
        var command = new FindCapabilityQueryBuilder()
            .WithReferenceId(referenceId)
            .WithCancellationToken(cancellationToken)
            .Build();

        return (await GetAllAsync(command)).FirstOrDefault();
    }

    public async Task<Capability?> GetByUidAsync(string uid, CancellationToken cancellationToken)
    {
        var command = new FindCapabilityQueryBuilder()
            .WithUid(uid)
            .WithCancellationToken(cancellationToken)
            .Build();

        return (await GetAllAsync(command)).FirstOrDefault();
    }

    async Task<IEnumerable<Capability>> GetAllAsync(CommandDefinition command)
    {
        var map = new Dictionary<int, Capability>();
        try
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();

            await Data.Repositories.Utils.DbRetry.ExecuteAsync(async () =>
            {
                await connection.QueryAsync<Capability, CapabilityPlatform?, CapabilityGroup?, CapabilityTypeSmartHome?, Capability>(
                    command: command,
                    map: (capability, platform, group, smartHome) =>
                    {
                        if (!map.TryGetValue(capability.Id, out var capabilitySelected))
                        {
                            map[capability.Id] = capability;
                            capabilitySelected = capability;
                        }

                        if (platform != null)
                            capabilitySelected.AddPlatform(platform);

                        if (group != null)
                            capabilitySelected.AddGroup(group);

                        if (smartHome != null)
                            capabilitySelected.AddSmartHomeType(smartHome);

                        return capabilitySelected;
                    },
                    splitOn: "Id");
                return true;
            }, logger, command.CancellationToken);

            foreach (var capability in map.Values)
            {
                if (AirConditionerState.IsDataType(capability.DataType))
                    capability.Value = AirConditionerState.Normalize(capability.Value);
            }
            return map.Values;
        }
        finally
        {
            if (connection.State != ConnectionState.Closed)
                connection.Close();
        }
    }

    public async Task DeleteAsync(int id)
    {
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            logger.LogInformation("Removendo capability {id}", id);
            const string sql = CapabilityQuery.RemoveCapability;
            await connection.ExecuteAsync(sql, new
            {
                id = id
            }, transaction);

            logger.LogInformation("Capability {id} removida", id);
            transaction.Commit();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error ao remover capabilities para o device {id}", id);
            transaction.Rollback();
            throw;
        }
        finally
        {
            connection.Close();
        }
    }

    public async Task UpdateAsync(Capability capability, CancellationToken cancellationToken, bool valueChanged = true)
    {
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {

            var current = await connection.QuerySingleOrDefaultAsync<Capability>(new CommandDefinition(
                CapabilityQuery.SelectStateByIdForUpdate, new { id = capability.Id }, transaction,
                cancellationToken: cancellationToken));
            if (current is null) throw new NotFoundExceptionDomain("Capability not found.");
            var targetType = await connection.QuerySingleOrDefaultAsync<string>(new CommandDefinition(
                CapabilityQuery.GetDataTypeByName, new { type = capability.Type }, transaction,
                cancellationToken: cancellationToken));
            var currentIsAir = AirConditionerState.IsDataType(current.DataType);
            var targetIsAir = AirConditionerState.IsDataType(targetType);
            var writeValue = !(currentIsAir || targetIsAir) || valueChanged;
            if (currentIsAir)
            {
                // Always use the state locked inside this transaction, never the controller snapshot.
                capability.Value = valueChanged
                    ? AirConditionerState.Apply(current.Value, capability.Value)
                    : current.Value;
            }
            else if (targetIsAir)
            {
                capability.Value = AirConditionerState.Initialize(valueChanged ? capability.Value : current.Value);
                writeValue = true;
            }

            logger.LogInformation("Removendo o relacionamento de plataforma para a capability {capabilityName} do device {id}", capability.Name, capability.Id);
            var command = new CommandDefinition(CapabilityQuery.RemovePlatformFromCapability, new
            {
                CapabilityId = capability.Id
            }, transaction: transaction, cancellationToken: cancellationToken);
            await connection.ExecuteAsync(command);
            logger.LogInformation("Relacionamento de plataforma removido para a capability {capabilityName} do device {id}", capability.Name, capability.Id);

            foreach (var platform in capability.Platforms)
            {
                logger.LogInformation("Adicionando plataforma {platformName} para a capability {capabilityName} do device {id}", platform.Platform, capability.Name, capability.Id);
                command = new CommandDefinition(CapabilityQuery.InsertPlatformToCapability, new
                {
                    CapabilityId = capability.Id,
                    Platform = platform.Platform,
                    ReferenceId = platform.ReferenceId
                }, transaction: transaction, cancellationToken: cancellationToken);
                await connection.ExecuteAsync(command);
                logger.LogInformation("Plataforma {platformName} adicionada para a capability {capabilityName} do device {id}", platform.Platform, capability.Name, capability.Id);
            }

            logger.LogInformation($"Removendo os relacionamentos de grupo para a capability {capability.Name} do device {capability.Id}");
            command = new CommandDefinition(CapabilityQuery.RemoveGroupFromCapability, new
            {
                CapabilityId = capability.Id
            }, transaction: transaction, cancellationToken: cancellationToken);
            await connection.ExecuteAsync(command);
            logger.LogInformation($"Relacionamentos de grupo removidos para a capability {capability.Name} do device {capability.Id}");

            foreach (var group in capability.Groups)
            {
                logger.LogInformation("Adicionando grupo {groupName} para a capability {capabilityName} do device {id}", group.Name, capability.Name, capability.Id);
                int groupId = await connection.ExecuteScalarAsync<int>("SELECT Id FROM `Groups` WHERE Name = @Name LIMIT 1", new { group.Name }, transaction);
                if (groupId == 0)
                {
                    logger.LogWarning("Grupo {groupName} não encontrado. Pulando adição para a capability {capabilityName} do device {id}", group.Name, capability.Name, capability.Id);

                    logger.LogInformation("Criando novo grupo {groupName} para a capability {capabilityName} do device {id}", group.Name, capability.Name, capability.Id);

                    groupId = await connection.ExecuteScalarAsync<int>(GroupQuery.Insert, new
                    {
                        group.Name,
                        Activated = true
                    }, transaction);
                    logger.LogInformation("Novo grupo {groupName} criado com ID {groupId} para a capability {capabilityName} do device {id}", group.Name, groupId, capability.Name, capability.Id);
                }

                command = new CommandDefinition(GroupQuery.InsertCapabilityForGroup, new
                {
                    CapabilityId = capability.Id,
                    GroupId = groupId
                }, transaction: transaction, cancellationToken: cancellationToken);
                await connection.ExecuteAsync(command);
                logger.LogInformation("Grupo {groupName} adicionado para a capability {capabilityName} do device {id}", group.Name, capability.Name, capability.Id);
            }

            logger.LogInformation("Atualizando capability {capabilityName} para o device {id}", capability.Name, capability.Id);
            const string sql = CapabilityQuery.UpdateForDevice;
            await connection.ExecuteAsync(new CommandDefinition(sql, new
            {
                id = capability.Id,
                writeValue,
                capability.Name,
                capability.Description,
                capability.Type,
                capability.Value,
                capability.Owner,
                capability.Active,
                icon_name = capability.IconName,
                IconActiveColor = capability.IconActiveColor,
                IconInactiveColor = capability.IconInactiveColor
            }, transaction, cancellationToken: cancellationToken));

            logger.LogInformation("Capability {capabilityName} atualizada para o device {id}", capability.Name, capability.Id);

            transaction.Commit();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error ao atualizar capability para o device {id}", capability.Id);
            transaction.Rollback();
            throw;
        }
        finally
        {
            connection.Close();
        }
    }

    public async Task<bool> UpdateValueAsync(string device_id, string capability_name, string value, CancellationToken cancellationToken)
    {
        try
        {
            if (connection.State != ConnectionState.Open) connection.Open();
            using var transaction = connection.BeginTransaction();
            // No blind retry across Commit: its outcome can be unknown after a connection failure.
            var capabilities = (await connection.QueryAsync<Capability>(new CommandDefinition(
                CapabilityQuery.SelectStateForUpdate, new { device_id, capability_name }, transaction,
                cancellationToken: cancellationToken))).ToList();
            if (capabilities.Count == 0) return false;

            var updated = false;
            foreach (var capability in capabilities)
            {
                var isAir = AirConditionerState.IsDataType(capability.DataType);
                var next = isAir ? AirConditionerState.Apply(capability.Value, value) : value;
                var rows = await connection.ExecuteAsync(new CommandDefinition(
                    CapabilityQuery.UpdateStateById, new { id = capability.Id, value = next }, transaction,
                    cancellationToken: cancellationToken));
                // A valid repeated AC command still addresses an existing capability.
                updated |= isAir || rows > 0;
            }
            transaction.Commit();
            return updated;
        }
        finally
        {
            if (connection.State != ConnectionState.Closed) connection.Close();
        }
    }
}
