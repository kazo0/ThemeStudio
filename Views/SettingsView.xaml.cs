using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ThemeStudio.Views;

public sealed partial class SettingsView : UserControl
{
	public SettingsView()
	{
		InitializeComponent();
		DataContext = App.State;
	}

	private void OnSaveClick(object sender, RoutedEventArgs e)
	{
		var valid = !string.IsNullOrWhiteSpace(App.State.WorkspaceName)
			&& System.Net.Mail.MailAddress.TryCreate(App.State.Email, out _);
		ValidationMessage.Visibility = valid ? Visibility.Collapsed : Visibility.Visible;
		if (valid) App.State.Status = "Preferences saved for this session. Your choices survive a theme switch.";
	}
}
