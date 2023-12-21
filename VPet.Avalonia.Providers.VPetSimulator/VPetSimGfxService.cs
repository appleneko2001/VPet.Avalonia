using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Media.Imaging;
using VPet.Avalonia.Extensions;
using VPet.Avalonia.Modules;
using VPet.Avalonia.Services.Interfaces;
using VPet.Avalonia.Systems.Graphics;
using VPet.Avalonia.Systems.Graphics.Sprites;

namespace VPet.Avalonia.Providers.VPetSimulator;

public class VPetSimGfxService : IApplicationService, IGfxServiceInterface
{
    private VPetSimGfxAssetsIndex? _gfxAssetsIndex;
    private readonly Dictionary<string, string> _cacheIndexes = new ();
    private readonly Dictionary<string, SpriteSheetSequenceInfo> _sequences = new();
    private readonly Dictionary<string, long> _usedBytesList = new();

    private string _assetsRootPath = string.Empty;
    
    public long TotalUsedBytes => _usedBytesList.Sum(a => a.Value);
    
    public void Init(string baseAssetPath)
    {
        _assetsRootPath = baseAssetPath;
        _gfxAssetsIndex = new VPetSimGfxAssetsIndex(baseAssetPath);
    }

    public void CreateSpriteSheetCacheIfNotExists(string cacheFolder, uint cacheSize, Action<double>? progressCallback)
    {
        // TODO: put metadata at start of stream and validate it next time of start, re-create cache if its required.
        var assetsIndexes = _gfxAssetsIndex?.GetAllAssets()
            .ToArray();
        
        if(assetsIndexes == null)
            throw new InvalidOperationException("GFX assets index is not done!");
        
        var index = 0;
        var length = assetsIndexes.Length;

        Parallel.ForEach(assetsIndexes, new ParallelOptions
        {
            MaxDegreeOfParallelism = 4
        }, pair =>
        {
            index++;
            
            var cacheFileName = pair.Value.PathToHash();
            var cachedAssetPath = Path.Combine(cacheFolder, cacheFileName);
            
            var assetPath = Path.Combine(_assetsRootPath, pair.Value);
            progressCallback?.Invoke((double)index / length);

            if (!File.Exists(cachedAssetPath))
                CreateGraphicsCache_Sequence(cacheFolder, cacheFileName, assetPath, cacheSize);

            if (_cacheIndexes.TryAdd(pair.Value, cachedAssetPath))
                return;
            
            var exist = _cacheIndexes[pair.Value];
            if(cachedAssetPath != exist)
                throw new InvalidOperationException($"Unable to link the cached asset to the actual gfx asset path. ({pair.Value} to {cachedAssetPath})");
        });
    }

    public void CreateAnimationSequencesInfo(uint cacheSize)
    {
        foreach (var spriteAssetRelPath in _cacheIndexes.Keys)
        {
            var spriteAssetPath = Path.Combine(_assetsRootPath, spriteAssetRelPath);
            
            var files = Directory.EnumerateFiles(spriteAssetPath)
                .OrderBy(a => a)
                .ToArray();

            var seqInfo = new SpriteSheetSequenceInfo
            {
                CellSize = cacheSize,
                Name = _gfxAssetsIndex?.GetInfo(spriteAssetRelPath)?.ToString() ?? "NO_NAME" 
            };

            var index = -1;
            // Workaround for VPetSimulator
            foreach (var assetPartPath in files)
            {
                var assetName = Path.GetFileNameWithoutExtension(assetPartPath);
                var assetTags = assetName
                    .Split('_')
                    .Reverse()
                    .ToList();
                var delay = -1;
                
                for (var i = 0; i < 1; i++)
                {
                    var remove1 = default(string?);
                    switch (i)
                    {
                        case 0:
                            foreach (var tag in assetTags)
                            {
                                if (!double.TryParse(tag, out var v))
                                    continue;
                                
                                delay = (int)Math.Round(v);
                                remove1 = tag;
                                break;
                            }
                            break;
                    }

                    if (remove1 != null)
                        assetTags.Remove(remove1);
                }

                index++;
                
                // TODO: Get grid column and row numbers.

                var zone = new Rect
                (
                    x: index * cacheSize,
                    y: 0,
                    width: cacheSize,
                    height: cacheSize
                );
                
                seqInfo.Frames.Add(new SpriteSheetFrameInfo
                {
                    Index = index != -1 ? index : throw new ArgumentException($"Unable to parse index: {assetPartPath}"),
                    DelayMilliseconds = delay != -1 ? delay : throw new ArgumentException($"Unable to parse delay: {assetPartPath}"),
                    Area = zone
                });
            }
            
            var gfxAssetInfo = _gfxAssetsIndex?.FindKey(spriteAssetPath);

            if (!gfxAssetInfo.HasValue)
                throw new NullReferenceException();
            
            _sequences.Add(spriteAssetRelPath, seqInfo);
        }
    }

