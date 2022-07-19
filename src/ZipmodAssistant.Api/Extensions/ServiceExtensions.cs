using Microsoft.Extensions.DependencyInjection;
using ZipmodAssistant.Api.Interfaces.Services;
using ZipmodAssistant.Api.Services;

namespace ZipmodAssistant.Api.Extensions
{
  public static class ServiceExtensions
  {
    public static IServiceCollection AddZipmodAssistant(this IServiceCollection provider) =>
      provider
        .AddScoped<ILoggerService, LoggerService>()
        .AddScoped<IRepositoryService, RepositoryService>()
        .AddSingleton<ISessionService, SessionService>()
        .AddScoped<IAssetService, AssetService>();
  }
}
