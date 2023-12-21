using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace VPet.Avalonia.Extensions;

public static class BitmapExtensions
{
    public static byte GetPixelSize(this PixelFormat format)
    {
        return (byte)(format.BitsPerPixel / 8);
    }

    public static long GetUsedBytes(this Bitmap bitmap)
        => bitmap.PixelSize.Width * bitmap.PixelSize.Height * (bitmap.Format?.GetPixelSize() ?? 0);

    /// <summary>
    /// Use it only with bitmap, not vector graphics!!!!
    /// </summary>
    public static long GetUsedBytes(this IImage image)
    {
        if(image is Bitmap bitmap)
            return bitmap.PixelSize.Width * bitmap.PixelSize.Height * (bitmap.Format?.GetPixelSize() ?? 0);

        return 0;
    }
}