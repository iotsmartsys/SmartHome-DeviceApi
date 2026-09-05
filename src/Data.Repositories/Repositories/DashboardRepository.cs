using System.Data;
using Core.Contracts.Repositories;
using Core.Entities;
using Dapper;

namespace Data.Repositories;

internal sealed class DashboardRepository(IDbConnection connection) : IDashboardRepository
{
    private IDbTransaction? transaction;
    private CommandDefinition Command(string sql, object? parameters, CancellationToken ct) =>
        new(sql, parameters, transaction, cancellationToken: ct);

    public async Task<T> WithWriteLockAsync<T>(Func<Task<T>> operation, CancellationToken ct)
    {
        if (transaction is not null) throw new InvalidOperationException("Nested Dashboard transaction.");
        if (connection.State != ConnectionState.Open) connection.Open();
        // READ COMMITTED makes each partial update observe the preceding committed writer.
        using var current = connection.BeginTransaction(IsolationLevel.ReadCommitted);
        transaction = current;
        try
        {
            var lockId = await connection.QuerySingleAsync<int>(Command(
                "SELECT id FROM dashboard_write_lock WHERE id=1 FOR UPDATE", null, ct));
            if (lockId != 1) throw new InvalidOperationException("Dashboard write lock missing.");
            var result = await operation();
            current.Commit();
            return result;
        }
        catch
        {
            current.Rollback();
            throw;
        }
        finally
        {
            transaction = null;
            connection.Close();
        }
    }

    public async Task<IReadOnlyList<Dashboard>> GetAllAsync(CancellationToken ct)
    {
        var dashboards = (await connection.QueryAsync<Dashboard>(Command(
            DashboardQuery.Dashboards + " ORDER BY display_order, id", null, ct))).ToArray();
        var widgets = await connection.QueryAsync<DashboardWidget>(Command(
            DashboardQuery.Widgets + " ORDER BY display_order, id", null, ct));
        var grouped = widgets.ToLookup(w => w.DashboardId);
        foreach (var dashboard in dashboards) dashboard.Widgets = grouped[dashboard.Id].ToList();
        return dashboards;
    }

    public async Task<Dashboard?> GetAsync(long id, CancellationToken ct)
    {
        var dashboard = await connection.QuerySingleOrDefaultAsync<Dashboard>(Command(
            DashboardQuery.Dashboards + " WHERE id=@id", new { id }, ct));
        if (dashboard is null) return null;
        dashboard.Widgets = (await connection.QueryAsync<DashboardWidget>(Command(
            DashboardQuery.Widgets + " WHERE dashboard_id=@id ORDER BY display_order,id", new { id }, ct))).ToList();
        return dashboard;
    }

    public async Task SaveAsync(Dashboard value, bool create, CancellationToken ct)
    {
        if (value.IsDefault)
            await connection.ExecuteAsync(Command("""
                UPDATE dashboards SET is_default=FALSE, updated_at=@now
                WHERE is_default=TRUE AND id<>@id
                """, new { id = value.Id, now = value.UpdatedAt ?? value.CreatedAt }, ct));
        if (create)
            value.Id = await connection.ExecuteScalarAsync<long>(Command("""
                INSERT INTO dashboards(name,description,layout_type,is_default,display_order,created_at,updated_at)
                VALUES(@Name,@Description,@LayoutType,@IsDefault,@DisplayOrder,@CreatedAt,@UpdatedAt);
                SELECT LAST_INSERT_ID();
                """, value, ct));
        else
            await connection.ExecuteAsync(Command("""
                UPDATE dashboards SET name=@Name,description=@Description,layout_type=@LayoutType,
                    is_default=@IsDefault,display_order=@DisplayOrder,updated_at=@UpdatedAt WHERE id=@Id
                """, value, ct));
    }

    public async Task SaveWidgetAsync(DashboardWidget value, bool create, CancellationToken ct)
    {
        if (create)
            value.Id = await connection.ExecuteScalarAsync<long>(Command("""
                INSERT INTO dashboard_widgets(dashboard_id,capability_id,title,widget_type,data_mode,
                    position_x,position_y,width,height,config_json,refresh_interval_seconds,display_order,created_at,updated_at)
                VALUES(@DashboardId,@CapabilityId,@Title,@WidgetType,@DataMode,@X,@Y,@Width,@Height,
                    @ConfigJson,@RefreshIntervalSeconds,@DisplayOrder,@CreatedAt,@UpdatedAt);
                SELECT LAST_INSERT_ID();
                """, value, ct));
        else
            await connection.ExecuteAsync(Command("""
                UPDATE dashboard_widgets SET capability_id=@CapabilityId,title=@Title,widget_type=@WidgetType,
                    data_mode=@DataMode,position_x=@X,position_y=@Y,width=@Width,height=@Height,config_json=@ConfigJson,
                    refresh_interval_seconds=@RefreshIntervalSeconds,display_order=@DisplayOrder,updated_at=@UpdatedAt
                WHERE id=@Id AND dashboard_id=@DashboardId
                """, value, ct));
    }

    public async Task DeleteAsync(long id, CancellationToken ct) =>
        await connection.ExecuteAsync(Command("DELETE FROM dashboards WHERE id=@id", new { id }, ct));
    public async Task DeleteWidgetAsync(long dashboardId, long widgetId, CancellationToken ct) =>
        await connection.ExecuteAsync(Command("DELETE FROM dashboard_widgets WHERE id=@widgetId AND dashboard_id=@dashboardId",
            new { dashboardId, widgetId }, ct));
    public async Task<IReadOnlyList<DashboardWidgetType>> GetWidgetTypesAsync(CancellationToken ct) =>
        (await connection.QueryAsync<DashboardWidgetType>(Command(DashboardQuery.Types, null, ct)))
        .OrderBy(t => t.Code, StringComparer.Ordinal).ToArray();
    public async Task<IReadOnlyList<DashboardCapabilitySource>> GetSourcesAsync(CancellationToken ct) =>
        (await connection.QueryAsync<DashboardCapabilitySource>(Command(DashboardQuery.Sources + " ORDER BY c.Id", null, ct))).ToArray();
    public Task<DashboardCapabilitySource?> GetSourceAsync(int id, CancellationToken ct) =>
        connection.QuerySingleOrDefaultAsync<DashboardCapabilitySource>(Command(DashboardQuery.Sources + " WHERE c.Id=@id" + (transaction is null ? "" : " LOCK IN SHARE MODE"), new { id }, ct));
    public Task<bool> DeviceExistsAsync(string deviceId, CancellationToken ct) =>
        connection.ExecuteScalarAsync<bool>(Command("SELECT EXISTS(SELECT 1 FROM Devices WHERE BINARY DeviceId=BINARY @deviceId)", new { deviceId }, ct));
}
