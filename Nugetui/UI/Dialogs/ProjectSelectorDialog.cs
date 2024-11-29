using Terminal.Gui;
using Nugetui.UI.Colorschemes;
using Nugetui.Input;

namespace Nugetui.UI.Dialogs;
public class ProjectSelectorDialog : Dialog
{
  private readonly ListView _listView;
  private readonly List<string> _projects;
  private string? _selectedProject;
  private static bool _isShowing;

  public ProjectSelectorDialog(List<string> projects) : base("Select Project")
  {
    this.Border.BorderStyle = BorderStyle.Single;
    this.Border.Effect3D = false;
    this.Border.BorderBrush = Color.White;
    this.ColorScheme = ColorschemeProvider.Dialog;

    _projects = projects;

    _listView = new ListView(projects)
    {
      X = 1,
      Y = 1,
      Width = Dim.Fill() - 2,
      Height = Dim.Fill() - 2,
      ColorScheme = Colors.TopLevel
    };

    _listView.KeyPress += OnKeyPress;

    Add(_listView);
    _listView.SetFocus();
    if (_projects.Any())
    {
      _listView.SelectedItem = 0;
    }
  }

  private void OnKeyPress(KeyEventEventArgs args)
  {
    VimNavigationHandler.HandleVertical(_listView, args.KeyEvent);
    if (args.KeyEvent.Key == Key.Enter && _listView.SelectedItem != -1 && _listView.SelectedItem < _projects.Count)
    {
      _selectedProject = _projects[_listView.SelectedItem];

      args.Handled = true;
      Application.MainLoop.Invoke(() =>
          {
            Application.RequestStop();
          });
    }
    else if (args.KeyEvent.Key == Key.Esc)
    {
      _selectedProject = null;

      args.Handled = true;
      Application.MainLoop.Invoke(() =>
          {
            Application.RequestStop();
          });
    }
  }

  public static async Task<string> SelectProject(List<string> projects)
  {
    var dialog = new ProjectSelectorDialog(projects);
    Application.MainLoop.Invoke(() =>
    {
      Application.Run(dialog);
    });
    await Task.Yield();
    return dialog._selectedProject ?? string.Empty;
  }

}
