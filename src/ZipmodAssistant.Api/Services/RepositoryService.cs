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
    private readonly IManifestService _manifestService;
    private readonly ICardProvider _cardProvider;
    private readonly ZipmodDbContext _dbContext;

    public RepositoryService(
      ILoggerService logger,
      IManifestService manifestService,
      ICardProvider cardProvider,
      ZipmodDbContext dbContext)
    {
      _loggerService = logger;
      _manifestService = manifestService;
      _cardProvider = cardProvider;
      _dbContext = dbContext;
    }

    public async Task<IBuildRepository> GetRepositoryFromDirectoryAsync(string rootDirectory)
    {
      IZipmodConfiguration configuration;
      var manifestLocation = Path.Combine(rootDirectory, "manifest.xml");
      if (File.Exists(manifestLocation))
      {
        configuration = await _manifestService.ReadConfigurationFromManifestAsync(manifestLocation);
      }
      else
      {
        throw new InvalidRepositoryException(rootDirectory);
      }
      var repository = new BuildRepository(rootDirectory, configuration, _dbContext);

      var repositoryItems = Directory.EnumerateFiles(rootDirectory, "*.(png|zip|zipmod)", SearchOption.AllDirectories)
        .Select(filename =>
        {
          var fileInfo = new FileInfo(filename);
          IRepositoryItem item = fileInfo.Extension switch
          {
            ".png" => new RepositoryImage(fileInfo, _cardProvider),
            ".zipmod" => new BuildRepository(fileInfo, _dbContext),
            ".zip" => new BuildRepository(fileInfo, _dbContext),
            _ => throw new Exception($"Invalid extension {fileInfo.Extension}"),
          };
          return item;
        });
      

      return repository;
    }

    public async Task ProcessRepositoryAsync(IBuildConfiguration buildConfiguration, IBuildRepository repository)
    {
      var startTime = DateTime.Now;
      _loggerService.Log($"Processing repository at {repository.RootDirectory} containing {repository.Count()} items");
      _loggerService.Log($"Using configuration input: {buildConfiguration.InputDirectory}, output: {buildConfiguration.OutputDirectory}, cache: {buildConfiguration.CacheDirectory}");
      foreach (var item in repository)
      {
        if (item.ItemType == RepositoryItemType.Unknown)
        {
          _loggerService.Log($"Unknown item found at {item.FileInfo.FullName}, skipping");
          continue;
        }
        var processResult = await item.ProcessAsync(buildConfiguration, repository);
        if (processResult)
        {
          _loggerService.Log($"Completed processing {item.FileInfo.FullName}");
        }
        else
        {
          _loggerService.Log($"Failed to process {item.FileInfo.FullName}");
        }
      }
      var endTime = DateTime.Now;
      _loggerService.Log($"Processing complete, took {(endTime - startTime):Y}");
    }
  }
}
