using AssetsTools.NET;
using AssetsTools.NET.Extra;
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
  public class RepositoryUnityResx : IRepositoryItem
  {
    public RepositoryItemType ItemType => RepositoryItemType.UnityResx;

    public FileInfo FileInfo { get; }

    public string Hash => string.Empty;

    public RepositoryUnityResx(FileInfo fileInfo)
    {
      FileInfo = fileInfo;
    }

    public async Task<IProcessResult> ProcessAsync(IBuildConfiguration buildConfig, IOutputService output)
    {
      // I'd like to somehow combine this and the file compression if possible
      if (buildConfig.RandomizeCab)
      {
        var bundleData = File.ReadAllBytes(FileInfo.FullName);
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
            await File.WriteAllBytesAsync(FileInfo.FullName, bundleData);
          }
          
        }
      }
      if (!buildConfig.SkipCompression)
      {
        var assetsManager = new AssetsManager();
        var bundle = assetsManager.LoadBundleFile(FileInfo.FullName);
        using var tempFileStream = output.ReserveCacheFile(this, buildConfig);
        using var assetsFileWriter = new AssetsFileWriter(tempFileStream);
        bundle.file.Pack(bundle.file.reader, assetsFileWriter, AssetBundleCompressionType.LZ4);
        return output.MarkAsCompleted(this, buildConfig);
      }
      return output.CopyOriginal(this, buildConfig);
    }
  }
}
