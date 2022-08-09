using Microsoft.Extensions.DependencyInjection;
using ZipmodAssistant.Api.Data;
using ZipmodAssistant.Api.Interfaces.Repositories;
using ZipmodAssistant.Api.Interfaces.Services;
using ZipmodAssistant.Api.Repositories;
using ZipmodAssistant.Api.Services;

namespace ZipmodAssistant.Api.Extensions
{
  public static class ServiceExtensions
  {
    public static IServiceCollection AddZipmodAssistant(this IServiceCollection provider) =>
      provider
        .AddScoped<IRepositoryService, RepositoryService>()
        .AddSingleton<ISessionService, SessionService>()
        .AddScoped<IAssetService, AssetService>()
        .AddScoped<IZipmodRepository, ZipmodRepository>();
  }
}
