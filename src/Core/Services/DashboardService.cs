using Core.Contracts.Repositories;
using Core.Contracts.Services;
using Core.Entities;
using Core.Exceptions;
using Microsoft.Extensions.Logging;

namespace Core.Services;

internal class DashboardService(ILogger<DashboardService> logger, IDashboardRepository repository) : IDashboardService
{
    public Task<IEnumerable<Dashboard>> GetAllAsync(CancellationToken cancellationToken) => repository.GetAllAsync(cancellationToken);
    public async Task<Dashboard> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        ValidateId(id);
        return await repository.GetByIdAsync(id, cancellationToken) ?? throw Missing("DASHBOARD_NOT_FOUND");
    }
    public async Task<Dashboard> AddAsync(DashboardRequest request, CancellationToken cancellationToken)
    {
        Dashboard dashboard = new();
        dashboard.Update(request, create: true);
        await repository.AddAsync(dashboard, cancellationToken);
        logger.LogInformation("Dashboard {DashboardId} criado", dashboard.Id);
        return dashboard;
    }
    public async Task<Dashboard> UpdateAsync(long id, DashboardRequest request, CancellationToken cancellationToken)
    {
        ValidateId(id);
        var dashboard = await repository.UpdateAsync(id, request, cancellationToken) ?? throw Missing("DASHBOARD_NOT_FOUND");
        logger.LogInformation("Dashboard {DashboardId} atualizado", id);
        return dashboard;
    }
    public async Task DeleteAsync(long id, CancellationToken cancellationToken)
    {
        ValidateId(id);
        if (!await repository.DeleteAsync(id, cancellationToken)) throw Missing("DASHBOARD_NOT_FOUND");
        logger.LogInformation("Dashboard {DashboardId} excluído", id);
    }
    public Task<DashboardWidget> AddWidgetAsync(long dashboardId, DashboardWidgetRequest request, CancellationToken cancellationToken)
    {
        ValidateId(dashboardId);
        return repository.AddWidgetAsync(dashboardId, request, cancellationToken);
    }
    public Task<DashboardWidget> UpdateWidgetAsync(long dashboardId, long widgetId, DashboardWidgetRequest request, CancellationToken cancellationToken)
    {
        ValidateId(dashboardId);
        ValidateId(widgetId);
        return repository.UpdateWidgetAsync(dashboardId, widgetId, request, cancellationToken);
    }
    public async Task DeleteWidgetAsync(long dashboardId, long widgetId, CancellationToken cancellationToken)
    {
        ValidateId(dashboardId);
        ValidateId(widgetId);
        if (!await repository.DeleteWidgetAsync(dashboardId, widgetId, cancellationToken)) throw Missing("WIDGET_NOT_FOUND");
    }
    public Task<IEnumerable<DashboardWidgetType>> GetWidgetTypesAsync(CancellationToken cancellationToken) => repository.GetWidgetTypesAsync(cancellationToken);
    public Task<IEnumerable<DashboardCapability>> GetCapabilitiesAsync(CancellationToken cancellationToken) => repository.GetCapabilitiesAsync(cancellationToken);
    public async Task<DashboardCapability> GetCapabilityByIdAsync(int id, CancellationToken cancellationToken)
    {
        ValidateId(id);
        return await repository.GetCapabilityByIdAsync(id, cancellationToken) ?? throw Missing("CAPABILITY_NOT_FOUND");
    }
    private static void ValidateId(long id)
    {
        if (id <= 0) throw new DashboardExceptionDomain("INVALID_REQUEST", "O id deve ser positivo.", "id");
    }
    private static DashboardExceptionDomain Missing(string code) => new(code, "Recurso não encontrado.");
}
