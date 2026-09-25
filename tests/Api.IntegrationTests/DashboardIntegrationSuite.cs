using System.Text.Json;

namespace Api.IntegrationTests;

internal sealed class DashboardIntegrationSuite(DashboardApiClient api, ManifestStore store)
{
    private RunManifest State => store.Manifest;
    private string DashboardPath(long id) => $"api/v1/dashboards/{id}";
    private string WidgetPath(long dashboardId, long id) => $"{DashboardPath(dashboardId)}/widgets/{id}";
    private CapabilityResponse capability = default!;
    private WidgetTypeResponse type = default!;
    private DashboardResponse dashboard = default!;
    private DashboardResponse otherDashboard = default!;
    private WidgetResponse widget = default!;

    internal async Task RunAsync(int? capabilityId)
    {
        State.Status = "Running";
        store.Save();
        await ScenarioAsync("IT-01", () => PrepareAsync(capabilityId));
        await ScenarioAsync("IT-02", CreateDashboardsAsync);
        await ScenarioAsync("IT-03", CreateWidgetsAsync);
        await ScenarioAsync("IT-04", QueryAsync);
        await ScenarioAsync("IT-05", UpdateDashboardAsync);
        await ScenarioAsync("IT-06", UpdateWidgetAsync);
        await ScenarioAsync("IT-07", ConfigureWidgetAsync);
        await ScenarioAsync("IT-08", ValidateErrorsAsync);
        await ScenarioAsync("IT-09", ConcurrentUpdatesAndResetAsync);
        State.Status = "Passed; cleanup pending";
        store.Save();
        Console.WriteLine("9 cenários aprovados. Dados preservados; execute cleanup separadamente quando desejar.");
    }

    private async Task PrepareAsync(int? capabilityId)
    {
        var types = (await api.GetAsync<ItemsResponse<WidgetTypeResponse>>("api/v1/dashboard-widget-types")).Items;
        Check.That(types.Select(item => item.Code).SequenceEqual(new[] { "gauge", "line_chart", "state_icon", "status_card", "value_card" }), "Catálogo inicial/ordenação divergente.");
        foreach (var item in types)
        {
            Check.Equal(item.Code != "line_chart", item.Enabled, "Habilitação do catálogo divergente.");
            Check.Equal(item.Code == "line_chart" ? "planned" : "available", item.Lifecycle, "Lifecycle divergente.");
            Check.Equal(item.Code == "line_chart" ? "history" : "current_value", item.DefaultDataMode, "Modo do catálogo divergente.");
            Check.Equal(JsonValueKind.Object, item.DefaultConfig.ValueKind, "defaultConfig deve ser objeto.");
        }
        var capabilities = (await api.GetAsync<ItemsResponse<CapabilityResponse>>("api/v1/dashboard-capabilities")).Items;
        Check.That(capabilities.Select(item => item.CapabilityId).SequenceEqual(capabilities.Select(item => item.CapabilityId).Order()), "Capabilities fora de ordem.");
        foreach (var item in capabilities)
        {
            Check.That(item.CapabilityId > 0, "CapabilityId inválido.");
            Check.Reading(item.Status, item.DataType, item.CurrentValue);
            if (item.LastUpdatedAt is not null) Check.Utc(item.LastUpdatedAt);
            var expected = types.Where(candidate => item.DataType is not null && candidate.Enabled &&
                candidate.DefaultDataMode == "current_value" && candidate.CompatibleDataTypes.Contains(item.DataType))
                .Select(candidate => candidate.Code).Order(StringComparer.Ordinal);
            Check.That(item.CompatibleWidgets.SequenceEqual(expected), "Compatibilidade da listagem divergente.");
        }
        var candidates = capabilities.Where(item => item.CompatibleWidgets.Length > 0 && !string.IsNullOrWhiteSpace(item.DeviceId));
        capability = (capabilityId.HasValue ? candidates.SingleOrDefault(item => item.CapabilityId == capabilityId)
            : candidates.OrderByDescending(item => item.DataType == "numeric").ThenBy(item => item.CapabilityId).FirstOrDefault())
            ?? throw new TestFailureException("Pré-condição: forneça uma capability existente, com DeviceId textual e tipo compatível; nenhuma fonte será criada/alterada.");
        var compatible = await api.GetAsync<CompatibleWidgetsResponse>($"api/v1/dashboard-capabilities/{capability.CapabilityId}/compatible-widgets");
        Check.Equal(capability.CapabilityId, compatible.CapabilityId, "CapabilityId divergente na consulta individual.");
        Check.Equal(capability.DataType, compatible.DataType, "DataType divergente.");
        Check.Equal(capability.CapabilityCode, compatible.CapabilityCode, "CapabilityCode divergente.");
        Check.That(compatible.CompatibleWidgets.Select(item => item.Code).SequenceEqual(capability.CompatibleWidgets), "Rotas de compatibilidade divergem.");
        foreach (var candidate in compatible.CompatibleWidgets)
            Check.JsonEqual(types.Single(item => item.Code == candidate.Code), candidate, "Catálogo e tipos compatíveis divergem.");
        type = compatible.CompatibleWidgets.FirstOrDefault(item => item.Code == "gauge") ?? compatible.CompatibleWidgets.First();
        State.CapabilityId = capability.CapabilityId;
        store.Save();
        Console.WriteLine($"Fixture: capabilityId={capability.CapabilityId}; widgetType={type.Code}.");
    }

