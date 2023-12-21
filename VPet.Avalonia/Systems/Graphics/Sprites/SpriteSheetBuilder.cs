using System.Collections.Immutable;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Media.Imaging;

namespace VPet.Avalonia.Systems.Graphics.Sprites;

/// <summary>
/// Sprite sheet sequence creator
/// </summary>
public class SpriteSheetBuilder : IDisposable
{
    private List<Bitmap> _appendedPics;
    private uint _targetSize;
    
    /// <summary>
    /// Create a instance
    /// </summary>
    /// <param name="size">The cell size</param>
    public SpriteSheetBuilder(uint size)
    {
        _appendedPics = new List<Bitmap>();
        _targetSize = size;
    }

    public void Append(Stream iStream)
    {
        using var bitmap = new Bitmap(iStream);
        Append(bitmap);
    }

    public void Append(Bitmap bitmap)
    {
        var pxSize = bitmap.PixelSize;
        
        if (pxSize.Height != pxSize.Width)
            throw new NotSupportedException("Only supports bitmaps with 1:1 aspect ratio (Height are same as Width).");
        
        _appendedPics.Add(bitmap
            .CreateScaledBitmap(new PixelSize((int)_targetSize, (int)_targetSize)));
    }

    /// <summary>
    /// Save the sprite sequences into the file stream. Please be make sure the disk space is enough.
    /// </summary>
    /// <param name="oStream">The output stream, it must be writable or you will get error.</param>
    public void SaveTo(Stream oStream)
    {
        SaveTo_HorizontalArrange(oStream);
        /*
        var array = _appendedPics.ToImmutableArray();
        
        var totalCount = array.Length;
        
        var columnCount = (int)Math.Ceiling(Math.Sqrt(totalCount));
        var rowCount = columnCount;
        var powerTwo = columnCount * columnCount;
        var wastedCellSpaces = powerTwo - totalCount;
        
        // If its fit to arrange it by quad-like
        if (wastedCellSpaces / (double)powerTwo < 0.1 && powerTwo != 1)
        {
            SaveTo_2DArrayArrange(oStream, array, columnCount, rowCount);
        }
        else if (CalculateUnevenGridPrivate(totalCount, 1, out columnCount, out rowCount))
        {
            SaveTo_2DArrayArrange(oStream, array, columnCount, rowCount);
        }
        else
        {
            SaveTo_HorizontalArrange(oStream);
        }*/
    }

    /// <summary>
    /// Find possible options to create sprites.
    /// </summary>
    // TODO: Improve algorithm
    private bool CalculateUnevenGridPrivate(int counts, int tolerate, out int w, out int h)
    {
        w = 0;
        h = 0;
        
        var combinations = new List<(int, int)?>();

        var length = Math.Ceiling(Math.Sqrt((double)(counts * counts) / 2));
        
        for (var a = 2; a <= length; ++a)
        {
            var r = counts % a;
            var d = a - r;
            if (r != 0)
            {
                if (d > tolerate && d < 0)
                    continue;

                r = counts / a;
            }
            
            combinations.Add(new ValueTuple<int, int>(a, r));
        }

        // No combinations found.
        if (combinations.Count == 0)
        {
            return false;
        }

        var selected = combinations.FirstOrDefault(a =>
        {
            if (!a.HasValue)
                return false;
            return a.Value.Item1 * a.Value.Item2 >= counts;
        });

        if (selected == null)
            return false;
        
        w = Math.Max(selected.Value.Item2, selected.Value.Item1);
        h = Math.Min(selected.Value.Item2, selected.Value.Item1);
        return true;
    }

