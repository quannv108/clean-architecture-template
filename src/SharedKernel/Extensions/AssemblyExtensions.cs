using System.Reflection;

namespace SharedKernel.Extensions;

/// <summary>
/// Extension methods for Assembly to get build date information
/// </summary>
public static class AssemblyExtensions
{
    /// <summary>
    /// Gets the file creation time of an assembly in UTC
    /// </summary>
    /// <param name="assembly">The assembly to get the file creation time for</param>
    /// <returns>The file creation time as a DateTime in UTC</returns>
    public static DateTime GetFileCreationTimeUtc(this Assembly assembly)
    {
        var filePath = assembly.Location;

        if (string.IsNullOrEmpty(filePath))
        {
            throw new InvalidOperationException(
                "Assembly location is not available. This may occur in single-file deployments.");
        }

        return File.GetCreationTimeUtc(filePath);
    }
}
