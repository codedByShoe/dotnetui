namespace Nugetui;
using Terminal.Gui;
using Nugetui.Services;
using Nugetui.UI.Views;

class Program
{
  static void Main(string[] args)
  {
    var httpClient = new HttpClient();
    var nugetService = new NugetService(httpClient);
    var dotnetService = new DotNetCliService();

    Application.Init();

    var mainWindow = new MainWindow(nugetService, dotnetService);
    Application.Top.Add(mainWindow);
    Application.Run();

  }
}
