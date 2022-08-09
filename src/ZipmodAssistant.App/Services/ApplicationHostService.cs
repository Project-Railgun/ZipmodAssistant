using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Wpf.Ui.Mvvm.Contracts;
using ZipmodAssistant.Api.Interfaces.Services;
using ZipmodAssistant.App.Interfaces.Models;
using ZipmodAssistant.App.Views;

namespace ZipmodAssistant.App.Services
{
  public class ApplicationHostService : IHostedService
  {
    private readonly IAppConfiguration _appConfiguration;
    private readonly ILogger<ApplicationHostService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly INavigationService _navigationService;
    private readonly IPageService _pageService;
    private readonly IThemeService _themeService;
    private readonly ITaskBarService _taskBarService;

    private INavigationWindow _navigationWindow;

    public ApplicationHostService(
      IAppConfiguration appConfiguration,
      ILogger<ApplicationHostService> logger,
      IServiceProvider serviceProvider,
      IPageService pageService,
      IThemeService themeService,
      ITaskBarService taskBarService,
      INavigationService navigationService)
    {
      _appConfiguration = appConfiguration;
      _logger = logger;
      _serviceProvider = serviceProvider;
      _pageService = pageService;
      _themeService = themeService;
      _taskBarService = taskBarService;
      _navigationService = navigationService;
    }

    public async Task StartAsync(CancellationToken cancelToken)
    {
      var startTime = DateTime.Now;
      if (!Directory.Exists("logs"))
      {
        Directory.CreateDirectory("logs");
      }
      
      _navigationService.SetPageService(_pageService);
      if (!Application.Current.Windows.OfType<Container>().Any())
      {
        _navigationWindow = _serviceProvider.GetService<INavigationWindow>();
        _navigationWindow!.ShowWindow();
      }
      //var notifyIcon = _serviceProvider.GetService<INotifyIconService>();
      //if (!notifyIcon!.IsRegistered)
      //{
      //  notifyIcon.SetParentWindow(_navigationWindow as Window);
      //  notifyIcon.Register();
      //}

      await Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
      
    }
  }
}
