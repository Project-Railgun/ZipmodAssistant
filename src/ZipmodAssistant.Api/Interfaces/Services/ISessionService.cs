using ZipmodAssistant.Api.Interfaces.Models;

namespace ZipmodAssistant.Api.Interfaces.Services
{
  /// <summary>
  ///   Used for storing the results of a repository build session
  /// </summary>
  public interface ISessionService
  {
    /// <summary>
    ///   Stores the result in local data to build a report from
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    Task CommitResultAsync(ISessionResult result);
    /// <summary>
    ///   Stores a collection of results in local data to build a report from
    /// </summary>
    /// <param name="results"></param>
    /// <returns></returns>
    Task CommitResultAsync(IEnumerable<ISessionResult> results);
    /// <summary>
    ///   Generates the session report as an HTML file, clearing local data after complete
    /// </summary>
    /// <returns></returns>
    Task<string> GenerateReportAsync();
    /// <summary>
    ///   Generates the session report as an HTML file
    /// </summary>
    /// <param name="clearHistory"></param>
    /// <returns></returns>
    Task<string> GenerateReportAsync(bool clearHistory);
  }
}
