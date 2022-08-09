using Aspose.Imaging;
using Aspose.Imaging.ImageOptions;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ZipmodAssistant.Api.Interfaces.Models;
using ZipmodAssistant.Api.Interfaces.Services;
using ZipmodAssistant.Tarot.Interfaces.Providers;
using ZipmodAssistant.Tarot.Utilities;

namespace ZipmodAssistant.Api.Services
{
  public class AssetService : IAssetService
  {
    private readonly ILogger<IAssetService> _logger;
    private readonly ICardProvider _cardProvider;

    private static readonly Regex _mapResourcePath = new(@"abdata(\/|\\)map(\/|\\)scene(\/|\\).+\.unity3d$");

    public AssetService(ILogger<IAssetService> logger, ICardProvider cardProvider)
    {
      _logger = logger;
      _cardProvider = cardProvider;
    }

    public async Task<bool> CompressImageAsync(IBuildConfiguration buildConfig, string filename)
    {
      if (filename.EndsWith(".compressed.png"))
      {
        return false;
      }
      try
      {
        if (!await CardUtilities.ContainsDataAfterIEndAsync(filename))
        {
          // compress
          return true;
        }
        return false;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "An exception occured when compressing {filename}", filename);
        return false;
      }
    }

    public Task<bool> CompressUnityResxAsync(IBuildConfiguration buildConfig, string filename) => Task.Run(() =>
    {
      // I'd like to do a globbing pattern
      if (_mapResourcePath.IsMatch(filename))
      {
        return false;
      }
      var assetsManager = new AssetsManager();
      var bundle = assetsManager.LoadBundleFile(filename);
      var tempFileStream = File.Create($"{filename}.tmp");
      using(var assetsFileWriter = new AssetsFileWriter(tempFileStream))
      {
        bundle.file.Pack(bundle.file.reader, assetsFileWriter, AssetBundleCompressionType.LZ4);
        tempFileStream.Dispose();
      }
      File.Move($"{filename}.tmp", filename, true);

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
