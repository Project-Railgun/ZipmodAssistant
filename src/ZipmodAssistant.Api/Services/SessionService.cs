using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZipmodAssistant.Api.Data;
using ZipmodAssistant.Api.Data.DataModels;
using ZipmodAssistant.Api.Interfaces.Models;
using ZipmodAssistant.Api.Interfaces.Services;

namespace ZipmodAssistant.Api.Services
{
  public class SessionService : ISessionService
  {
    private readonly ZipmodDbContext _dbContext;

    public SessionService(ZipmodDbContext dbContext)
    {
      _dbContext = dbContext;
    }

    public async Task CommitResultAsync(IProcessResult result)
    {
      var entry = new SessionResultEntry
      {
        Filename = result.Target.FileInfo.FullName,
      };
      await _dbContext.SessionResultEntries.AddAsync(entry);
    }

    public async Task CommitResultAsync(IEnumerable<IProcessResult> results)
    {
      var entries = results.Select(r => new SessionResultEntry
      {
        Filename = r.Target.FileInfo.FullName,
      });
      await _dbContext.SessionResultEntries.AddRangeAsync(entries);
    }

    public async Task<string> GenerateReportAsync()
    {
      throw new NotImplementedException();
    }

    public async Task<string> GenerateReportAsync(bool clearHistory)
    {
      throw new NotImplementedException();
    }
  }
}
