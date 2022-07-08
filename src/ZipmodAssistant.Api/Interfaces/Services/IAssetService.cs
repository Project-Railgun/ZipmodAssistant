using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZipmodAssistant.Api.Interfaces.Models;

namespace ZipmodAssistant.Api.Interfaces.Services
{
  public interface IAssetService
  {
    Task<bool> RandomizeCabAsync(IBuildConfiguration buildConfig, string filename);

    Task<bool> CompressUnityResxAsync(IBuildConfiguration buildConfig, string filename);

    Task<bool> CompressImageAsync(IBuildConfiguration buildConfig, string filename);
  }
}
