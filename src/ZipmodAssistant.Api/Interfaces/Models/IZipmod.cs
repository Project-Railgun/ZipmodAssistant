using ZipmodAssistant.Api.Interfaces.Services;

namespace ZipmodAssistant.Api.Interfaces.Models
{
  /// <summary>
  ///   Represents a zipmod inside of a <see cref="IBuildRepository"/>
  /// </summary>
  public interface IZipmod
  {
    /// <summary>
    ///   The file this zipmod is found at
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

    /// <summary>
    ///   Processes and validates the file
    /// </summary>
    /// <param name="buildConfig">The build configuration to use - typically owned by the <see cref="IBuildRepository"/></param>
    /// <returns></returns>
    Task ProcessAsync(IBuildConfiguration buildConfig, ISessionService session);
  }
}
