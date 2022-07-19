using ZipmodAssistant.Api.Enums;

namespace ZipmodAssistant.Api.Interfaces.Models
{
  /// <summary>
  ///   The base that all results inherit from. This is solely for logging
  ///   purposes
  /// </summary>
  public interface ISessionResult
  {
    /// <summary>
    ///   The <see cref="IZipmod"/> this result originated from
    /// </summary>
    IZipmod Target { get; }

    /// <summary>
    ///   The file within the <see cref="IZipmod"/> related to the result
    /// </summary>
    string Filename { get; }

    /// <summary>
    ///   The result type
    /// </summary>
    SessionResultType Type { get; }
  }
}
