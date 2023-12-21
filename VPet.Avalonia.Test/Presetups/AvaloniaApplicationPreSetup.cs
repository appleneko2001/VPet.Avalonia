using Avalonia;
using Avalonia.Headless;
using VPet.Avalonia;

[assembly: AvaloniaTestApplication(typeof(AvaloniaApplicationPreSetup))]

public class AvaloniaApplicationPreSetup
{
    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<PetApp>()
        .UseHeadless(new AvaloniaHeadlessPlatformOptions());
}