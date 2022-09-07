using ZipmodAssistant.Api.Interfaces.Services;

namespace ZipmodAssistant.Api.Interfaces.Models
{
  /// <summary>
  ///   Represents a zipmod inside of a <see cref="IBuildSetup"/>
  /// </summary>
  public interface IZipmod
  {
    /// <summary>
    ///   The file this zipmod is found at. If the zipmod is not an archive, it will be the manifest.xml
    /// </summary>
    FileInfo FileInfo { get; }

    /// <summary>
    ///   The directory this zipmod is being processed in
    /// </summary>
    string WorkingDirectory { get; }

    /// <summary>
    ///   A base64 encoded MD5 hash of the zipmod
    /// </summary>
    string Hash { get; }

    /// <summary>
    ///   The manifest for the zipmod
    /// </summary>
    IManifest Manifest { get; }
  }
}
