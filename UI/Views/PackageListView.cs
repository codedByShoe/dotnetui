namespace Nugetui.UI.Views;
using Nugetui.Services;
using Terminal.Gui;
using Nugetui.Models;
using Nugetui.Input;
using Nugetui.UI.Dialogs;

public class PackageListView : BaseView
{
  private readonly INugetService _nugetService;
  private readonly IDotNetCliService _cliService;
  private readonly ListView _listView;
  private List<NugetPackage> _currentPackages = new();

  public event Action<NugetPackage>? PackageSelected;
  public event Action<NugetPackage>? PackageInstalled;

  public PackageListView(INugetService nugetService, IDotNetCliService cliService) : base("Packages")
  {
    _nugetService = nugetService;
    _cliService = cliService;

    _listView = new ListView()
    {
      X = 0,
      Y = 0,
      Width = Dim.Fill(),
      Height = Dim.Fill(),
      ColorScheme = Colorschemes.ColorschemeProvider.Default
    };

    _listView.SelectedItemChanged += OnSelectedItemChanged;
    _listView.KeyPress += OnKeyPress;

    Add(_listView);
  }

  public async Task SearchPackagesAsync(string searchTerm)
  {
    _listView.SetSource(new[] { "Searching..." });
    try
    {
      _currentPackages = await _nugetService.SearchPackagesAsync(searchTerm);

      var displayItems = _currentPackages
        .Select(p => $"{p.Id} {p.Version}")
        .ToList();

      Application.MainLoop.Invoke(() =>
      {
        _listView.SetSource(displayItems);
        if (displayItems.Any())
        {
          _listView.SelectedItem = 0;
          PackageSelected?.Invoke(_currentPackages[0]);
        }
      });

    }
    catch (Exception ex)
    {
      // TODO: Handle errors in dialogs instead of the list component themselves so that the UI can be stopped or reset.
      _listView.SetSource(new[] { $"Error: {ex.Message}" });
    }
  }

  private void OnSelectedItemChanged(ListViewItemEventArgs args)
  {
    if (_listView.SelectedItem >= 0 && _listView.SelectedItem < _currentPackages.Count)
    {
      PackageSelected?.Invoke(_currentPackages[_listView.SelectedItem]);
    }
  }

  private async void OnKeyPress(KeyEventEventArgs args)
  {
    VimNavigationHandler.HandleVertical(_listView, args.KeyEvent);

    if (args.KeyEvent.Key == Key.Enter && _listView.SelectedItem >= 0 && _listView.SelectedItem < _currentPackages.Count)
    {
      var package = _currentPackages[_listView.SelectedItem];

      var success = await DialogHelpers.RunWithProgressDialog(
          "Installing Package",
          $"Installing {package.Id}",
          async (dialog) => await _cliService.InstallPackageAsync(package.Id)
          );
      if (success)
      {
        PackageInstalled?.Invoke(package);
      }
    }
  }
}
