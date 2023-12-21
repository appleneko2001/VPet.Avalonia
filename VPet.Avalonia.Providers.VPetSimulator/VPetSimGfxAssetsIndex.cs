using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LinePutScript;
using VPet.Avalonia.Enums;
using VPet.Avalonia.Systems.Graphics;

namespace VPet.Avalonia.Providers.VPetSimulator;

/// <summary>
/// This object is used for compatibility to use VPetSimulator gfx assets. (The alternative of "PetLoader.cs")
/// </summary>
public class VPetSimGfxAssetsIndex : GfxAssetsIndex
{
    /// <summary>
    /// Mapping the asset tags as pet mood and health state enum. 
    /// </summary>
    private static IReadOnlyDictionary<string, PetState> PetStateConverterDict = new Dictionary<string, PetState>
    {
        { "happy", PetState.Happy },
        { "nomal", PetState.Normal },
        { "normal", PetState.Normal },
        { "poorcondition", PetState.Bad },
        { "bad", PetState.Bad },
        { "ill", PetState.Ill }
    };

    /// <summary>
    /// Mapping the asset tags as pet activity enum
    /// </summary>
    private static IReadOnlyDictionary<string[], PetActivityState> PetActivityConverterDict =
        new Dictionary<string[], PetActivityState>
        {
            { new[] { "default" }, PetActivityState.CommonIdle },
            //{ "drink", }
            { new[] { "idle" }, PetActivityState.Idle },
            { new[] { "idel" }, PetActivityState.Idle },
            { new[] { "state","stateone" }, PetActivityState.Idle1 },
            { new[] { "state","statetwo" }, PetActivityState.Idle2 },
            { new[] { "stateone" }, PetActivityState.Idle1 },
            { new[] { "statetwo" }, PetActivityState.Idle2 },
            { new[] { "move" }, PetActivityState.Move },
            { new[] { "say" }, PetActivityState.Say },
            { new[] { "raise", "dynamic" }, PetActivityState.RaisedDynamic },
            { new[] { "raise", "static" }, PetActivityState.RaisedStatic },
            { new[] { "shutdown" }, PetActivityState.Shutdown },
            { new[] { "startup" }, PetActivityState.Startup },
            { new[] { "sleep" }, PetActivityState.Sleep },
            { new[] { "switch", "down" }, PetActivityState.SwitchDown },
            { new[] { "switch", "up" }, PetActivityState.SwitchUp },
            { new[] { "switch", "hunger" }, PetActivityState.SwitchHunger },
            { new[] { "switch", "thirsty" }, PetActivityState.SwitchThirsty },
            { new[] { "touch", "body" }, PetActivityState.TouchBody },
            { new[] { "touch", "head" }, PetActivityState.TouchHead },
            { new[] { "work" }, PetActivityState.Work }
        };

    /// <summary>
    /// Mapping the asset tags as gfx transition type.
    /// </summary>
    private static IReadOnlyDictionary<string, GfxAnimationType> GfxAnimationEnumConverterDict =
        new Dictionary<string, GfxAnimationType>
        {
            { "a", GfxAnimationType.Start },
            { "start", GfxAnimationType.Start },
            { "b", GfxAnimationType.Loop },
            { "loop", GfxAnimationType.Loop },
            { "c", GfxAnimationType.End },
            { "end", GfxAnimationType.End },
            { "single", GfxAnimationType.Single },
        };
    
    public VPetSimGfxAssetsIndex(string gfxPath)
    {
        LoadGfx(new DirectoryInfo(gfxPath), gfxPath);
    }