    // Somehow, c# is just sucks when you cant negotiate memory directly without "unsafe".
    // I have to put "unsafe" here to allow this method to direct negotiate memory
    // temporarily due to algorithms dependency.
    private unsafe void SaveTo_HorizontalArrange(Stream oStream)
    {
        var array = _appendedPics.ToImmutableArray();

        //Arrange sprite vertically
        // TODO : Better arrangement algorithm
        var bytes = 4;                              // A pixel bytes size (RGBA)
        var size = (int) _targetSize;
        var totalWidth = array.Length * size;
        var stride = size * bytes;

        using var newBitmap = new WriteableBitmap(new PixelSize(totalWidth, size), Vector.One * 96);
        using (var buffer = newBitmap.Lock())
        {
            var rowBytes = buffer.RowBytes;
            var bufLen = rowBytes * size;
            var sourceBuf = Marshal.AllocHGlobal(bufLen);

            for (var i = 0; i < array.Length; i++)
            {
                array[i].CopyPixels(new PixelRect(0, 0, size, size), sourceBuf, bufLen, stride);

                var sourceBufPtr = (byte*)sourceBuf.ToPointer();
                
                var anchorPos = i * size * bytes;
                for (var y = 0; y < size; y++)
                {
                    var finalPos = buffer.Address + (y * rowBytes + anchorPos);
                    var finalPtr = (byte*)finalPos.ToPointer();

                    // Copy a row pixels into the writeable bitmap row with offset.
                    for (var x = 0; x < stride; x++)
                    {
                        finalPtr[x] = sourceBufPtr[x + (y * stride)];
                    }
                }
            }
        }
        
        newBitmap.Save(oStream, 100);
    }

    // Save the sequences into the file stream and arrange sequences vertically.
    private void SaveTo_VerticalArrange(Stream oStream)
    {
        var array = _appendedPics.ToImmutableArray();

        //Arrange sprite vertically
        var size = (int) _targetSize;
        var totalHeight = array.Length * size;
        var stride = size * 4;

        using var newBitmap = new WriteableBitmap(new PixelSize(size, totalHeight), Vector.One * 96);
        using (var buffer = newBitmap.Lock())
        {
            var bufLen = buffer.RowBytes * size;
            for (var i = 0; i < array.Length; i++)
            {
                var sprite = array[i];
                var offset = (size * size * 4) * i ;
                sprite.CopyPixels(new PixelRect(0, 0, size, size), buffer.Address + offset, bufLen, stride);
            }
        }
        
        newBitmap.Save(oStream, 100);
    }

    private unsafe void SaveTo_2DArrayArrange(Stream oStream, ImmutableArray<Bitmap> input, int columnCount, int rowCount)
    {
        const int pixelBytes = 4; // A pixel bytes size (RGBA)
        var size = (int) _targetSize;
        var totalWidth = columnCount * size;
        var totalHeight = rowCount * size;
        var stride = size * pixelBytes;

        var rgba = new byte[4];

        using var newBitmap = new WriteableBitmap(new PixelSize(totalWidth, totalHeight), Vector.One * 96);
        using (var buffer = newBitmap.Lock())
        {
            var rowBytes = buffer.RowBytes;
            var bufLen = rowBytes * size;
            var sourceBuf = Marshal.AllocHGlobal(bufLen);
            
            for (var i = 0; i < input.Length; i++)
            {
                // Grid column
                var u = i % columnCount;
                
                // Grid row
                var v = i / columnCount;
                
                input[i].CopyPixels(new PixelRect(0, 0, size, size), sourceBuf, bufLen, stride);

                var sourceBufPtr = (byte*)sourceBuf.ToPointer();
                var bufferPtr = (byte*)buffer.Address.ToPointer();
                
                for (var sY = 0; sY < size; sY++)
                {
                    var sRow = sY * size;
                    var dRow = v * size * totalWidth + sY * totalWidth;
                    var col = u * size;
                    
                    // Copy a row pixels into the writeable bitmap row with offset.
                    for (var x = 0; x < size; x++)
                    {
                        var dX = col + x;
                        var indexToDest = dRow + dX;
                        var sourceIndex = sRow + x;
                        
                        for (var j = 0; j < pixelBytes; j++)
                        {
                            var indexToDestByte = indexToDest * pixelBytes + j;
                            var sourceIndexByte = sourceIndex * pixelBytes + j;
                            
                            bufferPtr[indexToDestByte] = sourceBufPtr[sourceIndexByte];
                            rgba[j] = sourceBufPtr[sourceIndexByte];
                        }
                    }
                }
            }
        }
        
        newBitmap.Save(oStream, 100);
    }

    public void Dispose()
    {
        foreach (var bitmap in _appendedPics)
        {
            bitmap.Dispose();
        }
        
        _appendedPics.Clear();
    }
}