using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using ZipmodAssistant.App.ViewModels;

namespace ZipmodAssistant.App.Logging
{
  [ProviderAlias("ProjectView")]
  [UnsupportedOSPlatform("browser")]
  public class ProjectViewLoggerProvider : ILoggerProvider
  {
    private readonly ConcurrentDictionary<string, ProjectViewLogger> _loggers = new(StringComparer.OrdinalIgnoreCase);
    private readonly IServiceProvider _serviceProvider;
    private bool _disposed;

    public ProjectViewLoggerProvider(IServiceProvider serviceProvider)
    {
      _serviceProvider = serviceProvider;
    }

    public ILogger CreateLogger(string categoryName) =>
      _loggers.GetOrAdd(categoryName, name => new ProjectViewLogger(name, _serviceProvider));

    public void Dispose()
    {
      if (_disposed)
      {
        return;
      }
      _disposed = true;
      GC.SuppressFinalize(this);
      _loggers.Clear();
    }
  }

  public class ProjectViewLogger : ILogger
  {
    private readonly string _name;
    private readonly IServiceProvider _serviceProvider;

    public ProjectViewLogger(string name, IServiceProvider serviceProvider)
    {
      _name = name;
      _serviceProvider = serviceProvider;
    }

    public IDisposable BeginScope<TState>(TState state) => default!;

    public bool IsEnabled(LogLevel logLevel)
    {
      
#if DEBUG
      return true;
#else
      return logLevel > LogLevel.Debug;
#endif
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
      var formatted = formatter(state, exception);
      if (formatted != null)
      {
        var viewModel = _serviceProvider.GetService<ProjectViewModel>();
        var message = $"[{logLevel}] {formatted}";
        viewModel?.LogMessages.Add(message);
      }
    }
  }
}
