namespace Nugetui.UI.Views;
using Terminal.Gui;
using Nugetui.UI.Colorschemes;

public class SearchInputView : BaseView
{
  private readonly TextField _inputField;
  public event Action<string>? SearchRequested;

  public SearchInputView() : base("Search Packages: ")
  {
    _inputField = new TextField()
    {
      X = 0,
      Y = 0,
      Width = Dim.Fill(),
      Height = Dim.Fill(),
      ColorScheme = ColorschemeProvider.Input
    };

    _inputField.KeyPress += OnKeyPress;

    Add(_inputField);

  }

  private void OnKeyPress(KeyEventEventArgs args)
  {
    if (args.KeyEvent.Key == Key.Enter && !string.IsNullOrWhiteSpace(_inputField.Text.ToString()))
    {
      var searchTerm = _inputField.Text.ToString();
      _inputField.Text = "";
      if (searchTerm is null) return;
      SearchRequested?.Invoke(searchTerm);
      args.Handled = true;
    }
  }
}
