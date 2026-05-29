using Microsoft.Extensions.DependencyInjection;
using NutriVision.Views;

namespace NutriVision;

public partial class AppShell : Shell
{
    public AppShell(IServiceProvider services)
    {
        InitializeComponent();

        Items.Add(new TabBar
        {
            Items =
            {
                CreateTab("Home", () => services.GetRequiredService<HomePage>()),
                CreateTab("Scan", () => services.GetRequiredService<ScanPage>()),
                CreateTab("History", () => services.GetRequiredService<HistoryPage>()),
                CreateTab("Stats", () => services.GetRequiredService<StatisticsPage>()),
                CreateTab("Settings", () => services.GetRequiredService<SettingsPage>())
            }
        });
    }

    private static ShellContent CreateTab(string title, Func<Page> pageFactory)
    {
        return new ShellContent
        {
            Title = title,
            ContentTemplate = new DataTemplate(pageFactory)
        };
    }
}
