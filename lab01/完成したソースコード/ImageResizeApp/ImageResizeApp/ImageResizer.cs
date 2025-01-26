using SkiaSharp;

namespace ImageResizeApp;
public class ImageResizer
{
    public byte[] ResizeImage(string name, 
        byte[] image, 
        int width)
    {
        var extension = Path.GetExtension(name);
        var imageFormat = extension.ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => SKEncodedImageFormat.Jpeg,
            ".png" => SKEncodedImageFormat.Png,
            ".bmp" => SKEncodedImageFormat.Bmp,
            ".gif" => SKEncodedImageFormat.Gif,
            _ => throw new NotSupportedException($"{extension} はサポートされていません。")
        };

        using var bitmap = SKBitmap.Decode(image);
        var height = (int)(width * (float)bitmap.Height / bitmap.Width);
        using var thumbnail = bitmap.Resize(new SKSizeI(width, height), SKSamplingOptions.Default);
        using var data = thumbnail.Encode(imageFormat, 50);
        return data.ToArray();
    }
}
