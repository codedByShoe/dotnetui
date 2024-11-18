namespace Nugetui.UI.Views;
using Terminal.Gui;

public class SearchInputView : BaseView
{
  private readonly TextField _inputField;
  public event Action<string>? SearchRequested;

  public SearchInputView() : base("Search Packages: ")
  {
    _inputField = new TextField()
    {

    };
  }
}
