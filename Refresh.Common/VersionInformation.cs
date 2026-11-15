using System.Reflection;

namespace Refresh.Common;

public static class VersionInformation
{
    public static readonly string Version;

    static VersionInformation()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        AssemblyInformationalVersionAttribute? versionAttribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();

        string[] versionParts = versionAttribute?.InformationalVersion.Split('+') ?? [];
        string semanticVersion = versionParts.Length >= 1 ? versionParts[0] : "";
        string commitHash = versionParts.Length >= 2 ? versionParts[1] : "";

        // Dotnet sets the semantic version to 1.0.0 on build time if we haven't specified anything
        // (and therefore our dockerfiles use it as default value),
        // and some of our workflows set it to 0.0.0 if unspecified.
        // Additionally, we're long past v1.0.0, so we can safely not include the semantic version in our final string
        // if it's one of these 2 default values.
        if (semanticVersion is "0.0.0" or "1.0.0") semanticVersion = "";

        // Neither commit hash nor valid semantic version given, so show it as "unknown"
        if (string.IsNullOrWhiteSpace(semanticVersion) && string.IsNullOrWhiteSpace(commitHash))
            Version = "unknown";

        // Only commit hash is given, so only show the hash, without semantic and without "+" prefix
        else if (string.IsNullOrWhiteSpace(semanticVersion))
            Version = commitHash;

        // Somehow, no hash is given. I don't know if there is even a case where this can realistically happen,
        // but guard against it regardless. If only a valid semantic version if given, show it without any hash.
        else if (string.IsNullOrWhiteSpace(commitHash))
            Version = semanticVersion;

        else
            Version = $"{semanticVersion}+{commitHash}"; // original formatting
    }
}