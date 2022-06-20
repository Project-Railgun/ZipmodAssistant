using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZipmodAssistant.Api.Enums;

namespace ZipmodAssistant.Api.Interfaces.Services
{
  /// <summary>
  ///   The logging service used in the library
  /// </summary>
  public interface ILoggerService
  {
    EventHandler<string>? MessageLogged { get; set; }

    void Log(string message);
    void Log(Exception exception);
    void Log(string message, LogReason reason);
  }
}
