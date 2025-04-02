using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using DynamicData;
using LinePutScript;
using VPet.Avalonia.Debugging;
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
            { ["default"], PetActivityState.CommonIdle },
            //{ "drink", }
            { ["idle"], PetActivityState.Idle },
            { ["idel"], PetActivityState.Idle },
            { ["state","stateone"], PetActivityState.Idle1 },
            { ["state","statetwo"], PetActivityState.Idle2 },
            { ["stateone"], PetActivityState.Idle1 },
            { ["statetwo"], PetActivityState.Idle2 },
            { ["move"], PetActivityState.Move },
            { ["say"], PetActivityState.Speaking },
            { ["raise", "dynamic"], PetActivityState.RaisedDynamic },
            { ["raise", "static"], PetActivityState.RaisedStatic },
            { ["shutdown"], PetActivityState.Shutdown },
            { ["startup"], PetActivityState.Startup },
            { ["sleep"], PetActivityState.Sleep },
            { ["switch", "down"], PetActivityState.StateDown },
            { ["switch", "up"], PetActivityState.StateUp },
            { ["switch", "hunger"], PetActivityState.StateHunger },
            { ["switch", "thirsty"], PetActivityState.StateThirsty },
            { ["touch", "body"], PetActivityState.TouchBody },
            { ["touch", "head"], PetActivityState.TouchHead },
            { ["work"], PetActivityState.Activity }
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

    private static List<string> unsupportedAssetsList = new ();

    internal static void PrintAllUnsupportedAssetsListInternal()
    {
        Console.WriteLine("Unsupported assets entries: ");
        foreach (var entry in unsupportedAssetsList)
        {
            Console.WriteLine(entry);
        }
    }
    
    /// <summary>
    /// Workaround to solve the strange construction of game assets indexing issue
    /// </summary>
    internal Dictionary<(PetActivityState activity, PetState health, string tag), 
            ImmutableDictionary<GfxAnimationType, ImmutableArray<PetGfxInfo>>>
        GfxGroupedVariantDict = new();
    
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

        var separators = new[]
        {
            Path.PathSeparator,
            Path.AltDirectorySeparatorChar,
            '\\',
            '/',
            '_'
            // Compatibility to VPetSimulator, because their asset naming uses underscore as separator.
        };

        string ReplaceCharsToChar(string s, char[] chs, char c)
        {
            foreach (var t in chs)
            {
                var i = -1;
                while ((i = s.IndexOf(t)) > -1)
                {
                    s = s.Remove(i, 1).Insert(i, c.ToString());
                }
            }

            return s;
        }

        var assetName = ReplaceCharsToChar(relativePath, separators, '\n');
        
        var assetTags = assetName.Split('\n')
            .Where(a => !string.IsNullOrWhiteSpace(a))
            .ToList();

        
        
        //Console.WriteLine($"Parsing tags {string.Join(',', assetTags)}");

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

            if (pairs?.Length == 0)
            {
                Console.WriteLine($"Unknown asset: {relativePath}, skipping...");
                unsupportedAssetsList.Add(relativePath);
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
        
        var tagsResultArray = assetTags.ToImmutableArray();
        if (tagsResultArray.Length == 0)
            tagsResultArray = ["NONE"];

        return new PetGfxInfo
        {
            Name = name,
            Activity = activityType,
            State = modeType,
            Transition = gfxAnimationType,
            PathHashCode = dirPath.GetHashCode(),
            Tags = tagsResultArray
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

    // Time to mapping up sequences list with variants
    public void AnalyseSequenceAutoGrouping()
    {
        var states = PetStateConverterDict.Values
            .GroupBy(a => a)
            .Select(a => a.First())
            .ToImmutableArray();

        var activities = PetActivityConverterDict.Values
            .GroupBy(a => a)
            .Select(a => a.First())
            .ToImmutableArray();

        var list = new List<(PetActivityState activity, PetState health)>();

        foreach (var activity in activities)
        {
            list.AddRange(states
                .Select(health => new ValueTuple<PetActivityState, PetState>(activity, health)));
        }

        var allIndexes = GetAllAssets().ToList();
        var maps = new Dictionary<(PetActivityState activity, PetState health, string tag), IReadOnlyList<PetGfxInfo>>();

        foreach (var filter in list)
        {
            var blocklist = new List<KeyValuePair<PetGfxInfo, string>>();
            
            IReadOnlyList<KeyValuePair<PetGfxInfo, string>> SearchLocal((PetActivityState activity, PetState health) valueTuple)
            {
                return allIndexes
                    .Where(a => a.Key.Activity == valueTuple.activity && a.Key.State == valueTuple.health)
                    .Where(a => !blocklist.Any(b => Equals(b.Key, a.Key)))
                    .ToImmutableArray();
            }
            
            var searchResult = SearchLocal(filter);

            while (searchResult.Count > 0)
            {
                var pair = searchResult.First();
                
                if(blocklist.Contains(pair))
                    continue;
                
                var info = pair.Key;
                // if the sequence is not grouped
                if (info.IsSingle)
                {
                    maps.TryAdd(
                        new ValueTuple<PetActivityState, PetState, string>(filter.activity, filter.health,
                            info.Tags.FirstOrDefault() ?? "NONE"),
                        [info]);
                    
                    blocklist.Add(pair);
                    allIndexes.Remove(pair);
                    searchResult = SearchLocal(filter);
                    continue;
                }

                // but if its grouped, then try to find out related sequences and group them up
                Console.WriteLine("Trying to mapping up sequence by using a single register:");
                Console.WriteLine($"{pair.Key}");

                var result = TryRecogniseSequenceGroupByTags(info, searchResult, filter) ??
                             TryRecogniseSequenceGroupByActivityAndHealthState(info, searchResult, filter);

                if (result != null)
                {
                    maps.TryAdd(
                        new ValueTuple<PetActivityState, PetState, string>(filter.activity, filter.health,
                            info.Tags.FirstOrDefault() ?? "NONE"), result.Sequences);
                    
                    blocklist.AddRange(result.Group);
                    allIndexes.RemoveMany(result.Group);
                    searchResult = SearchLocal(filter);
                }

                else
                {
                    blocklist.Add(pair);
                    searchResult = SearchLocal(filter);
                }
            }
        }

        var m = maps
            .Select(w => (w.Key,
                w.Value.GroupBy(a => a.Transition)
                    .ToImmutableDictionary(b => b.Key, c => c.ToImmutableArray())))
            .ToImmutableArray();

        GfxGroupedVariantDict = m
            .ToDictionary(a => a.Key, b => b.Item2);
    }

    // TODO: refactoring
    private static GfxSequenceGroupModel? TryRecogniseSequenceGroupByTags(PetGfxInfo info,
        IReadOnlyList<KeyValuePair<PetGfxInfo, string>> searchResult,
        (PetActivityState, PetState) filter)
    {
        // set up the simple analyser
        bool foundA = false, foundB = false, foundC = false;

        // Method 1: try match by using tags
                
        var tags = info.Tags.Reverse().ToImmutableList();

        foreach (var tag in tags)
        {
            var possibleGroup = searchResult.Where(a =>
                    a.Key.Activity == info.Activity && a.Key.State == info.State && a.Key.Tags.Contains(tag))
                .ToImmutableArray();

            foreach (var item in possibleGroup)
            {
                if (foundA && foundB && foundC)
                    break;

                switch (item.Key.Transition)
                {
                    case GfxAnimationType.Start:
                        foundA = true;
                        break;

                    case GfxAnimationType.Loop:
                        foundB = true;
                        break;

                    case GfxAnimationType.End:
                        foundC = true;
                        break;
                }
            }

            var founds = foundA && foundB && foundC;
            if (!founds)
            {
                DebuggerConsole.WriteLine(typeof(VPetSimGfxAssetsIndex), MessageSeverity.Error,
                    "Found invalid sequence group, ignoring...");
                continue;
            }

            var key = new ValueTuple<PetActivityState, PetState, string>(filter.Item1, filter.Item2,
                tags.FirstOrDefault());
            var result = new GfxSequenceGroupModel
            {
                Activity = key.Item1,
                Health = key.Item2,
                Tags = key.Item3, 
                Group = possibleGroup,
            };
                    
            DebuggerConsole.WriteLine(typeof(VPetSimGfxAssetsIndex), MessageSeverity.Info,
                $"Mapped {key.Item1} {key.Item2} {key.Item3} group. Groups: {possibleGroup.Length}");
            return result;
        }

        DebuggerConsole.WriteLine(typeof(VPetSimGfxAssetsIndex), MessageSeverity.Error,
            "What a savage, match by tags method failed.");
        
        DebuggerConsole.WriteLine(typeof(VPetSimGfxAssetsIndex), MessageSeverity.Error,
            "What a savage, match by activity and health state tags method failed.");
        return null;
    }

    private static GfxSequenceGroupModel? TryRecogniseSequenceGroupByActivityAndHealthState(PetGfxInfo info,
        IReadOnlyList<KeyValuePair<PetGfxInfo, string>> searchResult,
        (PetActivityState, PetState) filter)
    {
        // set up the simple analyser
        bool foundA = false, foundB = false, foundC = false;

        // Method 1: try match by using tags
                
        var tags = info.Tags.Reverse().ToImmutableList();

        foreach (var tag in tags)
        {
            var possibleGroup = searchResult.Where(a =>
                    a.Key.Activity == filter.Item1 && a.Key.State == filter.Item2)
                .ToImmutableArray();

            foreach (var item in possibleGroup)
            {
                if (foundA && foundB && foundC)
                    break;

                switch (item.Key.Transition)
                {
                    case GfxAnimationType.Start:
                        foundA = true;
                        break;

                    case GfxAnimationType.Loop:
                        foundB = true;
                        break;

                    case GfxAnimationType.End:
                        foundC = true;
                        break;
                }
            }

            var founds = foundA && foundB && foundC;
            if (!founds)
            {
                Console.WriteLine($"Found invalid sequence group, ignoring...");
                continue;
            }

            var key = new ValueTuple<PetActivityState, PetState, string>(filter.Item1, filter.Item2,
                tags.FirstOrDefault());
            var result = new GfxSequenceGroupModel
            {
                Activity = key.Item1,
                Health = key.Item2,
                Tags = key.Item3, 
                Group = possibleGroup,
            };
                    
            Console.WriteLine($"Mapped {key.Item1} {key.Item2} {key.Item3} group. Groups: {possibleGroup.Length}");
            return result;
        }


        return null;
    }
    /*
    private static bool CompareParentDirPathPrivate(string aPath, string dirName)
    {
        
    }*/
    
    

    /*
    private static GfxSequenceGroupModel? TryRecogniseSequenceGroupByParentDirPath(PetGfxInfo info,
        IReadOnlyList<KeyValuePair<PetGfxInfo, string>> searchResult,
        (PetActivityState, PetState) filter)
    {
        // set up the simple analyser
        bool foundA = false, foundB = false, foundC = false;

        // Method 1: try match by using tags
                
        var tags = searchResult
            .Select(a => a.Value)
            .SelectMany(a => a.Split('\\','/'))
            .ToImmutableList();

        foreach (var tag in tags)
        {
            var possibleGroup = searchResult.Where(a =>
                    a.Key.Activity == info.Activity && a.Key.State == info.State && a.Value.Contains(tag))
                .ToImmutableArray();

            foreach (var item in possibleGroup)
            {
                if (foundA && foundB && foundC)
                    break;

                switch (item.Key.Transition)
                {
                    case GfxAnimationType.Start:
                        foundA = true;
                        break;

                    case GfxAnimationType.Loop:
                        foundB = true;
                        break;

                    case GfxAnimationType.End:
                        foundC = true;
                        break;
                }
            }

            var founds = foundA && foundB && foundC;
            if (!founds)
            {
                Console.WriteLine($"Found invalid sequence group, ignoring...");
                continue;
            }

            var key = new ValueTuple<PetActivityState, PetState, string>(filter.Item1, filter.Item2,
                tags.FirstOrDefault());
            var result = new GfxSequenceGroupModel
            {
                Activity = key.Item1,
                Health = key.Item2,
                Tags = key.Item3, 
                Group = possibleGroup,
            };
                    
            Console.WriteLine($"Mapped {key.Item1} {key.Item2} {key.Item3} group. Groups: {possibleGroup.Length}");
            return result;
        }

        Console.WriteLine($"What a savage, match by tags method failed.");
        return null;
    }*/
}