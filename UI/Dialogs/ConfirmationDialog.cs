namespace Nugetui.UI.Dialogs;
using Terminal.Gui;

public static class ConfirmationDialog
{
  private static bool Show(string title, string message)
  {
    var result = false;
    var dialog = new Dialog(title, 60, 10);

    // Style the dialog
    dialog.Border.BorderStyle = BorderStyle.Single;
    dialog.ColorScheme = new ColorScheme
    {
      Normal = Application.Driver.MakeAttribute(Color.White, Color.Black),
      Focus = Application.Driver.MakeAttribute(Color.White, Color.Black),
      HotNormal = Application.Driver.MakeAttribute(Color.White, Color.Black),
      HotFocus = Application.Driver.MakeAttribute(Color.White, Color.Black)
    };

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

    // Style for buttons
    var buttonScheme = new ColorScheme
    {
      Normal = Application.Driver.MakeAttribute(Color.White, Color.Blue),
      Focus = Application.Driver.MakeAttribute(Color.White, Color.BrightBlue),
      HotNormal = Application.Driver.MakeAttribute(Color.White, Color.Blue),
      HotFocus = Application.Driver.MakeAttribute(Color.White, Color.BrightBlue)
    };

    // Add No button
    var noButton = new Button("No")
    {
      X = Pos.Center() - 6,
      Y = Pos.Center() + 1,
      ColorScheme = buttonScheme,
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
      ColorScheme = buttonScheme
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