    private async Task CreateDashboardsAsync()
    {
        foreach (var fixture in State.Dashboards)
        {
            var created = await api.PostAsync<DashboardResponse>("api/v1/dashboards",
                new DashboardCreateRequest(fixture.Name(State.RunId), fixture.Marker(State.RunId)));
            if (created.Body.Id > 0)
            {
                fixture.Id = created.Body.Id;
                store.Save();
            }
            Check.That(created.Body.Id > 0, "DashboardId deve ser positivo.");
            api.Location(created.Location, DashboardPath(created.Body.Id));
            Check.Equal(fixture.Name(State.RunId), created.Body.Name, "Nome não persistido.");
            Check.That(fixture.Owns(created.Body, State.RunId), "Marcador da fixture não persistido.");
            Check.Equal("grid", created.Body.LayoutType, "Layout default divergente.");
            Check.That(!created.Body.IsDefault && created.Body.DisplayOrder == 0 && created.Body.Widgets.Length == 0 && created.Body.UpdatedAt is null, "Defaults de criação divergentes.");
            Check.Utc(created.Body.CreatedAt);
            if (fixture.Key == "primary") dashboard = created.Body;
            else otherDashboard = created.Body;
        }
        Check.That(dashboard.Id != otherDashboard.Id, "Dashboards devem ter IDs distintos.");
    }

    private async Task CreateWidgetsAsync()
    {
        var fixture = State.Dashboards.Single(item => item.Key == "primary");
        for (var index = 1; index <= 2; index++)
        {
            var created = await api.PostAsync<WidgetResponse>($"{DashboardPath(dashboard.Id)}/widgets",
                new WidgetCreateRequest(capability.CapabilityId, capability.DeviceId, type.Code, $"Widget {index}"));
            if (created.Body.Id > 0)
            {
                fixture.WidgetIds.Add(created.Body.Id);
                store.Save();
            }
            api.Location(created.Location, DashboardPath(dashboard.Id));
            Check.That(created.Body.Id > 0 && fixture.WidgetIds.Distinct().Count() == fixture.WidgetIds.Count, "WidgetId inválido/repetido.");
            Check.Equal(dashboard.Id, created.Body.DashboardId, "DashboardId do widget divergente.");
            Check.Equal(capability.CapabilityId, created.Body.CapabilityId, "CapabilityId do widget divergente.");
            Check.Equal(capability.DeviceId, created.Body.DeviceId, "DeviceId deve preservar o identificador público textual.");
            Check.Equal(capability.CapabilityCode, created.Body.CapabilityCode, "CapabilityCode divergente.");
            Check.Equal(capability.DataType, created.Body.DataType, "DataType divergente.");
            Check.Equal(type.Code, created.Body.WidgetType, "WidgetType divergente.");
            Check.Equal($"Widget {index}", created.Body.Title, "Título divergente.");
            Check.Equal("current_value", created.Body.DataMode, "DataMode default divergente.");
            Check.Equal(new Position(0, 0, 1, 1), created.Body.Position, "Posição default divergente.");
            Check.JsonEqual(type.DefaultConfig, created.Body.Config, "Config deve conter todos os defaults, inclusive nulls.");
            Check.That(created.Body.RefreshIntervalSeconds is null && created.Body.DisplayOrder == 0 && created.Body.UpdatedAt is null, "Defaults de widget divergentes.");
            Check.Utc(created.Body.CreatedAt);
            if (index == 1) widget = created.Body;
        }
    }

