namespace Nugetui.UI.Views;
using Terminal.Gui;
using Nugetui.UI.Colorschemes;

public abstract class BaseView : View
{
  protected BaseView(string title) : base(title)
  {
    this.WithColorscheme(ColorschemeProvider.Default);
    Border.BorderStyle = BorderStyle.Single;
    Border.Effect3D = false;
    Border.BorderBrush = Color.White;
  }

  public virtual void RefreshView() { }
}
