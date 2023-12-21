using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using Avalonia;
using Avalonia.ReactiveUI;
using VPet.Avalonia.Linux.Interops;

namespace VPet.Avalonia.Linux;

internal static class Program
{
    private static LinuxPlatformBridgeBase? _platformSpecificBridge;
    
    internal static void Main(string[] args)
    {
        // Tell users about avaloniaUI issue (Skia in mostly bruh)
        // https://github.com/AvaloniaUI/Avalonia/issues/4427
        // https://github.com/Blessing-Studio/WonderLab.Override/issues/3
        ValidateSkiaFontManagerWorkaround();
        
        var platformFeatures = new X11PlatformBridge();

        _platformSpecificBridge = platformFeatures;
        
        try
        {
            platformFeatures.ShowMessageBoxNative("Test", "");
            
            CreateAppBuilderPrivate()
                .StartWithClassicDesktopLifetime(args);
        }
        catch (Exception e)
        {
            throw new AggregateException(e);
            platformFeatures.ShowMessageBoxNative("Severe error appeared! Aborting...", e.Message);
        }
    }

    private static AppBuilder CreateAppBuilderPrivate()
    {
        return AppBuilder
            .Configure<PetApp>()
            .AfterSetup(OnAppPreSetupComplete)
            .UsePlatformDetect()
            //.With(CreateFontManagerOptions)
            .With(new X11PlatformOptions
            {
                RenderingMode = new[]
                {
                    X11RenderingMode.Glx
                },
                EnableSessionManagement = true
            })
            .UseReactiveUI();
    }

    private static void ValidateSkiaFontManagerWorkaround()
    {
        const string file = "noworkaround_skiafontmanager";
        // Allow users to ignore the workaround
        if (Directory.EnumerateFiles(".")
            .Any(a => Path.GetFileNameWithoutExtension(a).ToLower() == file))
            return;

        var env = Environment.GetEnvironmentVariable("LC_CTYPE");
        if (string.Equals(env, "en_US.UTF-8", StringComparison.OrdinalIgnoreCase))
            return;
        
        var builder = new StringBuilder("Please append environment \"LC_CTYPE=en_US.UTF-8\" before run this application!\n")
            .AppendLine("Its not our fault, its dramatic Skia font detection!")
            .AppendLine($"or you can create a file named \"{file}\" (regardless exist extension name or not) to ignore this message!")
            .AppendLine("Issue at upstream/avaloniaUI: https://github.com/AvaloniaUI/Avalonia/issues/4427");
        //throw new PlatformNotSupportedException();/
        
        Console.WriteLine(builder.ToString());

        //"Please append environment \"LC_CTYPE=en_US.UTF-8\" before run this application! Its not our fault, its dramatic Skia font detection!"
    }

    private static void OnAppPreSetupComplete(AppBuilder builder)
    {
        if (builder.Instance is not PetApp app)
            throw new Exception($"Wrong application launched! Expecting: {typeof(PetApp).FullName}");
        
        app.SetPlatformSpecificBridge(_platformSpecificBridge!);
        
        //throw new NotImplementedException();
    }
}