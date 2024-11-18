namespace Nugetui.UI.Views;
using Nugetui.Services;
using Terminal.Gui;
using Nugetui.Models;

public class PackageListView : BaseView
{
  private readonly INugetService _nugetService;
  private readonly ListView _listView;
  private List<NugetPackage> _currentPackages = new();

  public event Action<NugetPackage>? PackageSelected;
  public event Action<NugetPackage>? PackageInstalled;

  public PackageListView(INugetService nugetService) : base("Packages")
  {
    _nugetService = nugetService;

    _listView = new ListView()
    {
      X = 0,
      Y = 0,
      Width = Dim.Fill(),
      Height = Dim.Fill(),
      ColorScheme = Colorschemes.ColorschemeProvider.Default
    };

  }
}
