using Microsoft.UI.Xaml;

namespace ThemeStudio;

public sealed partial class App : Application
{
	internal static string[] Arguments { get; set; } = [];
	private static Window? _mainWindow;
	internal static Window MainWindow => _mainWindow ?? throw new InvalidOperationException("The window has not launched.");
	internal static StudioState State { get; } = new();
	internal static ThemeController Themes { get; } = new(State);

	public App() => InitializeComponent();

	protected override void OnLaunched(LaunchActivatedEventArgs args)
	{
		Themes.Install();
		_mainWindow = new Window { Title = "Forma — Theme Studio" };
		MainWindow.Content = new MainPage();
#if !__WASM__
		MainWindow.AppWindow.Resize(new Windows.Graphics.SizeInt32 { Width = 1500, Height = 1050 });
		if (Arguments.Contains("--smoke") || Arguments.Contains("--blog")) ((FrameworkElement)MainWindow.Content).Loaded += OnSmokeLoaded;
#endif
		MainWindow.Activate();
	}

#if !__WASM__
	private async void OnSmokeLoaded(object sender, RoutedEventArgs e)
	{
		try
		{
			((FrameworkElement)sender).Loaded -= OnSmokeLoaded;
			await (Arguments.Contains("--blog") ? BlogCaptures.RunAsync() : SmokeRunner.RunAsync());
		}
		catch (Exception exception)
		{
			Console.Error.WriteLine(exception);
			Environment.Exit(1);
		}
	}
#endif
}
