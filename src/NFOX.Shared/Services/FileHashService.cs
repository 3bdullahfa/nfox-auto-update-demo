using System.Security.Cryptography;

namespace NFOX.Shared.Services;

public static class FileHashService
{
    public static string ComputeSha256(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        var hash = SHA256.HashData(stream);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    public static void VerifySha256(string filePath, string expectedSha256)
    {
        var actual = ComputeSha256(filePath);
        if (!actual.Equals(expectedSha256, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"SHA256 mismatch for {Path.GetFileName(filePath)}. Expected {expectedSha256}, got {actual}.");
        }
    }
}
