namespace Nugetui.UI.Views;
using Terminal.Gui;
using Nugetui.Services;
using Nugetui.Models;

public class MainWindow : Window
{
  private readonly PackageListView _packageListView;
  private readonly PackageDetailsView _packageDetailsView;
  private readonly InstalledPackagesView _installedPackagesView;
  private readonly SearchInputView _searchInputView;

  public MainWindow(INugetService nugetService, IDotNetCliService dotnetService) : base("NugeTUI")
  {
    X = 0;
    Y = 1;
    Width = Dim.Fill();
    Height = Dim.Fill() - 1;
    ColorScheme = Colors.TopLevel;

    _packageListView = new PackageListView(nugetService, dotnetService)
    {
      X = 0,
      Y = 0,
      Width = Dim.Percent(50),
      Height = Dim.Fill() - 4,
    };


    _packageDetailsView = new PackageDetailsView()
    {
      X = Pos.Percent(50),
      Y = 0,
      Width = Dim.Fill(),
      Height = Dim.Percent(50)
    };

    _installedPackagesView = new InstalledPackagesView(dotnetService)
    {
      X = Pos.Percent(50),
      Y = Pos.Percent(50),
      Width = Dim.Fill(),
      Height = Dim.Fill() - 4
    };
    _searchInputView = new SearchInputView()
    {
      X = 0,
      Y = Pos.AnchorEnd(3),
      Width = Dim.Fill(),
      Height = 3
    };

    _packageListView.PackageSelected += OnPackageSelected;
    _packageListView.PackageInstalled += OnPackageInstalled;
    _searchInputView.SearchRequested += OnSearchRequested;
    _installedPackagesView.PackageUninstalled += OnPackageUninstalled;

    Add(_packageListView);
    Add(_packageDetailsView);
    Add(_installedPackagesView);
    Add(_searchInputView);

    Add(new Label("Quit: Ctrl+c | Navigate: j/k | Install Package: Enter")
    {
      X = 0,
      Y = Pos.AnchorEnd(4)
    });

    _searchInputView.SetFocus();
  }

  private async void OnSearchRequested(string searchTerm)
  {
    await _packageListView.SearchPackagesAsync(searchTerm);
    _packageListView.SetFocus();
  }

  private void OnPackageSelected(NugetPackage package)
  {
    _packageDetailsView.DisplayPackage(package);
  }

  private void OnPackageInstalled(NugetPackage package)
  {
    _installedPackagesView.RefreshPackages();
  }

  private void OnPackageUninstalled(string packageId)
  {
    _installedPackagesView.RefreshPackages();
  }

}
