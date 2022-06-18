using ZipmodAssistant.Api.Interfaces.Models;

namespace ZipmodAssistant.Api.Interfaces.Services
{
  public interface ISessionService
  {
    Task CommitResultAsync(IProcessResult result);
    Task CommitResultAsync(IEnumerable<IProcessResult> results);
    Task<string> GenerateReportAsync();
    Task<string> GenerateReportAsync(bool clearHistory);
  }
}
