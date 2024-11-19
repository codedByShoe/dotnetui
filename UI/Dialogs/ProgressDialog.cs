namespace Nugetui.UI.Dialogs;
using Terminal.Gui;
using Nugetui.UI.Colorschemes;

public class ProgressDialog : Dialog
{
  private readonly Label messageLabel;

  public ProgressDialog(string title, string message) : base(title, 60, 8)
  {

    this.WithColorscheme(ColorschemeProvider.Dialog);
    this.Border.BorderStyle = BorderStyle.Single;
    this.Border.Effect3D = false;
    this.Border.BorderBrush = Color.White;

    messageLabel = new Label(message)
    {
      X = Pos.Center(),
      Y = Pos.Center(),
      Width = Dim.Fill() - 4
    };

    Add(messageLabel);

  }

  public void UpdateMessage(string message)
  {
    messageLabel.Text = message;
  }

  public void ShowDialog()
  {
    Application.Run(this);
  }

}
