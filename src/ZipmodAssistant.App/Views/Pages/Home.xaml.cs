using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
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
using Wpf.Ui.Mvvm.Contracts;
using Wpf.Ui.TaskBar;
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
    private readonly ILoggerService _logger;
    private readonly IRepositoryService _repositoryService;
    private readonly ISessionService _sessionService;
    private readonly INavigationWindow _navigationWindow;
    private readonly ITaskBarService _taskBarService;

    private HomeViewModel ViewModel => (HomeViewModel)DataContext;

    public Home(
      INavigationWindow navigationWindow,
      HomeViewModel homeViewModel,
      IRepositoryService repositoryService,
      ILoggerService loggerService,
      ISessionService sessionService,
      ITaskBarService taskBarService)
    {
      _navigationWindow = navigationWindow;
      _logger = loggerService;
      _repositoryService = repositoryService;
      _sessionService = sessionService;
      _taskBarService = taskBarService;
      DataContext = homeViewModel;
      InitializeComponent();

      _logger.MessageLogged += (sender, message) => Dispatcher.Invoke(() =>
      {
        ViewModel.LogMessages.Add(message);
        LogMessageScroll.ScrollToBottom();
      });
      _logger.Log("Initiated logging");
      _logger.Log($"{Assembly.GetEntryAssembly().GetName().Name} v{Assembly.GetEntryAssembly().GetName().Version}");
      ViewModel.PropertyChanged += (sender, e) =>
      {
        var jsonData = JsonSerializer.SerializeToUtf8Bytes(ViewModel);
        using var homeViewModelFile = File.Create("config.json");
        homeViewModelFile.Write(jsonData);
      };
    }

    private async void CopyAllClicked(object sender, RoutedEventArgs e)
    {
      Clipboard.SetText(string.Join(Environment.NewLine, ViewModel.LogMessages));
      await ClipboardNotification.ShowAsync("Notification", "Copied to clipboard");
    }

    private async void StartClicked(object sender, RoutedEventArgs e)
    {
      ViewModel.IsBuilding = true;
      ViewModel.BuildProgress = 0;
      try
      {
        _taskBarService.SetState(_navigationWindow as Window, TaskBarProgressState.Indeterminate);
        ViewModel.ValidateDirectoryConfiguration();
        var repository = await _repositoryService.GetRepositoryFromDirectoryAsync(ViewModel);
        ViewModel.BuildProgress = 20;
        await _repositoryService.ProcessRepositoryAsync(repository);
        ViewModel.BuildProgress = 80;
      }
      catch (DirectoryNotFoundException ex)
      {
        _logger.Log(ex);
      }
      finally
      {
        _taskBarService.SetState(_navigationWindow as Window, TaskBarProgressState.None);
        var report = await _sessionService.GenerateReportAsync();
        ViewModel.BuildProgress = 90;
        var reportFilename = $"report-{DateTime.Now:MM_dd_yyyy__HH_mm}.html";
        await File.WriteAllTextAsync(reportFilename, report);
        ViewModel.BuildProgress = 100;
        Process.Start(@"cmd.exe", @"/c " + reportFilename);
        _logger.Log($"Report generated at {reportFilename}");
        ViewModel.IsBuilding = false;
        if (!ViewModel.SkipCleanup)
        {
          foreach (var dir in Directory.EnumerateDirectories(ViewModel.CacheDirectory))
          {
            Directory.Delete(dir, true);
          }
          foreach (var file in Directory.EnumerateFiles(ViewModel.CacheDirectory))
          {
            File.Delete(file);
          }
        }
      }
    }
  }
}