    private async Task QueryAsync()
    {
        var list = (await api.GetAsync<ItemsResponse<DashboardSummary>>("api/v1/dashboards")).Items;
        Check.That(list.Select(item => (item.DisplayOrder, item.Id)).SequenceEqual(list.OrderBy(item => item.DisplayOrder).ThenBy(item => item.Id).Select(item => (item.DisplayOrder, item.Id))), "Dashboards fora de ordem.");
        Check.Equal(2, list.Single(item => item.Id == dashboard.Id).WidgetCount, "Contagem de widgets divergente.");
        Check.Equal(0, list.Single(item => item.Id == otherDashboard.Id).WidgetCount, "Contagem do segundo dashboard divergente.");
        var found = await api.GetAsync<DashboardResponse>(DashboardPath(dashboard.Id));
        Check.Equal(dashboard.Name, found.Name, "GET alterou nome.");
        Check.Equal(dashboard.Description, found.Description, "GET alterou descrição.");
        Check.Equal(dashboard.CreatedAt, found.CreatedAt, "CreatedAt deve ser imutável.");
        Check.That(found.Widgets.Select(item => item.Id).SequenceEqual(State.Dashboards[0].WidgetIds.Order()), "Widgets criados fora de ordem/ausentes.");
        Check.JsonEqual(widget, found.Widgets.Single(item => item.Id == widget.Id), "POST e GET do widget divergem.");
        var data = await api.GetAsync<DashboardDataResponse>($"{DashboardPath(dashboard.Id)}/data");
        Check.Equal(dashboard.Id, data.DashboardId, "Dados pertencem a outro dashboard.");
        Check.Equal(dashboard.Name, data.Name, "Nome dos dados divergente.");
        Check.Equal("grid", data.LayoutType, "Layout dos dados divergente.");
        Check.That(data.Widgets.Select(item => item.WidgetId).SequenceEqual(found.Widgets.Select(item => item.Id)), "Renderização omitiu/reordenou widget.");
        var generated = Check.Utc(data.GeneratedAt);
        foreach (var item in data.Widgets)
        {
            var saved = found.Widgets.Single(candidate => candidate.Id == item.WidgetId);
            Check.Equal(saved.Title, item.Title, "Título não preservado nos dados.");
            Check.Equal(saved.CapabilityId, item.CapabilityId, "CapabilityId não preservado.");
            Check.Equal(saved.DeviceId, item.DeviceId, "DeviceId não preservado.");
            Check.Equal(saved.Position, item.Position, "Posição não preservada.");
            Check.JsonEqual(saved.Config, item.Config, "Config não preservada na renderização.");
            Check.Reading(item.Status, item.DataType, item.Value);
            if (item.Status is not ("ok" or "stale")) Check.That(item.Label is null && item.Icon is null, "Falha não pode fabricar apresentação.");
            if (item.LastUpdatedAt is not null)
            {
                var updated = Check.Utc(item.LastUpdatedAt);
                if (item.Status == "ok") Check.That(generated >= updated && (generated - updated).TotalSeconds <= 300, "Idade de ok inválida.");
                if (item.Status == "stale") Check.That((generated - updated).TotalSeconds > 300, "Idade de stale inválida.");
            }
            else Check.That(item.Status is not ("ok" or "stale"), "Leitura utilizável sem timestamp.");
        }
    }

