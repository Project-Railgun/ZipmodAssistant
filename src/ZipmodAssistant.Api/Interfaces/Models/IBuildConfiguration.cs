namespace ZipmodAssistant.Api.Interfaces.Models
{
  public interface IBuildConfiguration
  {
    string InputDirectory { get; }
    string OutputDirectory { get; }
    string CacheDirectory { get; }
    bool RandomizeCab { get; }
    bool SkipRenaming { get; }
    bool SkipCompression { get; }
    bool SkipCleanup { get; }
  }
}
