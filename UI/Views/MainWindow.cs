namespace Nugetui.UI.Views;
using Terminal.Gui;
using Nugetui.Services;

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
    ColorScheme = Colorschemes.ColorschemeProvider.Default;
  }

}
