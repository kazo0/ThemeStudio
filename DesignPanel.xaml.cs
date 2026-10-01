using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;

namespace ThemeStudio;

public sealed partial class DesignPanel : UserControl
{
	public event EventHandler? ThemeChanged;
	public event EventHandler? TokensChanged;
	private readonly StudioState _state = App.State;
	private readonly DispatcherTimer _refresh = new() { Interval = TimeSpan.FromMilliseconds(120) };
	private bool _ready;
	private bool _colorPending;
	private bool _tokenPending;

	public DesignPanel()
	{
		InitializeComponent();
		LoadValues();
		_refresh.Tick += OnRefresh;
		Unloaded += (_, _) => _refresh.Stop();
	}

	private void LoadValues()
	{
		_ready = false;
		PrimaryInput.Text = _state.PrimarySeed;
		SecondaryInput.Text = _state.SecondarySeed;
		TertiaryInput.Text = _state.TertiarySeed;
		SeedToggle.IsOn = _state.UseSeeds;
		CornerSlider.Value = _state.CornerRadius;
		SpacingSlider.Value = _state.Spacing;
		DensityPicker.SelectedIndex = _state.DensityIndex;
		FontPicker.SelectedIndex = _state.FontIndex;
		TypeSlider.Value = _state.TypeScale;
		PresetPicker.SelectedIndex = 0;
		foreach (var button in new[] { SimpleButton, MaterialButton })
			button.Style = (Style)Application.Current.Resources[Enum.Parse<DesignSystem>((string)button.Tag) == _state.DesignSystem ? "FilledButtonStyle" : "FilledTonalButtonStyle"];
		UpdateLabels();
		_ready = true;
	}

	private void OnDesignSystemClick(object sender, RoutedEventArgs e) => SwitchTheme(Enum.Parse<DesignSystem>((string)((Button)sender).Tag));
	private void SwitchTheme(DesignSystem system)
	{
		if (_state.DesignSystem == system) return;
		_state.DesignSystem = system;
		_refresh.Stop();
		ThemeChanged?.Invoke(this, EventArgs.Empty);
	}

	private void OnSeedToggle(object sender, RoutedEventArgs e)
	{
		if (!_ready) return;
		_state.UseSeeds = SeedToggle.IsOn;
		_colorPending = true;
		ScheduleRefresh();
	}

	private void OnSeedTextChanged(object sender, TextChangedEventArgs e)
	{
		if (!_ready) return;
		var valid = ThemeController.TryParseColor(PrimaryInput.Text, out _)
			&& ThemeController.TryParseColor(SecondaryInput.Text, out _)
			&& ThemeController.TryParseColor(TertiaryInput.Text, out _);
		SeedError.Visibility = valid ? Visibility.Collapsed : Visibility.Visible;
		if (!valid) return;
		_state.PrimarySeed = PrimaryInput.Text;
		_state.SecondarySeed = SecondaryInput.Text;
		_state.TertiarySeed = TertiaryInput.Text;
		_colorPending = true;
		UpdateLabels();
		ScheduleRefresh();
	}

	private void OnColorPickerClick(object sender, RoutedEventArgs e) => SeedPicker.Color = ThemeController.ParseColor(_state.PrimarySeed);
	private void OnPickerColorChanged(ColorPicker sender, ColorChangedEventArgs args)
	{
		if (_ready) PrimaryInput.Text = $"#{args.NewColor.R:X2}{args.NewColor.G:X2}{args.NewColor.B:X2}";
	}

	private void OnTokenChanged(object sender, RangeBaseValueChangedEventArgs e) => ReadTokens();
	private void OnTokenSelectionChanged(object sender, SelectionChangedEventArgs e) => ReadTokens();
	private void ReadTokens()
	{
		if (!_ready) return;
		_state.CornerRadius = CornerSlider.Value;
		_state.Spacing = SpacingSlider.Value;
		_state.DensityIndex = DensityPicker.SelectedIndex;
		_state.FontIndex = FontPicker.SelectedIndex;
		_state.TypeScale = TypeSlider.Value;
		_tokenPending = true;
		UpdateLabels();
		ScheduleRefresh();
	}

	private void UpdateLabels()
	{
		CornerLabel.Text = $"Corner radius · {_state.CornerRadius:0.#} px base";
		SpacingLabel.Text = $"Spacing · {_state.Spacing:0.#} px base";
		TypeLabel.Text = $"Type scale · {_state.TypeScale:P0}";
		SeedSwatch.Background = new SolidColorBrush(ThemeController.ParseColor(_state.PrimarySeed));
	}

	private void ScheduleRefresh() => _refresh.Start();
	private void OnRefresh(object? sender, object e)
	{
		_refresh.Stop();
		if (_colorPending) App.Themes.ApplyColors();
		if (_tokenPending) App.Themes.ApplyTokens();
		_colorPending = _tokenPending = false;
		TokensChanged?.Invoke(this, EventArgs.Empty);
	}

	private void OnPresetChanged(object sender, SelectionChangedEventArgs e)
	{
		if (!_ready || PresetPicker.SelectedIndex <= 0) return;
		var choice = PresetPicker.SelectedIndex;
		(_state.PrimarySeed, _state.SecondarySeed, _state.TertiarySeed) = choice switch
		{
			2 => ("#34785C", "#667B4A", "#D1A059"),
			3 => ("#B85C3C", "#867359", "#788B63"),
			4 => ("#3579C8", "#457D8C", "#9870C2"),
			_ => ("#6956D8", "#537C77", "#C78356")
		};
		_state.UseSeeds = true;
		_state.IsDark = choice == 4;
		_state.CornerRadius = choice == 3 ? 2 : 4;
		_state.Spacing = 4;
		_state.DensityIndex = choice == 2 ? 2 : 1;
		LoadValues();
		_colorPending = _tokenPending = true;
		ScheduleRefresh();
	}

	private void OnResetClick(object sender, RoutedEventArgs e)
	{
		_state.DesignSystem = DesignSystem.Simple;
		_state.IsDark = false;
		_state.PrimarySeed = "#6956D8";
		_state.SecondarySeed = "#537C77";
		_state.TertiarySeed = "#C78356";
		_state.UseSeeds = true;
		_state.CornerRadius = _state.Spacing = 4;
		_state.DensityIndex = 1;
		_state.FontIndex = 0;
		_state.TypeScale = 1;
		_refresh.Stop();
		ThemeChanged?.Invoke(this, EventArgs.Empty);
	}
}
