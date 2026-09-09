using Uno.UI.Hosting;

namespace ThemeStudio;

internal static class Program
{
	[STAThread]
	public static void Main(string[] args)
	{
		App.Arguments = args;
		UnoPlatformHostBuilder.Create()
			.App(() => new App())
			.UseWin32()
			.UseX11()
			.UseLinuxFrameBuffer()
			.UseMacOS()
			.Build()
			.Run();
	}
}
