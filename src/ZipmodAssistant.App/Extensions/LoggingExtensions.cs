using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Linq;
using System.Windows.Threading;
using ZipmodAssistant.App.Logging;
using ZipmodAssistant.App.ViewModels;

namespace ZipmodAssistant.App.Extensions
{
  public static class LoggingExtensions
  {
    public static LoggerConfiguration InMemory(
      this LoggerSinkConfiguration logger,
      IFormatProvider? formatProvider = null) =>
        logger
          .Sink(new InMemorySink(formatProvider))
          .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
          .MinimumLevel.Override("System", LogEventLevel.Information)
          .MinimumLevel.Override("ZipmodAssistant", LogEventLevel.Debug);

    public static ViewModel SubscribeToInMemorySink(
      this ViewModel viewModel, Dispatcher dispatcher)
    {
      var messages = InMemorySink.Subscribe((messages) => dispatcher.InvokeAsync(() =>
      {
        foreach (var message in messages)
        {
          viewModel.LogMessages.Add(FormatMessage(message));
        }
      }));
      viewModel.LogMessages = new(messages.Select(FormatMessage));
      return viewModel;
    }

    static string FormatMessage(string message) => $"[{DateTime.Now:hh:mm:ss}] {message.Replace("\"", null)}";
  }
}
