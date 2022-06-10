using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZipmodAssistant.Tarot.Interfaces.Models
{
  public interface IAdditionalData
  {
    IList<string> RequiredPluginGuids { get; }
    IList<string> RequiredZipmodNames { get; }
    int Version { get; }
    IDictionary<string, object> Data { get; }
  }
}