    /// <summary>
    /// Parse asset path as animation key
    /// </summary>
    /// <param name="path">The target folder</param>
    /// <param name="info">Some parameters like rootPath and name, etc.</param>
    /// <returns>An gfx animation key instance.</returns>
    /// <exception cref="ArgumentException">Not enough information to confirm which kind of this asset is.</exception>
    private static PetGfxInfo? ParseByFilePath(FileSystemInfo path, ILine info)
    {
        var rootPath = info[(gstr)"rootPath"] ?? 
                       throw new ArgumentException("Unable to get root path of the gfx folder.");

        var dirPath = Path.GetDirectoryName(path.FullName)!;
        var nameWithoutExt = Path.GetFileNameWithoutExtension(path.Name);
        
        var relativePath = Path.GetRelativePath(rootPath, Path.Combine(dirPath, nameWithoutExt));
        var assetName = relativePath
            .Replace(Path.PathSeparator, '\n')
            .Replace(Path.AltDirectorySeparatorChar, '\n')
            // Compatibility to VPetSimulator, because their asset naming uses underscore as separator.
            .Replace('_', '\n');
        var assetTags = assetName.Split('\n')
            .Where(a => !string.IsNullOrWhiteSpace(a))
            .ToList();

        if (!Enum.TryParse(info[(gstr)"mode"], true, out PetState modeType))
        {
            var stateText = assetTags
                .FirstOrDefault(tag => PetStateConverterDict.ContainsKey(tag.ToLower()));

            if (stateText == null)
            {
                modeType = PetState.Normal;
            }
            else
            {
                modeType = PetStateConverterDict.TryGetValue(stateText.ToLower(), out var state) ? 
                    state : PetState.Normal;
                assetTags.Remove(stateText);
            }
        }

        if (!Enum.TryParse(info[(gstr)"graph"], true, out PetActivityState activityType))
        {
            var tags = assetTags.ToList();
            var removedTags = new List<string>();

            var pairs = PetActivityConverterDict.Where(a =>
            {
                var awaitingRemoveList = new List<string>();
                
                if (!a.Key.All(b => assetTags.Any(c =>
                    {
                        if (!c.Equals(b, StringComparison.OrdinalIgnoreCase))
                            return false;
                        
                        awaitingRemoveList.Add(c);
                        return true;

                    })))
                    return false;
                
                var f = true;
                foreach (var key in awaitingRemoveList)
                {
                    if (!tags.Remove(key))
                    {
                        f = false;
                        break;
                    }
                        
                    if(!removedTags.Contains(key))
                        removedTags.Add(key);
                }

                return f;

            }).ToArray();

            if (pairs == null || pairs.Length == 0)
            {
                Console.WriteLine($"Unknown asset: {relativePath}, skipping...");
                return null;
                //Console.WriteLine($"Unknown asset: {relativePath}, use it as \"Dummy\" (none)...");
            }
            else
            {
                activityType = pairs.First().Value;
            }
            
            foreach (var p in removedTags)
            {
                assetTags.Remove(p);
            }
        }

        if (!Enum.TryParse(info[(gstr)"animat"], true, out GfxAnimationType gfxAnimationType))
        {
            var typeText = assetTags
                .FirstOrDefault(tag => GfxAnimationEnumConverterDict.ContainsKey(tag.ToLower()));

            if (typeText == null || !GfxAnimationEnumConverterDict.TryGetValue(typeText.ToLower(), out gfxAnimationType))
                gfxAnimationType = GfxAnimationType.Single;

            else
            {
                assetTags.Remove(typeText);
            }
        }

        var name = info.Info;
        double? lastRemovedNum = null;
        if (string.IsNullOrWhiteSpace(name))
        {
            while (assetTags.Count > 0 && (double.TryParse(assetTags.Last(), out var num) || assetTags.Last().StartsWith("~")))
            {
                lastRemovedNum = num;
                assetTags.Remove(assetTags.Last());
            }
            if (assetTags.Count > 0)
                name = string.Join('_', assetTags);
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            name = $"{activityType.ToString().ToLower()}{(lastRemovedNum.HasValue ? $"_{lastRemovedNum.ToString()}" : "")}";
        }

        return new PetGfxInfo
        {
            Name = name,
            Activity = activityType,
            State = modeType,
            Transition = gfxAnimationType,
            PathHashCode = dirPath.GetHashCode()
        };
    }

    private void LoadGfx(DirectoryInfo di, string rootPath)
    {
        if (File.Exists(Path.Combine(di.FullName, "info.lps")))
        {
            
        }

        var dirs = di.EnumerateDirectories()
            .ToArray();
        if (dirs.Length == 0)
        {
            var param = new Sub("rootPath", rootPath);
            var paths = di.GetFiles();

            var relativePath = Path.GetRelativePath(rootPath, di.FullName);

            switch (paths.Length)
            {
                case -1:
                case 0:
                    break;
                case 1:
                {
                    var name = paths.First();
                    //throw new NotSupportedException(
                    //$"The single frame gfx object is not ready. Asset path: {di.FullName}");
                    var pAnimLine = new Line("picture", "", "", param);
                    var gfxInfo = ParseByFilePath(name, pAnimLine);
                    
                    if(gfxInfo != null)
                        AddIndexInternal(gfxInfo.Value, relativePath);
                    break;
                }
                default:
                {
                    var name = paths.First();
                    
                    var pAnimLine = new Line("pnganimation", "", "", param);
                    var gfxInfo = ParseByFilePath(name, pAnimLine);
                    
                    if(gfxInfo != null)
                        AddIndexInternal(gfxInfo.Value, relativePath);
                    break;
                }
            }
        }
        else
        {
            foreach (var p in dirs)
            {
                LoadGfx(p, rootPath);
            }
        }
    }
}