using System;
using System.Collections.Generic;
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

    private HomeViewModel ViewModel => (HomeViewModel)DataContext;

    public Home()
    {
      InitializeComponent();
      _logger = this.GetService<ILoggerService>();
      _repositoryService = this.GetService<IRepositoryService>();
      _sessionService = this.GetService<ISessionService>();

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

    private async void StartClicked(object sender, RoutedEventArgs e)
    {
      ViewModel.IsBuilding = true;
      ViewModel.BuildProgress = 0;
      try
      {
        ViewModel.ValidateDirectoryConfiguration();
        var repository = await _repositoryService.GetRepositoryFromDirectoryAsync(ViewModel);
        ViewModel.BuildProgress = 20;
        await _repositoryService.ProcessRepositoryAsync(repository);
        ViewModel.BuildProgress = 50;
        do
        {
          ViewModel.BuildProgress += 5;
          await Task.Delay(200);
        }
        while (ViewModel.BuildProgress < 100);
      }
      catch (DirectoryNotFoundException ex)
      {
        _logger.Log(ex);
      }
      finally
      {
        // var report = await _sessionService.GenerateReportAsync();
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
