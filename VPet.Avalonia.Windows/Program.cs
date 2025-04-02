using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Avalonia;
using Avalonia.ReactiveUI;
using Microsoft.Win32;
using Steamworks;

namespace VPet.Avalonia.Windows;

internal static class Program
{
    //private static LinuxPlatformBridgeBase? _platformSpecificBridge;
    //HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 1920960
    private static string? AssetsPath;
    private static uint VPetSimAppId = 1920960;
    
    internal static void Main(string[] args)
    {
        // Tell users about avaloniaUI issue (Skia in mostly bruh)
        // https://github.com/AvaloniaUI/Avalonia/issues/4427
        // https://github.com/Blessing-Studio/WonderLab.Override/issues/3
        //ValidateSkiaFontManagerWorkaround();
        //var platformFeatures = new X11PlatformBridge();
        //_platformSpecificBridge = platformFeatures;
        foreach (var arg in args)
        {
            switch (arg.ToLowerInvariant())
            {
                case "restart":
                {
                    // TODO: use shared mutex instead.
                    Thread.Sleep(3000);
                } break;
            }
        }
        
        // Try to find existed VPet-Simulator installation by windows register and SteamAPI
        var assetsPath = TryGetSteamAppInstallationViaWinRegister() ??
                         TryGetSteamAppInstallationViaSteamApi();
        
        if (!string.IsNullOrEmpty(assetsPath)) //assetsPath = Path.Combine(assetsPath, "mod");
            AssetsPath = assetsPath;
        
        try
        {
            CreateAppBuilderPrivate()
                .StartWithClassicDesktopLifetime(args);
        }
        catch (Exception e)
        {
            throw new AggregateException(e);
            //platformFeatures.ShowMessageBoxNative("Severe error appeared! Aborting...", e.Message);
        }
    }

    private static AppBuilder CreateAppBuilderPrivate()
    {
        return AppBuilder
            .Configure<PetApp>()
            .AfterSetup(OnAppPreSetupComplete)
            .UsePlatformDetect()
            .With(new Win32PlatformOptions
            {
                RenderingMode = new []
                {
                    //Win32RenderingMode.Vulkan,
                    Win32RenderingMode.AngleEgl
                }
            })
            //.With(CreateFontManagerOptions)
            .UseReactiveUI();
    }
    
    // ikr im in windows platform stfu
#pragma warning disable CA1416

    private static string? TryGetSteamAppInstallationViaWinRegister()
    {
        var platform = Environment.OSVersion.Platform;
        if (platform != PlatformID.Win32NT)
        {
            Console.WriteLine($"Platform {platform} is not supports Windows Register feature");
            return null;
        }
        
        try
        {
            using var hklm = Registry.LocalMachine;
            using var subkey =
                hklm.OpenSubKey($"SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Steam App {VPetSimAppId}");

            var pathObject = subkey?.GetValue("InstallLocation");

            if (pathObject is string s && !string.IsNullOrEmpty(s))
                return s;
        }
        catch(Exception e)
        {
            return null;
        }

        return null;
    }

    private static string? TryGetSteamAppInstallationViaSteamApi()
    {
        // Process should end after it writes info.
        // but steamAPI will still workin until all processes is gone
        // prob need smth like updater or anything else to let it sideload or what to make steamAPI lost focus 
        Console.WriteLine("Trying to use SteamAPI to get installation path.");
        var cachePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{VPetSimAppId}_installation_path.txt");
        var file = new FileInfo(cachePath);
        
        try
        {
            if (file.Exists)
            {
                try
                {
                    using var reader = file.OpenText();
                    return reader.ReadToEnd();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            
            if (SteamAPI.InitEx(out var error ) != ESteamAPIInitResult.k_ESteamAPIInitResult_OK)
                throw new InvalidOperationException($"Cannot load SteamAPI: {error}");

            SteamApps.GetAppInstallDir(new AppId_t(VPetSimAppId), out var folder, 1024);

            try
            {
                using var stream = file.OpenWrite();
                using var writer = new StreamWriter(stream);

                writer.Write(folder);
                writer.Close();

                //TODO: it doesnt work, process kill is not happen at all...
                Console.WriteLine("Restarting...");

                var myself = Process.GetCurrentProcess();
                var restartInfo = new ProcessStartInfo(
                    myself.MainModule?.FileName ?? throw new ApplicationException(), "restart");

                Process.Start(restartInfo);
                throw new ApplicationException("Restart by using throw crash after process start.");
            }
            catch (ApplicationException)
            {
                throw;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
            return folder;
        }
        finally
        {
            SteamAPI.Shutdown();   
        }
    }
    
#pragma warning restore CA1416

    private static void OnAppPreSetupComplete(AppBuilder builder)
    {
        if (builder.Instance is not PetApp app)
            throw new Exception($"Wrong application launched! Expecting: {typeof(PetApp).FullName}");

        if(!string.IsNullOrEmpty(AssetsPath))
            app.AppendAdditionalModPackPath(AssetsPath);
        app.SetPlatformSpecificBridge(new WindowsPlatformBridge());
        
        //throw new NotImplementedException();
    }
}