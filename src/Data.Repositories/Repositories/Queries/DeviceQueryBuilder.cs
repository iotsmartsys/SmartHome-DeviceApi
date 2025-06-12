using System.Data;
using Dapper;

namespace Data.Repositories;

internal class DeviceQueryBuilder(string sql = DeviceQuery.GetAllDevices)
{
    private string? _device_id;
    private string? _name;
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

        return new CommandDefinition(sql, new { device_id = _device_id, name = _name }, transaction: _transaction, cancellationToken: _cancellationToken);
    }

}