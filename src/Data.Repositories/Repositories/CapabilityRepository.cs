using System.Data;
using Core.Contracts.Repositories;
using Core.Entities;
using Dapper;
using Microsoft.Extensions.Logging;

namespace Data.Repositories;

internal class CapabilityRepository(ILogger<CapabilityRepository> logger, IDbConnection connection) : ICapabilityRepository, IRepository
{
    public async Task AddAsync(string device_id, IEnumerable<Capability> capabilities)
    {
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            int idDevice = await connection.ExecuteScalarAsync<int>("SELECT Id FROM Devices WHERE DeviceId = @device_id", new { device_id }, transaction);
            if (idDevice == 0)
            {
                logger.LogWarning("Device {deviceId} not found", device_id);
                throw new NotFoundExceptionDomain($"Device {device_id} not found");
            }

            foreach (var capability in capabilities)
            {
                logger.LogInformation("Adicionando capability {capabilityName} para o device {deviceId}", capability.Name, device_id);
                const string sql = CapabilityQuery.InsertCapability;
                await connection.ExecuteAsync(sql, new
                {
                    DeviceId = idDevice,
                    capability.Name,
                    capability.Description,
                    capability.Type,
                    capability.Value,
                    capability.Owner
                }, transaction);

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

    public async Task<Capability?> GetByNameAsync(CancellationToken cancellationToken, string capability_name)
    {
        var command = new FindCapabilityQueryBuilder()
            .WithName(capability_name)
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

    public async Task<Capability?> GetByReferenceIdAsync(CancellationToken cancellationToken, string referenceId)
    {
        var command = new FindCapabilityQueryBuilder()
            .WithReferenceId(referenceId)
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

    public async Task UpdateAsync(Capability capability, CancellationToken cancellationToken)
    {
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {

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
            await connection.ExecuteAsync(sql, new
            {
                id = capability.Id,
                capability.Name,
                capability.Description,
                capability.Type,
                capability.Value,
                capability.Owner,
                capability.Active,
                icon_name = capability.IconName,
                IconActiveColor = capability.IconActiveColor,
                IconInactiveColor = capability.IconInactiveColor
            }, transaction);

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

    public async Task<bool> UpdateValueAsync(string capability_name, string value, CancellationToken cancellationToken)
    {
        // Log em nível debug para reduzir overhead em cenários de alta taxa
        logger.LogDebug("Atualizando valor da capability {capabilityName}", capability_name);
        const string sql = CapabilityQuery.UpdateValue;
        try
        {
            var rows_affecteds = await Data.Repositories.Utils.DbRetry.ExecuteAsync(async () =>
            {
                if (connection.State != ConnectionState.Open)
                    connection.Open();
                var cmd = new CommandDefinition(sql, new { capability_name, value }, cancellationToken: cancellationToken);
                return await connection.ExecuteAsync(cmd);
            }, logger, cancellationToken);

            logger.LogDebug("Valor da capability {capabilityName} atualizado", capability_name);
            return rows_affecteds > 0;
        }
        finally
        {
            if (connection.State != ConnectionState.Closed)
                connection.Close();
        }
    }
}
