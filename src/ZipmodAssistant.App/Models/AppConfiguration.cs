using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZipmodAssistant.App.Interfaces.Models;

namespace ZipmodAssistant.App.Models
{
  public class AppConfiguration : IAppConfiguration
  {
    public string LogDirectory { get; set; } = "logs";
  }
}
