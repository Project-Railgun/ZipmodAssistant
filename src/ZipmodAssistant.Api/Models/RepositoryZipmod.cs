using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZipmodAssistant.Api.Data;
using ZipmodAssistant.Api.Data.DataModels;
using ZipmodAssistant.Api.Enums;
using ZipmodAssistant.Api.Interfaces.Models;
using ZipmodAssistant.Api.Interfaces.Services;

namespace ZipmodAssistant.Api.Models
{
  public class RepositoryZipmod : IZipmod
  {
    public RepositoryItemType ItemType => RepositoryItemType.Repository;

    public FileInfo FileInfo { get; }

    public string Hash => Manifest?.Hash ?? string.Empty;
    public Manifest? Manifest { get; private set; }

    private readonly ZipmodDbContext _dbContext;

    public RepositoryZipmod(FileInfo fileInfo, ZipmodDbContext dbContext)
    {
      FileInfo = fileInfo;
      _dbContext = dbContext;
    }

    public async Task<IProcessResult> ProcessAsync(IBuildConfiguration buildConfig, IOutputService output)
    {
      using var zipArchive = new ZipArchive(FileInfo.OpenRead());
      var cachedLocation = output.ReserveCacheDirectory(this, buildConfig);
      zipArchive.ExtractToDirectory(cachedLocation);
      var manifestFile = new FileInfo(Path.Join(cachedLocation, "manifest.xml"));
      if (!manifestFile.Exists)
      {
        return output.MarkAsMalformed(this, buildConfig, "No manifest.xml found");
      }
      manifestFile.CopyTo("manifest-orig.xml", true);
      using var manifestStream = manifestFile.OpenRead();
      Manifest = await Models.Manifest.ReadFromStreamAsync(manifestStream);

      var historyEntry = await _dbContext.ManifestHistoryEntries.FindAsync(Manifest.Hash);
      if (!TryValidateRepository(output, buildConfig, historyEntry, out var validateResult))
      {
        return validateResult;
      }

      await _dbContext.ManifestHistoryEntries.AddAsync(new()
      {
        Hash = Manifest.Hash,
        Guid = Manifest.Guid,
        Version = Manifest.Version,
        IsBlackListed = true,
      });
      return output.MarkAsCompleted(this, buildConfig);
    }


    bool TryValidateRepository(IOutputService output, IBuildConfiguration buildConfig, ManifestHistoryEntry? historyEntry, out IProcessResult result)
    {
      if (historyEntry?.IsBlackListed == true)
      {
        result = output.MarkAsBlacklisted(this, buildConfig);
        return false;
      }
      if (string.IsNullOrEmpty(Manifest.Guid))
      {
        result = output.MarkAsMalformed(this, buildConfig, "No GUID found");
        return false;
      }
      if (Version.TryParse(Manifest.Version, out var newVersion))
      {
        if (historyEntry != null && Version.Parse(historyEntry.Version) > newVersion)
        {
          result = output.MarkAsSkipped(this, buildConfig, $"A newer version is available: {historyEntry.Version}");
          return false;
        }
        result = new SuccessProcessResult(this);
        return true;
      }
      else
      {
        result = output.MarkAsMalformed(this, buildConfig, $"Invalid version: {Manifest.Version}");
        return false;
      }
    }
  }
}
