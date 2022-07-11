using Microsoft.Extensions.DependencyInjection;
using ZipmodAssistant.Api.Interfaces.Services;
using ZipmodAssistant.Api.Services;

namespace ZipmodAssistant.Api.Extensions
{
  public static class ServiceExtensions
  {
    public static void AddZipmodAssistant(this IServiceCollection provider)
    {
      provider
        .AddScoped<ILoggerService, LoggerService>()
        .AddScoped<IOutputService, OutputService>()
        .AddScoped<IRepositoryService, RepositoryService>()
        .AddScoped<ISessionService, SessionService>()
        .AddScoped<IAssetService, AssetService>();
    }
  }
}
