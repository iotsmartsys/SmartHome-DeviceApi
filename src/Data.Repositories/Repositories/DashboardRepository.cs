using System.Data;
using System.Text.Json;
using Core.Contracts.Repositories;
using Core.Contracts.Services;
using Core.Entities;
using Core.Exceptions;
using Dapper;
using Microsoft.Extensions.Logging;

namespace Data.Repositories;

internal class DashboardRepository(ILogger<DashboardRepository> logger, IDbConnection connection) : IDashboardRepository
{
    private IDbTransaction? transaction;

    public async Task<IEnumerable<Dashboard>> GetAllAsync(CancellationToken cancellationToken)
    {
        var command = new CommandDefinition(DashboardQuery.GetAllDashboards, transaction: transaction, cancellationToken: cancellationToken);
        var dashboards = (await connection.QueryAsync<Dashboard>(command)).ToArray();
        var widgets = await connection.QueryAsync<DashboardWidgetRow>(new CommandDefinition(
            DashboardWidgetQuery.GetAllWidgets, transaction: transaction, cancellationToken: cancellationToken));
        var grouped = widgets.Select(MapWidget).ToLookup(widget => widget.DashboardId);
        foreach (var dashboard in dashboards) dashboard.Widgets = grouped[dashboard.Id].ToArray();
        return dashboards;
    }