    private async Task UpdateDashboardAsync()
    {
        var original = await api.GetAsync<DashboardResponse>(DashboardPath(dashboard.Id));
        var fixture = State.Dashboards.Single(item => item.Key == "primary");
        var updated = await api.PutAsync<DashboardResponse>(DashboardPath(dashboard.Id), new DashboardUpdateRequest(fixture.Name(State.RunId, updated: true), 7));
        Check.Equal(fixture.Name(State.RunId, updated: true), updated.Name, "PUT não alterou nome.");
        Check.Equal(7, updated.DisplayOrder, "PUT não alterou ordem.");
        Check.Equal(original.Description, updated.Description, "Campo description omitido deve manter.");
        Check.Equal(original.CreatedAt, updated.CreatedAt, "PUT alterou CreatedAt.");
        Check.That(updated.UpdatedAt is not null && !updated.IsDefault, "Atualização deve registrar instante e preservar isDefault=false.");
        Check.Utc(updated.UpdatedAt!);
        Check.JsonEqual(original.Widgets, updated.Widgets, "PUT de dashboard alterou widgets.");
        Check.JsonEqual(updated, await api.GetAsync<DashboardResponse>(DashboardPath(dashboard.Id)), "PUT não persistiu o dashboard.");
        var unchanged = await api.PutAsync<DashboardResponse>(DashboardPath(dashboard.Id), new EmptyRequest());
        Check.JsonEqual(updated, unchanged, "PUT {} deve ser no-op, inclusive updatedAt.");
        var reset = await api.PutAsync<DashboardResponse>(DashboardPath(dashboard.Id), new DashboardResetRequest());
        Check.That(reset.DisplayOrder == 0 && !reset.IsDefault && reset.LayoutType == "grid", "Null não restaurou defaults.");
        Check.Equal(updated.Name, reset.Name, "Reset alterou nome omitido.");
        Check.Equal(updated.Description, reset.Description, "Reset alterou descrição omitida.");
        Check.JsonEqual(reset, await api.GetAsync<DashboardResponse>(DashboardPath(dashboard.Id)), "Reset não persistiu.");
        dashboard = reset;
    }

    private async Task UpdateWidgetAsync()
    {
        var updated = await api.PutAsync<WidgetResponse>(WidgetPath(dashboard.Id, widget.Id),
            new WidgetUpdateRequest("Widget alterado", new Position(3, 2, 4, 4), 60, 8));
        Check.That(updated.Title == "Widget alterado" && updated.RefreshIntervalSeconds == 60 && updated.DisplayOrder == 8, "PUT do widget não aplicou campos.");
        Check.Equal(new Position(3, 2, 4, 4), updated.Position, "Posição nos limites válidos não aceita.");
        Check.Equal(widget.CreatedAt, updated.CreatedAt, "PUT alterou CreatedAt do widget.");
        Check.Equal(widget.CapabilityId, updated.CapabilityId, "Campo omitido capabilityId foi alterado.");
        Check.JsonEqual(widget.Config, updated.Config, "Config omitida deve manter.");
        Check.That(updated.UpdatedAt is not null, "PUT efetivo deve definir updatedAt.");
        Check.Utc(updated.UpdatedAt!);
        Check.JsonEqual(updated, await ReadWidgetAsync(), "PUT do widget não persistiu.");
        var partial = await api.PutAsync<WidgetResponse>(WidgetPath(dashboard.Id, widget.Id), new WidgetPositionRequest(new PositionHeightRequest(null)));
        Check.Equal(new Position(3, 2, 4, 1), partial.Position, "Posição parcial deve manter membros omitidos e resetar height=null.");
        Check.Equal(updated.Title, partial.Title, "Posição parcial alterou título.");
        Check.JsonEqual(partial, await ReadWidgetAsync(), "Posição parcial não persistiu.");
        widget = partial;
    }

