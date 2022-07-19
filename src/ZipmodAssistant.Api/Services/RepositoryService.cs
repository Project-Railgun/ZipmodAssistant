using Microsoft.Extensions.DependencyInjection;
using SharpCompress.Archives;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZipmodAssistant.Api.Data;
using ZipmodAssistant.Api.Enums;
using ZipmodAssistant.Api.Exceptions;
using ZipmodAssistant.Api.Extensions;
using ZipmodAssistant.Api.Interfaces.Models;
using ZipmodAssistant.Api.Interfaces.Services;
using ZipmodAssistant.Api.Models;
using ZipmodAssistant.Api.Utilities;
using ZipmodAssistant.Tarot.Interfaces.Providers;

namespace ZipmodAssistant.Api.Services
{
  public class RepositoryService : IRepositoryService
  {
    private readonly ILoggerService _logger;
    private readonly ISessionService _sessionService;
    private readonly IAssetService _assetService;
    private readonly ICardProvider _cardProvider;
    private readonly IServiceProvider _serviceProvider;

    public RepositoryService(
      ILoggerService logger,
      ISessionService sessionService,
      IAssetService assetService,
      ICardProvider cardProvider,
      IServiceProvider serviceProvider)
    {
      _logger = logger;
      _sessionService = sessionService;
      _assetService = assetService;
      _cardProvider = cardProvider;
      _serviceProvider = serviceProvider;
    }

    public async Task<IBuildRepository> GetRepositoryFromDirectoryAsync(IBuildConfiguration configuration)
    {
      var repository = new BuildRepository(configuration);
      var repositoryLock = new object();

      var dbContext = _serviceProvider.GetService<ZipmodDbContext>();
      var files = Directory.EnumerateFiles(configuration.InputDirectory, "*.*", SearchOption.AllDirectories);
      foreach (var filename in files)
      {
        var fileInfo = new FileInfo(filename);
        Manifest? manifest = default;
        try
        {
          if (fileInfo.Name.Equals("manifest.xml"))
          {
            manifest = await Manifest.ReadFromStreamAsync(fileInfo.OpenRead());
          }
          else if (fileInfo.Extension.Equals(".zipmod") || fileInfo.Extension.Equals(".zip"))
          {
            using var zipArchive = System.IO.Compression.ZipFile.OpenRead(filename);
            using var manifestStream = zipArchive.GetEntry("manifest.xml").Open();
            manifest = await Manifest.ReadFromStreamAsync(manifestStream);
          }
          else
          {
            continue;
          }
          var priorEntry = await dbContext.ManifestHistoryEntries.FindAsync(manifest.Hash);
          if (priorEntry != null)
          {
            if (priorEntry.CanSkip)
            {
              continue;
            }
            // TODO: version check
          }
          var tempDirectory = Path.Join(configuration.CacheDirectory, manifest.Guid);
          var zipmod = new Zipmod(fileInfo, tempDirectory, manifest);
          if (zipmod != default)
          {
            lock (repositoryLock)
            {
              repository.Add(zipmod);
            }
          }
        }
        catch (Exception ex)
        {
          _logger.Log($"Failed to add {filename}\n{ex}", LogReason.Error);
        }
      }

      return repository;
    }

    static string GetZipmodFile(IZipmod zipmod, string filename) => Path.Join(zipmod.WorkingDirectory, filename);

    void UpdateManifests(IZipmod zipmod)
    {
      File.Copy(GetZipmodFile(zipmod, "manifest.xml"), GetZipmodFile(zipmod, "manifest-orig.xml"));
      File.WriteAllText(GetZipmodFile(zipmod, "manifest.xml"), $"""
        <!-- Generated with ZipmodAssistant -->
        {zipmod.Manifest}
        """);
      _logger.Log($"Updated manifests for {zipmod.Manifest.Guid}", LogReason.Debug);
    }

    void MoveZipmodToWorkingDirectory(IZipmod zipmod)
    {
      if (zipmod.FileInfo.Name.Equals("manifest.xml") && zipmod.FileInfo.Directory != null)
      {
        zipmod.FileInfo.Directory.CopyTo(zipmod.WorkingDirectory, true);
        _logger.Log($"Copied {zipmod.Manifest.Guid} to {zipmod.WorkingDirectory}");
      }
      else
      {
        System.IO.Compression.ZipFile.ExtractToDirectory(zipmod.FileInfo.FullName, zipmod.WorkingDirectory, true);
        _logger.Log($"Extracted zipmod to temp directory {zipmod.WorkingDirectory}");
      }
    }

