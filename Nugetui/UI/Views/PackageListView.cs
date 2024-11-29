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

    _listView = new ListView(_currentPackages)
    {
      X = 0,
      Y = 0,
      Width = Dim.Fill(),
      Height = Dim.Fill(),
      ColorScheme = Colors.TopLevel
    };

    _listView.SelectedItemChanged += OnSelectedItemChanged;
    _listView.KeyPress += OnKeyPress;

    Add(_listView);
  }

  public async Task SearchPackagesAsync(string searchTerm)
  {
    try
    {
      _currentPackages.Clear();
      _currentPackages = await ProgressDialog.RunAsync(
            "Package Search",
            $"Searching for {searchTerm}",
             () => _nugetService.SearchPackagesAsync(searchTerm)
            );

      Application.MainLoop.Invoke(() =>
      {
        var displayItems = _currentPackages
          .Select(p => $"{p.Id} {p.Version}").ToList();
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

      var selectedProject = await ProjectSelectorDialog.SelectProject(_cliService.GetProjectsFromSln());

      if (!string.IsNullOrWhiteSpace(selectedProject))
      {

        await Task.Delay(1000);

        var (success, output) = await ProgressDialog.RunAsync(
            "Installing Package",
            $"Installing {package.Id}",
                () => _cliService.InstallPackageAsync(package.Id, selectedProject)
            );
        if (success)
        {
          PackageInstalled?.Invoke(package);
        }
        else
        {
          MessageBox.ErrorQuery("Installation Failed", $"Failed to install {package.Id} \n \n {output}");
        }
      }

    }
  }
}