    private async Task ConfigureWidgetAsync()
    {
        var configured = await api.PutAsync<WidgetResponse>(WidgetPath(dashboard.Id, widget.Id), new WidgetConfigRequest<UnitConfigRequest>(new("IT")));
        Check.Equal("IT", configured.Config.GetProperty("unit").GetString(), "Override de unit não aplicado.");
        foreach (var member in type.DefaultConfig.EnumerateObject().Where(member => member.Name != "unit"))
            Check.JsonEqual(member.Value, configured.Config.GetProperty(member.Name), "Override deve completar demais defaults.");
        Check.JsonEqual(configured, await ReadWidgetAsync(), "Configuração não persistiu.");
        var defaults = await api.PutAsync<WidgetResponse>(WidgetPath(dashboard.Id, widget.Id), new WidgetConfigRequest<EmptyRequest>(new()));
        Check.JsonEqual(type.DefaultConfig, defaults.Config, "config={} deve partir dos defaults, não da config anterior.");
        if (type.Code == "gauge")
        {
            await api.PutAsync<WidgetResponse>(WidgetPath(dashboard.Id, widget.Id), new WidgetConfigRequest<GaugeConfigRequest>(new(50, 2)));
            var merge = await api.PutAsync<WidgetResponse>(WidgetPath(dashboard.Id, widget.Id), new WidgetConfigRequest<GaugeMaxRequest>(new(60)));
            Check.Equal(60d, merge.Config.GetProperty("max").GetDouble(), "max não alterado.");
            Check.Equal(1, merge.Config.GetProperty("decimals").GetInt32(), "Merge incorretamente reteve decimals anterior.");
            Check.JsonEqual(merge, await ReadWidgetAsync(), "Merge de gauge não persistiu.");
        }
        else
        {
            const string note = "Exemplo numérico de gauge não aplicável à capability selecionada; merge comum verificado.";
            State.Results.Single(item => item.Id == "IT-07").Note = note;
            Console.WriteLine("IT-07: " + note);
        }
        var reset = await api.PutAsync<WidgetResponse>(WidgetPath(dashboard.Id, widget.Id), new WidgetConfigRequest<UnitConfigRequest?>(null));
        Check.JsonEqual(type.DefaultConfig, reset.Config, "config=null não restaurou defaults.");
        Check.JsonEqual(reset, await ReadWidgetAsync(), "Reset de config não persistiu.");
        widget = reset;
    }

