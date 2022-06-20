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
  public class RepositoryZipmod : IRepositoryItem
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
      var manifestFromZip = zipArchive.GetEntry("manifest.xml");
      if (manifestFromZip == null)
      {
        return output.MarkAsMalformed(this, "No manifest.xml found");
      }
      using var manifestStream = manifestFromZip.Open();
      Manifest = await Models.Manifest.ReadFromStreamAsync(manifestStream);

      var historyEntry = await _dbContext.ManifestHistoryEntries.FindAsync(Manifest.Hash);
      if (!TryValidateRepository(output, historyEntry, out var validateResult))
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
      zipArchive.ExtractToDirectory(output.ReserveCache(this));
      return output.MarkAsCompleted(this);
    }


    bool TryValidateRepository(IOutputService output, ManifestHistoryEntry? historyEntry, out IProcessResult result)
    {
      if (historyEntry?.IsBlackListed == true)
      {
        result = output.MarkAsBlacklisted(this);
        return false;
      }
      if (string.IsNullOrEmpty(Manifest.Guid))
      {
        result = output.MarkAsMalformed(this, "No GUID found");
        return false;
      }
      if (Version.TryParse(Manifest.Version, out var newVersion))
      {
        if (historyEntry != null && Version.Parse(historyEntry.Version) > newVersion)
        {
          result = output.MarkAsSkipped(this, $"A newer version is available: {historyEntry.Version}");
          return false;
        }
        result = new SuccessProcessResult(this);
        return true;
      }
      else
      {
        result = output.MarkAsMalformed(this, $"Invalid version: {Manifest.Version}");
        return false;
      }
    }
  }
}
