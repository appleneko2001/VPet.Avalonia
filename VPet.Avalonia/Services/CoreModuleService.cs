using System.Collections.Immutable;
using System.Reflection;
using VPet.Avalonia.Debugging;
using VPet.Avalonia.Extensions;
using VPet.Avalonia.Modules;

using System.Runtime.Loader;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using VPet.Avalonia.Interfaces;
using VPet.Avalonia.Jsons;
using VPet.Avalonia.Options;
using VPet.Avalonia.Primitives;

namespace VPet.Avalonia.Services;

public class CoreModuleService : IDisposable
{
    private readonly string _modulesPath;
    private readonly List<IModuleCore> _loadedModules = new();
    private readonly Dictionary<string, Assembly> _loadedDependencies = new();

    private readonly AppDomain _currentDomain = AppDomain.CurrentDomain;
    
    public CoreModuleService()
    {
        try
        {
            _modulesPath = PetApp.ApplicationRootPath.GetPath("modules");
        }
        catch (DirectoryNotFoundException)
        {
            this.WriteLine(MessageSeverity.Info, "Seems like there does not exist the \"modules\" folder. Creating one...");
            _modulesPath = Directory.CreateDirectory("modules").FullName;
        }
        catch (Exception e)
        {
            throw new AggregateException("Unable to instantiate the core module service!", e);
        }
        
        _currentDomain.AssemblyLoad += CurrentDomain_OnAssemblyLoad;
        _currentDomain.AssemblyResolve += CurrentDomain_OnAssemblyResolve;
    }

    private Assembly CurrentDomain_OnAssemblyResolve(object sender, ResolveEventArgs args)
    {
        if (_loadedDependencies.TryGetValue(args.Name, out var assem))
            return assem;

        var founds = _loadedDependencies
            .Where(a => string.Equals(a.Value.FullName, args.Name))
            .ToImmutableArray();

        switch (founds.Length)
        {
            case 0:
                throw new InvalidOperationException($"The acquired assembly \"{args.Name}\" is not loaded.");
            
            case 1:
            default:
                return founds.FirstOrDefault().Value;
        }
    }

    private void CurrentDomain_OnAssemblyLoad(object sender, AssemblyLoadEventArgs args)
    {
        this.WriteLine(MessageSeverity.Info, $"Assembly \"{args.LoadedAssembly.FullName}\" is loaded.");
    }

    public void LoadModules(string rootPath)
    {
        foreach (var fullPath in Directory.EnumerateFiles(_modulesPath))
        {
            try
            {
                var fi = new FileInfo(fullPath);
                var name = fi.Name;
                if (!name.StartsWith("VPet.Avalonia.Providers.", StringComparison.OrdinalIgnoreCase) ||
                    !name.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                    continue;

                var nameWithoutExt = Path.GetFileNameWithoutExtension(name);

                var depsIndexFilePath = Directory
                    .EnumerateFiles(_modulesPath)
                    .FirstOrDefault(s =>
                    {
                        var t = Path.GetFileName(s);
                        return t.StartsWith(nameWithoutExt) &&
                               t.EndsWith(".deps.json", StringComparison.OrdinalIgnoreCase);
                    }) ?? null;
                
                if(depsIndexFilePath != null)
                    LoadDeps(depsIndexFilePath, nameWithoutExt, _modulesPath);

                //var asmLoadCtx = AssemblyLoadContext.Default;
                var assembly = _currentDomain.Load(File.ReadAllBytes(fi.FullName));
                var entryObject = assembly
                    .GetExportedTypes()
                    .FirstOrDefault(a => typeof(IModuleCore).IsAssignableFrom(a));

                if (entryObject == null)
                    throw new InvalidOperationException($"{fullPath} is not a valid core module.");

                var inst = assembly.CreateInstance(
                    entryObject.FullName ??
                    throw new InvalidOperationException("Unable to find the full name of the class."));

                if (inst is not IModuleCore core)
                    throw new InvalidOperationException($"{fullPath} is not a valid core module.");
                
                core.Initialise(rootPath);
                _loadedModules.Add(core);
            }
            catch (Exception e)
            {
                this.WriteLine(MessageSeverity.Error, $"Unable to load module at \"{fullPath}\". {e}");
            }
        }
    }

    public void InitAssetsProviders(OptionsTable options, Action<double> callback)
    {
        foreach (var module in _loadedModules)
        {
            try
            {
                module.InitAssets(options, callback);
            }
            catch (Exception e)
            {
                this.WriteLine(MessageSeverity.Error, $"Unable to initialise the assets provider: {e}");
            }
        }
    }
    
    public IGfxServiceInterface GetGfxService()
    {
        var first = _loadedModules.FirstOrDefault();

        if (first == null)
            throw new InvalidOperationException("No available GFX service");
        
        return first.GetGfxService();
    }

    private void LoadDeps(string depsJsonPath, string assemName, string depsLocation)
    {
        using var jsonStream = File.OpenRead(depsJsonPath);

        var depsInfo = JsonSerializer.Deserialize<DotnetLibraryDepsJson>(jsonStream);

        if (depsInfo == null)
            throw new JsonException($"Unable to deserialise the dependencies descriptor file \"{depsJsonPath}\".");
        
        var runtimeTarget = depsInfo.RuntimeTarget.Name;

        var moduleInfo = depsInfo
            .Targets[runtimeTarget]
            .FirstOrDefault(s => s.Key.StartsWith(assemName, StringComparison.OrdinalIgnoreCase))
            .Value;

        var dependencies = moduleInfo.Dependencies;

        foreach (var dependency in dependencies)
        {
            var name = dependency.Key;
            var ver = dependency.Value;

            // Load dependency into the current application domain
            if (_currentDomain.GetAssemblies()
                .Any(a => a.FullName.StartsWith(name)))
                continue;
            
            var depFileName = depsInfo.GetAssemblyFileName(runtimeTarget, name, ver);
            var depFilePath = Path.Combine(depsLocation, depFileName);

            this.WriteLine(MessageSeverity.Info, $"Load core module dependency: {name}, Version={ver}...");
            var assem = _currentDomain.Load(File.ReadAllBytes(depFilePath));
            
            _loadedDependencies.Add(name, assem);
        }
    }
    
    public void Dispose()
    {
        // TODO release managed resources here
    }
}