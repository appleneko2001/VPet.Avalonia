namespace VPet.Avalonia.Services;

public class WorkDirService
{
    public static string CachePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "VPet.Avalonia", "cache");

    public static void CreateCacheFolderIfNotExist()
    {
        var di = new DirectoryInfo(CachePath);
        di.Create();

        if (!di.Exists)
            throw new DirectoryNotFoundException("Unable to find cache folder.");
    }
}