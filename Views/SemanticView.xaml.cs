using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace ThemeStudio.Views;

public sealed partial class SemanticView : UserControl
{
	public SemanticView()
	{
		InitializeComponent();
		Loaded += (_, _) => RefreshValues();
	}

	internal void RefreshValues()
	{
		var resources = Application.Current.Resources;
		foreach (var role in new[] { "Primary", "Secondary", "Tertiary", "Error" })
		{
			foreach (var suffix in new[] { "", "Container" })
			{
				var key = role + suffix;
				// Read the realized ThemeResource: app-level lookups use the application's
				// appearance instead of this root's requested appearance.
				if (((Border)FindName(key + "Tile")).Background is SolidColorBrush brush)
				{
					var color = brush.Color;
					((TextBlock)FindName(key + "Hex")).Text = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
				}
			}
		}
		TokenValues.Text = $"Space400   {resources["Space400"]:0.##} px\n"
			+ $"Space600   {resources["Space600"]:0.##} px\n"
			+ $"Radius300   {((CornerRadius)resources["Radius300CornerRadius"]).TopLeft:0.##} px\n"
			+ $"BodyMediumFontSize   {resources["BodyMediumFontSize"]:0.##} px\n"
			+ $"Density   {App.Themes.Active.DefaultDensity}\n"
			+ "Density multiplies spacing; fixed control heights stay constant.";
	}

	private void OnActionClick(object sender, RoutedEventArgs e) => App.State.Status = $"“{((Button)sender).Content}” uses the same semantic style in both themes.";
}
