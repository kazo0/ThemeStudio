#if !__WASM__
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;
using Windows.Storage;

namespace ThemeStudio;

// `--blog`: captures the stills and seed-sweep frames used by the kazo0.dev
// "Simple Design and Semantic Tokens" post. Output lands in bin/.../BlogShots.
internal static class BlogCaptures
{
	private static string OutputDirectory => Path.Combine(AppContext.BaseDirectory, "BlogShots");
	private static StudioState State => App.State;

	internal static async Task RunAsync()
	{
		Directory.CreateDirectory(OutputDirectory);
		await Task.Delay(600);

		// Hero: the same Overview screen under Simple and Material, design panel hidden.
		State.PanelVisible = false;
		Resize(1040, 1000);
		await ReloadAsync();
		await CaptureAsync("hero-simple");
		State.DesignSystem = DesignSystem.Material;
		await ReloadAsync();
		await CaptureAsync("hero-material");

		// Density: Simple Settings screen at Compact / Regular / Comfy.
		State.DesignSystem = DesignSystem.Simple;
		State.Page = "Settings";
		Resize(700, 1000);
		foreach (var (index, name) in new[] { (0, "compact"), (1, "regular"), (2, "comfy") })
		{
			State.DensityIndex = index;
			await ReloadAsync();
			await CaptureAsync($"density-{name}");
		}

		// Seed sweep: Overview with the panel visible, primary seed hue rotating a full turn.
		State.DensityIndex = 1;
		State.Page = "Overview";
		State.PanelVisible = false;
		Resize(1500, 1050);
		await ReloadAsync();
		// Hold the default grayscale palette, flip Seed colors on, then sweep the hue wheel.
		var frame = 0;
		for (; frame < 10; frame++) await CaptureAsync($"seed-{frame:000}");
		State.UseSeeds = true;
		await Task.Delay(400);
		const int sweep = 60;
		for (var step = 0; step < sweep; step++, frame++)
		{
			var hue = (250 + 360.0 * step / sweep) % 360;
			State.PrimarySeed = Hex(hue, 0.6, 0.8);
			App.Themes.ApplyColors();
			await Task.Delay(320);
			await CaptureAsync($"seed-{frame:000}");
		}

		Console.WriteLine($"BLOG SHOTS DONE: {OutputDirectory}");
		Environment.Exit(0);
	}

	private static void Resize(int width, int height) =>
		App.MainWindow.AppWindow.Resize(new Windows.Graphics.SizeInt32 { Width = width, Height = height });

	private static async Task ReloadAsync()
	{
		App.Themes.Install();
		App.MainWindow.Content = new MainPage();
		await Task.Delay(900);
	}

	private static string Hex(double hue, double saturation, double value)
	{
		var c = value * saturation;
		var x = c * (1 - Math.Abs(hue / 60 % 2 - 1));
		var m = value - c;
		var (r, g, b) = (int)(hue / 60) switch
		{
			0 => (c, x, 0d), 1 => (x, c, 0d), 2 => (0d, c, x), 3 => (0d, x, c), 4 => (x, 0d, c), _ => (c, 0d, x)
		};
		return $"#{(int)((r + m) * 255):X2}{(int)((g + m) * 255):X2}{(int)((b + m) * 255):X2}";
	}

	private static async Task CaptureAsync(string name)
	{
		var bitmap = new RenderTargetBitmap();
		await bitmap.RenderAsync(App.MainWindow.Content);
		var pixels = await bitmap.GetPixelsAsync();
		var path = Path.Combine(OutputDirectory, name + ".png");
		File.WriteAllBytes(path, []);
		var file = await StorageFile.GetFileFromPathAsync(path);
		using var stream = await file.OpenAsync(FileAccessMode.ReadWrite);
		var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
		encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied,
			(uint)bitmap.PixelWidth, (uint)bitmap.PixelHeight, 96, 96, pixels.ToArray());
		await encoder.FlushAsync();
	}
}
#endif
