using Uno.UI.Hosting;

namespace ThemeStudio;

internal static class Program
{
	public static async Task Main(string[] args)
	{
		App.Arguments = args;
		await UnoPlatformHostBuilder.Create()
			.App(() => new App())
			.UseWebAssembly()
			.Build()
			.RunAsync();
	}
}
