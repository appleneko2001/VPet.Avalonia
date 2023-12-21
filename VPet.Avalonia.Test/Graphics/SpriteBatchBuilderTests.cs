using System;
using System.IO;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Headless.NUnit;
using Avalonia.Media.Imaging;
using NUnit.Framework;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using VPet.Avalonia.Systems.Graphics.Sprites;

namespace VPet.Avalonia.Test.Graphics;

public class SpriteBatchBuilderTests
{
    private Stream[] _tileStreams = null!;
    private const int TileSize = 4;
    private const int TileCounts = 4;

    private AppBuilder? _instance;
    
    [OneTimeSetUp]
    public unsafe void Setup()
    {
        const int w = TileSize;
        const int h = TileSize;
        const int pixelSize = sizeof(int);
        
        var random = new Random(DateTime.Now.Nanosecond);
        _tileStreams = new Stream[TileCounts];

        for (var i = 0; i < TileCounts; i++)
        {
            var tile = new Image<Rgba32>(w, h);
            var rgba = new byte[pixelSize];

            random.NextBytes(rgba);

            if (!tile.DangerousTryGetSinglePixelMemory(out var mem))
                throw new NullReferenceException("Unable to access image data buffer.");
            
            var ptr = (byte*)mem.Pin().Pointer;
            
            for (var j = 0; j < w * h * pixelSize; j+= pixelSize)
            {
                for (var c = 0; c < pixelSize; c++)
                {
                    ptr[j + c] = rgba[c];
                }
            }
            
            var stream = new MemoryStream();
            tile.SaveAsBmp(stream);
            stream.Seek(0, SeekOrigin.Begin);
            
            _tileStreams[i] = stream;
        }
    }

    [OneTimeTearDown]
    public void Finalise()
    {
        foreach (var tile in _tileStreams)
        {
            tile.Close();
        }

        _tileStreams = Array.Empty<Stream>();
        _instance = null;
    }
    
    [AvaloniaTest]
    public unsafe void TestBuildAtlasGrid()
    {
        using var outBuf = new MemoryStream();
        using (var builder = new SpriteSheetBuilder(TileSize))
        {
            foreach (var tile in _tileStreams)
            {
                builder.Append(tile);
            }
        
            builder.SaveTo(outBuf);
        }

        var errors = 0;

        var testBitmap = new Bitmap(outBuf);

        var oneSideSize = TileSize * (int)Math.Sqrt(TileCounts);
        var size = new PixelSize(oneSideSize, oneSideSize);
        if (testBitmap.PixelSize != size)
            throw new ArgumentException("Output bitmap size dis-matched. " +
                                        $"Expecting: {size}, Actual: {testBitmap.PixelSize}");

        var stride = testBitmap.PixelSize.Width * sizeof(int);
        var testBufSize = testBitmap.PixelSize.Width *
                              testBitmap.PixelSize.Height * sizeof(int);
        var testBuf = Marshal.AllocHGlobal(testBufSize);

        if (testBuf == IntPtr.Zero)
            throw new NullReferenceException("Memory allocation failed.");
        
        var testBufPtr = (byte*)testBuf.ToPointer();

        testBitmap.CopyPixels(new PixelRect(0, 0, testBitmap.PixelSize.Width, testBitmap.PixelSize.Height),
            testBuf, testBufSize, stride);
        
        for (var i = 0; i < testBufSize; i++)
        {
        }
    }
}