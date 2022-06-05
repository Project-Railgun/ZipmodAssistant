using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZipmodAssistant.Api.Enums;
using ZipmodAssistant.Api.Interfaces.Services;

namespace ZipmodAssistant.Api.Services
{
  public class LoggerService : ILoggerService
  {
    public EventHandler<string>? MessageLogged { get; set; }

    public void Log(string message)
    {
      MessageLogged?.Invoke(this, FormatLogMessage(message, LogReason.Info));
    }

    public void Log(Exception exception)
    {
      MessageLogged?.Invoke(this, FormatLogMessage(exception.ToString().Split(Environment.NewLine)[0], LogReason.Error));
    }

    public void Log(string message, LogReason reason)
    {
      MessageLogged?.Invoke(this, FormatLogMessage(message, reason));
    }

    private static string FormatLogMessage(string message, LogReason reason) => $"{DateTime.Now:T} [{reason}]: {message}";
  }
}
