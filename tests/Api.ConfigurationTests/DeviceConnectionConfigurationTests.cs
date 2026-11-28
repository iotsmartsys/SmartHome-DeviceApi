using Api.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Api.ConfigurationTests;

public sealed class DeviceConnectionConfigurationTests
{
    private const string SyntheticConnectionString =
        "Server=127.0.0.1;Database=devices_synthetic;User ID=synthetic_user;Password=synthetic_only;";

    [Fact]
    public void DevelopmentUsesLocalProviderWhenSecretDirectoryIsMissing()
    {
        var configuration = ConfigurationWithEnvironmentValue(SyntheticConnectionString);
        var missingDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

        DeviceConnectionConfiguration.AddKeyPerFileProviderAndValidate(
            configuration,
            "Development",
            missingDirectory);

        Assert.Equal(SyntheticConnectionString, configuration.GetConnectionString("Devices"));
    }

    [Fact]
    public void DevelopmentWithoutFileOrConnectionValueFailsWithSanitizedError()
    {
        var configuration = new ConfigurationManager();
        var missingDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

        var exception = Assert.Throws<InvalidOperationException>(() =>
            DeviceConnectionConfiguration.AddKeyPerFileProviderAndValidate(
                configuration,
                "Development",
                missingDirectory));

        Assert.Equal("Device database configuration is invalid.", exception.Message);
    }

    [Fact]
    public void PresentFileOverridesPreviouslyConfiguredValue()
    {
        using var directory = new TemporaryDirectory();
        var configuration = ConfigurationWithEnvironmentValue("Server=localhost;Database=environment;User ID=env;Password=synthetic;");
        File.WriteAllText(
            Path.Combine(directory.Path, DeviceConnectionConfiguration.SecretFileName),
            SyntheticConnectionString);

        DeviceConnectionConfiguration.AddKeyPerFileProviderAndValidate(
            configuration,
            "Development",
            directory.Path);

        Assert.Equal(SyntheticConnectionString, configuration.GetConnectionString("Devices"));
    }

    [Theory]
    [InlineData("Development", "")]
    [InlineData("Development", "not-a-connection-string")]
    [InlineData("Development", "Server=localhost;Database=synthetic;User ID=synthetic;NoSuchSetting=1")]
    [InlineData("Development", "Server=localhost;Database=synthetic;User ID=synthetic;NoSuchSetting=SYNTHETIC_SENTINEL_ONLY")]
    [InlineData("Development", "Server=localhost;Database=synthetic;User ID=synthetic;")]
    [InlineData("Production", "")]
    [InlineData("Production", "not-a-connection-string")]
    [InlineData("Production", "Server=localhost;Database=synthetic;User ID=synthetic;NoSuchSetting=1")]
    [InlineData("Production", "Server=localhost;Database=synthetic;User ID=synthetic;NoSuchSetting=SYNTHETIC_SENTINEL_ONLY")]
    [InlineData("Production", "Server=localhost;Database=synthetic;User ID=synthetic;")]
    public void PresentInvalidFileFailsWithoutEnvironmentFallback(string environmentName, string fileValue)
    {
        using var directory = new TemporaryDirectory();
        var configuration = ConfigurationWithEnvironmentValue(SyntheticConnectionString);
        File.WriteAllText(
            Path.Combine(directory.Path, DeviceConnectionConfiguration.SecretFileName),
            fileValue);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            DeviceConnectionConfiguration.AddKeyPerFileProviderAndValidate(
                configuration,
                environmentName,
                directory.Path));

        Assert.DoesNotContain(SyntheticConnectionString, exception.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain("SYNTHETIC_SENTINEL_ONLY", exception.ToString(), StringComparison.Ordinal);
        Assert.NotEqual(SyntheticConnectionString, configuration.GetConnectionString("Devices"));
    }

