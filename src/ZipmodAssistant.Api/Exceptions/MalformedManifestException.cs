using ZipmodAssistant.Api.Interfaces.Models;

namespace ZipmodAssistant.Api.Exceptions
{
  public class MalformedManifestException : Exception
  {
    public IManifest Manifest { get; set; }

    public MalformedManifestException(IManifest manifest, string paramName) : base($"Malformed manifest.xml: {paramName}")
    {
      Manifest = manifest;
    }
  }
}
