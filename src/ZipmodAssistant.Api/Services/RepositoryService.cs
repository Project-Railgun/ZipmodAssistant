using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZipmodAssistant.Api.Enums;
using ZipmodAssistant.Api.Exceptions;
using ZipmodAssistant.Api.Interfaces.Models;
using ZipmodAssistant.Api.Interfaces.Services;
using ZipmodAssistant.Api.Models;

namespace ZipmodAssistant.Api.Services
{
  public class RepositoryService : IRepositoryService
  {
    private readonly ILoggerService _loggerService;
    private readonly IManifestService _manifestService;

    public RepositoryService(ILoggerService logger, IManifestService manifestService)
    {
      _loggerService = logger;
      _manifestService = manifestService;
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
      var repository = new BuildRepository(rootDirectory, configuration);

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
          _loggerService.Log($"Unknown item found at {item.FileLocation}, skipping");
          continue;
        }
        var processResult = await item.ProcessAsync(buildConfiguration, repository);
        if (processResult)
        {
          _loggerService.Log($"Completed processing {item.FileLocation}");
        }
        else
        {
          _loggerService.Log($"Failed to process {item.FileLocation}");
        }
      }
      var endTime = DateTime.Now;
      _loggerService.Log($"Processing complete, took {(endTime - startTime):Y}");
    }
  }
}
