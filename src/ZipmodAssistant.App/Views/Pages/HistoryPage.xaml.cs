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
using Button = Wpf.Ui.Controls.Button;

namespace ZipmodAssistant.App.Views.Pages
{
  /// <summary>
  /// Interaction logic for History.xaml
  /// </summary>
  public partial class HistoryPage : UiPage
  {
    private readonly ILogger<HistoryPage> _logger;

    public HistoryViewModel ViewModel => (HistoryViewModel)DataContext;

    public HistoryPage(HistoryViewModel viewModel, ILogger<HistoryPage> logger)
    {
      InitializeComponent();
      DataContext = viewModel;
      _logger = logger;
    }

    async void OnPageLoaded(object sender, RoutedEventArgs e)
    {
      await ViewModel.LoadZipmodsAsync();
    }

    async void OnZipmodToggled(object sender, RoutedEventArgs e)
    {
      if (sender is ToggleSwitch toggleSwitch)
      {
        if (toggleSwitch.DataContext is PriorZipmodEntry entry)
        {
          await ViewModel.SetCanSkipAsync(entry.Guid, toggleSwitch.IsChecked ?? false);
        }
      }
    }

    async void OnDeleteModClicked(object sender, RoutedEventArgs e)
    {
      var result = await ConfirmDeleteDialog.ShowAndWaitAsync();
      if (result == Wpf.Ui.Controls.Interfaces.IDialogControl.ButtonPressed.Right && sender is Button button)
      {
        if (button.DataContext is PriorZipmodEntry entry)
        {
          await ViewModel.DeleteModFromHistoryAsync(entry.Guid);
        }
      }
      ConfirmDeleteDialog.Hide();
    }

    async void OnDeleteAllClicked(object sender, RoutedEventArgs e)
    {
      var result = await ConfirmDeleteDialog.ShowAndWaitAsync();
      if (result == Wpf.Ui.Controls.Interfaces.IDialogControl.ButtonPressed.Right && sender is Button button)
      {
        await ViewModel.DeleteAllAsync();
      }
      ConfirmDeleteDialog.Hide();
    }
  }
}
