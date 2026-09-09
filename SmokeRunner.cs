#if !__WASM__
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Automation.Provider;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;
using Windows.Storage;

namespace ThemeStudio;

// Runs against the real visual tree, using the same controls a presenter uses.
// No test-only theme implementation or extra package dependency is involved.
internal static class SmokeRunner
{
	private static readonly List<string> Results = [];
	private static MainPage CurrentPage => App.MainWindow.Content as MainPage ?? throw new InvalidOperationException("The demo page is missing.");
	private static string OutputDirectory => Path.Combine(AppContext.BaseDirectory, "SmokeResults");

	internal static async Task RunAsync()
	{
		Directory.CreateDirectory(OutputDirectory);
		try
		{
			await Task.Delay(600);
			await CaptureAsync("01-simple-overview");
			var state = App.State;
			var page = CurrentPage;
			page.Navigate("Settings");
			await SettleAsync();
			Find<TextBox>("WorkspaceNameInput").Text = "A workspace that stays";
			Check(state.WorkspaceName == "A workspace that stays", "Text input updates durable state");
			var buttonStyle = Application.Current.Resources["FilledButtonStyle"];
			Click("MaterialButton");
			await SettleAsync();
			Check(App.Themes.Active is Uno.Material.MaterialTheme, "Material button installs MaterialTheme");
			var seedToggle = Find<ToggleSwitch>("SeedToggle");
			await Task.Delay(1500);
			var toggleState = Elements(seedToggle).SelectMany(VisualStateManager.GetVisualStateGroups)
				.First(group => group.Name == "ToggleStates").CurrentState?.Name;
			Check(seedToggle.IsOn && toggleState == "On", $"Material seed toggle renders its enabled state (actual: {toggleState})");
			var knobOn = Elements(seedToggle).First(element => element.Name == "KnobOn");
			var track = Elements(seedToggle).First(element => element.Name == "SwitchKnobBounds");
			var knobCenter = knobOn.TransformToVisual(track).TransformPoint(new Windows.Foundation.Point(knobOn.ActualWidth / 2, 0)).X;
			Check(knobOn.Opacity > 0.99 && knobCenter > track.ActualWidth / 2,
				$"Enabled Material knob is visibly on the right (opacity: {knobOn.Opacity}, center: {knobCenter}, track: {track.ActualWidth})");
			Check(!ReferenceEquals(buttonStyle, Application.Current.Resources["FilledButtonStyle"]), "Semantic key resolves a different style after switch");
			Check(Find<TextBox>("WorkspaceNameInput").Text == "A workspace that stays", "Form text survives Simple to Material switch");
			await CaptureAsync("02-material-settings");
			Click("SimpleButton");
			await SettleAsync();
			Check(App.Themes.Active is Uno.Simple.SimpleTheme, "Simple button restores SimpleTheme");
			Check(Find<TextBox>("WorkspaceNameInput").Text == "A workspace that stays", "Form text survives the return switch");

			var primary = (SolidColorBrush)Application.Current.Resources["PrimaryBrush"];
			var previous = primary.Color;
			Find<TextBox>("PrimaryInput").Text = "#127950";
			await SettleAsync();
			Check(ReferenceEquals(primary, Application.Current.Resources["PrimaryBrush"]), "Seed change preserves live brush identity");
			Check(primary.Color != previous, "Seed change recolors the existing brush");
			Find<TextBox>("PrimaryInput").Text = "#oops";
			await SettleAsync();
			Check(state.PrimarySeed == "#127950" && Find<TextBlock>("SeedError").Visibility == Visibility.Visible, "Invalid seed is rejected without changing the active palette");
			Find<TextBox>("PrimaryInput").Text = "#6956D8";
			Find<Slider>("SpacingSlider").Value = 6;
			Find<ComboBox>("DensityPicker").SelectedIndex = 0;
			Find<Slider>("CornerSlider").Value = 2;
			Find<ComboBox>("FontPicker").SelectedIndex = 2;
			var fontSize = (double)Application.Current.Resources["BodyMediumFontSize"];
			Find<Slider>("TypeSlider").Value = 1.2;
			await SettleAsync();
			Check((double)Application.Current.Resources["Space400"] == 18, "Spacing 6 × compact 0.75 × Space400 factor 4 resolves to 18");
			Check(((CornerRadius)Application.Current.Resources["Radius300CornerRadius"]).TopLeft == 6, "Corner base 2 generates Radius300 = 6");
			Check(Find<Border>("ProfileCard").CornerRadius.TopLeft == 6 && Find<Border>("ProfileCard").Padding.Left == 27, "Existing card re-resolves shape and spacing tokens");
			Check(Math.Abs((double)Application.Current.Resources["BodyMediumFontSize"] - fontSize * 1.2) < 0.01, "Type scale changes shared font size");
			Check(Find<TextBox>("WorkspaceNameInput").FontFamily.Source.Contains("Roboto"), "Typeface refresh reaches an existing input");
			await CaptureAsync("03-live-tokens");

			page = CurrentPage;
			page.Navigate("Projects");
			await SettleAsync();
			var count = state.Projects.Count;
			ClickContent("+ New project");
			Find<TextBox>("ProjectNameInput").Text = "Our next big idea";
			await SettleAsync();
			Click("MaterialButton");
			await SettleAsync();
			Check(Find<TextBox>("ProjectNameInput").Text == "Our next big idea" && Find<Border>("NewProjectForm").Visibility == Visibility.Visible, "Unsubmitted project draft survives a theme switch");
			ClickContent("Create project");
			await SettleAsync();
			Check(state.Projects.Count == count + 1 && state.Projects[0].Name == "Our next big idea", "Creating a project updates the workspace");
			Find<TextBox>("SearchInput").Text = "no matching project";
			await SettleAsync();
			Check(Find<TextBlock>("EmptyState").Visibility == Visibility.Visible, "Project search displays an empty state");
			Find<TextBox>("SearchInput").Text = "";
			Click("SimpleButton");
			await SettleAsync();
			Click("MaterialButton");
			await SettleAsync();
			Check(state.Projects.Count == count + 1, "Created project survives theme switch");
			Click("AppearanceButton");
			await SettleAsync();
			Check((CurrentPage).ActualTheme == ElementTheme.Dark, "Appearance control changes to dark");
			await CaptureAsync("04-material-dark-projects");
			(CurrentPage).Navigate("Semantic lab");
			await SettleAsync();
			Check(Find<TextBlock>("TokenValues").Text.Contains("Space400"), "Semantic lab resolves current tokens");
			var darkPrimary = ((SolidColorBrush)Find<Border>("PrimaryTile").Background).Color;
			Click("AppearanceButton");
			await SettleAsync();
			var lightPrimary = ((SolidColorBrush)Find<Border>("PrimaryTile").Background).Color;
			Check(darkPrimary != lightPrimary && Find<TextBlock>("PrimaryHex").Text == $"#{lightPrimary.R:X2}{lightPrimary.G:X2}{lightPrimary.B:X2}", "Semantic palette and hex labels follow the root appearance");
			await CaptureAsync("05-semantic-lab");
			Find<ComboBox>("PresetPicker").SelectedIndex = 2;
			await SettleAsync();
			Check(state.PrimarySeed == "#34785C" && state.DensityIndex == 2 && !state.IsDark, "Botanical preset updates color, density, and appearance");
			ClickContent("Reset design");
			await SettleAsync();
			Check(!state.IsMaterial && !state.IsDark && state.TypeScale == 1 && state.Spacing == 4, "Reset restores the baseline design");
			Check(state.WorkspaceName == "A workspace that stays", "Reset retains workspace edits");
			App.MainWindow.AppWindow.Resize(new Windows.Graphics.SizeInt32 { Width = 460, Height = 900 });
			(CurrentPage).Navigate("Overview");
			await SettleAsync();
			Click("PanelButton");
			await SettleAsync();
			Check(Find<ComboBox>("CompactNavigation").Visibility == Visibility.Visible, "Narrow viewport exposes compact navigation");
			await CaptureAsync("06-narrow-overview");
			await File.WriteAllLinesAsync(Path.Combine(OutputDirectory, "results.txt"), Results);
			Console.WriteLine($"SMOKE PASSED: {Results.Count} assertions. {OutputDirectory}");
			Environment.Exit(0);
		}
		catch (Exception exception)
		{
			Results.Add("FAIL: " + exception);
			await File.WriteAllLinesAsync(Path.Combine(OutputDirectory, "results.txt"), Results);
			throw;
		}
	}

