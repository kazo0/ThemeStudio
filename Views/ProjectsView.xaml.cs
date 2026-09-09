using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ThemeStudio.Views;

public sealed partial class ProjectsView : UserControl
{
	public ProjectsView()
	{
		InitializeComponent();
		DataContext = App.State;
		NewProjectForm.Visibility = App.State.IsProjectFormOpen ? Visibility.Visible : Visibility.Collapsed;
		RefreshProjects();
	}

	private void RefreshProjects()
	{
		if (ProjectList is null) return;
		var projects = App.State.Projects.Where(p => p.Name.Contains(SearchInput.Text, StringComparison.OrdinalIgnoreCase)).ToArray();
		ProjectList.ItemsSource = projects;
		ProjectCount.Text = $"{projects.Length} projects";
		EmptyState.Visibility = projects.Length == 0 ? Visibility.Visible : Visibility.Collapsed;
	}

	private void OnSearchChanged(object sender, TextChangedEventArgs e) => RefreshProjects();
	private void OnNewProject(object sender, RoutedEventArgs e)
	{
		NewProjectForm.Visibility = Visibility.Visible;
		App.State.IsProjectFormOpen = true;
		ProjectNameInput.Focus(FocusState.Programmatic);
	}
	private void OnCancelProject(object sender, RoutedEventArgs e)
	{
		App.State.IsProjectFormOpen = false;
		NewProjectForm.Visibility = Visibility.Collapsed;
	}
	private void OnCreateProject(object sender, RoutedEventArgs e)
	{
		var name = ProjectNameInput.Text.Trim();
		NameError.Visibility = name.Length == 0 ? Visibility.Visible : Visibility.Collapsed;
		if (name.Length == 0) return;
		App.State.Projects.Insert(0, new ProjectItem(name, "New project", "AM", "Alex Morgan", 0, "Planning"));
		App.State.Status = $"Created “{name}”. Switch themes — your project stays here.";
		ProjectNameInput.Text = string.Empty;
		SearchInput.Text = string.Empty;
		NewProjectForm.Visibility = Visibility.Collapsed;
		App.State.IsProjectFormOpen = false;
		RefreshProjects();
	}
}
