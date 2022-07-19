using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZipmodAssistant.Api.Enums;
using ZipmodAssistant.Api.Interfaces.Models;

namespace ZipmodAssistant.Api.Models
{
  public class SessionResult : ISessionResult
  {
    public IZipmod Target { get; }
    public string Filename { get; }
    public SessionResultType Type { get; }

    public SessionResult(IZipmod target, string filename, SessionResultType type)
    {
      Target = target;
      Filename = filename;
      Type = type;
    }
  }
}