    /// <summary>
    /// Preload all gfx sequences asset to memory without create an actual instances for them. You have to create Bitmap instance manually.
    /// </summary>
    public void PreloadAllGfxSequences()
    {
        PreloadAllGfxSequencesPrivate(
            a => new LazyBitmapInstance(() => new Bitmap(a)));
    }

    /// <summary>
    /// Preload all gfx sequences asset to memory and create Bitmap instance. <i><b>This will uses a lot RAM memory</b></i> for more faster performance.
    /// </summary>
    public void PreloadAllGfxSequencesAndCreateInstance()
    {
        PreloadAllGfxSequencesPrivate(
            a => new BitmapInstance(CreateBitmapInstancePrivate(a)));
    }

    public void PreloadAllGfxSequencesWithHotLoadUnload()
    {
        PreloadAllGfxSequencesPrivate(
            a => new HotLoadBitmapInstance(() => new Bitmap(a)));
    }
    
    /// <summary>
    /// Preload all GFX assets and sequences data to RAM, instance will be initialised after acquirement, also it will be clear after not being used. 
    /// </summary>
    public void PreloadAllGfxSequencesWithHotLoadUnloadAndPreserveRam()
    {
        PreloadAllGfxSequencesPrivate(
            a =>
            {
                using var stream = File.OpenRead(a);
                return new LazyMemoryBitmapInstance(stream);
            });
    }

    private void PreloadAllGfxSequencesPrivate(Func<string, IBitmapInstance> factory)
    {
        foreach (var pair in _sequences)
        {
            var sequence = pair.Value;
            
            if(sequence.Sprite != null)
                continue;

            sequence.Sprite = factory(_cacheIndexes[pair.Key]); //new Bitmap(CacheIndexes[pair.Key]);
        }
    }

    public IEnumerable<SpriteSheetSequenceInfo> SearchSequences(Func<PetGfxInfo, bool> condition)
    {
        var assets = _gfxAssetsIndex?
            .Search(condition)
            .ToArray();

        return _sequences
            .Where(a => assets?.Contains(a.Key) ?? false)
            .Select(a => a.Value);
    }
    
    private void CreateGraphicsCache_Sequence(string cacheFolder, string cacheName, string assetPath, uint cacheSize)
    {
        var cacheFilePath = Path.Combine(cacheFolder, cacheName);
        
        var files = Directory.EnumerateFiles(assetPath)
            .OrderBy(a => a);
        
        // Load and creating graphics caches
        using var builder = new SpriteSheetBuilder(cacheSize);
        foreach (var filePath in files)
        {
            using var iStream = File.OpenRead(filePath);
            builder.Append(iStream);
        }

        var parentDir = Path.GetDirectoryName(cacheFilePath);
        Directory.CreateDirectory(parentDir!);

        using var oStream = File.Create(cacheFilePath);
        builder.SaveTo(oStream);
    }

    private Bitmap CreateBitmapInstancePrivate(string path)
    {
        var inst = CreateBitmapInstanceImpl_Common(path);
        _usedBytesList.Add(path, inst.GetUsedBytes());

        return inst;
    }

    private Bitmap CreateBitmapInstanceImpl_Common(string path)
    {
        return new Bitmap(path);
    }

    // Keep them if anything happened to the avaloniaUI until its being stable to use.
    /*
    private Bitmap CreateBitmapInstanceImpl_WriteableBitmap(string path)
    {
        using var iStream = File.OpenRead(path);
        return WriteableBitmap.Decode(iStream);
    }
    
    private Bitmap CreateBitmapInstanceImpl_CopyToRTB(string path)
    {
        using var iStream = File.OpenRead(path);
        using var iBitmap = new Bitmap(path);
        var rtb = new RenderTargetBitmap(iBitmap.PixelSize);
        using var ctx = rtb.CreateDrawingContext();
        {
            ctx.DrawImage(iBitmap, new Rect(iBitmap.Size));
        }

        return rtb;
    }

    private long CalculateUsedBytesPrivate(PixelSize size)
    {
        return size.Width * size.Height * sizeof(int);
    }
    */
    
    public void Dispose()
    {
        foreach (var pair in _sequences)
        {
            pair.Value.Sprite?.Dispose();
        }
    }
}