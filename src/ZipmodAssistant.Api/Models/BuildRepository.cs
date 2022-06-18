using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ZipmodAssistant.Api.Data;
using ZipmodAssistant.Api.Data.DataModels;
using ZipmodAssistant.Api.Enums;
using ZipmodAssistant.Api.Exceptions;
using ZipmodAssistant.Api.Extensions;
using ZipmodAssistant.Api.Interfaces.Models;
using ZipmodAssistant.Api.Interfaces.Services;

namespace ZipmodAssistant.Api.Models
{
  public class BuildRepository : List<IRepositoryItem>, IBuildRepository
  {
    public IZipmodConfiguration Configuration { get; set; }
    public string RootDirectory { get; set; }
    public FileInfo FileInfo { get; }
    public RepositoryItemType ItemType => RepositoryItemType.Repository;
    public byte[] Hash => Array.Empty<byte>();
    public Manifest RepositoryManifest { get; private set; }

    private readonly ZipmodDbContext _dbContext;

    public BuildRepository(string rootDirectory, IZipmodConfiguration zipmodConfiguration, ZipmodDbContext dbContext)
    {
      RootDirectory = rootDirectory;
      Configuration = zipmodConfiguration;
      _dbContext = dbContext;
    }

    public BuildRepository(FileInfo fileInfo, ZipmodDbContext dbContext)
    {
      FileInfo = fileInfo;
      _dbContext = dbContext;
    }

    // TODO: in the morning, change this to unpack the zipmod and clean up in output folder
    public async Task<IProcessResult> ProcessAsync(IOutputService output, IBuildRepository repository)
    {
      if (FileInfo == null)
      {
        return new NoChangeProcessResult(this);
      }
      using var zipArchive = new ZipArchive(FileInfo.OpenRead());
      var manifestEntry = zipArchive.GetEntry("manifest.xml");
      if (manifestEntry == null)
      {
        return output.MarkAsMalformed(this, "No manifest.xml found");
      }
      using var manifestStream = manifestEntry.Open();
      RepositoryManifest = await Manifest.ReadFromStreamAsync(manifestStream);
      var historyEntry = await _dbContext.ManifestHistoryEntries.FindAsync(RepositoryManifest.Hash);
      if (!TryValidateRepository(output, historyEntry, out var processResult))
      {
        return processResult;
      }

      await _dbContext.ManifestHistoryEntries.AddAsync(new()
      {
        Hash = RepositoryManifest.Hash,
        Guid = RepositoryManifest.Guid,
        IsBlackListed = false,
        Version = RepositoryManifest.Version,
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
      if (string.IsNullOrEmpty(RepositoryManifest.Guid))
      {
        result = output.MarkAsMalformed(this, "No GUID found");
        return false;
      }
      if (Version.TryParse(RepositoryManifest.Version, out var newVersion))
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
        result = output.MarkAsMalformed(this, $"Invalid version: {RepositoryManifest.Version}");
        return false;
      }
    }
  }
}
