using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ZipmodAssistant.App.ViewModels;

namespace ZipmodAssistant.App.Views.Pages
{
  /// <summary>
  /// Interaction logic for Home.xaml
  /// </summary>
  public partial class Home : Page
  {
    public HomeViewModel ViewModel { get; set; } = new();

    public Home()
    {
      InitializeComponent();
      DataContext = ViewModel;
    }

    private void StartClicked(object sender, RoutedEventArgs e)
    {
      try
      {
        ViewModel.ValidateDirectoryConfiguration();
      }
      catch (DirectoryNotFoundException ex)
      {
        Console.WriteLine(ex);
      }
    }
  }
}
