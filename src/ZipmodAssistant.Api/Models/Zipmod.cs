using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZipmodAssistant.Api.Enums;
using ZipmodAssistant.Api.Interfaces.Models;
using ZipmodAssistant.Api.Interfaces.Services;

namespace ZipmodAssistant.Api.Models
{
  public class Zipmod : IZipmod
  {
    public string WorkingDirectory { get; private set; }

    public FileInfo FileInfo { get; }

    public IManifest Manifest { get; private set; }

    public string Hash { get; }

    public Zipmod(IBuildConfiguration buildConfig, string manifestOrZipLocation)
    {
      FileInfo = new FileInfo(manifestOrZipLocation);
      if (FileInfo.Extension.Equals(".xml", StringComparison.CurrentCultureIgnoreCase))
      {
        WorkingDirectory = FileInfo.Directory.FullName;
      }
      else
      {
        WorkingDirectory = Path.Join(buildConfig.CacheDirectory, manifestOrZipLocation.GetHashCode().ToString());
      }
    }

    public async Task ProcessAsync(IBuildConfiguration buildConfig, ISessionService session)
    {
      Directory.CreateDirectory(WorkingDirectory);
      if (
        FileInfo.Extension.Equals(".zip", StringComparison.CurrentCultureIgnoreCase) ||
        FileInfo.Extension.Equals(".zipmod", StringComparison.CurrentCultureIgnoreCase))
      {

      }
      throw new NotImplementedException();
    }
  }
}
