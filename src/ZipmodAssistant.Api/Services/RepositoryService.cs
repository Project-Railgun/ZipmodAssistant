using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
using ZipmodAssistant.Api.Interfaces.Repositories;
using ZipmodAssistant.Api.Interfaces.Services;
using ZipmodAssistant.Api.Models;
using ZipmodAssistant.Api.Utilities;
using ZipmodAssistant.Tarot.Interfaces.Providers;

namespace ZipmodAssistant.Api.Services
{
  public class RepositoryService : IRepositoryService
  {
    private readonly ILogger<IRepositoryService> _logger;
    private readonly ISessionService _sessionService;
    private readonly IAssetService _assetService;
    private readonly ICardProvider _cardProvider;
    private readonly IZipmodRepository _zipmodRepository;

    public RepositoryService(
      ILogger<IRepositoryService> logger,
      ISessionService sessionService,
      IAssetService assetService,
      ICardProvider cardProvider,
      IZipmodRepository zipmodRepository)
    {
      _logger = logger;
      _sessionService = sessionService;
      _assetService = assetService;
      _cardProvider = cardProvider;
      _zipmodRepository = zipmodRepository;
    }

    public async Task<IBuildSetup> GetRepositoryFromDirectoryAsync(IBuildConfiguration configuration)
    {
      var repository = new BuildSetup(configuration);
      var repositoryLock = new object();

      var files = Directory.EnumerateFiles(configuration.InputDirectory, "*.*", SearchOption.AllDirectories);
      foreach (var filename in files)
      {
        var fileInfo = new FileInfo(filename);
        Manifest manifest;
        try
        {
          if (fileInfo.Name.Equals("manifest.xml"))
          {
            try
            {
              manifest = await Manifest.ReadFromStreamAsync(fileInfo.OpenRead());
            }
            catch (MalformedManifestException)
            {
              _logger.LogError("Received bad manifest: {filename}", filename);
              continue;
            }
          }
          else if (fileInfo.Extension.Equals(".zipmod") || fileInfo.Extension.Equals(".zip"))
          {
            try
            {
              using var zipArchive = System.IO.Compression.ZipFile.OpenRead(filename);
              using var manifestStream = zipArchive.GetEntry("manifest.xml").Open();
              manifest = await Manifest.ReadFromStreamAsync(manifestStream);
            }
            catch (MalformedManifestException)
            {
              _logger.LogDebug("Received bad manifest: {filename}", filename);
              fileInfo.MoveToSafely(Path.Join(configuration.OutputDirectory, "Malformed"));
              continue;
            }
          }
          else
          {
            continue;
          }
          if (configuration.SkipKnownMods && await _zipmodRepository.IsManifestInHistoryAsync(manifest))
          {
            _logger.LogDebug("{manifestGuid} is a known mod, skipping", manifest.Guid);
            continue;
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
          _logger.LogError(ex, "Failed to add {filename}", filename);
        }
      }

      return repository;
    }

    void UpdateManifests(IZipmod zipmod, IBuildConfiguration buildConfiguration)
    {
      File.Copy(zipmod.GetPath("manifest.xml"), zipmod.GetPath("manifest-orig.xml"), true);
      if (buildConfiguration.Games.Any())
      {
        zipmod.Manifest.Games = buildConfiguration.Games.Select(g => g.ToString()).ToArray();
      }
      File.WriteAllText(zipmod.GetPath("manifest.xml"), $"""
        <!-- Generated with ZipmodAssistant -->
        {zipmod.Manifest}
        """);
      _logger.LogDebug("Updated manifests for {manifestGuid}", zipmod.Manifest.Guid);
    }

    void MoveZipmodToWorkingDirectory(IZipmod zipmod)
    {
      if (zipmod.FileInfo.Name.Equals("manifest.xml") && zipmod.FileInfo.Directory != null)
      {
        zipmod.FileInfo.Directory.CopyTo(zipmod.WorkingDirectory, true);
        _logger.LogInformation("Copied {manifestGuid} to {workingDirectory}", zipmod.Manifest.Guid, zipmod.WorkingDirectory);
      }
      else
      {
        System.IO.Compression.ZipFile.ExtractToDirectory(zipmod.FileInfo.FullName, zipmod.WorkingDirectory, true);
        _logger.LogInformation("Extracted zipmod to temp directory {workingDirectory}", zipmod.WorkingDirectory);
      }
    }

    public async Task ProcessRepositoryAsync(IBuildSetup repository)
    {
      var startTime = DateTime.Now;
      await Parallel.ForEachAsync(repository, async (zipmod, cancelToken) =>
      {
        try
        {
          MoveZipmodToWorkingDirectory(zipmod);
          // this has to get called after extracted to the cache directory so we can scan for zipmod type
          if (await _zipmodRepository.IsNewerVersionAvailableAsync(zipmod))
          {
            _logger.LogInformation("{manifestGuid} has a newer version locally, skipping", zipmod.Manifest.Guid);
          }
          UpdateManifests(zipmod, repository.Configuration);
          var zipmodFiles = Directory.EnumerateFiles(zipmod.WorkingDirectory, "*.*", SearchOption.AllDirectories);
          await Parallel.ForEachAsync(zipmodFiles, async (file, cancelToken) =>
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
                  _logger.LogDebug("Randomizing CAB for {file}", file);
                  var randomizeCabDuration = await TimingUtilities.TimeAsync(async () =>
                  {
                    var didRandomize = await _assetService.RandomizeCabAsync(repository.Configuration, file);
                    await _sessionService.CommitResultAsync(new SessionResult(zipmod, file, didRandomize ? SessionResultType.ResourceCabRandomized : SessionResultType.NoChange));
                  });
                  _logger.LogDebug("CAB randomization of {file} took {time}ms", file, randomizeCabDuration.TotalMilliseconds);
                }
                if (repository.Configuration.SkipCompression)
                {
                  _logger.LogDebug("Compression skipped, copying");
                }
                else
                {
                  _logger.LogDebug("Compressing unity3d file");
                  var compressionDuration = await TimingUtilities.TimeAsync(async () =>
                  {
                    var didCompress = await _assetService.CompressUnityResxAsync(repository.Configuration, file);
                    await _sessionService.CommitResultAsync(new SessionResult(zipmod, file, didCompress ? SessionResultType.ResourceCompressed : SessionResultType.NoChange));
                    
                  });
                  _logger.LogDebug("Compression of {file} took {time}ms", file, compressionDuration.TotalMilliseconds);
                }
                break;
              case ".csv":
              case ".xml":
              case ".txt":
                await _sessionService.CommitResultAsync(new SessionResult(zipmod, file, SessionResultType.ResourceSkipped));
                break;
              case ".tmp":
                // this file is being worked on, skip
                break;
              default:
                _logger.LogDebug("Invalid file {file}, deleting", file);
                await _sessionService.CommitResultAsync(new SessionResult(zipmod, file, SessionResultType.ResourceDeleted));
                fileInfo.Delete();
                break;
            }
          });
          var zipmodType = zipmod.GetZipmodType();
          var outputDirectory = Path.Join(
            repository.Configuration.OutputDirectory,
              "treated",
              zipmodType.ToString()
            );
          var outputFilename = Path.Join(
              outputDirectory,
              TextUtilities.ResolveFilenameFromManifest(zipmod.Manifest));
          Directory.CreateDirectory(outputDirectory);
          if (File.Exists(outputFilename))
          {
            File.Delete(outputFilename);
          }
          using var archive = ZipArchive.Create();
          archive.AddAllFromDirectory(zipmod.WorkingDirectory);
          archive.SaveTo(outputFilename, CompressionType.None);
          await _sessionService.CommitResultAsync(new SessionResult(zipmod, outputFilename, SessionResultType.ZipmodCreated));
          if (await _zipmodRepository.AddZipmodAsync(zipmod))
          {
            _logger.LogDebug("Beginning tracking of {manifestGuid}", zipmod.Manifest.Guid);
          }
          else if (await _zipmodRepository.UpdateZipmodAsync(zipmod))
          {
            _logger.LogDebug("Updating prior entry of {manifestGuid}", zipmod.Manifest.Guid);
          }
          else
          {
            _logger.LogError("Failed to update history of {manifestGuid}", zipmod.Manifest.Guid);
          }
          if (!repository.Configuration.SkipCleanup)
          {
            Directory.Delete(zipmod.WorkingDirectory, true);
            _logger.LogInformation("Cleaned {cacheDirectory}", repository.Configuration.CacheDirectory);
          }
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, "Repository failed");
        }
      });
      var endTime = DateTime.Now;
      _logger.LogInformation("Processing complete, took {time}ms", (endTime - startTime).TotalMilliseconds);
    }
  }
}
