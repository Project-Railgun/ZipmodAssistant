using ZipmodAssistant.Api.Interfaces.Models;

namespace ZipmodAssistant.Api.Interfaces.Services
{
  /// <summary>
  ///   Provides functionality around processing a resource repository
  /// </summary>
  public interface IBuildService
  {
    /// <summary>
    ///   Builds a <see cref="IBuildSetup"/> from the input directory in <paramref name="configuration"/>
    /// </summary>
    /// <param name="configuration"></param>
    /// <returns></returns>
    Task<IBuildSetup> GetRepositoryFromDirectoryAsync(IBuildConfiguration configuration);
    /// <summary>
    ///   Iterates through each item within <paramref name="repository"/> and executes the owning action
    /// </summary>
    /// <param name="repository"></param>
    /// <returns></returns>
    Task ProcessRepositoryAsync(IBuildSetup repository);
  }
}
