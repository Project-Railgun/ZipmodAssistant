using System.Windows;
using WPFUI.Appearance;

namespace ZipmodAssistant.App.Views
{
  /// <summary>
  /// Interaction logic for Container.xaml
  /// </summary>
  public partial class Container
  {
    public Container()
    {
      InitializeComponent();
      WPFUI.Appearance.Background.Apply(this, BackgroundType.Mica);
    }

    void WindowLoaded(object sender, RoutedEventArgs e)
    {
      Watcher.Watch(this, BackgroundType.Mica, true);
    }
  }
}
