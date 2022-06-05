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
using ZipmodAssistant.Api.Enums;
using ZipmodAssistant.Api.Interfaces.Services;
using ZipmodAssistant.App.Extensions;
using ZipmodAssistant.App.ViewModels;

namespace ZipmodAssistant.App.Views.Pages
{
  /// <summary>
  /// Interaction logic for Home.xaml
  /// </summary>
  public partial class Home : Page
  {
    private ILoggerService _logger;

    private HomeViewModel _viewModel => (HomeViewModel)DataContext;

    public Home()
    {
      InitializeComponent();
      _logger = this.GetService<ILoggerService>();
      _logger.MessageLogged += (sender, message) =>
      {
        _viewModel.LogMessages.Add(message);
        LogMessageScroll.ScrollToBottom();
      };
    }

    private async void StartClicked(object sender, RoutedEventArgs e)
    {
      _viewModel.IsBuilding = true;
      _viewModel.BuildProgress = 0;
      try
      {
        _viewModel.ValidateDirectoryConfiguration();
        do
        {
          _viewModel.BuildProgress += 5;
          await Task.Delay(200);
        }
        while (_viewModel.BuildProgress < 100);
      }
      catch (DirectoryNotFoundException ex)
      {
        _logger.Log(ex);
      }
      finally
      {
        _viewModel.IsBuilding = false;
      }
    }
  }
}
