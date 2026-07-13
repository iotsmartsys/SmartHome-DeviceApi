using System.ComponentModel.DataAnnotations;
using Api.Models;
using Core.Contracts.Repositories;
using Core.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

public sealed class DeviceMetricsTests
{
    [Fact]
    public void Valid_payload_passes_validation() => Assert.Empty(Validate(ValidRequest()));

    [Theory]
    [InlineData(101, 20, -50)]
    [InlineData(20, -1, -50)]
    [InlineData(20, 20, -151)]
    public void Invalid_ranges_fail_validation(decimal cpu, decimal memory, int rssi)
    {
        var request = ValidRequest(cpu, memory, rssi);
        Assert.NotEmpty(Validate(request));
    }

    [Fact]
    public async Task Save_returns_no_content_and_forwards_metrics()
    {
        var repository = new FakeRepository { Exists = true };
        var controller = new DeviceMetricsController(NullLogger<DeviceMetricsController>.Instance);

        var result = await controller.Save(ValidRequest(), repository, default);

        Assert.IsType<NoContentResult>(result);
        Assert.NotNull(repository.SavedMetrics);
    }

    [Fact]
    public async Task Save_returns_not_found_for_unknown_device()
    {
        var controller = new DeviceMetricsController(NullLogger<DeviceMetricsController>.Instance);
        var result = await controller.Save(ValidRequest(), new FakeRepository(), default);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task History_rejects_page_size_above_limit()
    {
        var controller = new DeviceMetricsController(NullLogger<DeviceMetricsController>.Instance);
        var result = await controller.GetHistory("esp", new FakeRepository { Exists = true }, pageSize: 501);
        var details = Assert.IsType<ValidationProblemDetails>(Assert.IsType<ObjectResult>(result).Value);
        Assert.Contains("pageSize", details.Errors.Keys);
    }

    private static DeviceMetricsRequest ValidRequest(decimal cpu = 50, decimal memory = 25, int rssi = -50) => new()
    {
        device_id = "esp32c6-FFFE17", uptime_ms = 10, cpu_cores = 1, cpu_percent = cpu,
        memory_percent = memory, temperature_c = 40, frequency_mhz = 160,
        network = new() { rssi = rssi, last_disconnection_uptime_ms = 0, disconnection_reason = 0, connection_count = 1 }
    };

    private static IReadOnlyCollection<ValidationResult> Validate(object value)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(value, new ValidationContext(value), results, true);
        if (value is DeviceMetricsRequest { network: not null } request)
            Validator.TryValidateObject(request.network, new ValidationContext(request.network), results, true);
        return results;
    }

    private sealed class FakeRepository : IDeviceMetricsRepository
    {
        public bool Exists { get; init; }
        public DeviceMetrics? SavedMetrics { get; private set; }
        public Task<bool> SaveAsync(string externalDeviceId, DeviceMetrics metrics, CancellationToken cancellationToken)
        { SavedMetrics = metrics; return Task.FromResult(Exists); }
        public Task<DeviceMetricsCurrent?> GetCurrentAsync(string externalDeviceId, CancellationToken cancellationToken) => Task.FromResult<DeviceMetricsCurrent?>(null);
        public Task<bool> DeviceExistsAsync(string externalDeviceId, CancellationToken cancellationToken) => Task.FromResult(Exists);
        public Task<DeviceMetricsHistoryPage> GetHistoryAsync(string externalDeviceId, DateTime? from, DateTime? to, int page, int pageSize, CancellationToken cancellationToken) =>
            Task.FromResult(new DeviceMetricsHistoryPage(page, pageSize, 0, []));
    }
}
