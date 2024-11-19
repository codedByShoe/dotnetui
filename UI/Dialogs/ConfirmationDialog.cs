namespace Nugetui.UI.Dialogs;
using Terminal.Gui;
using Nugetui.UI.Colorschemes;

public static class ConfirmationDialog
{
  public static bool Show(string title, string message)
  {
    var result = false;
    var dialog = new Dialog(title, 60, 10);

    // Style the dialog
    dialog.Border.BorderStyle = BorderStyle.Single;
    dialog.ColorScheme = ColorschemeProvider.Dialog;

    // Center the dialog
    dialog.X = Pos.Center();
    dialog.Y = Pos.Center();

    // Add the message
    var label = new Label(message)
    {
      X = Pos.Center(),
      Y = Pos.Center() - 1,
      ColorScheme = dialog.ColorScheme
    };

    // Add No button
    var noButton = new Button("No")
    {
      X = Pos.Center() - 6,
      Y = Pos.Center() + 1,
      ColorScheme = ColorschemeProvider.Button,
      IsDefault = true
    };
    noButton.Clicked += () =>
    {
      result = false;
      Application.RequestStop();
    };

    // Add Yes button
    var yesButton = new Button("Yes")
    {
      X = Pos.Center() + 6,
      Y = Pos.Center() + 1,
      ColorScheme = ColorschemeProvider.Button
    };
    yesButton.Clicked += () =>
    {
      result = true;
      Application.RequestStop();
    };

    // Add controls to dialog
    dialog.Add(label, noButton, yesButton);

    // Run the dialog
    Application.Run(dialog);

    // Return the result
    return result;
  }
}
