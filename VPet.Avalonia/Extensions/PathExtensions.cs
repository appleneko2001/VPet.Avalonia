using System.Collections.Immutable;
using VPet.Avalonia.Algorithms.Hashs;

namespace VPet.Avalonia.Extensions;

public static class PathExtensions
{
    /// <summary>
    /// Get the folder location regardless case-sensitive or case-insensitive
    /// </summary>
    /// <param name="baseDir">The beginning folder location.</param>
    /// <param name="folders">The path to the folder destination.</param>
    /// <returns>The full path to the folder with the case of characters (if exist).</returns>
    public static string GetPath(this string baseDir, params string[] folders)
    {
        var pwd = baseDir;
        var queue = new Queue<string>(folders);

        while (queue.TryDequeue(out var target))
        {
            var currentDirFolders = Directory
                .EnumerateDirectories(pwd)
                .Select(s => new DirectoryInfo(s).Name)
                .ToImmutableArray();
            
            var found = currentDirFolders
                .Where(s => string.Equals(s, target, StringComparison.OrdinalIgnoreCase))
                .ToImmutableArray();

            pwd = found.Length switch
            {
                1 => Path.Combine(pwd, found.FirstOrDefault()!),
                
                0 => throw new DirectoryNotFoundException(
                    $"The folder location \"{pwd}\" doesn't have folder named \"{target}\"."),
                
                _ => throw new InvalidOperationException(
                    $"The folder location \"{pwd}\" does have multiple folders named \"{target}\".")
            };
        }

        return pwd;
    }
    
    public static string CreateFolderThroughPath(this string baseDir, params string[] folders)
    {
        var pwd = baseDir;
        var queue = new Queue<string>(folders);

        while (queue.TryDequeue(out var target))
        {
            var currentDirFolders = Directory
                .EnumerateDirectories(pwd)
                .Select(s => new DirectoryInfo(s).Name)
                .ToImmutableArray();
            
            var found = currentDirFolders
                .Where(s => string.Equals(s, target, StringComparison.OrdinalIgnoreCase))
                .ToImmutableArray();

            pwd = found.Length switch
            {
                1 => Path.Combine(pwd, found.FirstOrDefault()!),
                0 => Directory.CreateDirectory(Path.Combine(pwd, target)).FullName,
                _ => throw new InvalidOperationException(
                    $"The folder location \"{pwd}\" does have multiple folders named \"{target}\".")
            };
        }

        return pwd;
    }

    public static string PathToHash(this string path)
    {
        return new CityHashAlgorithm().GetHash(path);
    }
}