namespace Nugetui.UI.Views;
using Terminal.Gui;
using Nugetui.UI.Colorschemes;
using Nugetui.Models;

public class PackageDetailsView : BaseView
{
  private readonly ListView _listView;

  public PackageDetailsView() : base("Package Details")
  {
    _listView = new ListView()
    {
      X = 0,
      Y = 0,
      Width = Dim.Fill(),
      Height = Dim.Fill(),
      ColorScheme = ColorschemeProvider.Default
    };

    Add(_listView);
  }

  public void DisplayPackage(NugetPackage package)
  {
    var details = new List<string>
    {
      $"Id: {package.Id}",
      $"Version: {package.Version}",
      $"Downloads: {package.TotalDownloads:N0}",
      "",
      "Description:",
      package.Description
    };

    _listView.SetSource(details);
  }
}
