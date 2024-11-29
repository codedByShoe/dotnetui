namespace Nugetui.UI.Views;
using Terminal.Gui;

public abstract class BaseView : FrameView
{
  protected BaseView(string title) : base(title)
  {
    this.ColorScheme = Colors.TopLevel;
  }

  public virtual void RefreshView() { }
}
