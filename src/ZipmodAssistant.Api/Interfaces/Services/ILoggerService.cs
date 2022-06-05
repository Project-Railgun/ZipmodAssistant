using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZipmodAssistant.Api.Enums;

namespace ZipmodAssistant.Api.Interfaces.Services
{
  public interface ILoggerService
  {
    EventHandler<string>? MessageLogged { get; set; }

    void Log(string message);
    void Log(Exception exception);
    void Log(string message, LogReason reason);
  }
}
