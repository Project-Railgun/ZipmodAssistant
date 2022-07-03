using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZipmodAssistant.Api.Data;
using ZipmodAssistant.Api.Enums;
using ZipmodAssistant.Api.Exceptions;
using ZipmodAssistant.Api.Interfaces.Models;
using ZipmodAssistant.Api.Interfaces.Services;
using ZipmodAssistant.Api.Models;
using ZipmodAssistant.Tarot.Interfaces.Providers;

namespace ZipmodAssistant.Api.Services
{
  public class RepositoryService : IRepositoryService
  {
    private readonly ILoggerService _loggerService;
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
      _loggerService = logger;
      _outputService = outputService;
      _sessionService = sessionService;
      _cardProvider = cardProvider;
      _dbContext = dbContext;
    }

    public async Task<IBuildRepository> GetRepositoryFromDirectoryAsync(IBuildConfiguration configuration)
    {
      var repository = new BuildRepository(configuration, _dbContext);
      var repositoryLock = new object();

      var repositoryItems = Directory.EnumerateFiles(configuration.InputDirectory, "*.*", SearchOption.AllDirectories);
        //.Select(filename =>
        //{
        //  var fileInfo = new FileInfo(filename);
        //  IRepositoryItem item = fileInfo.Extension switch
        //  {
        //    ".png" => new RepositoryImage(fileInfo, _cardProvider),
        //    ".zipmod" => new RepositoryZipmod(fileInfo, _dbContext),
        //    ".zip" => new RepositoryZipmod(fileInfo, _dbContext),
        //    ".unity3d" => new RepositoryUnityResx(fileInfo),
        //    _ => throw new Exception($"Invalid extension {fileInfo.Extension}"),
        //  };
        //  return item;
        //});
      await Parallel.ForEachAsync(repositoryItems, async (filename, cancelToken) =>
      {
        await Task.Run(() =>
        {
          var fileInfo = new FileInfo(filename);
          IRepositoryItem? item = fileInfo.Extension switch
          {
            ".png" => new RepositoryImage(fileInfo, _cardProvider),
            ".zipmod" => new RepositoryZipmod(fileInfo, _dbContext),
            ".zip" => new RepositoryZipmod(fileInfo, _dbContext),
            ".unity3d" => new RepositoryUnityResx(fileInfo),
            _ => null,
          };
          lock (repositoryLock)
          {
            if (item != null)
            {
              repository.Add(item);
            }
          }
        }, cancelToken);
      });

      return repository;
    }

    public async Task ProcessRepositoryAsync(IBuildRepository repository)
    {
      var startTime = DateTime.Now;
      _loggerService.Log($"Using configuration input: {repository.Configuration.InputDirectory}, output: {repository.Configuration.OutputDirectory}, cache: {repository.Configuration.CacheDirectory}");
      foreach (var item in repository)
      {
        if (item.ItemType == RepositoryItemType.Unknown)
        {
          _loggerService.Log($"Unknown item found at {item.FileInfo.FullName}, skipping");
          continue;
        }
        try
        {
          var processResult = await item.ProcessAsync(repository.Configuration, _outputService);
          await _sessionService.CommitResultAsync(processResult);
        }
        catch (MalformedManifestException ex)
        {
          _loggerService.Log(ex);
        }
      }
      var endTime = DateTime.Now;
      _loggerService.Log($"Processing complete, took {(endTime - startTime).TotalMilliseconds}ms");
    }
  }
}
