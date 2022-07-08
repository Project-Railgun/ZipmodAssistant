using AssetsTools.NET;
using AssetsTools.NET.Extra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZipmodAssistant.Api.Interfaces.Models;
using ZipmodAssistant.Api.Interfaces.Services;

namespace ZipmodAssistant.Api.Services
{
  public class AssetService : IAssetService
  {
    private readonly ILoggerService _logger;
    private readonly IOutputService _outputService;

    public AssetService(ILoggerService logger, IOutputService outputService)
    {
      _logger = logger;
      _outputService = outputService;
    }

    public async Task<bool> CompressImageAsync(IBuildConfiguration buildConfig, string filename)
    {
      throw new NotImplementedException();
    }

    public Task<bool> CompressUnityResxAsync(IBuildConfiguration buildConfig, string filename) => Task.Run(() =>
    {
      var assetsManager = new AssetsManager();
      var bundle = assetsManager.LoadBundleFile(filename);
      using var tempFileStream = File.Create($"{filename}.tmp");
      using var assetsFileWriter = new AssetsFileWriter(tempFileStream);
      bundle.file.Pack(bundle.file.reader, assetsFileWriter, AssetBundleCompressionType.LZ4);
      return true;
    });

    public async Task<bool> RandomizeCabAsync(IBuildConfiguration buildConfig, string filename)
    {
      // TODO: find a better way, this shit is innefficient due to reading the whole file to memory
      var bundleData = File.ReadAllBytes(filename);
      var bundleDataAsAscii = Encoding.ASCII.GetString(bundleData, 0, Math.Min(1024, bundleData.Length - 4));
      var cabIndex = bundleDataAsAscii.IndexOf("CAB-", StringComparison.Ordinal);
      if (cabIndex >= 0)
      {
        var cabLength = bundleDataAsAscii[cabIndex..].IndexOf('\0');
        if (cabLength <= 36)
        {
          var rngBuffer = new byte[16];
          Random.Shared.NextBytes(rngBuffer);
          var cab = $"CAB-{string.Concat(rngBuffer.Select(b => ((int)b).ToString("X2"))).ToLower()}";
          Buffer.BlockCopy(Encoding.ASCII.GetBytes(cab), 36 - cabLength, bundleData, cabIndex + 4, cabLength - 4);
          await File.WriteAllBytesAsync(filename, bundleData);
          return true;
        }
      }
      return false;
    }
  }
}
