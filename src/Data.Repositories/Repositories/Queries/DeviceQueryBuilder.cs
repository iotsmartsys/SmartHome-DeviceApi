using System.Data;
using Core.Contracts.Repositories;
using Dapper;

namespace Data.Repositories;

internal class DeviceQueryBuilder(string sql = DeviceQuery.GetAllDevices)
{
    private string? _device_id;
    private string? _name;
    private string? _description;
    private bool? _is_active;
    private string[]? _platform;
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

    public DeviceQueryBuilder WithIsActive(bool is_active)
    {
        _is_active = is_active;
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
        if (_platform != null && _platform.Length > 0)
            sql += " AND d.Platform IN @platform";
        if (_is_active.HasValue)
            sql += " AND d.Active = @is_active";

        return new CommandDefinition(sql, new
        {
            device_id = _device_id,
            name = _name,
            description = _description,
            platform = _platform,
            is_active = _is_active
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
            _platform = find.Platform.Split(',');
        if (find.IsActive.HasValue)
            _is_active = find.IsActive.Value;

        return this;
    }
}