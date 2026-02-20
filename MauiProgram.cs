using BugFlow.Data;
using BugFlow.Pages.Comentarii;
using BugFlow.Pages.Issues;
using BugFlow.Pages.Membri;
using BugFlow.Pages.Proiecte;
using BugFlow.Pages.Raport;
using Microsoft.Extensions.Logging;

namespace BugFlow;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		var dbPath = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
			"bugflow.db3");
		builder.Services.AddSingleton(new BugFlowDatabase(dbPath));

		builder.Services.AddTransient<ProiecteListPage>();
		builder.Services.AddTransient<MembriListPage>();
		builder.Services.AddTransient<IssuesListPage>();
		builder.Services.AddTransient<ComentariiListPage>();
		builder.Services.AddTransient<RaportPage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
