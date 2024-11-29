using Nugetui.Services;
using Nugetui.UI.Views;
using Terminal.Gui;

var httpClient = new HttpClient();
var nugetService = new NugetService(httpClient);
var dotnetService = new DotNetCliService();

Application.Init();

var mainWindow = new MainWindow(nugetService, dotnetService);

// Add global key handler for Ctrl+C
Application.Top.KeyPress += (args) =>
{
    if (args.KeyEvent.Key == (Key.CtrlMask | Key.C))
    {
        Application.Shutdown();
        args.Handled = true;
    }
};

Application.Top.Add(mainWindow);
Application.Run();