    private async Task ValidateErrorsAsync()
    {
        var path = DashboardPath(dashboard.Id);
        var before = await api.GetAsync<DashboardResponse>(path);
        await api.ErrorAsync(HttpMethod.Put, path, new DashboardNameRequest(" "), 400, "INVALID_DASHBOARD_NAME", "name");
        await api.ErrorAsync(HttpMethod.Put, path, new DashboardNameRequest(null), 400, "INVALID_DASHBOARD_NAME", "name");
        await api.ErrorAsync(HttpMethod.Put, path, new DashboardNumericNameRequest(123), 400, "INVALID_REQUEST", "name");
        await api.ErrorAsync(HttpMethod.Put, path, new DashboardLayoutRequest("list"), 400, "INVALID_LAYOUT_TYPE", "layoutType");
        await api.ErrorAsync(HttpMethod.Put, path, new UnknownFieldRequest("invalid"), 400, "INVALID_REQUEST", "unknown");
        await api.InvalidBodyAsync(path, "{", "application/json", 400, "INVALID_REQUEST");
        await api.InvalidBodyAsync(path, "null", "application/json", 400, "INVALID_REQUEST");
        await api.InvalidBodyAsync(path, "{}", "text/plain", 415, "UNSUPPORTED_MEDIA_TYPE");
        foreach (var id in new[] { "abc", "0", "-1", "9223372036854775808" })
            await api.ErrorAsync(HttpMethod.Get, $"api/v1/dashboards/{id}", null, 400, "INVALID_REQUEST");
        await api.ErrorAsync(HttpMethod.Post, $"{path}/widgets", new WidgetCreateRequest(capability.CapabilityId, capability.DeviceId, "line_chart", "Disabled"), 422, "WIDGET_TYPE_DISABLED");
        var widgetPath = WidgetPath(dashboard.Id, widget.Id);
        await api.ErrorAsync(HttpMethod.Put, WidgetPath(otherDashboard.Id, widget.Id), new EmptyRequest(), 404, "WIDGET_NOT_FOUND");
        await api.ErrorAsync(HttpMethod.Put, widgetPath, new WidgetModeRequest("history"), 400, "INVALID_DATA_MODE", "dataMode");
        foreach (var position in new[] { new Position(-1, 0, 1, 1), new Position(0, -1, 1, 1), new Position(0, 0, 0, 1), new Position(0, 0, 1, 5) })
            await api.ErrorAsync(HttpMethod.Put, widgetPath, new WidgetFullPositionRequest(position), 400, "INVALID_WIDGET_POSITION");
        foreach (var refresh in new[] { 0, 86401 })
            await api.ErrorAsync(HttpMethod.Put, widgetPath, new WidgetRefreshRequest(refresh), 400, "INVALID_REFRESH_INTERVAL", "refreshIntervalSeconds");
        await api.ErrorAsync(HttpMethod.Put, widgetPath, new WidgetConfigRequest<UnknownFieldRequest>(new("invalid")), 400, "INVALID_WIDGET_CONFIG");
        if (type.Code is "gauge" or "value_card")
            await api.ErrorAsync(HttpMethod.Put, widgetPath, new WidgetConfigRequest<DecimalsConfigRequest>(new(7)), 400, "INVALID_WIDGET_CONFIG", "config.decimals");
        Check.JsonEqual(before, await api.GetAsync<DashboardResponse>(path), "Request inválido produziu mutação parcial.");
        Check.That((await api.GetAsync<DashboardResponse>(DashboardPath(otherDashboard.Id))).Widgets.Length == 0, "Request aninhado afetou outro dashboard.");
    }

    private async Task ConcurrentUpdatesAndResetAsync()
    {
        var path = WidgetPath(dashboard.Id, widget.Id);
        await Task.WhenAll(
            api.PutAsync<WidgetResponse>(path, new WidgetTitleRequest("Título concorrente")),
            api.PutAsync<WidgetResponse>(path, new WidgetRefreshRequest(120)));
        var updated = await ReadWidgetAsync();
        Check.That(updated.Title == "Título concorrente" && updated.RefreshIntervalSeconds == 120, "PUTs concorrentes perderam campo omitido.");
        Check.Equal(widget.CapabilityId, updated.CapabilityId, "Concorrência alterou capabilityId.");
        Check.JsonEqual(widget.Config, updated.Config, "Concorrência alterou config omitida.");
        var reset = await api.PutAsync<WidgetResponse>(path, new WidgetResetRequest());
        Check.That(reset.Title is null && reset.RefreshIntervalSeconds is null && reset.DisplayOrder == 0, "Reset de widget não restaurou null/defaults.");
        Check.Equal(new Position(0, 0, 1, 1), reset.Position, "Reset de posição divergente.");
        Check.JsonEqual(type.DefaultConfig, reset.Config, "Reset de config divergente.");
        Check.Equal(widget.CapabilityId, reset.CapabilityId, "Reset alterou capabilityId omitido.");
        Check.JsonEqual(reset, await ReadWidgetAsync(), "Reset não persistiu.");
        var noOp = await api.PutAsync<WidgetResponse>(path, new EmptyRequest());
        Check.JsonEqual(reset, noOp, "PUT {} alterou widget/updatedAt.");
        Check.JsonEqual(noOp, await ReadWidgetAsync(), "No-op alterou persistência.");
    }

    private async Task<WidgetResponse> ReadWidgetAsync() =>
        (await api.GetAsync<DashboardResponse>(DashboardPath(dashboard.Id))).Widgets.Single(item => item.Id == widget.Id);

