namespace ZipmodAssistant.Api.Interfaces.Models
{
  public interface IManifest
  {
    string FileLocation { get; set; }
    string Guid { get; set; }
    string Name { get; set; }
    string Version { get; set; }
    string Author { get; set; }
    string Description { get; set; }
    string Website { get; set; }
    string[] Games { get; set; }
  }
}
