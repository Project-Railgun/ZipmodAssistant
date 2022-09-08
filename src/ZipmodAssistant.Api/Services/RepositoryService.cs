using Aspose.Imaging.MemoryManagement;
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
using ZipmodAssistant.Shared.Enums;
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
        try
        {
          if (fileInfo.Extension.Equals(".zipmod") || fileInfo.Extension.Equals(".zip"))
          {
            try
            {
              using var zipArchive = System.IO.Compression.ZipFile.OpenRead(filename);
              using var manifestStream = zipArchive.GetEntry("manifest.xml").Open();
              var manifest = await Manifest.ReadFromStreamAsync(manifestStream);
              if (string.IsNullOrEmpty(manifest.Author))
              {
                _logger.LogDebug("{manifestGuid} has an empty author, skipping", manifest.Guid);
              }
              var tempDirectory = Path.Join(configuration.CacheDirectory, TextUtilities.GetFileSafeGuid(manifest.Guid));
              var zipmod = new Zipmod(fileInfo, tempDirectory, manifest);
              if (zipmod != default)
              {
                lock (repositoryLock)
                {
                  repository.Add(zipmod);
                }
              }
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
      System.IO.Compression.ZipFile.ExtractToDirectory(zipmod.FileInfo.FullName, zipmod.WorkingDirectory, true);
      _logger.LogInformation("Extracted zipmod to temp directory {workingDirectory}", zipmod.WorkingDirectory);
    }

    public async Task ProcessRepositoryAsync(IBuildSetup repository)
    {
      var startTime = DateTime.Now;
      var treatedDirectory = Path.Join(repository.Configuration.OutputDirectory, "Treated", "Games");
      var skippedDirectory = Path.Join(repository.Configuration.OutputDirectory, "Skipped");
      var originalDirectory = Path.Join(repository.Configuration.OutputDirectory, "Original");

      await Parallel.ForEachAsync(repository, async (zipmod, cancelToken) =>
      {
        try
        {
          zipmod.FileInfo.CopyTo(new[] { originalDirectory }, true);
          if (repository.Configuration.SkipKnownMods && await _zipmodRepository.IsManifestInHistoryAsync(zipmod.Manifest))
          {
            _logger.LogDebug("{manifestGuid} is a known mod, skipping", zipmod.Manifest.Guid);
            await _sessionService.CommitResultAsync(new SessionResult(zipmod, zipmod.FileInfo.FullName, SessionResultType.NoChange));
            zipmod.FileInfo.CopyTo(new[] { skippedDirectory }, true);
            return;
          }
          MoveZipmodToWorkingDirectory(zipmod);

          var zipmodType = zipmod.GetZipmodType();
          var newFilename = repository.Configuration.SkipRenaming
            ? zipmod.FileInfo.Name
            : TextUtilities.ResolveFilenameFromManifest(zipmod.Manifest);
          var tempZipFilename = Path.Join(repository.Configuration.CacheDirectory, newFilename);


          // this has to get called after extracted to the cache directory so we can scan for zipmod type
          if (await _zipmodRepository.IsNewerVersionAvailableAsync(zipmod))
          {
            _logger.LogInformation("{manifestGuid} has a newer version locally, skipping", zipmod.Manifest.Guid);
            zipmod.FileInfo.CopyTo(skippedDirectory, true);
            return;
          }
          UpdateManifests(zipmod, repository.Configuration);
          IEnumerable<string> gameDirectories;
          if (zipmod.Manifest.Games?.Count(game => !string.IsNullOrEmpty(game)) > 0)
          {
            gameDirectories = zipmod.Manifest.Games.Select(game =>
              Path.Join(
                treatedDirectory,
                game.ToString(),
                zipmodType.ToString()));
          }
          else
          {
            gameDirectories = new[] { Path.Join(treatedDirectory, "None", zipmodType.ToString()) };
          }

          var looseFileDirectories = gameDirectories.Select(dir => Path.Join(dir, "Loose Images")).ToArray();
          var zipmodFiles = Directory.EnumerateFiles(zipmod.WorkingDirectory, "*.*", SearchOption.AllDirectories);
          await Parallel.ForEachAsync(zipmodFiles, async (file, cancelToken) =>
          {
            var fileInfo = new FileInfo(file);
            var path = file.Replace(zipmod.WorkingDirectory, string.Empty);
            switch (fileInfo.Extension.ToLower())
            {
              case ".png":
                var didCompress = await _assetService.CompressImageAsync(repository.Configuration, file);
                // only files with no data after IEND are compressed
                if (!didCompress)
                {
                  await _sessionService.CommitResultAsync(new SessionResult(zipmod, file, SessionResultType.ResourceCopied));
                }
                else
                {
                  fileInfo.CopyTo(looseFileDirectories, true);
                  await _sessionService.CommitResultAsync(new SessionResult(zipmod, file, SessionResultType.ImageCompressed));
                }
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
                  await _sessionService.CommitResultAsync(new SessionResult(zipmod, file, SessionResultType.ResourceCopied));
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
              case ".jpg":
              case ".jpeg":
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



          using var archive = ZipArchive.Create();
          archive.AddAllFromDirectory(zipmod.WorkingDirectory);
          archive.SaveTo(tempZipFilename, CompressionType.None);
          var tempZipFileInfo = new FileInfo(tempZipFilename);
          // save to temp zipfile to prepare copying
          var outputtedZipFiles = tempZipFileInfo.CopyTo(gameDirectories, true);
          foreach (var zipFile in outputtedZipFiles)
          {
            await _sessionService.CommitResultAsync(new SessionResult(zipmod, zipFile, SessionResultType.ResourceCopied));
          }

          // await _sessionService.CommitResultAsync(new SessionResult(zipmod, outputFilename, SessionResultType.ZipmodCreated));
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

        }
        catch (Exception ex)
        {
          _logger.LogError(ex, "Repository failed");
        }
      });
      if (!repository.Configuration.SkipCleanup)
      {
        try
        {
          Directory.Delete(repository.Configuration.CacheDirectory, true);
          _logger.LogInformation("Cleaned {cacheDirectory}", repository.Configuration.CacheDirectory);
        }
        catch (DirectoryNotFoundException)
        {
          // ignore and swallow
        }
      }
      var endTime = DateTime.Now;
      _logger.LogInformation("Processing complete, took {time}ms", (endTime - startTime).TotalMilliseconds);
    }
  }
}
