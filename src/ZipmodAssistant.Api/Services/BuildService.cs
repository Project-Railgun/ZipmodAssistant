using Aspose.Imaging.MemoryManagement;
using Ionic.Zip;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using ZipmodAssistant.Tarot.Utilities;
using ZipFile = Ionic.Zip.ZipFile;

namespace ZipmodAssistant.Api.Services
{
  public class BuildService : IBuildService
  {
    private class BuildDirWrapper
    {
      private readonly IBuildConfiguration _configuration;

      public string LooseImages => Path.Join(_configuration.OutputDirectory, LOOSEIMAGES_DIRNAME);
      public string LooseCards => Path.Join(_configuration.OutputDirectory, LOOSECARDS_DIRNAME);
      public string Malformed => Path.Join(_configuration.OutputDirectory, MALFORMED_DIRNAME);
      public string Treated => Path.Join(_configuration.OutputDirectory, TREATED_DIRNAME, "Games");
      public string Original => Path.Join(_configuration.OutputDirectory, ORIG_DIRNAME);
      public string Skipped => Path.Join(_configuration.OutputDirectory, SKIPPED_DIRNAME);

      public BuildDirWrapper(IBuildConfiguration configuration)
      {
        _configuration = configuration;
      }

      public void EnsureCreated()
      {
        Directory.CreateDirectory(LooseImages);
        Directory.CreateDirectory(LooseCards);
        Directory.CreateDirectory(Malformed);
        Directory.CreateDirectory(Treated);
        Directory.CreateDirectory(Original);
        Directory.CreateDirectory(Skipped);
      }
    }

    private const string MANIFEST_FILENAME = "manifest.xml";
    private const string MANIFESTORIG_FILENAME = "manifest-orig.xml";
    private const string MALFORMED_DIRNAME = "Malformed";
    private const string LOOSEIMAGES_DIRNAME = "LooseImages";
    private const string LOOSECARDS_DIRNAME = "LooseCards";
    private const string TREATED_DIRNAME = "Treated";
    private const string ORIG_DIRNAME = "Original";
    private const string SKIPPED_DIRNAME = "Skipped";

    private static readonly Regex _charResxRegex = new(@"abdata\\list\\characustom\\(.+\\)*.+(_head_00)\.csv$");

    private readonly ILogger<IBuildService> _logger;
    private readonly ISessionService _sessionService;
    private readonly IAssetService _assetService;
    private readonly ICardProvider _cardProvider;
    private readonly IZipmodRepository _zipmodRepository;

