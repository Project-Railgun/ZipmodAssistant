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
using ZipmodAssistant.Tarot.Interfaces.Providers;

namespace ZipmodAssistant.Api.Services
{
  public class RepositoryService : IRepositoryService
  {
    private readonly ILoggerService _logger;
    private readonly IOutputService _outputService;
    private readonly ISessionService _sessionService;
    private readonly ICardProvider _cardProvider;
    private readonly ZipmodDbContext _dbContext;

    public RepositoryService(
      ILoggerService logger,
      IOutputService outputService,
      ISessionService sessionService,
      ICardProvider cardProvider,
      ZipmodDbContext dbContext)
    {
      _logger = logger;
      _outputService = outputService;
      _sessionService = sessionService;
      _cardProvider = cardProvider;
      _dbContext = dbContext;
    }

    public async Task<IBuildRepository> GetRepositoryFromDirectoryAsync(IBuildConfiguration configuration)
    {
      var repository = new BuildRepository(configuration, _dbContext);
      var repositoryLock = new object();

      var files = Directory.EnumerateFiles(configuration.InputDirectory, "*.*", SearchOption.AllDirectories);
      
      await Parallel.ForEachAsync(files, async (filename, cancelToken) =>
      {
        var fileInfo = new FileInfo(filename);
        Zipmod? zipmod = default;
        try
        {
          if (fileInfo.Name.Equals("manifest.xml", StringComparison.InvariantCultureIgnoreCase))
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
            zipmod = new(fileInfo, Path.Join(configuration.OutputDirectory, fileInfo.Name), manifest);
            _logger.Log($"Discovered zipmod at {fileInfo.FullName}");
          }
          if (zipmod != default)
          {
            lock (repositoryLock)
            {
              repository.Add(zipmod);
            }
          }
        } catch (Exception ex)
        {
          _logger.Log($"Failed to add {filename}\n{ex}", LogReason.Error);
        }
      });

      return repository;
    }

    public async Task ProcessRepositoryAsync(IBuildRepository repository)
    {
      var startTime = DateTime.Now;
      _logger.Log($"Using configuration input: {repository.Configuration.InputDirectory}, output: {repository.Configuration.OutputDirectory}, cache: {repository.Configuration.CacheDirectory}");
      Directory.CreateDirectory(repository.Configuration.CacheDirectory);
      await Parallel.ForEachAsync(repository, async (zipmod, cancelToken) =>
      {
        try
        {
          if (
            zipmod.FileInfo.Name.Equals("manifest.xml", StringComparison.CurrentCultureIgnoreCase) &&
            zipmod.FileInfo.Directory != null)
          {
            zipmod.FileInfo.Directory.CopyTo(zipmod.WorkingDirectory, true);
            _logger.Log($"Copied {zipmod.Manifest.Guid} to {zipmod.WorkingDirectory}");
          }
          else
          {
            var historyEntry = await _dbContext.ManifestHistoryEntries.FindAsync(new[] { zipmod.Hash }, cancelToken);
            if (historyEntry?.CanSkip == true && repository.Configuration.SkipKnownMods)
            {
              _logger.Log($"Skipping zipmod {zipmod.Manifest.Guid}");
              return;
            }
            ZipFile.ExtractToDirectory(zipmod.FileInfo.FullName, zipmod.WorkingDirectory, true);
            _logger.Log($"Extracted zipmod to temp directory {zipmod.WorkingDirectory}");
          }
          File.Copy(Path.Join(zipmod.WorkingDirectory, "manifest.xml"), Path.Join(zipmod.WorkingDirectory, "manifest-orig.xml"));

          foreach (var file in Directory.EnumerateFiles(zipmod.WorkingDirectory, "*.*"))
          {
            var fileInfo = new FileInfo(file);
            if (fileInfo.Extension.Equals(".png", StringComparison.InvariantCultureIgnoreCase))
            {
              var card = await _cardProvider.TryReadCardAsync(fileInfo);
              if (card == null)
              {
                _logger.Log($"No data found after IEND for {file}, skipping");
                continue;
              }
            }
          }
        }
        catch (Exception ex)
        {
          _logger.Log(ex);
        }
      });
      if (!repository.Configuration.SkipCleanup)
      {
        foreach (var subdirectory in Directory.EnumerateDirectories(repository.Configuration.CacheDirectory))
        {
          Directory.Delete(subdirectory, true);
        }
      }
      var endTime = DateTime.Now;
      _logger.Log($"Processing complete, took {(endTime - startTime).TotalMilliseconds}ms");
    }
  }
}
