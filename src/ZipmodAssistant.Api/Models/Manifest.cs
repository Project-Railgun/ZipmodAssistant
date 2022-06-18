using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using ZipmodAssistant.Api.Extensions;
using ZipmodAssistant.Api.Interfaces.Models;

namespace ZipmodAssistant.Api.Models
{
  [XmlRoot("manifest")]
  public class Manifest : IManifest
  {
    public string FileLocation { get; set; } = string.Empty;
    public string Hash { get; private set; }
    [XmlAttribute("schema-ver")]
    public string SchemaVersion { get; set; } = string.Empty;
    [XmlElement("guid", IsNullable = false)]
    public string Guid { get; set; } = string.Empty;
    [XmlElement("name")]
    public string Name { get; set; } = string.Empty;
    [XmlElement("version")]
    public string Version { get; set; } = string.Empty;
    [XmlElement("author")]
    public string Author { get; set; } = string.Empty;
    [XmlElement("description")]
    public string Description { get; set; } = string.Empty;
    [XmlElement("website")]
    public string Website { get; set; } = string.Empty;
    [XmlElement("game")]
    public string[] Games { get; set; } = Array.Empty<string>();

    private string _rawContent = string.Empty;

    public static async Task<Manifest> ReadFromStreamAsync(Stream stream)
    {
      var serializer = new XmlSerializer(typeof(Manifest));
      var deserialized = serializer.Deserialize(stream);
      if (deserialized is Manifest manifest)
      {
        var streamReader = new StreamReader(stream);
        streamReader.BaseStream.Seek(0, SeekOrigin.Begin);
        manifest._rawContent = await streamReader.ReadToEndAsync();
        streamReader.BaseStream.Seek(0, SeekOrigin.Begin);
        using var md5 = MD5.Create();
        var streamHash = await md5.ComputeHashAsync(streamReader.BaseStream);
        manifest.Hash = Convert.ToBase64String(streamHash);
        return manifest;
      }
      throw new ArgumentException("Invalid manifest XML", nameof(stream));
    }

    public override string ToString()
    {
      if (!string.IsNullOrEmpty(_rawContent))
      {
        return _rawContent;
      }
      var serializer = new XmlSerializer(typeof(Manifest));
      using var ms = new MemoryStream();
      serializer.Serialize(ms, this);
      return Encoding.UTF8.GetString(ms.GetBuffer());
    }
  }
}
