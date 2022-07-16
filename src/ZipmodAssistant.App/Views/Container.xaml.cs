using System.Windows;
using Wpf.Ui.Appearance;

namespace ZipmodAssistant.App.Views
{
  /// <summary>
  /// Interaction logic for Container.xaml
  /// </summary>
  public partial class Container : Window
  {
    public Container()
    {
      InitializeComponent();
      Wpf.Ui.Appearance.Background.Apply(this, BackgroundType.Mica);
      
    }

    void WindowLoaded(object sender, RoutedEventArgs e)
    {
      Watcher.Watch(this, BackgroundType.Mica, true, true);
    }

    void ExitClicked(object sender, RoutedEventArgs e)
    {
      Application.Current.Shutdown();
    }
  }
}
