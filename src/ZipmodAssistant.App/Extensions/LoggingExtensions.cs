using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using ZipmodAssistant.App.Logging;

namespace ZipmodAssistant.App.Extensions
{
  public static class LoggingExtensions
  {
    public static ILoggingBuilder AddProjectView(this ILoggingBuilder builder)
    {
      builder.AddConfiguration();
      builder.Services.TryAddEnumerable(
        ServiceDescriptor.Singleton<ILoggerProvider, ProjectViewLoggerProvider>());
      return builder;
    }
  }
}
