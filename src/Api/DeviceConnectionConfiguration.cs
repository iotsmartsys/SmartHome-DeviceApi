using Microsoft.Extensions.Configuration;
using MySqlConnector;

namespace Api.Configuration;

internal static class DeviceConnectionConfiguration
{
    internal const string SecretDirectory = "/run/secrets";
    internal const string SecretFileName = "ConnectionStrings__Devices";

    private const string DevelopmentEnvironment = "Development";
    private const string ProductionEnvironment = "Production";

    internal static string RequireSupportedEnvironment(
        string effectiveEnvironment,
        string? aspNetCoreEnvironment,
        string? dotNetEnvironment,
        IReadOnlyList<string> arguments)
    {
        var configuredEnvironments = new List<string>();
        AddEnvironment(aspNetCoreEnvironment, configuredEnvironments);
        AddEnvironment(dotNetEnvironment, configuredEnvironments);
        AddArgumentEnvironments(arguments, configuredEnvironments);

        if (configuredEnvironments.Count == 0 ||
            configuredEnvironments.Any(string.IsNullOrWhiteSpace) ||
            configuredEnvironments.Distinct(StringComparer.Ordinal).Count() != 1)
        {
            throw EnvironmentConfigurationError();
        }

        var configuredEnvironment = configuredEnvironments[0];
        if (configuredEnvironment is not (DevelopmentEnvironment or ProductionEnvironment) ||
            !string.Equals(effectiveEnvironment, configuredEnvironment, StringComparison.Ordinal))
        {
            throw EnvironmentConfigurationError();
        }

        return configuredEnvironment;
    }

    internal static void AddKeyPerFileProviderAndValidate(
        IConfigurationManager configuration,
        string environmentName,
        string secretDirectory)
    {
        if (environmentName is not (DevelopmentEnvironment or ProductionEnvironment))
        {
            throw EnvironmentConfigurationError();
        }

        var secretFileExists = FindSecretFile(secretDirectory);
        var isProduction = string.Equals(environmentName, ProductionEnvironment, StringComparison.Ordinal);

        if (isProduction && !secretFileExists)
        {
            throw new InvalidOperationException("Device database configuration file is required.");
        }

        if (DirectoryExists(secretDirectory))
        {
            try
            {
                configuration.AddKeyPerFile(
                    directoryPath: secretDirectory,
                    optional: !isProduction);
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException)
            {
                throw new InvalidOperationException("Device database configuration file cannot be read.");
            }
        }

        var connectionString = configuration.GetConnectionString("Devices");
        if (secretFileExists && string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Device database configuration file is empty or invalid.");
        }

        if (isProduction && !secretFileExists)
        {
            throw new InvalidOperationException("Device database configuration file is required.");
        }

        ValidateConnectionString(connectionString);
    }

    private static bool FindSecretFile(string secretDirectory)
    {
        if (!DirectoryExists(secretDirectory))
        {
            return false;
        }

        string[] entries;
        try
        {
            entries = Directory.GetFileSystemEntries(secretDirectory);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException)
        {
            throw new InvalidOperationException("Device database configuration directory cannot be read.");
        }

        var expectedPath = Path.Combine(secretDirectory, SecretFileName);
        var entryExists = entries.Any(entry => string.Equals(entry, expectedPath, StringComparison.Ordinal));
        if (!entryExists)
        {
            return false;
        }

        try
        {
            var attributes = File.GetAttributes(expectedPath);
            if ((attributes & FileAttributes.Directory) != 0)
            {
                throw new InvalidOperationException("Device database configuration file is invalid.");
            }
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException)
        {
            throw new InvalidOperationException("Device database configuration file cannot be read.");
        }

        return true;
    }

    private static bool DirectoryExists(string path)
    {
        try
        {
            _ = Directory.GetFileSystemEntries(path);
            return true;
        }
        catch (DirectoryNotFoundException)
        {
            return false;
        }
        catch (FileNotFoundException)
        {
            return false;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException)
        {
            throw new InvalidOperationException("Device database configuration directory is unavailable.");
        }
    }

    private static void ValidateConnectionString(string? connectionString)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException();
            }

            var parsed = new MySqlConnectionStringBuilder(connectionString);
            if (string.IsNullOrWhiteSpace(parsed.Server) ||
                string.IsNullOrWhiteSpace(parsed.Database) ||
                string.IsNullOrWhiteSpace(parsed.UserID) ||
                string.IsNullOrEmpty(parsed.Password))
            {
                throw new ArgumentException();
            }
        }
        catch (Exception exception) when (exception is ArgumentException or FormatException or MySqlException)
        {
            throw new InvalidOperationException("Device database configuration is invalid.");
        }
    }

    private static void AddEnvironment(string? value, ICollection<string> values)
    {
        if (value is not null)
        {
            values.Add(value);
        }
    }

    private static void AddArgumentEnvironments(IReadOnlyList<string> arguments, ICollection<string> values)
    {
        for (var index = 0; index < arguments.Count; index++)
        {
            var argument = arguments[index];
            if (argument.StartsWith("--environment=", StringComparison.OrdinalIgnoreCase))
            {
                values.Add(argument["--environment=".Length..]);
                continue;
            }

            if (!string.Equals(argument, "--environment", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (index + 1 >= arguments.Count)
            {
                values.Add(string.Empty);
                continue;
            }

            values.Add(arguments[++index]);
        }
    }

    private static InvalidOperationException EnvironmentConfigurationError() =>
        new("Application environment must be explicitly configured as Development or Production.");
}