    [Fact]
    public void ExpectedPathThatIsNotAFileFailsWithoutEnvironmentFallback()
    {
        using var directory = new TemporaryDirectory();
        var configuration = ConfigurationWithEnvironmentValue(SyntheticConnectionString);
        Directory.CreateDirectory(Path.Combine(directory.Path, DeviceConnectionConfiguration.SecretFileName));

        var exception = Assert.Throws<InvalidOperationException>(() =>
            DeviceConnectionConfiguration.AddKeyPerFileProviderAndValidate(
                configuration,
                "Development",
                directory.Path));

        Assert.Equal("Device database configuration file is invalid.", exception.Message);
        Assert.DoesNotContain(SyntheticConnectionString, exception.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void ProductionRequiresFileEvenWhenEnvironmentContainsValue()
    {
        var configuration = ConfigurationWithEnvironmentValue(SyntheticConnectionString);
        var missingDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

        var exception = Assert.Throws<InvalidOperationException>(() =>
            DeviceConnectionConfiguration.AddKeyPerFileProviderAndValidate(
                configuration,
                "Production",
                missingDirectory));

        Assert.Equal("Device database configuration file is required.", exception.Message);
    }

    [Fact]
    public void ProductionRequiresExpectedFileWhenDirectoryExists()
    {
        using var directory = new TemporaryDirectory();
        var configuration = ConfigurationWithEnvironmentValue(SyntheticConnectionString);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            DeviceConnectionConfiguration.AddKeyPerFileProviderAndValidate(
                configuration,
                "Production",
                directory.Path));

        Assert.Equal("Device database configuration file is required.", exception.Message);
        Assert.DoesNotContain(SyntheticConnectionString, exception.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void ProductionUsesFileInsteadOfConflictingEnvironmentValue()
    {
        using var directory = new TemporaryDirectory();
        var configuration = ConfigurationWithEnvironmentValue("Server=localhost;Database=environment;User ID=env;Password=synthetic;");
        File.WriteAllText(
            Path.Combine(directory.Path, DeviceConnectionConfiguration.SecretFileName),
            SyntheticConnectionString);

        DeviceConnectionConfiguration.AddKeyPerFileProviderAndValidate(
            configuration,
            "Production",
            directory.Path);

        Assert.Equal(SyntheticConnectionString, configuration.GetConnectionString("Devices"));
    }

    [Theory]
    [InlineData("Production", null, null, "")]
    [InlineData("Staging", "Staging", null, "")]
    [InlineData("Development", "Development", "Production", "")]
    [InlineData("Development", "Production", null, "--environment=Development")]
    public void MissingUnknownOrConflictingEnvironmentFails(
        string effectiveEnvironment,
        string? aspNetCoreEnvironment,
        string? dotNetEnvironment,
        string arguments)
    {
        var parsedArguments = arguments.Length == 0 ? Array.Empty<string>() : new[] { arguments };

        Assert.Throws<InvalidOperationException>(() =>
            DeviceConnectionConfiguration.RequireSupportedEnvironment(
                effectiveEnvironment,
                aspNetCoreEnvironment,
                dotNetEnvironment,
                parsedArguments));
    }

    [Fact]
    public void ExplicitDevelopmentEnvironmentIsAccepted()
    {
        Assert.Equal(
            "Development",
            DeviceConnectionConfiguration.RequireSupportedEnvironment(
                "Development",
                "Development",
                null,
                Array.Empty<string>()));
    }

    [Fact]
    public async Task HttpFailureDoesNotExposeExceptionValueInLogOrResponse()
    {
        const string sentinel = "SYNTHETIC_SENTINEL_ONLY";
        var logger = new RecordingLogger<ExceptionHandler>();
        var context = new DefaultHttpContext
        {
            RequestServices = new ServiceCollection().BuildServiceProvider()
        };
        await using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;
        var middleware = new ExceptionHandler(_ =>
            Task.FromException(new InvalidOperationException($"Synthetic failure: {sentinel}")));

        await middleware.InvokeAsync(context, logger);

        responseBody.Position = 0;
        using var reader = new StreamReader(responseBody);
        var response = await reader.ReadToEndAsync();
        Assert.DoesNotContain(sentinel, response, StringComparison.Ordinal);
        Assert.DoesNotContain(sentinel, string.Join('\n', logger.Messages), StringComparison.Ordinal);
    }

    private static ConfigurationManager ConfigurationWithEnvironmentValue(string value)
    {
        var configuration = new ConfigurationManager();
        configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Devices"] = value
        });
        return configuration;
    }

    private sealed class TemporaryDirectory : IDisposable
    {
        internal TemporaryDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        internal string Path { get; }

        public void Dispose() => Directory.Delete(Path, recursive: true);
    }

    private sealed class RecordingLogger<T> : ILogger<T>
    {
        internal List<string> Messages { get; } = new();

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Messages.Add(formatter(state, exception));
            if (exception is not null)
            {
                Messages.Add(exception.ToString());
            }
        }
    }
}
