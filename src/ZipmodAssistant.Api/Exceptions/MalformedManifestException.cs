using ZipmodAssistant.Api.Interfaces.Models;

namespace ZipmodAssistant.Api.Exceptions
{
  public class MalformedManifestException : Exception
  {
    public MalformedManifestException(string reason) : base($"Malformed manifest.xml: {reason}")
    {

    }
  }
}
