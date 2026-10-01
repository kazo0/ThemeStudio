using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml.Media.Imaging;
using QRCoder;

namespace ThemeStudio;

internal static class RepoLink
{
	internal const string Url = "https://github.com/kazo0/ThemeStudio";

	// Draws the code at whole pixels per module so its edges stay sharp; the matrix already includes the quiet zone.
	internal static WriteableBitmap CreateQrCode(int moduleSize = 4)
	{
		using var generator = new QRCodeGenerator();
		using var data = generator.CreateQrCode(Url, QRCodeGenerator.ECCLevel.M);
		var matrix = data.ModuleMatrix;
		var size = matrix.Count * moduleSize;
		var pixels = new byte[size * size * 4];
		for (var y = 0; y < size; y++)
			for (var x = 0; x < size; x++)
			{
				var shade = matrix[y / moduleSize][x / moduleSize] ? (byte)0 : (byte)255;
				var offset = (y * size + x) * 4;
				pixels[offset] = pixels[offset + 1] = pixels[offset + 2] = shade;
				pixels[offset + 3] = 255;
			}
		var bitmap = new WriteableBitmap(size, size);
		using (var stream = bitmap.PixelBuffer.AsStream()) stream.Write(pixels);
		bitmap.Invalidate();
		return bitmap;
	}
}
