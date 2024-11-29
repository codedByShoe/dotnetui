namespace Nugetui.UI.Colorschemes;
using Terminal.Gui;

public static class ColorschemeExtensions
{
  public static View WithColorscheme(this View view, ColorScheme scheme)
  {
    view.ColorScheme = scheme;
    return view;
  }
}
