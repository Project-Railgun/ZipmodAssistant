namespace ZipmodAssistant.Api.Interfaces.Models
{
  /// <summary>
  ///   Represents a zipmod's manifest file
  /// </summary>
  public interface IManifest
  {
    /// <summary>
    ///   The location of the manifest
    /// </summary>
    string FileLocation { get; set; }
    /// <summary>
    ///   The GUID of the zipmod. Cannot be empty or blank
    /// </summary>
    string Guid { get; set; }
    /// <summary>
    ///   The name of the zipmod. Optional
    /// </summary>
    string Name { get; set; }
    /// <summary>
    ///   The version of the zipmod
    /// </summary>
    /// <remarks>
    ///   Validation does not occur in this model. For that, look
    ///   at <see cref="Api.Models.RepositoryZipmod"/>
    /// </remarks>
    string Version { get; set; }
    /// <summary>
    ///   The author of the zipmod. Cannot be empty or blank
    /// </summary>
    string Author { get; set; }
    /// <summary>
    ///   The description of the zipmod. Optional
    /// </summary>
    string Description { get; set; }
    /// <summary>
    ///   The description of the zipmod. Optional
    /// </summary>
    string Website { get; set; }
    /// <summary>
    ///   An array of games this zipmod is for. If empty, there
    ///   are no restrictions
    /// </summary>
    string[] Games { get; set; }
  }
}