    internal async Task CleanupAsync()
    {
        State.Status = "Cleaning";
        store.Save();
        // Always rediscover, even if a previous cleanup passed: an interrupted POST may have completed late.
        var list = (await api.GetAsync<ItemsResponse<DashboardSummary>>("api/v1/dashboards")).Items;
        var sourceExisted = State.CapabilityId.HasValue && await api.GetCapabilityOrMissingAsync(State.CapabilityId.Value) is not null;
        var failures = new List<string>();
        foreach (var fixture in State.Dashboards)
        {
            var ids = list.Where(item => item.Description == fixture.Marker(State.RunId)).Select(item => item.Id)
                .Concat(fixture.Id.HasValue ? [fixture.Id.Value] : Array.Empty<long>()).Distinct().Order().ToArray();
            foreach (var id in ids)
            {
                try
                {
                    await ScenarioAsync($"CLEAN-{fixture.Key}-{id}", async () =>
                    {
                        var found = await api.GetDashboardOrMissingAsync(id);
                        if (found is null) return;
                        Check.That(fixture.Owns(found, State.RunId), $"Limpeza recusada para dashboard {id}: marcador da execução não confere.");
                        foreach (var child in found.Widgets)
                        {
                            var childPath = WidgetPath(id, child.Id);
                            await api.DeleteAsync(childPath);
                            var current = await api.GetAsync<DashboardResponse>(DashboardPath(id));
                            Check.That(current.Widgets.All(item => item.Id != child.Id), "Widget continua presente após DELETE.");
                            await api.ErrorAsync(HttpMethod.Delete, childPath, null, 404, "WIDGET_NOT_FOUND");
                        }
                        // Recheck identity immediately before deleting the parent.
                        var parent = await api.GetAsync<DashboardResponse>(DashboardPath(id));
                        Check.That(fixture.Owns(parent, State.RunId), "Identidade do dashboard mudou durante cleanup.");
                        await api.DeleteAsync(DashboardPath(id));
                        Check.That(await api.GetDashboardOrMissingAsync(id) is null, "Dashboard continua presente após DELETE.");
                        await api.ErrorAsync(HttpMethod.Delete, DashboardPath(id), null, 404, "DASHBOARD_NOT_FOUND");
                    }, append: true);
                }
                catch (Exception exception) when (exception is not OperationCanceledException)
                {
                    failures.Add($"dashboard {id}: {exception.Message}");
                    // Preserve other independent cleanup work; failed resources remain discoverable next time.
                }
            }
        }
        await ScenarioAsync("CLEAN-verify", async () =>
        {
            var remaining = (await api.GetAsync<ItemsResponse<DashboardSummary>>("api/v1/dashboards")).Items;
            Check.That(!remaining.Any(item => State.Dashboards.Any(fixture => item.Description == fixture.Marker(State.RunId))), "Ainda existem dashboards desta execução.");
            if (sourceExisted)
            {
                var source = await api.GetAsync<CompatibleWidgetsResponse>($"api/v1/dashboard-capabilities/{State.CapabilityId}/compatible-widgets");
                Check.Equal(State.CapabilityId!.Value, source.CapabilityId, "Limpeza afetou a referência de origem.");
            }
        }, append: true);
        if (failures.Count != 0) throw new TestFailureException(string.Join(Environment.NewLine, failures));
        State.Status = "Cleaned";
        store.Save();
        Console.WriteLine("Limpeza concluída. Manifesto preservado para conferência e repetição.");
    }

    private async Task ScenarioAsync(string id, Func<Task> action, bool append = false)
    {
        var result = append ? new ScenarioResult { Id = id } : State.Results.Single(item => item.Id == id);
        if (append) State.Results.Add(result);
        result.Status = "Running";
        result.StartedAt = DateTimeOffset.UtcNow;
        store.Save();
        try
        {
            await action();
            result.Status = "Passed";
            Console.WriteLine($"PASS {id}");
        }
        catch (Exception exception)
        {
            result.Status = "Failed";
            result.Error = exception is OperationCanceledException ? "Interrompido ou timeout." : exception.Message;
            Console.Error.WriteLine($"FAIL {id}: {result.Error}");
            throw;
        }
        finally
        {
            result.FinishedAt = DateTimeOffset.UtcNow;
            store.Save();
        }
    }
}
