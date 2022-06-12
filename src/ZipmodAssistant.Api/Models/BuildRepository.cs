using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ZipmodAssistant.Api.Data;
using ZipmodAssistant.Api.Enums;
using ZipmodAssistant.Api.Exceptions;
using ZipmodAssistant.Api.Extensions;
using ZipmodAssistant.Api.Interfaces.Models;

namespace ZipmodAssistant.Api.Models
{
  public class BuildRepository : List<IRepositoryItem>, IBuildRepository
  {
    public IZipmodConfiguration Configuration { get; set; }
    public string RootDirectory { get; set; }
    public FileInfo FileInfo { get; }
    public RepositoryItemType ItemType => RepositoryItemType.Repository;
    public byte[] Hash => Array.Empty<byte>();

    private ZipmodDbContext _dbContext;

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

    public async Task<bool> ProcessAsync(IBuildConfiguration buildConfiguration, IBuildRepository repository)
    {
      // this will only get called if the repository is a zipmod/zip
      if (FileInfo != null)
      {
        var zipmodName = FileInfo.NameWithoutExtension();
        using var zipArchive = new ZipArchive(FileInfo.OpenRead());
        var manifestEntry = zipArchive.GetEntry("manifest.xml");
        if (manifestEntry == null)
        {
          return false;
        }
        using var manifestStream = manifestEntry.Open();
        var md5 = await CalculateStreamMd5Async(manifestStream);
        if (await IsMd5EligibleForSkipAsync(buildConfiguration, md5))
        {
          // copy all contents to output directory
          var outputFolder = Path.Combine(buildConfiguration.OutputDirectory, zipmodName);
          zipArchive.ExtractToDirectory(outputFolder);
          return true;
        }
        else
        {
          var cacheFolder = Path.Combine(buildConfiguration.CacheDirectory, zipmodName);
          // reset position of input stream
          manifestStream.Seek(0, SeekOrigin.Begin);
          var manifest = await Manifest.ReadFromStreamAsync(manifestStream);
          if (string.IsNullOrEmpty(manifest.Guid))
          {
            throw new MalformedManifestException(manifest, nameof(manifest.Guid));
          }
          if (string.IsNullOrEmpty(manifest.Name))
          {
            manifest.Name = zipmodName;
          }
          if (!Version.TryParse(manifest.Version, out var _))
          {
            throw new MalformedManifestException(manifest, nameof(manifest.Version));
          }
        }
      }
      return true;
    }

    private static async Task<string> CalculateStreamMd5Async(Stream stream)
    {
      using var md5 = MD5.Create();
      var streamHash = await md5.ComputeHashAsync(stream);
      return Convert.ToBase64String(streamHash);
    }

    private async Task<bool> IsMd5EligibleForSkipAsync(IBuildConfiguration buildConfiguration, string md5)
    {
      var priorEntry = await _dbContext.ManifestHistoryEntries.FindAsync(md5);
      return priorEntry?.IsBlackListed == true && buildConfiguration.SkipKnownMods;
    }
  }
}
