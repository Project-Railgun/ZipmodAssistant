namespace ZipmodAssistant.Api.Interfaces.Models
{
  /// <summary>
  ///   Contains a collection of <see cref="IRepositoryItem"/> with a set configuration
  /// </summary>
  public interface IBuildRepository : IList<IRepositoryItem>
  {
    /// <summary>
    ///   The build configuration to use during processing
    /// </summary>
    IBuildConfiguration Configuration { get; }
  }
}
