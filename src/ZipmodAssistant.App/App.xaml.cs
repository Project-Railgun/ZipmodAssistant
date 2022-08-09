﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Threading;
using Wpf.Ui.Mvvm.Contracts;
using Wpf.Ui.Mvvm.Services;
using ZipmodAssistant.Api.Data;
using ZipmodAssistant.Api.Extensions;
using ZipmodAssistant.Api.Interfaces.Models;
using ZipmodAssistant.Api.Interfaces.Services;
using ZipmodAssistant.Api.Services;
using ZipmodAssistant.App.Commands;
using ZipmodAssistant.App.Extensions;
using ZipmodAssistant.App.Interfaces.Models;
using ZipmodAssistant.App.Interfaces.Services;
using ZipmodAssistant.App.Models;
using ZipmodAssistant.App.Services;
using ZipmodAssistant.App.ViewModels;
using ZipmodAssistant.App.Views;
using ZipmodAssistant.App.Views.Pages;
using ZipmodAssistant.Tarot.Extensions;

namespace ZipmodAssistant.App
{
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application
  {
    private readonly IServiceProvider _serviceProvider;
    private IHost _host;

    public App()
    {
      var services = new ServiceCollection();
      _serviceProvider = services.BuildServiceProvider();
      AppDomain.CurrentDomain.UnhandledException += UnhandledException;
    }

    void UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
      // TODO: generate a bug report and send it
      throw e.ExceptionObject as Exception;
    }

    void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
      throw e.Exception;
    }

    async void OnStartup(object sender, StartupEventArgs args)
    {
      AppDomain.CurrentDomain.UnhandledException += UnhandledException;
      _host = CreateHost(args.Args);
      await _host.StartAsync();
    }

    async void OnExit(object sender, ExitEventArgs args)
    {
      // TODO: allow keeping minimized to tray
      await _host.StopAsync();
      _host.Dispose();
    }

    static IHost CreateHost(string[] args) =>
      Host.CreateDefaultBuilder(args)
      .ConfigureServices(ConfigureServices)
      .ConfigureLogging(ConfigureLogging)
      .Build();

    static void ConfigureLogging(HostBuilderContext context, ILoggingBuilder logging) => logging
      .ClearProviders()
      .AddConsole()
      .AddProjectView()
      .SetMinimumLevel(LogLevel.Debug);

    static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
      var configuration = context.Configuration.GetSection(nameof(AppConfiguration));
      services
        .AddHostedService<ApplicationHostService>()
        // configure data sources
        .AddSqlite<ZipmodDbContext>("Data Source = ZipmodAssistant.db")

        // configure services
        .AddZipmodAssistant()
        .AddTarot()
        .AddSingleton<IPageService, PageService>()
        .AddSingleton<IThemeService, ThemeService>()
        .AddSingleton<INavigationService, NavigationService>()
        .AddSingleton<ITaskBarService, TaskBarService>()
        .AddSingleton<IProjectService, ProjectService>()
        .AddSingleton<INavigationWindow, Container>()
        // configure view models
        .AddSingleton<IBuildConfiguration, ProjectViewModel>()
        .AddSingleton<ProjectViewModel>()
        .AddSingleton<HomeViewModel>()
        .AddSingleton<ContainerViewModel>()
        // configure views
        .AddSingleton<HomePage>()
        .AddSingleton<HistoryPage>()
        .AddSingleton<ProjectPage>()
        .AddSingleton<SettingsPage>()
        .AddSingleton<IAppConfiguration, AppConfiguration>()
        .Configure<AppConfiguration>(configuration);
    }
  }
}
