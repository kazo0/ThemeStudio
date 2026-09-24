using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ThemeStudio;

public abstract class ObservableState : INotifyPropertyChanged
{
	public event PropertyChangedEventHandler? PropertyChanged;
	protected void Set<T>(ref T field, T value, [CallerMemberName] string? property = null)
	{
		if (EqualityComparer<T>.Default.Equals(field, value)) return;
		field = value;
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
	}
}

public enum DesignSystem
{
	Simple,
	Material,
	Cupertino
}

[Microsoft.UI.Xaml.Data.Bindable]
public sealed class StudioState : ObservableState
{
	public DesignSystem DesignSystem { get; set; }
	public bool IsDark { get; set; }
	public bool PanelVisible { get; set; } = true;
	public string Page { get; set; } = "Overview";
	public string PrimarySeed { get; set; } = "#6956D8";
	public string SecondarySeed { get; set; } = "#537C77";
	public string TertiarySeed { get; set; } = "#C78356";
	public bool UseSeeds { get; set; }
	public double CornerRadius { get; set; } = 4;
	public double Spacing { get; set; } = 4;
	public int DensityIndex { get; set; } = 1;
	public int FontIndex { get; set; }
	public double TypeScale { get; set; } = 1;
	private string _workspaceName = "Forma Studio";
	private string _email = "alex@forma.design";
	private bool _notifications = true;
	private bool _digest = true;
	private string _status = "All changes are local to this demo.";
	private string _projectQuery = "";
	private string _projectDraft = "";
	public bool IsProjectFormOpen { get; set; }
	public string ProjectQuery { get => _projectQuery; set => Set(ref _projectQuery, value); }
	public string ProjectDraft { get => _projectDraft; set => Set(ref _projectDraft, value); }
	public string WorkspaceName { get => _workspaceName; set => Set(ref _workspaceName, value); }
	public string Email { get => _email; set => Set(ref _email, value); }
	public bool Notifications { get => _notifications; set => Set(ref _notifications, value); }
	public bool Digest { get => _digest; set => Set(ref _digest, value); }
	public string Status { get => _status; set => Set(ref _status, value); }
	public ObservableCollection<ProjectItem> Projects { get; } =
	[
		new("Orbit brand system", "Brand identity", "JL", "Jun Lee", 72, "In progress"),
		new("A quieter kind of inbox", "Product design", "AM", "Alex Morgan", 45, "In progress"),
		new("Fieldnotes launch", "Website", "SK", "Sam Kim", 92, "In review"),
		new("Good things, daily", "Mobile experience", "EP", "Ellie Park", 28, "Planning")
	];
}

[Microsoft.UI.Xaml.Data.Bindable]
public sealed class ProjectItem(string name, string category, string initials, string owner, double progress, string status) : ObservableState
{
	public string Name { get; } = name;
	public string Category { get; } = category;
	public string Initials { get; } = initials;
	public string Owner { get; } = owner;
	public double Progress { get; } = progress;
	public string Status { get; } = status;
	private bool _completed;
	public bool Completed { get => _completed; set => Set(ref _completed, value); }
}
