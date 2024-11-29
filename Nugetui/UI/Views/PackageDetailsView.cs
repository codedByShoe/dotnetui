namespace Nugetui.UI.Views;
using Terminal.Gui;
using Nugetui.Models;
using Nugetui.Input;

public class PackageDetailsView : BaseView
{
  private readonly ListView _listView;
  private List<string> _selectedPackage = new();

  public PackageDetailsView() : base("Package Details")
  {
    _listView = new ListView(_selectedPackage)
    {
      X = 0,
      Y = 0,
      Width = Dim.Fill(),
      Height = Dim.Fill(),
      ColorScheme = Colors.TopLevel
    };

    _listView.KeyPress += OnKeyPress;

    Add(_listView);
  }

  public void DisplayPackage(NugetPackage package)
  {
    _selectedPackage.Clear();

    _selectedPackage.Add($"Id: {package.Id}");
    _selectedPackage.Add($"Version: {package.Version}");
    _selectedPackage.Add($"Downloads: {package.TotalDownloads}");
    _selectedPackage.Add($"Description: {package.Description}");

    _listView.SetSource(_selectedPackage);
  }

  private void OnKeyPress(KeyEventEventArgs args)
  {
    VimNavigationHandler.HandleVertical(_listView, args.KeyEvent);
  }
}
