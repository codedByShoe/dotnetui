namespace Nugetui.UI.Views;
using Terminal.Gui;
using Nugetui.Services;
using Nugetui.Input;
using Nugetui.UI.Dialogs;

public class InstalledPackagesView : BaseView
{
  private readonly ListView _listView;
  private readonly IDotNetCliService _cliService;

  public event Action<string>? PackageUninstalled;

  public InstalledPackagesView(IDotNetCliService cliService) : base("Installed Packages")
  {
    _cliService = cliService;

    _listView = new ListView()
    {
      X = 0,
      Y = 0,
      Width = Dim.Fill(),
      Height = Dim.Fill(),
      ColorScheme = Colors.TopLevel
    };

    _listView.KeyPress += OnKeyPress;
    Add(_listView);

    RefreshPackages();
  }

  public void RefreshPackages()
  {
    var packages = _cliService.GetInstalledPackages();
    _listView.SetSource(packages);
  }

  private async void OnKeyPress(KeyEventEventArgs args)
  {
    VimNavigationHandler.HandleVertical(_listView, args.KeyEvent);

    if ((args.KeyEvent.Key == Key.x || args.KeyEvent.Key == Key.X) && _listView.SelectedItem >= 0)
    {
      var selectedLine = _listView.Source.ToList()[_listView.SelectedItem]?.ToString();
      var packageId = selectedLine?.Split(' ')[1];

      if (!string.IsNullOrWhiteSpace(packageId))
      {
        ConfirmationDialog.Show("Confirm Delete", $"Are you sure you want to delete {packageId}?");

        var (success, output) = await ProgressDialog.RunAsync(
            "Uninstalling Package",
            $"Uninstalling {packageId}",
               () => _cliService.UninstallPackageAsync(packageId)
            );

        if (success)
        {
          RefreshPackages();
          PackageUninstalled?.Invoke(packageId);
        }
        else
        {
          MessageBox.ErrorQuery($"Failed to remove package {packageId}", output);
        }
      }
    }
  }
}
