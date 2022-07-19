using HandlebarsDotNet;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using ZipmodAssistant.Api.Data;
using ZipmodAssistant.Api.Data.DataModels;
using ZipmodAssistant.Api.Interfaces.Models;
using ZipmodAssistant.Api.Interfaces.Services;

namespace ZipmodAssistant.Api.Services
{
  public class SessionService : ISessionService
  {
    private readonly IServiceProvider _serviceProvider;
    private readonly object _accessLock = new();
    private string? _sessionId;

    public SessionService(IServiceProvider serviceProvider)
    {
      _serviceProvider = serviceProvider;
    }

    static string GetNewSessionId()
    {
      var buffer = new byte[8];
      Random.Shared.NextBytes(buffer);
      return Convert.ToBase64String(buffer);
    }

    public async Task CommitResultAsync(ISessionResult result)
    {
      if (_sessionId == default)
      {
        _sessionId = GetNewSessionId();
      }
      var dbContext = _serviceProvider.GetService<ZipmodDbContext>();
      var entry = new SessionResultEntry
      {
        Filename = result.Filename,
        ZipmodHash = result.Target.Hash,
        Type = result.Type,
        SessionId = _sessionId,
        CreatedAt = DateTime.Now,
      };
      lock (_accessLock)
      {
        dbContext.SessionResultEntries.Add(entry);
        dbContext.SaveChanges();
      }
    }

    public async Task CommitResultAsync(IEnumerable<ISessionResult> results)
    {
      if (_sessionId == default)
      {
        _sessionId = GetNewSessionId();
      }
      var now = DateTime.Now;
      var dbContext = _serviceProvider.GetService<ZipmodDbContext>();
      var entries = results.Select(r => new SessionResultEntry
      {
        Filename = r.Filename,
        ZipmodHash = r.Target.Hash,
        Type = r.Type,
        SessionId = _sessionId,
        CreatedAt = now,
      });
      lock (_accessLock)
      {
        dbContext.SessionResultEntries.AddRange(entries);
        dbContext.SaveChanges();
      }
    }

    public Task<string> GenerateReportAsync() => GenerateReportAsync(true);

    public Task<string> GenerateReportAsync(bool clearHistory)
    {
      
      if (!File.Exists("report.hjs"))
      {
        throw new FileNotFoundException("report.hjs");
      }
      var templateContents = File.ReadAllText("report.hjs");
      var template = Handlebars.Compile(templateContents);

      var dbContext = _serviceProvider.GetService<ZipmodDbContext>();
      var results = dbContext.SessionResultEntries
        .Where(e => e.SessionId == _sessionId)
        .OrderBy(e => e.ZipmodHash)
        .ToList();

      var args = new
      {
        CurrentDate = DateTime.Now.ToShortDateString(),
        SessionId = _sessionId,
        Entries = results,
      };
      _sessionId = default;
      return Task.FromResult(template(args));
    }
  }
}
