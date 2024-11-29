namespace Nugetui.UI.Colorschemes;
using Terminal.Gui;

public static class ColorschemeProvider
{

  public static ColorScheme Input => new()
  {
    Normal = Application.Driver.MakeAttribute(Color.White, Color.Blue),
    Focus = Application.Driver.MakeAttribute(Color.White, Color.Black),
    HotNormal = Application.Driver.MakeAttribute(Color.White, Color.Blue),
    HotFocus = Application.Driver.MakeAttribute(Color.White, Color.Black)
  };

  public static ColorScheme Dialog => new()
  {
    Normal = Application.Driver.MakeAttribute(Color.White, Color.Black),
    Focus = Application.Driver.MakeAttribute(Color.White, Color.Black),
    HotNormal = Application.Driver.MakeAttribute(Color.White, Color.Black),
    HotFocus = Application.Driver.MakeAttribute(Color.White, Color.Black)
  };

  public static ColorScheme Error => new()
  {
    Normal = Application.Driver.MakeAttribute(Color.White, Color.Red),
    Focus = Application.Driver.MakeAttribute(Color.White, Color.Black),
    HotNormal = Application.Driver.MakeAttribute(Color.White, Color.Red),
    HotFocus = Application.Driver.MakeAttribute(Color.White, Color.Black)
  };

  public static ColorScheme Button => new()
  {
    Normal = Application.Driver.MakeAttribute(Color.Black, Color.Gray),
    Focus = Application.Driver.MakeAttribute(Color.White, Color.Black),
    HotNormal = Application.Driver.MakeAttribute(Color.Black, Color.Gray),
    HotFocus = Application.Driver.MakeAttribute(Color.White, Color.Black)
  };

  public static ColorScheme Selected => new()
  {
    Normal = Application.Driver.MakeAttribute(Color.White, Color.Black),
    Focus = Application.Driver.MakeAttribute(Color.White, Color.Blue),
    HotNormal = Application.Driver.MakeAttribute(Color.White, Color.Black),
    HotFocus = Application.Driver.MakeAttribute(Color.White, Color.Blue)
  };
}


