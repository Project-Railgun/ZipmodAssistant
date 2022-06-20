using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Windows;
using ZipmodAssistant.Api.Data;
using ZipmodAssistant.Api.Extensions;
using ZipmodAssistant.Api.Interfaces.Services;
using ZipmodAssistant.Api.Services;
using ZipmodAssistant.App.Extensions;
using ZipmodAssistant.App.ViewModels;
using ZipmodAssistant.App.Views;

namespace ZipmodAssistant.App
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
  public partial class App : Application
  {
    private readonly IServiceProvider _serviceProvider;

    public App()
    {
      var services = new ServiceCollection();
      ConfigureServices(services);
      _serviceProvider = services.BuildServiceProvider();
      AppDomain.CurrentDomain.UnhandledException += UnhandledException;
      DependencyInjectionSource.ServiceProvider = _serviceProvider;
    }

    void UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
      throw new NotImplementedException();
    }

    void OnStartup(object sender, StartupEventArgs args)
    {
      var main = _serviceProvider.GetService<Container>();
      if (main == null)
      {
        throw new ApplicationException("Container is null. Ensure the services are properly configured");
      }
      main.Show();
    }

    static void ConfigureServices(IServiceCollection services)
    {
      // configure data sources
      services.AddSqlite<ZipmodDbContext>("Data Source = ZipmodAssistant.db");

      // configure services
      services.AddZipmodAssistant();
      // configure view models
      services.AddScoped<HomeViewModel>();

      // configure windows
      services.AddSingleton<Container>();
    }
  }
}
