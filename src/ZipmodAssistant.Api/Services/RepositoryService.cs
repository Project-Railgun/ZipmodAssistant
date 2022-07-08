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

      var repositoryItems = Directory.EnumerateFiles(configuration.InputDirectory, "*.(zipmod|zip)", SearchOption.AllDirectories);
     
      await Parallel.ForEachAsync(repositoryItems, async (filename, cancelToken) =>
      {
        var zipmod = new Zipmod(configuration, filename);
        lock (repositoryLock)
        {
          repository.Add(zipmod);
        }
      });

      return repository;
    }

    public async Task ProcessRepositoryAsync(IBuildRepository repository)
    {
      var startTime = DateTime.Now;
      _logger.Log($"Using configuration input: {repository.Configuration.InputDirectory}, output: {repository.Configuration.OutputDirectory}, cache: {repository.Configuration.CacheDirectory}");
      await Parallel.ForEachAsync(repository, async (zipmod, cancelToken) =>
      {
        try
        {
          if (
            zipmod.FileInfo.Extension.Equals(".zip", StringComparison.CurrentCultureIgnoreCase) ||
            zipmod.FileInfo.Extension.Equals(".zipmod", StringComparison.CurrentCultureIgnoreCase))
          {

          }
          await zipmod.ProcessAsync(repository.Configuration, _sessionService);
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