    public async Task ProcessRepositoryAsync(IBuildRepository repository)
    {
      var startTime = DateTime.Now;
      await Parallel.ForEachAsync(repository, async (zipmod, cancelToken) =>
      {
        var dbContext = _serviceProvider.GetService<ZipmodDbContext>();
        try
        {
          MoveZipmodToWorkingDirectory(zipmod);
          UpdateManifests(zipmod);
          await Parallel.ForEachAsync(Directory.EnumerateFiles(zipmod.WorkingDirectory, "*.*", SearchOption.AllDirectories), async (file, cancelToken) =>
          {
            var fileInfo = new FileInfo(file);
            var path = file.Replace(zipmod.WorkingDirectory, string.Empty);
            switch (fileInfo.Extension.ToLower())
            {
              case ".png":
                var didCompress = await _assetService.CompressImageAsync(repository.Configuration, file);
                await _sessionService.CommitResultAsync(new SessionResult(zipmod, file, didCompress ? SessionResultType.ImageCompressed : SessionResultType.NoChange));
                break;
              case ".unity3d":
                if (repository.Configuration.RandomizeCab)
                {
                  _logger.Log("Randomizing CAB", LogReason.Debug);
                  var randomizeCabDuration = await TimingUtilities.TimeAsync(async () =>
                  {
                    var didRandomize = await _assetService.RandomizeCabAsync(repository.Configuration, file);
                    await _sessionService.CommitResultAsync(new SessionResult(zipmod, file, didRandomize ? SessionResultType.ResourceCabRandomized : SessionResultType.NoChange));
                  });
                  _logger.Log($"CAB randomization of {file} took {randomizeCabDuration.TotalMilliseconds}ms", LogReason.Debug);
                }
                if (repository.Configuration.SkipCompression)
                {
                  _logger.Log("Compression skipped, copying", LogReason.Debug);
                }
                else
                {
                  _logger.Log("Compressing unity3d file", LogReason.Debug);
                  var compressionDuration = await TimingUtilities.TimeAsync(async () =>
                  {
                    var didCompress = await _assetService.CompressUnityResxAsync(repository.Configuration, file);
                    await _sessionService.CommitResultAsync(new SessionResult(zipmod, file, didCompress ? SessionResultType.ResourceCompressed : SessionResultType.NoChange));
                    
                  });
                  _logger.Log($"Compression of {file} took {compressionDuration.TotalMilliseconds}ms", LogReason.Debug);
                }
                break;
              case ".csv":
              case ".xml":
              case ".txt":
                await _sessionService.CommitResultAsync(new SessionResult(zipmod, file, SessionResultType.ResourceSkipped));
                break;
              default:
                _logger.Log($"Invalid file, deleting", LogReason.Debug);
                await _sessionService.CommitResultAsync(new SessionResult(zipmod, file, SessionResultType.ResourceDeleted));
                fileInfo.Delete();
                break;
            }
          });
          var outputFilename = Path.Join(
              repository.Configuration.OutputDirectory,
              "treated",
              TextUtilities.ResolveFilenameFromManifest(zipmod.Manifest));
          if (File.Exists(outputFilename))
          {
            File.Delete(outputFilename);
          }
          using var archive = ZipArchive.Create();
          archive.AddAllFromDirectory(zipmod.WorkingDirectory);
          archive.SaveTo(outputFilename, CompressionType.None);
          await _sessionService.CommitResultAsync(new SessionResult(zipmod, outputFilename, SessionResultType.ZipmodCreated));
          if (!repository.Configuration.SkipCleanup)
          {
            Directory.Delete(zipmod.WorkingDirectory);
            _logger.Log($"Cleaned {repository.Configuration.CacheDirectory}");
          }
        }
        catch (Exception ex)
        {
          _logger.Log(ex);
        }
      });
      var endTime = DateTime.Now;
      _logger.Log($"Processing complete, took {(endTime - startTime).TotalMilliseconds}ms");
    }
  }
}
