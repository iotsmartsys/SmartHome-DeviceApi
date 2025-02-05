using System.Data;
using Core.Contracts.Repositories;
using Core.Entities;
using Dapper;
using Microsoft.Extensions.Logging;

namespace Data.Repositories;

internal class PropertyRepository(ILogger<PropertyRepository> logger, IDbConnection connection) : IPropertyRepository
{
    public async Task AddAsync(string device_id, Property property, CancellationToken cancellationToken)
    {
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            int idDevice = await connection.ExecuteScalarAsync<int>(PropertyQuery.GetIdDevice, new { device_id }, transaction);
            if (idDevice == 0)
            {
                logger.LogWarning("Device {deviceId} not found", device_id);
                throw new NotFoundExceptionDomain($"Device {device_id} not found");
            }

            logger.LogInformation("Adicionando property {propertyName} para o device {deviceId}", property.Name, device_id);
            var command = new CommandDefinition(PropertyQuery.AddForDevice, new
            {
                deviceId = idDevice,
                name = property.Name,
                description = property.Description,
                value = property.Value
            },
            transaction: transaction,
            cancellationToken: cancellationToken);
            await connection.ExecuteAsync(command);

            logger.LogInformation("Property {propertyName} adicionada para o device {deviceId}", property.Name, device_id);
            transaction.Commit();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao adicionar property para o device {deviceId}", device_id);
            transaction.Rollback();
            throw;
        }
    }

    public async Task<IEnumerable<Property>> GetByDeviceAsync(string device_id, Criteria<Property>? criteria, CancellationToken cancellationToken)
    {
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            logger.LogInformation("Buscando properties para o device {deviceId}", device_id);
            var entity = criteria?.Entity;
            var command = new PropertyQueryBuilder(device_id)
                .WithCancellationToken(cancellationToken)
                .WithCriteria(criteria)
                .Build();
            var properties = await connection.QueryAsync<Property>(command);

            logger.LogInformation("Properties encontradas: {properties}", properties);
            return properties;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao buscar properties para o device {deviceId}", device_id);
            throw;
        }
    }

    public async Task<Property?> GetByIdAsync(string device_id, int id, CancellationToken cancellationToken)
    {
        connection.Open();
        try
        {
            var command = new PropertyQueryBuilder(device_id)
                .WithCancellationToken(cancellationToken)
                .WithId(id)
                .Build();

            logger.LogInformation("Buscando property para o device {deviceId}", device_id);
            var property = await connection.QueryFirstOrDefaultAsync<Property>(command);

            logger.LogInformation("Property encontrada: {property}", property);
            return property;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao buscar property para o device {deviceId}", device_id);
            throw;
        }
    }

    public async Task RemoveAsync(string device_id, Property property, CancellationToken cancellationToken)
    {
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            logger.LogInformation("Buscando device {deviceId}", device_id);
            var idDevice = await connection.ExecuteScalarAsync<int>(PropertyQuery.GetIdDevice, new { device_id }, transaction);
            logger.LogInformation("Removendo property {propertyName} para o device {deviceId}", property.Name, device_id);
            var command = new CommandDefinition(PropertyQuery.RemoveForDevice, new
            {
                deviceId = idDevice,
                id = property.Id
            },
            transaction: transaction,
            cancellationToken: cancellationToken);
            await connection.ExecuteAsync(command);

            logger.LogInformation("Property {propertyName} removida para o device {deviceId}", property.Name, device_id);
            transaction.Commit();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao remover property para o device {deviceId}", device_id);
            transaction.Rollback();
            throw;
        }
    }

    public async Task UpdateAsync(string device_id, Property property, CancellationToken cancellationToken)
    {
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            logger.LogInformation("Buscando device {deviceId}", device_id);
            var idDevice = await connection.ExecuteScalarAsync<int>(PropertyQuery.GetIdDevice, new { device_id }, transaction);
            logger.LogInformation("Atualizando property {propertyName} para o device {deviceId}", property.Name, device_id);
            var command = new CommandDefinition(PropertyQuery.UpdateForDevice, new
            {
                id = property.Id,
                deviceId = idDevice,
                name = property.Name,
                description = property.Description,
                value = property.Value
            },
            transaction: transaction,
            cancellationToken: cancellationToken);
            await connection.ExecuteAsync(command);

            logger.LogInformation("Property {propertyName} atualizada para o device {deviceId}", property.Name, device_id);
            transaction.Commit();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao atualizar property para o device {deviceId}", device_id);
            transaction.Rollback();
            throw;
        }
    }
}
