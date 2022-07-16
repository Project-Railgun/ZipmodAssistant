using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO.Compression;
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
    private readonly IOutputService _outputService;
    private readonly ISessionService _sessionService;
    private readonly IAssetService _assetService;
    private readonly ICardProvider _cardProvider;
    private readonly IServiceProvider _serviceProvider;

    public RepositoryService(
      ILoggerService logger,
      IOutputService outputService,
      ISessionService sessionService,
      IAssetService assetService,
      ICardProvider cardProvider,
      IServiceProvider serviceProvider)
    {
      _logger = logger;
      _outputService = outputService;
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
        Zipmod? zipmod = default;
        try
        {
          if (fileInfo.Name.Equals("manifest.xml"))
          {
            var manifest = await Manifest.ReadFromStreamAsync(fileInfo.OpenRead());
            var tempDirectory = Path.Join(configuration.CacheDirectory, manifest.Guid);
            zipmod = new(fileInfo, tempDirectory, manifest);
            _logger.Log($"Discovered mod directory at {fileInfo.Directory.FullName}");
          }
          else if (fileInfo.Extension.Equals(".zipmod") || fileInfo.Extension.Equals(".zip"))
          {
            using var zipArchive = ZipFile.OpenRead(filename);
            using var manifestStream = zipArchive.GetEntry("manifest.xml").Open();
            var manifest = await Manifest.ReadFromStreamAsync(manifestStream);
            var priorEntry = await dbContext.ManifestHistoryEntries.FindAsync(manifest.Hash);
            if (priorEntry != null)
            {
              if (priorEntry.CanSkip)
              {
                continue;
              }
              // TODO: version check
            }
            zipmod = new(fileInfo, Path.Join(configuration.OutputDirectory, fileInfo.NameWithoutExtension()), manifest);
            _logger.Log($"Discovered zipmod at {fileInfo.FullName}");
          }
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

    public async Task ProcessRepositoryAsync(IBuildRepository repository)
    {
      var startTime = DateTime.Now;
      _logger.Log($"Using configuration input: {repository.Configuration.InputDirectory}, output: {repository.Configuration.OutputDirectory}, cache: {repository.Configuration.CacheDirectory}");
      
      await Parallel.ForEachAsync(repository, async (zipmod, cancelToken) =>
      {
        var dbContext = _serviceProvider.GetService<ZipmodDbContext>();
        try
        {
          var tempOutputDirectory = Path.Join(repository.Configuration.CacheDirectory, zipmod.Manifest.Guid, ".output");
          Directory.CreateDirectory(tempOutputDirectory);
          if (zipmod.FileInfo.Name.Equals("manifest.xml") && zipmod.FileInfo.Directory != null)
          {
            zipmod.FileInfo.Directory.CopyTo(zipmod.WorkingDirectory, true);
            _logger.Log($"Copied {zipmod.Manifest.Guid} to {zipmod.WorkingDirectory}");
          }
          else
          {
            ZipFile.ExtractToDirectory(zipmod.FileInfo.FullName, zipmod.WorkingDirectory, true);
            _logger.Log($"Extracted zipmod to temp directory {zipmod.WorkingDirectory}");
          }
          File.Copy(
            Path.Join(zipmod.WorkingDirectory, "manifest.xml"),
            Path.Join(zipmod.WorkingDirectory, "manifest-orig.xml"),
            true);
          await Parallel.ForEachAsync(Directory.EnumerateFiles(zipmod.WorkingDirectory, "*.*", SearchOption.AllDirectories), async (file, cancelToken) =>
          {
            var fileInfo = new FileInfo(file);
            var path = file.Replace(zipmod.WorkingDirectory, string.Empty);
            var tempOutputPath = Path.Join(tempOutputDirectory, Path.GetRelativePath(zipmod.WorkingDirectory, file));
            _logger.Log(path, LogReason.Debug);
            switch (fileInfo.Extension.ToLower())
            {
              case ".png":
                var didCompress = await _assetService.CompressImageAsync(repository.Configuration, file);
                break;
              case ".unity3d":
                if (repository.Configuration.RandomizeCab)
                {
                  _logger.Log("Randomizing CAB", LogReason.Debug);
                  var randomizeCabDuration = await TimingUtilities.TimeAsync(async () =>
                  {
                    var didRandomize = await _assetService.RandomizeCabAsync(repository.Configuration, file);
                  });
                  _logger.Log($"CAB randomization of {file} took {randomizeCabDuration.TotalMilliseconds}ms", LogReason.Debug);
                }
                if (repository.Configuration.SkipCompression)
                {
                  _logger.Log("Compression skipped, copying", LogReason.Debug);
                  fileInfo.MoveToSafely(tempOutputPath, true);
                }
                else
                {
                  _logger.Log("Compressing unity3d file", LogReason.Debug);
                  var compressionDuration = await TimingUtilities.TimeAsync(async () =>
                  {
                    var didCompress = await _assetService.CompressUnityResxAsync(repository.Configuration, file);
                    // TODO: do something with this
                  });
                  _logger.Log($"Compression of {file} took {compressionDuration.TotalMilliseconds}ms", LogReason.Debug);
                }
                break;
              case ".csv":
                break;
              default:
                _logger.Log($"Invalid file, deleting", LogReason.Debug);
                fileInfo.Delete();
                break;
            }
          });
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
