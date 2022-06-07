using System.Xml;
using ZipmodAssistant.Api.Interfaces.Models;

namespace ZipmodAssistant.Api.Interfaces.Services
{
  public interface IManifestService
  {
    Task<XmlDocument> WriteConfigurationToManifestAsync(IZipmodConfiguration zipmodConfiguration);
    Task<IZipmodConfiguration> ReadConfigurationFromManifestAsync(XmlDocument manifest);
    Task<IZipmodConfiguration> ReadConfigurationFromManifestAsync(string location);

    Task<bool> ValidateManifestAsync(XmlDocument manifest);
    Task<bool> ValidateManifestAsync(string location);

    Task<byte[]> GetManifestHashAsync();
  }
}
