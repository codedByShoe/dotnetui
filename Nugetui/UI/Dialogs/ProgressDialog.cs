namespace Nugetui.UI.Dialogs;
using Terminal.Gui;
using Nugetui.UI.Colorschemes;

public class ProgressDialog : Dialog
{
  private static bool _isShowing;
  private readonly Label _messageLabel;

  public ProgressDialog(string title, string message) : base(title, 60, 8)
  {
    this.Border.BorderStyle = BorderStyle.Single;
    this.Border.Effect3D = false;
    this.Border.BorderBrush = Color.White;
    this.ColorScheme = ColorschemeProvider.Dialog;

    _messageLabel = new Label(message)
    {
      X = Pos.Center(),
      Y = Pos.Center(),
      Width = Dim.Fill() - 4,
      ColorScheme = ColorschemeProvider.Dialog
    };

    Add(_messageLabel);
  }

  public void Close()
  {
    if (!_isShowing) return;
    Application.MainLoop.Invoke(() =>
    {
      Application.RequestStop();
      _isShowing = false;
    });
  }

  public static async Task<T> RunAsync<T>(string title, string message, Func<Task<T>> action)
  {
    _isShowing = true;
    var dialog = new ProgressDialog(title, message);
    var result = default(T);
    Application.MainLoop.Invoke(() =>
        {
          Application.Run(dialog);
        });
    try
    {
      result = await Task.Run(action);
    }
    finally
    {
      dialog.Close();
    }


    return result;
  }
}

