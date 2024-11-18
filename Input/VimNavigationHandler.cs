namespace Nugetui.Input;
using Terminal.Gui;

public static class VimNavigationHandler
{
  static void HandleVertical(ListView listView, KeyEvent keyEvent)
  {
    if (listView.Source?.Count > 0)
    {
      switch (keyEvent.Key)
      {
        case Key.j:
        case Key.J:
          if (listView.SelectedItem < listView.Source.Count - 1)
          {
            listView.SelectedItem++;
          }
          break;

        case Key.k:
        case Key.K:
          if (listView.SelectedItem > 0)
          {
            listView.SelectedItem--;
          }
          break;
      }
    }
  }
}
