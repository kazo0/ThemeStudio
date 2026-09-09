using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ThemeStudio.Views;

public sealed partial class OverviewView : UserControl
{
	public OverviewView()
	{
		InitializeComponent();
		ActiveProjectsCount.Text = App.State.Projects.Count.ToString();
	}

	public event EventHandler? OpenProjectsRequested;

	private void OnOpenProjects(object sender, RoutedEventArgs e)
		=> OpenProjectsRequested?.Invoke(this, EventArgs.Empty);

	private void OnSizeChanged(object sender, SizeChangedEventArgs e)
	{
		var compact = e.NewSize.Width < 600;
		var narrow = e.NewSize.Width < 480;
		HeroTextColumn.Width = new GridLength(compact ? 1 : 3, GridUnitType.Star);
		HeroArtColumn.Width = compact ? new GridLength(0) : new GridLength(2, GridUnitType.Star);
		Grid.SetColumn(HeroArt, compact ? 0 : 1);
		Grid.SetRow(HeroArt, compact ? 1 : 0);
		Grid.SetColumnSpan(HeroText, compact ? 2 : 1);
		Grid.SetColumnSpan(HeroArt, compact ? 2 : 1);
		HeroArt.MaxHeight = compact ? 170 : 245;

		MetricColumn2.Width = narrow ? new GridLength(0) : new GridLength(1, GridUnitType.Star);
		MetricColumn3.Width = narrow ? new GridLength(0) : new GridLength(1, GridUnitType.Star);
		Grid.SetColumn(Metric2, narrow ? 0 : 1);
		Grid.SetColumn(Metric3, narrow ? 0 : 2);
		Grid.SetRow(Metric2, narrow ? 1 : 0);
		Grid.SetRow(Metric3, narrow ? 2 : 0);
		Grid.SetColumnSpan(Metric1, narrow ? 3 : 1);
		Grid.SetColumnSpan(Metric2, narrow ? 3 : 1);
		Grid.SetColumnSpan(Metric3, narrow ? 3 : 1);

		ProjectColumn2.Width = compact ? new GridLength(0) : new GridLength(1, GridUnitType.Star);
		Grid.SetColumn(Project2, compact ? 0 : 1);
		Grid.SetRow(Project2, compact ? 1 : 0);
		Grid.SetColumnSpan(Project1, compact ? 2 : 1);
		Grid.SetColumnSpan(Project2, compact ? 2 : 1);
	}
}