    public async Task<Dashboard?> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        var command = new CommandDefinition(DashboardQuery.GetById, new { id }, transaction: transaction, cancellationToken: cancellationToken);
        var dashboard = await connection.QuerySingleOrDefaultAsync<Dashboard>(command);
        if (dashboard is null) return null;
        var widgets = await connection.QueryAsync<DashboardWidgetRow>(new CommandDefinition(
            DashboardWidgetQuery.GetByDashboardId, new { dashboardId = id }, transaction: transaction, cancellationToken: cancellationToken));
        dashboard.Widgets = widgets.Select(MapWidget).ToArray();
        return dashboard;
    }

    public async Task AddAsync(Dashboard dashboard, CancellationToken cancellationToken)
    {
        using var current = await BeginWriteAsync(cancellationToken);
        try
        {
            await ClearDefaultAsync(dashboard, cancellationToken);
            var command = new CommandDefinition(DashboardQuery.Insert, dashboard, transaction: current, cancellationToken: cancellationToken);
            dashboard.Id = await connection.ExecuteScalarAsync<long>(command);
            current.Commit();
        }
        catch (Exception exception)
        {
            Rollback(current);
            logger.LogError(exception, "Erro ao persistir Dashboard");
            throw;
        }
        finally
        {
            transaction = null;
            connection.Close();
        }
    }

    public async Task<Dashboard?> UpdateAsync(long id, DashboardRequest request, CancellationToken cancellationToken)
    {
        using var current = await BeginWriteAsync(cancellationToken);
        try
        {
            var dashboard = await GetByIdAsync(id, cancellationToken);
            if (dashboard is not null && dashboard.Update(request))
            {
                await ClearDefaultAsync(dashboard, cancellationToken);
                var command = new CommandDefinition(DashboardQuery.Update, dashboard, transaction: current, cancellationToken: cancellationToken);
                await connection.ExecuteAsync(command);
            }
            current.Commit();
            return dashboard;
        }
        catch (Exception exception)
        {
            Rollback(current);
            logger.LogError(exception, "Erro ao persistir Dashboard");
            throw;
        }
        finally
        {
            transaction = null;
            connection.Close();
        }
    }

    public async Task<bool> DeleteAsync(long id, CancellationToken cancellationToken)
    {
        using var current = await BeginWriteAsync(cancellationToken);
        try
        {
            var command = new CommandDefinition(DashboardQuery.Delete, new { id }, transaction: current, cancellationToken: cancellationToken);
            var affectedRows = await connection.ExecuteAsync(command);
            current.Commit();
            return affectedRows > 0;
        }
        catch (Exception exception)
        {
            Rollback(current);
            logger.LogError(exception, "Erro ao persistir Dashboard");
            throw;
        }
        finally
        {
            transaction = null;
            connection.Close();
        }
    }

    public async Task<DashboardWidget> AddWidgetAsync(long dashboardId, DashboardWidgetRequest request, CancellationToken cancellationToken)
    {
        using var current = await BeginWriteAsync(cancellationToken);
        try
        {
            var widget = await PrepareWidgetAsync(dashboardId, null, request, cancellationToken);
            var command = new CommandDefinition(DashboardWidgetQuery.Insert, WidgetParameters(widget), transaction: current, cancellationToken: cancellationToken);
            widget.Id = await connection.ExecuteScalarAsync<long>(command);
            current.Commit();
            return widget;
        }
        catch (Exception exception)
        {
            Rollback(current);
            logger.LogError(exception, "Erro ao persistir Dashboard");
            throw;
        }
        finally
        {
            transaction = null;
            connection.Close();
        }
    }

    public async Task<DashboardWidget> UpdateWidgetAsync(long dashboardId, long widgetId, DashboardWidgetRequest request, CancellationToken cancellationToken)
    {
        using var current = await BeginWriteAsync(cancellationToken);
        try
        {
            var widget = await PrepareWidgetAsync(dashboardId, widgetId, request, cancellationToken);
            // Unchanged values retain UpdatedAt; the SQL assignment cannot erase omitted fields.
            var command = new CommandDefinition(DashboardWidgetQuery.Update, WidgetParameters(widget), transaction: current, cancellationToken: cancellationToken);
            await connection.ExecuteAsync(command);
            current.Commit();
            return widget;
        }
        catch (Exception exception)
        {
            Rollback(current);
            logger.LogError(exception, "Erro ao persistir Dashboard");
            throw;
        }
        finally
        {
            transaction = null;
            connection.Close();
        }
    }

    public async Task<bool> DeleteWidgetAsync(long dashboardId, long widgetId, CancellationToken cancellationToken)
    {
        using var current = await BeginWriteAsync(cancellationToken);
        try
        {
            if (await GetByIdAsync(dashboardId, cancellationToken) is null) throw Missing("DASHBOARD_NOT_FOUND");
            var command = new CommandDefinition(DashboardWidgetQuery.Delete, new { dashboardId, widgetId }, transaction: current, cancellationToken: cancellationToken);
            var affectedRows = await connection.ExecuteAsync(command);
            current.Commit();
            return affectedRows > 0;
        }
        catch (Exception exception)
        {
            Rollback(current);
            logger.LogError(exception, "Erro ao persistir Dashboard");
            throw;
        }
        finally
        {
            transaction = null;
            connection.Close();
        }
    }

    private async Task<DashboardWidget> PrepareWidgetAsync(long dashboardId, long? widgetId, DashboardWidgetRequest request, CancellationToken cancellationToken)
    {
        var dashboard = await GetByIdAsync(dashboardId, cancellationToken) ?? throw Missing("DASHBOARD_NOT_FOUND");
        var widget = widgetId.HasValue
            ? dashboard.Widgets.SingleOrDefault(widget => widget.Id == widgetId.Value) ?? throw Missing("WIDGET_NOT_FOUND")
            : new DashboardWidget { DashboardId = dashboardId };
        var capabilityId = !widgetId.HasValue || request.CapabilityIdSpecified ? request.CapabilityId : widget.CapabilityId;
        var widgetType = !widgetId.HasValue || request.WidgetTypeSpecified ? request.WidgetType : widget.WidgetType;
        if (capabilityId is null or <= 0) throw new DashboardExceptionDomain("INVALID_REQUEST", "capabilityId deve ser positivo.", "capabilityId");
        if (string.IsNullOrEmpty(widgetType)) throw new DashboardExceptionDomain("INVALID_REQUEST", "widgetType é obrigatório.", "widgetType");
        var type = (await GetWidgetTypesAsync(cancellationToken)).SingleOrDefault(type => type.Code == widgetType) ?? throw Missing("WIDGET_TYPE_NOT_FOUND");
        if (!type.Enabled) throw new DashboardExceptionDomain("WIDGET_TYPE_DISABLED", "Tipo de widget desabilitado.", "widgetType");
        var source = await GetCapabilityByIdAsync(capabilityId.Value, cancellationToken) ?? throw Missing("CAPABILITY_NOT_FOUND");
        if (request.DeviceIdSpecified && request.DeviceId is not null)
        {
            var command = new CommandDefinition(DashboardCapabilityQuery.DeviceExists, new { deviceId = request.DeviceId }, transaction: transaction, cancellationToken: cancellationToken);
            if (!await connection.ExecuteScalarAsync<bool>(command)) throw Missing("DEVICE_NOT_FOUND");
            if (!string.Equals(request.DeviceId, source.DeviceId, StringComparison.Ordinal))
                throw new DashboardExceptionDomain("DEVICE_CAPABILITY_MISMATCH", "Device não pertence à capability.", "deviceId");
        }
        widget.Update(request, type, source, create: !widgetId.HasValue);
        return widget;
    }

    private async Task<IDbTransaction> BeginWriteAsync(CancellationToken cancellationToken)
    {
        if (connection.State != ConnectionState.Open) connection.Open();
        transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);
        try
        {
            var command = new CommandDefinition(DashboardQuery.GetWriteLock, transaction: transaction, cancellationToken: cancellationToken);
            await connection.QuerySingleAsync<int>(command);
            return transaction;
        }
        catch
        {
            Rollback(transaction);
            transaction.Dispose();
            transaction = null;
            connection.Close();
            throw;
        }
    }

    private static void Rollback(IDbTransaction current)
    {
        try { current.Rollback(); }
        catch (System.Data.Common.DbException) { } // Preserve the original error if the connection was lost.
    }

    private async Task ClearDefaultAsync(Dashboard dashboard, CancellationToken cancellationToken)
    {
        if (!dashboard.IsDefault) return;
        var command = new CommandDefinition(DashboardQuery.RemoveDefault,
            new { id = dashboard.Id, updatedAt = dashboard.UpdatedAt ?? dashboard.CreatedAt }, transaction: transaction, cancellationToken: cancellationToken);
        await connection.ExecuteAsync(command);
    }

    public async Task<IEnumerable<DashboardWidgetType>> GetWidgetTypesAsync(CancellationToken cancellationToken)
    {
        var command = new CommandDefinition(DashboardWidgetTypeQuery.GetAllWidgetTypes, transaction: transaction, cancellationToken: cancellationToken);
        var rows = await connection.QueryAsync<DashboardWidgetTypeRow>(command);
        return rows.Select(row =>
        {
            row.CompatibleDataTypes = JsonSerializer.Deserialize<string[]>(row.CompatibleDataTypesJson)!;
            row.DefaultConfig = ReadConfig(row.DefaultConfigJson);
            return (DashboardWidgetType)row;
        }).OrderBy(type => type.Code, StringComparer.Ordinal).ToArray();
    }

    public Task<IEnumerable<DashboardCapability>> GetCapabilitiesAsync(CancellationToken cancellationToken)
    {
        var command = new CommandDefinition(DashboardCapabilityQuery.GetOrdered, transaction: transaction, cancellationToken: cancellationToken);
        return connection.QueryAsync<DashboardCapability>(command);
    }

    public Task<DashboardCapability?> GetCapabilityByIdAsync(int id, CancellationToken cancellationToken)
    {
        var command = new CommandDefinition(transaction is null ? DashboardCapabilityQuery.GetById : DashboardCapabilityQuery.GetByIdForUpdate,
            new { id }, transaction: transaction, cancellationToken: cancellationToken);
        return connection.QuerySingleOrDefaultAsync<DashboardCapability>(command);
    }

    private static DashboardExceptionDomain Missing(string code) => new(code, "Recurso não encontrado.");

    private static DashboardWidget MapWidget(DashboardWidgetRow row)
    {
        try { row.Config = ReadConfig(row.ConfigJson); }
        catch (Exception exception) when (exception is JsonException or ArgumentException or InvalidOperationException or FormatException or OverflowException)
        {
            row.Config = new DashboardWidgetConfig();
            row.ConfigurationInvalid = true;
        }
        return row;
    }

    private static object WidgetParameters(DashboardWidget widget) => new
    {
        widget.Id, widget.DashboardId, widget.CapabilityId, widget.Title, widget.WidgetType, widget.DataMode,
        widget.X, widget.Y, widget.Width, widget.Height, ConfigJson = WriteConfig(widget.Config),
        widget.RefreshIntervalSeconds, widget.DisplayOrder, widget.CreatedAt, widget.UpdatedAt
    };

    private static string WriteConfig(DashboardWidgetConfig config) => JsonSerializer.Serialize(
        config.Fields.ToDictionary(field => JsonNamingPolicy.CamelCase.ConvertName(field.ToString()), field => config.GetValue(field)));

    private static DashboardWidgetConfig ReadConfig(string json)
    {
        using var document = JsonDocument.Parse(json);
        var config = new DashboardWidgetConfig();
        foreach (var property in document.RootElement.EnumerateObject())
        {
            var field = Enum.Parse<DashboardConfigField>(property.Name, ignoreCase: true);
            object? value = property.Value.ValueKind == JsonValueKind.Null ? null : field switch
            {
                DashboardConfigField.Min or DashboardConfigField.Max or DashboardConfigField.WarningFrom or DashboardConfigField.DangerFrom => property.Value.GetDouble(),
                DashboardConfigField.Decimals => property.Value.GetInt32(),
                DashboardConfigField.ShowLastUpdated or DashboardConfigField.InvertState => property.Value.GetBoolean(),
                _ => property.Value.GetString()
            };
            config.SetValue(field, value);
        }
        return config;
    }

    private class DashboardWidgetRow : DashboardWidget
    {
        public string ConfigJson { get; set; } = "{}";
    }
    private class DashboardWidgetTypeRow : DashboardWidgetType
    {
        public string CompatibleDataTypesJson { get; set; } = "[]";
        public string DefaultConfigJson { get; set; } = "{}";
    }
}
