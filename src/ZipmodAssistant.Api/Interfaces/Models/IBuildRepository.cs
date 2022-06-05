using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZipmodAssistant.Api.Interfaces.Models
{
  public interface IBuildRepository
  {
    IEnumerable<string> PngLocations { get; }
    IEnumerable<string> ZipmodLocations { get; }
    string? ManifestLocation { get; }
    IZipmodConfiguration Configuration { get; }
  }
}
