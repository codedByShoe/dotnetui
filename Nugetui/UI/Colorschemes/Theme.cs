namespace Nugetui.UI.Colorschemes;
using Terminal.Gui;

public class Theme
{
  public required ColorScheme Default { get; init; }
  public required ColorScheme Input { get; init; }
  public required ColorScheme Dialog { get; init; }
  public required ColorScheme Error { get; init; }
  public required ColorScheme Button { get; init; }
  public required ColorScheme Selected { get; init; }
}
