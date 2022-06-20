using ZipmodAssistant.Api.Interfaces.Models;

namespace ZipmodAssistant.Api.Interfaces.Services
{
  /// <summary>
  ///   Provides functionality around processing a resource repository
  /// </summary>
  public interface IRepositoryService
  {
    /// <summary>
    ///   Builds a <see cref="IBuildRepository"/> from the input directory in <paramref name="configuration"/>
    /// </summary>
    /// <param name="configuration"></param>
    /// <returns></returns>
    Task<IBuildRepository> GetRepositoryFromDirectoryAsync(IBuildConfiguration configuration);
    /// <summary>
    ///   Iterates through each item within <paramref name="repository"/> and executes the owning action
    /// </summary>
    /// <param name="repository"></param>
    /// <returns></returns>
    Task ProcessRepositoryAsync(IBuildRepository repository);
  }
}