	private static Task SettleAsync() => Task.Delay(450);
	private static void Check(bool condition, string description)
	{
		if (!condition) throw new InvalidOperationException(description);
		Results.Add("PASS: " + description);
		Console.WriteLine(Results[^1]);
	}

	private static IEnumerable<FrameworkElement> Elements(DependencyObject? parent)
	{
		if (parent is null) yield break;
		if (parent is FrameworkElement element) yield return element;
		for (var index = 0; index < VisualTreeHelper.GetChildrenCount(parent); index++)
			foreach (var child in Elements(VisualTreeHelper.GetChild(parent, index))) yield return child;
	}
	private static T Find<T>(string name) where T : FrameworkElement => Elements(App.MainWindow.Content)
		.OfType<T>().FirstOrDefault(element => element.Name == name)
		?? throw new InvalidOperationException($"Missing control: {name}");
	private static void Click(string name) => Invoke(Find<Button>(name));
	private static void ClickContent(string content) => Invoke(Elements(App.MainWindow.Content)
		.OfType<Button>().First(button => button.Content is string text && text == content));
	private static void Invoke(Button button) => ((IInvokeProvider)new ButtonAutomationPeer(button)).Invoke();

	private static async Task CaptureAsync(string name)
	{
		var bitmap = new RenderTargetBitmap();
		await bitmap.RenderAsync(App.MainWindow.Content);
		var pixels = await bitmap.GetPixelsAsync();
		var file = await StorageFile.GetFileFromPathAsync(CreateImageFile(name));
		using var stream = await file.OpenAsync(FileAccessMode.ReadWrite);
		var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
		encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied,
			(uint)bitmap.PixelWidth, (uint)bitmap.PixelHeight, 96, 96, pixels.ToArray());
		await encoder.FlushAsync();
	}
	private static string CreateImageFile(string name)
	{
		var path = Path.Combine(OutputDirectory, name + ".png");
		File.WriteAllBytes(path, []);
		return path;
	}
}
#endif
