using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using Serilog;
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
    private IHost _host;
    private const string DB_CONNECTION_STRING = "Data Source = ZipmodAssistant.db";

    public App()
    {
      var services = new ServiceCollection();
      AppDomain.CurrentDomain.UnhandledException += UnhandledException;
      Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Information)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .CreateLogger();
      Serilog.Debugging.SelfLog.Enable(Console.Error);
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
      if (!Directory.Exists("logs"))
      {
        Directory.CreateDirectory("logs");
      }

      _host = CreateHost(args.Args);
      await _host.StartAsync();
    }

    async void OnExit(object sender, ExitEventArgs args)
    {
      // TODO: allow keeping minimized to tray
      await _host.StopAsync();
      _host.Dispose();
      Log.CloseAndFlush();
    }

    static IHost CreateHost(string[] args) =>
      Host.CreateDefaultBuilder(args)
        .ConfigureServices(ConfigureServices)
        .UseSerilog(ConfigureLogging)
        .Build();

    static void ConfigureLogging(HostBuilderContext context, IServiceProvider services, LoggerConfiguration logger) => logger
      .ReadFrom.Services(services)
      .Enrich.FromLogContext()
#if DEBUG
      .WriteTo.Debug()
#else
      .WriteTo.Console()
#endif
      .WriteTo.File(
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", $"log_{DateTime.Now:MM_dd_yyyy__hh_mm_ss}.txt"),
        retainedFileCountLimit: 20,
        shared: true,
        flushToDiskInterval: TimeSpan.FromSeconds(1)
      )
      .WriteTo.InMemory()
      .WriteTo.Logger(Log.Logger);

    static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
      var configuration = context.Configuration.GetSection(nameof(AppConfiguration));
      services
        .AddHostedService<ApplicationHostService>()
        // configure data sources
        .AddDbContext<ZipmodDbContext>(options =>
          options.UseSqlite(DB_CONNECTION_STRING),
          ServiceLifetime.Transient
        )

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
        .AddSingleton<HistoryViewModel>()
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
