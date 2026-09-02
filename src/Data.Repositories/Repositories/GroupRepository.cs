using System.Data;
using Core.Contracts.Repositories;
using Core.Entities;
using Dapper;
using Microsoft.Extensions.Logging;

namespace Data.Repositories;

internal class GroupRepository(ILogger<GroupRepository> logger, IDbConnection connection) : IGroupRepository
{
    public async Task AddAsync(Group group, CancellationToken cancellationToken)
    {
        connection.Open();
        using var transaction = connection.BeginTransaction();
        try
        {
            var command = new CommandDefinition(GroupQuery.Insert, new
            {
                name = group.Name,
                activated = group.IsActive,
                IconName = group.Icon?.Name
            }, cancellationToken: cancellationToken, transaction: transaction);
            group.Id = await connection.ExecuteScalarAsync<int>(command);

            foreach (var capability in group.Capabilities)
            {
                await connection.ExecuteAsync(new CommandDefinition(GroupQuery.InsertCapabilityForGroup, new
                {
                    groupId = group.Id,
                    capabilityId = capability.Id
                }, cancellationToken: cancellationToken, transaction: transaction));
            }
            transaction.Commit();
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            logger.LogError(ex, "Erro ao adicionar grupo");
            throw;
        }
        finally
        {
            connection.Close();
        }
        logger.LogInformation($"Grupo {group.Name} adicionado com sucesso com ID {group.Id}");
    }

    public async Task<IEnumerable<Group>> GetAllAsync(CancellationToken cancellationToken)
    {
        var command = new GroupQueryBuilder()
            .WithCancellationToken(cancellationToken)
            .Build();
        return await GetAllAsync(command, cancellationToken);
    }

    public async Task<Group?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var command = new GroupQueryBuilder()
            .WithId(id)
            .WithCancellationToken(cancellationToken)
            .Build();

        var groups = await GetAllAsync(command, cancellationToken);

        return groups.FirstOrDefault();
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        connection.Open();
        using var transaction = connection.BeginTransaction();
        try
        {
            await connection.ExecuteAsync(new CommandDefinition(
                GroupQuery.DeleteAllCapabilityForGroup,
                new { groupId = id },
                transaction: transaction,
                cancellationToken: cancellationToken));

            var affectedRows = await connection.ExecuteAsync(new CommandDefinition(
                GroupQuery.Delete,
                new { id },
                transaction: transaction,
                cancellationToken: cancellationToken));

            if (affectedRows == 0)
            {
                transaction.Rollback();
                return false;
            }

            transaction.Commit();
            return true;
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            logger.LogError(ex, "Erro ao excluir grupo {GroupId}", id);
            throw;
        }
        finally
        {
            connection.Close();
        }
    }

    public async Task UpdateAsync(Group group, CancellationToken cancellationToken)
    {
        connection.Open();
        using var transaction = connection.BeginTransaction();
        try
        {
            var command = new CommandDefinition(GroupQuery.Update, new
            {
                id = group.Id,
                name = group.Name,
                activated = group.IsActive,
                IconName = group.Icon?.Name
            }, cancellationToken: cancellationToken, transaction: transaction);
            await connection.ExecuteAsync(command);

            await connection.ExecuteAsync(new CommandDefinition(GroupQuery.DeleteAllCapabilityForGroup, new { groupId = group.Id }, cancellationToken: cancellationToken, transaction: transaction));
            foreach (var capability in group.Capabilities)
            {
                capability.Id = await connection.ExecuteAsync(new CommandDefinition(GroupQuery.InsertCapabilityForGroup, new
                {
                    groupId = group.Id,
                    capabilityId = capability.Id
                }, cancellationToken: cancellationToken, transaction: transaction));
            }
            transaction.Commit();
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            logger.LogError(ex, "Erro ao atualizar grupo");
            throw;
        }
        finally
        {
            connection.Close();
        }
    }

    public async Task<bool> UpdateAdministrativeAsync(
        int id,
        string? name,
        bool? active,
        string? iconName,
        bool updateIcon,
        CancellationToken cancellationToken)
    {
        connection.Open();
        using var transaction = connection.BeginTransaction();
        try
        {
            var existingId = await connection.QueryFirstOrDefaultAsync<int?>(new CommandDefinition(
                GroupQuery.GetIdForUpdate,
                new { id },
                transaction: transaction,
                cancellationToken: cancellationToken));

            if (!existingId.HasValue)
            {
                transaction.Rollback();
                return false;
            }

            if (name is not null)
            {
                await connection.ExecuteAsync(new CommandDefinition(
                    GroupQuery.UpdateName,
                    new { id, name },
                    transaction: transaction,
                    cancellationToken: cancellationToken));
            }

            if (active.HasValue)
            {
                await connection.ExecuteAsync(new CommandDefinition(
                    GroupQuery.UpdateActive,
                    new { id, active = active.Value },
                    transaction: transaction,
                    cancellationToken: cancellationToken));
            }

            if (updateIcon)
            {
                await connection.ExecuteAsync(new CommandDefinition(
                    GroupQuery.UpdateIcon,
                    new { id, iconName },
                    transaction: transaction,
                    cancellationToken: cancellationToken));
            }

            transaction.Commit();
            return true;
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            logger.LogError(ex, "Erro ao atualizar campos administrativos do grupo {GroupId}", id);
            throw;
        }
        finally
        {
            connection.Close();
        }
    }

    public async Task AddCapabilityToGroupAsync(int groupId, CapabilityGroup capability, CancellationToken cancellationToken)
    {
        var command = new CommandDefinition(GroupQuery.InsertCapabilityForGroup, new
        {
            groupId,
            capabilityId = capability.Id
        }, cancellationToken: cancellationToken);
        capability.Id = await connection.ExecuteAsync(command);
    }

    public async Task DeleteCapabilityForGroupAsync(int groupId, int capabilityId, CancellationToken cancellationToken)
    {
        var command = new CommandDefinition(GroupQuery.DeleteCapabilityForGroup, new { groupId, capabilityId }, cancellationToken: cancellationToken);
        await connection.ExecuteAsync(command);
    }

    async Task<IEnumerable<Group>> GetAllAsync(CommandDefinition command, CancellationToken cancellationToken)
    {
        List<Group> groups = new List<Group>();
        await connection.QueryAsync<Group, Capability?, IconGroup?, Group>(
            command,
            (group, capability, icon) =>
            {
                logger.LogInformation($"Mapeando grupo: {group.Name}, Capacidade: {capability?.Name}, Ícone: {icon?.Name}");
                if (!groups.Any(g => g.Id == group.Id))
                {
                    logger.LogInformation($"Adicionando novo grupo: {group.Name}");
                    groups.Add(group);
                }

                if (capability != null)
                {
                    logger.LogInformation($"Adicionando capacidade {capability.Name} ao grupo {group.Name}");
                    var existingGroup = groups.First(g => g.Id == group.Id);
                    existingGroup.AddCapability(capability);
                }
                if (!string.IsNullOrWhiteSpace(icon?.Name))
                {
                    logger.LogInformation($"Definindo ícone {icon.Name} para o grupo {group.Name}");
                    group.Icon = icon;
                }

                return group;
            },
            splitOn: "Id"
        );

        return groups;
    }
}
