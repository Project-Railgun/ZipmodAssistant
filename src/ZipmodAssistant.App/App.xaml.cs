using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;
using ZipmodAssistant.Api.Data;
using ZipmodAssistant.Api.Interfaces.Services;
using ZipmodAssistant.Api.Services;
using ZipmodAssistant.App.Views;

namespace ZipmodAssistant.App
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
  public partial class App : Application
  {
    private IServiceProvider _serviceProvider;

    public App()
    {
      var services = new ServiceCollection();
      ConfigureServices(services);
      _serviceProvider = services.BuildServiceProvider();
    }

    void ConfigureServices(IServiceCollection services)
    {
      // configure data sources
      services.AddDbContext<ZipmodDbContext>(options =>
      {
        options.UseSqlite("Data Source = ZipmodAssistant.db");
      });

      // configure services
      services.AddScoped<IManifestService, ManifestService>();

      // configure windows
      services.AddSingleton<Container>();
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
  }
}
