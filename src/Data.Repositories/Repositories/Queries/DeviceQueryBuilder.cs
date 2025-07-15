using System.Data;
using Core.Contracts.Repositories;
using Core.Entities;
using Dapper;

namespace Data.Repositories;

internal class DeviceQueryBuilder(string sql = DeviceQuery.GetAllDevices)
{
    private string? _device_id;
    private string? _name;
    private string? _description;
    private string? _platform;
    private IDbTransaction? _transaction;
    private CancellationToken _cancellationToken = default;

    public DeviceQueryBuilder WithDeviceId(string device_id)
    {
        _device_id = device_id;
        return this;
    }

    public DeviceQueryBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public DeviceQueryBuilder WithTransaction(IDbTransaction transaction)
    {
        _transaction = transaction;
        return this;
    }

    public DeviceQueryBuilder WithCancellationToken(CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;
        return this;
    }

    public CommandDefinition Build()
    {
        if (!string.IsNullOrEmpty(_device_id))
            sql += " AND d.DeviceId = @device_id";
        if (!string.IsNullOrEmpty(_name))
            sql += " AND d.Name = @name";
        if (!string.IsNullOrEmpty(_description))
            sql += " AND d.Description = @description";
        if (!string.IsNullOrEmpty(_platform))
            sql += " AND d.Platform = @platform";

        return new CommandDefinition(sql, new
        {
            device_id = _device_id,
            name = _name,
            description = _description,
            platform = _platform
        }, transaction: _transaction, cancellationToken: _cancellationToken);
    }

    internal DeviceQueryBuilder WithFind(DeviceFind? find)
    {
        if (find == null) return this;

        if (!string.IsNullOrEmpty(find.DeviceId))
            _device_id = find.DeviceId;
        if (!string.IsNullOrEmpty(find.Name))
            _name = find.Name;
        if (!string.IsNullOrEmpty(find.Description))
            _description = find.Description;
        if (!string.IsNullOrEmpty(find.Platform))
            _platform = find.Platform;

        return this;
    }
}