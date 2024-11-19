namespace Nugetui.UI.Dialogs;
using Terminal.Gui;

public static class DialogHelpers
{
  public static async Task<T> RunWithProgressDialog<T>(string title, string message, Func<ProgressDialog, Task<T>> task)
  {
    var dialog = new ProgressDialog(title, message);
    var tcs = new TaskCompletionSource<T>();

    Application.MainLoop.Invoke(async () =>
    {
      try
      {
        var result = await task(dialog);
        tcs.SetResult(result);
      }
      catch (Exception ex)
      {
        tcs.SetException(ex);
      }
      finally
      {
        Application.MainLoop.Invoke(() =>
        {
          dialog.Running = false;
        });
      }
    });

    dialog.ShowDialog();
    return await tcs.Task;
  }
}
