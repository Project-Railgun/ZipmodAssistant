using ZipmodAssistant.Shared.Enums;

namespace ZipmodAssistant.Api.Interfaces.Models
{
  /// <summary>
  ///   Declares a build configuration for a single execution
  /// </summary>
  public interface IBuildConfiguration
  {
    /// <summary>
    ///   The directory to scan for files
    /// </summary>
    string InputDirectory { get; set; }
    /// <summary>
    ///   The directory where results are copied to
    /// </summary>
    string OutputDirectory { get; set; }
    /// <summary>
    ///   The directory where resources are temporarily copied to
    /// </summary>
    string CacheDirectory { get; set; }
    /// <summary>
    ///   Whether or not to randomize unity3d file CABs
    /// </summary>
    bool RandomizeCab { get; set; }
    /// <summary>
    ///   Whether or not to skip file renaming
    /// </summary>
    bool SkipRenaming { get; set; }
    /// <summary>
    ///   Whether or not to compress resources files (PNGs, unity3ds, etc)
    /// </summary>
    bool SkipCompression { get; set; }
    /// <summary>
    ///   Whether or not to clear the cache directory on complete
    /// </summary>
    bool SkipCleanup { get; set; }
    /// <summary>
    ///   Whether or not to skip known mods that exist in the output directory/db
    /// </summary>
    bool SkipKnownMods { get; set; }
    IEnumerable<TargetGame> Games { get; set; }
  }
}
