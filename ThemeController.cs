using System.Globalization;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Uno.Cupertino;
using Uno.Material;
using Uno.Simple;
using Uno.Themes;
using Windows.UI;

namespace ThemeStudio;

internal sealed class ThemeController(StudioState state)
{
	private BaseTheme? _active;
	private readonly Dictionary<string, double> _fontSizes = new();
	internal BaseTheme Active => _active ?? throw new InvalidOperationException("Install a theme first.");
	internal static readonly string[] TypographySlots =
	[
		"DisplayLarge", "DisplayMedium", "DisplaySmall", "HeadlineLarge", "HeadlineMedium", "HeadlineSmall",
		"TitleLarge", "TitleMedium", "TitleSmall", "BodyLarge", "BodyMedium", "BodySmall", "LabelLarge",
		"LabelMedium", "LabelSmall", "LabelExtraSmall", "CaptionLarge", "CaptionMedium", "CaptionSmall"
	];

	internal void Install()
	{
		var dictionaries = Application.Current.Resources.MergedDictionaries;
		if (_active is not null) dictionaries.Remove(_active);
		_active = state.DesignSystem switch
		{
			DesignSystem.Material => new MaterialTheme(),
			DesignSystem.Cupertino => new CupertinoTheme(),
			_ => new SimpleTheme()
		};
		dictionaries.Add(_active);
		_fontSizes.Clear();
		foreach (var slot in TypographySlots)
		{
			var key = slot + "FontSize";
			if (Application.Current.Resources.TryGetValue(key, out var value) && value is double size)
				_fontSizes[key] = size;
		}
		ApplyTokens();
		ApplyColors();
	}

	internal void ApplyColors()
	{
		Active.Colors = state.UseSeeds
			? new ThemeColors
			{
				PrimarySeed = ParseColor(state.PrimarySeed),
				SecondarySeed = ParseColor(state.SecondarySeed),
				TertiarySeed = ParseColor(state.TertiarySeed)
			}
			: null;
	}

	internal void ApplyTokens()
	{
		Active.DefaultCornerRadius = state.CornerRadius;
		Active.DefaultSpacing = state.Spacing;
		Active.DefaultDensity = (Density)state.DensityIndex;
		if (state.FontIndex == 0) Active.ClearValue(BaseTheme.DefaultFontFamilyProperty);
		else Active.DefaultFontFamily = new FontFamily(state.FontIndex == 1
			? "ms-appx:///Uno.Fonts.Inter/Fonts/Inter.ttf#Inter"
			: "ms-appx:///Uno.Fonts.Roboto/Fonts/Roboto.ttf#Roboto");
		var fonts = new ResourceDictionary();
		foreach (var (key, size) in _fontSizes) fonts[key] = size * state.TypeScale;
		Active.FontOverrideDictionary = fonts;
	}

	internal void Refresh(FrameworkElement root)
	{
		var appearance = state.IsDark ? ElementTheme.Dark : ElementTheme.Light;
		root.RequestedTheme = appearance == ElementTheme.Dark ? ElementTheme.Light : ElementTheme.Dark;
		root.RequestedTheme = appearance;
	}

	internal static bool TryParseColor(string text, out Color color)
	{
		color = default;
		var hex = text.Trim().TrimStart('#');
		if (hex.Length != 6 || !uint.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var rgb)) return false;
		color = Color.FromArgb(255, (byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb);
		return true;
	}

	internal static Color ParseColor(string text) => TryParseColor(text, out var color)
		? color : throw new ArgumentException("Enter a six-digit hex color.", nameof(text));
}
