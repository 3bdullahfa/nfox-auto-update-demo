namespace NFOX.Shared.Services;

public static class VersionService
{
    public static int Compare(string left, string right)
    {
        var leftVersion = Parse(left);
        var rightVersion = Parse(right);
        return leftVersion.CompareTo(rightVersion);
    }

    public static bool IsNewer(string currentVersion, string latestVersion) => Compare(latestVersion, currentVersion) > 0;

    private static Version Parse(string value)
    {
        if (!Version.TryParse(value, out var version))
        {
            throw new FormatException($"Invalid version format: {value}");
        }

        return version;
    }
}
