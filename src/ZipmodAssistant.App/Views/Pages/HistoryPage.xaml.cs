using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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
using Wpf.Ui.Controls;
using ZipmodAssistant.Api.Data.DataModels;
using ZipmodAssistant.App.ViewModels;

namespace ZipmodAssistant.App.Views.Pages
{
  /// <summary>
  /// Interaction logic for History.xaml
  /// </summary>
  public partial class HistoryPage : Page
  {
    private readonly ILogger<HistoryPage> _logger;

    public HistoryViewModel ViewModel => (HistoryViewModel)DataContext;

    public HistoryPage(HistoryViewModel viewModel, ILogger<HistoryPage> logger)
    {
      InitializeComponent();
      DataContext = viewModel;
      _logger = logger;
    }

    private async void OnPageLoaded(object sender, RoutedEventArgs e)
    {
      await ViewModel.LoadZipmodsAsync();
    }

    private async void OnZipmodToggled(object sender, RoutedEventArgs e)
    {
      if (sender is ToggleSwitch toggleSwitch)
      {
        if (toggleSwitch.DataContext is PriorZipmodEntry entry)
        {
          await ViewModel.SetCanSkipAsync(entry.Guid, toggleSwitch.IsChecked ?? false);
        }
      }
    }
  }
}