    public BuildService(
      ILogger<IBuildService> logger,
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
      var dirs = new BuildDirWrapper(configuration);
      dirs.EnsureCreated();

      var files = Directory.EnumerateFiles(configuration.InputDirectory, "*.*", SearchOption.AllDirectories);
      foreach (var filename in files)
      {
        var fileInfo = new FileInfo(filename);
        try
        {
          if (fileInfo.Extension == ".zipmod" || fileInfo.Extension == ".zip")
          {
            try
            {
              using var zipArchive = System.IO.Compression.ZipFile.OpenRead(filename);
              using var manifestStream = zipArchive.GetEntry(MANIFEST_FILENAME).Open();
              var manifest = await Manifest.ReadFromStreamAsync(manifestStream);
              if (string.IsNullOrEmpty(manifest.Guid))
              {
                throw new MalformedManifestException("No GUID found");
              }
              if (string.IsNullOrEmpty(manifest.Author))
              {
                throw new MalformedManifestException($"{manifest.Guid} has an empty author");
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
              _logger.LogInformation("Received bad manifest: {filename}", filename);
              fileInfo.CopyTo(Path.Join(dirs.Malformed, filename), true);
              continue;
            }
          }
          else if (fileInfo.Extension == ".png")
          {
            if (await CardUtilities.ContainsDataAfterIEndAsync(filename))
            {
              fileInfo.CopyTo(Path.Join(dirs.LooseCards, fileInfo.Name), true);
            }
            else
            {
              fileInfo.CopyTo(Path.Join(dirs.LooseImages, fileInfo.Name), true);
            }
            fileInfo.Delete();
          }
          else
          {
            _logger.LogInformation("Unknown file type {filename}, skipping", filename);
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
      File.Copy(zipmod.GetPath(MANIFEST_FILENAME), zipmod.GetPath(MANIFESTORIG_FILENAME), true);
      if (buildConfiguration.Games.Any())
      {
        zipmod.Manifest.Games = buildConfiguration.Games.Select(g => g.ToString()).ToArray();
      }
      
      File.WriteAllText(zipmod.GetPath(MANIFEST_FILENAME), zipmod.Manifest.ToString());
      _logger.LogInformation("Updated manifests for {manifestGuid}", zipmod.Manifest.Guid);
    }

    void MoveZipmodToWorkingDirectory(IZipmod zipmod)
    {
      System.IO.Compression.ZipFile.ExtractToDirectory(zipmod.FileInfo.FullName, zipmod.WorkingDirectory, true);
      _logger.LogInformation("Extracted zipmod to temp directory {workingDirectory}", zipmod.WorkingDirectory);
    }

    public async Task ProcessRepositoryAsync(IBuildSetup repository)
    {
      var startTime = DateTime.Now;
      var dirs = new BuildDirWrapper(repository.Configuration);
      dirs.EnsureCreated();

      await Parallel.ForEachAsync(repository, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount / 2 }, async (zipmod, cancelToken) =>
      {
        try
        {
          if (repository.Configuration.SkipKnownMods && await _zipmodRepository.IsManifestInHistoryAsync(zipmod.Manifest))
          {
            _logger.LogInformation("{manifestGuid} is a known mod, skipping", zipmod.Manifest.Guid);
            await _sessionService.CommitResultAsync(new SessionResult(zipmod, zipmod.FileInfo.FullName, SessionResultType.NoChange));
            zipmod.FileInfo.CopyTo(Path.Join(dirs.Skipped, zipmod.FileInfo.Name), true);
            return;
          }
          MoveZipmodToWorkingDirectory(zipmod);

          var zipmodType = zipmod.GetZipmodType();


          // this has to get called after extracted to the cache directory so we can scan for zipmod type
          if (await _zipmodRepository.IsNewerVersionAvailableAsync(zipmod))
          {
            _logger.LogInformation("{manifestGuid} has a newer version locally, skipping", zipmod.Manifest.Guid);
            zipmod.FileInfo.CopyTo(dirs.Skipped, true);
            return;
          }
          UpdateManifests(zipmod, repository.Configuration);
          string gameDirectory;
          if (zipmod.Manifest.Games?.Count(game => !string.IsNullOrWhiteSpace(game)) > 0)
          {
            gameDirectory = Path.Join(
              dirs.Treated,
              string.Join(' ', zipmod.Manifest.Games.Select(game => game.ToString().Replace(" ", ""))),
              zipmodType.ToString());
          }
          else
          {
            gameDirectory = Path.Join(dirs.Treated, "None", zipmodType.ToString());
          }

          var newFilename = repository.Configuration.SkipRenaming
            ? zipmod.FileInfo.Name
            : TextUtilities.ResolveFilenameFromManifest(zipmod.Manifest);
          var outputFilename = Path.Join(gameDirectory, newFilename);
          var zipmodFiles = Directory.EnumerateFiles(zipmod.WorkingDirectory, "*.*", SearchOption.AllDirectories);
          var hasCharaMods = zipmodFiles.Any(_charResxRegex.IsMatch);

          Directory.CreateDirectory(gameDirectory);

          await Parallel.ForEachAsync(zipmodFiles, async (filename, cancelToken) =>
          {
            var fileInfo = new FileInfo(filename);
            var path = filename.Replace(zipmod.WorkingDirectory, string.Empty);

            if (!IsModFile(fileInfo))
            {
              switch (fileInfo.Extension)
              {
                case ".jpg":
                case ".jpeg":
                  fileInfo.CopyTo(Path.Join(dirs.LooseImages, fileInfo.Name), true);
                  fileInfo.Delete();
                  return;
                case ".png":
                  if (await CardUtilities.ContainsDataAfterIEndAsync(filename))
                  {
                    fileInfo.CopyTo(Path.Join(dirs.LooseCards, fileInfo.Name), true);
                  }
                  else
                  {
                    fileInfo.CopyTo(Path.Join(dirs.LooseImages, fileInfo.Name), true);
                  }
                  fileInfo.Delete();
                  return;
                default:
                  return;
              }
            }

            switch (fileInfo.Extension.ToLower())
            {
              case ".png":
                if (fileInfo.Name.EndsWith("-crushed.png"))
                {
                  break;
                }
                if (await CardUtilities.ContainsDataAfterIEndAsync(filename))
                {
                  // dealing with a card
                  fileInfo.CopyTo(Path.Join(dirs.LooseCards, fileInfo.Name), true);
                  fileInfo.Delete();
                  await _sessionService.CommitResultAsync(new SessionResult(zipmod, filename, SessionResultType.ImageDeleted));
                }
                else
                {
                  var didCompressImage = false;
                  var compressImageTime = await TimingUtilities.TimeAsync(async () =>
                  {
                    didCompressImage = await _assetService.CompressImageAsync(repository.Configuration, filename);
                  });
                  if (didCompressImage)
                  {
                    _logger.LogInformation("Image compression of {filename} took {time}ms", filename, compressImageTime.TotalMilliseconds);
                    await _sessionService.CommitResultAsync(new SessionResult(zipmod, filename, SessionResultType.ImageCompressed));
                  }
                  else
                  {
                    // an error occured
                    fileInfo.CopyTo(Path.Join(dirs.LooseImages, fileInfo.Name), true);
                    await _sessionService.CommitResultAsync(new SessionResult(zipmod, filename, SessionResultType.NoChange));
                  }
                }
                // if compressed, it'll create a {filename}-orig.png, with the new file being compressed
                
                break;
              case ".unity3d":
                if (repository.Configuration.RandomizeCab)
                {
                  _logger.LogInformation("Randomizing CAB for {file}", filename);
                  var didRandomizeCab = false;
                  var randomizeCabDuration = await TimingUtilities.TimeAsync(async () =>
                  {
                    didRandomizeCab = await _assetService.RandomizeCabAsync(repository.Configuration, filename);
                    
                  });
                  await _sessionService.CommitResultAsync(new SessionResult(zipmod, filename, didRandomizeCab ? SessionResultType.ResourceCabRandomized : SessionResultType.NoChange));
                  if (didRandomizeCab)
                  {
                    _logger.LogInformation("CAB randomization of {file} took {time}ms", filename, randomizeCabDuration.TotalMilliseconds);
                  }
                  
                }
                if (repository.Configuration.SkipCompression)
                {
                  _logger.LogInformation("Compression skipped, copying");
                  await _sessionService.CommitResultAsync(new SessionResult(zipmod, filename, SessionResultType.ResourceCopied));
                }
                else if (repository.Configuration.SkipCharaMods && hasCharaMods)
                {
                  _logger.LogInformation("Compression skipped for chara mods");
                  await _sessionService.CommitResultAsync(new SessionResult(zipmod, filename, SessionResultType.ResourceCopied));
                }
                else
                {
                  _logger.LogInformation("Compressing unity3d file");
                  var didCompressResx = false;
                  var compressionDuration = await TimingUtilities.TimeAsync(async () =>
                  {
                    didCompressResx = await _assetService.CompressUnityResxAsync(repository.Configuration, filename);
                  });
                  await _sessionService.CommitResultAsync(new SessionResult(zipmod, filename, didCompressResx ? SessionResultType.ResourceCompressed : SessionResultType.NoChange));
                  if (didCompressResx)
                  {
                    _logger.LogInformation("Compression of {file} took {time}ms", filename, compressionDuration.TotalMilliseconds);
                  }
                  
                }
                break;
              case ".csv":
              case ".xml":
              case ".txt":
                await _sessionService.CommitResultAsync(new SessionResult(zipmod, filename, SessionResultType.ResourceSkipped));
                break;
              case ".jpg":
              case ".jpeg":
                break;
              case ".tmp":
                // this file is being worked on, skip
                break;
              default:
                _logger.LogInformation("Invalid file {file}, deleting", filename);
                await _sessionService.CommitResultAsync(new SessionResult(zipmod, filename, SessionResultType.ResourceDeleted));
                fileInfo.Delete();
                break;
            }
          });

          if (File.Exists(outputFilename))
          {
            File.Delete(outputFilename);
          }
          
          using (var archive = new ZipFile(outputFilename))
          {
            archive.CompressionLevel = Ionic.Zlib.CompressionLevel.Level0;
            archive.CompressionMethod = CompressionMethod.None;
            
            archive.AddDirectory(zipmod.WorkingDirectory);
            archive.Save();
          }

          // gonna have to do this instead of MoveTo because it throws permissions errors
          zipmod.FileInfo.CopyTo(Path.Join(dirs.Original, zipmod.FileInfo.Name), true);
          zipmod.FileInfo.Delete();
          _logger.LogInformation("Output {originalFilename} to {newFilename}", zipmod.FileInfo.FullName, outputFilename);

          await _sessionService.CommitResultAsync(new SessionResult(zipmod, outputFilename, SessionResultType.ResourceCopied));

          if (await _zipmodRepository.AddZipmodAsync(zipmod))
          {
            _logger.LogInformation("Beginning tracking of {manifestGuid}", zipmod.Manifest.Guid);
          }
          else if (await _zipmodRepository.UpdateZipmodAsync(zipmod))
          {
            _logger.LogInformation("Updating prior entry of {manifestGuid}", zipmod.Manifest.Guid);
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

    static bool IsModFile(FileInfo fileInfo) =>
      fileInfo.Name == MANIFEST_FILENAME ||
      fileInfo.Name == MANIFESTORIG_FILENAME ||
      fileInfo.FullName.Contains("abdata" + Path.DirectorySeparatorChar);
  }
}
