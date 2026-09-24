using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ThemeStudio.Views;

namespace ThemeStudio;

public sealed partial class MainPage : Page
{
	private readonly StudioState _state = App.State;
	private bool _ready;

	public MainPage()
	{
		InitializeComponent();
		DataContext = _state;
		RequestedTheme = _state.IsDark ? ElementTheme.Dark : ElementTheme.Light;
		Inspector.ThemeChanged += OnThemeChanged;
		Inspector.TokensChanged += OnTokensChanged;
		_ready = true;
		Navigate(_state.Page);
		UpdateLabels();
	}

	internal void Navigate(string page)
	{
		_state.Page = page;
		PageContent.Content = page switch
		{
			"Projects" => new ProjectsView(),
			"Settings" => new SettingsView(),
			"Semantic lab" => new SemanticView(),
			_ => CreateOverview()
		};
		ContentScroll.ChangeView(null, 0, null);
		foreach (var nav in new[] { OverviewNav, ProjectsNav, SettingsNav, TokensNav })
			nav.Style = (Style)Application.Current.Resources[(string)nav.Tag == page ? "FilledTonalButtonStyle" : "TextButtonStyle"];
		CompactNavigation.SelectedItem = page;
	}

	private OverviewView CreateOverview()
	{
		var view = new OverviewView();
		view.OpenProjectsRequested += (_, _) => Navigate("Projects");
		return view;
	}

	private void OnNavigate(object sender, RoutedEventArgs e) => Navigate((string)((Button)sender).Tag);
	private void OnCompactNavigate(object sender, SelectionChangedEventArgs e)
	{
		if (_ready && CompactNavigation.SelectedItem is string page && page != _state.Page) Navigate(page);
	}

	private void OnThemeChanged(object? sender, EventArgs e)
	{
		// A new tree resolves the new theme's semantic style aliases and templates.
		// User-entered content lives in StudioState, outside that tree.
		App.Themes.Install();
		App.MainWindow.Content = new MainPage();
	}

	private void OnTokensChanged(object? sender, EventArgs e)
	{
		App.Themes.Refresh(this);
		if (PageContent.Content is SemanticView lab) lab.RefreshValues();
		UpdateLabels();
	}

	private void OnAppearanceClick(object sender, RoutedEventArgs e)
	{
		_state.IsDark = !_state.IsDark;
		App.Themes.Refresh(this);
		UpdateLabels();
		if (PageContent.Content is SemanticView lab) lab.RefreshValues();
	}

	private void UpdateLabels()
	{
		AppearanceButton.Content = _state.IsDark ? "Light mode" : "Dark mode";
		ThemeLabel.Text = $"{_state.DesignSystem.ToString().ToUpperInvariant()}  /  {(_state.IsDark ? "DARK" : "LIGHT")}";
		ArrangeWorkspace(ActualWidth);
	}

	private void OnPanelClick(object sender, RoutedEventArgs e)
	{
		_state.PanelVisible = !_state.PanelVisible;
		ArrangeWorkspace(ActualWidth);
	}

	private void OnLayoutSizeChanged(object sender, SizeChangedEventArgs e) => ArrangeWorkspace(e.NewSize.Width);

	private void ArrangeWorkspace(double width)
	{
		if (Workspace is null) return;
		var narrow = width < 1000;
		var overlay = width < 720;
		NavigationRail.Visibility = narrow ? Visibility.Collapsed : Visibility.Visible;
		Workspace.ColumnDefinitions[0].Width = new GridLength(narrow ? 0 : 180);
		CompactNavigation.Visibility = narrow ? Visibility.Visible : Visibility.Collapsed;
		StudioLabel.Visibility = width < 600 ? Visibility.Collapsed : Visibility.Visible;
		TopBar.Padding = new Thickness(width < 600 ? 12 : 24, 14, width < 600 ? 12 : 24, 14);
		AppearanceButton.Content = width < 600 ? (_state.IsDark ? "Light" : "Dark") : (_state.IsDark ? "Light mode" : "Dark mode");
		PanelButton.Content = width < 600 ? "Design" : "Design panel";
		PageContent.Padding = (Thickness)Application.Current.Resources[width < 600 ? "Space400Thickness" : "Space800Thickness"];
		InspectorBorder.Visibility = _state.PanelVisible ? Visibility.Visible : Visibility.Collapsed;
		Workspace.ColumnDefinitions[2].Width = new GridLength(_state.PanelVisible && !overlay ? 320 : 0);
		Grid.SetColumn(InspectorBorder, overlay ? 1 : 2);
		InspectorBorder.HorizontalAlignment = overlay ? HorizontalAlignment.Right : HorizontalAlignment.Stretch;
		InspectorBorder.Width = overlay ? Math.Min(320, Math.Max(0, width - 20)) : double.NaN;
	}
}
