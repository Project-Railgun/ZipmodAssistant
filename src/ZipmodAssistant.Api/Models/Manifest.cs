using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using ZipmodAssistant.Api.Exceptions;
using ZipmodAssistant.Api.Extensions;
using ZipmodAssistant.Api.Interfaces.Models;

namespace ZipmodAssistant.Api.Models
{
  [XmlRoot("manifest")]
  public class Manifest : IManifest
  {
    private string _version = string.Empty;

    [XmlIgnore]
    public string FileLocation { get; set; } = string.Empty;
    [XmlIgnore]
    public string Hash { get; set; } = string.Empty;
    [XmlAttribute("schema-ver")]
    public string SchemaVersion { get; set; } = string.Empty;
    [XmlElement("guid", IsNullable = false)]
    public string Guid { get; set; } = string.Empty;
    [XmlElement("name")]
    public string Name { get; set; } = string.Empty;
    [XmlElement("version")]
    public string Version
    {
      get => _version;
      set
      {
        var version = new int[] { 0, 0, 0 };
        var versionSegments = value.Split('.');
        for (var i = 0; i < versionSegments.Length; i++)
        {
          var versionSegment = versionSegments[i];
          if (int.TryParse(versionSegment.StartsWith('v') ? versionSegment[1..] : versionSegment, out var segint))
          {
            version[i] = segint;
          }
          else
          {
            throw new FormatException($"Manifest has invalid version: {value}");
          }
        }
        _version = string.Join(".", version);
      }
    }
    [XmlElement("author")]
    public string Author { get; set; } = string.Empty;
    [XmlElement("description")]
    public string Description { get; set; } = string.Empty;
    [XmlElement("website")]
    public string Website { get; set; } = string.Empty;
    [XmlElement("game")]
    public string[] Games { get; set; } = Array.Empty<string>();

    public static Task<Manifest> ReadFromFileAsync(string filename) => ReadFromStreamAsync(File.OpenRead(filename));

    public static async Task<Manifest> ReadFromStreamAsync(Stream _stream)
    {
      var buffer = new byte[_stream.Length];
      await _stream.ReadAsync(buffer);
      var stream = new MemoryStream(buffer);
      var serializer = new XmlSerializer(typeof(Manifest));
      var deserialized = serializer.Deserialize(stream);
      if (deserialized is Manifest manifest)
      {
        var streamReader = new StreamReader(stream);
        
        streamReader.BaseStream.Seek(0, SeekOrigin.Begin);
        using var md5 = MD5.Create();
        var streamHash = await md5.ComputeHashAsync(streamReader.BaseStream);
        manifest.Hash = Convert.ToBase64String(streamHash);
        return manifest;
      }
      throw new MalformedManifestException("Invalid manifest");
    }

    public override string ToString()
    {
      var xmlWriterSettings = new XmlWriterSettings
      {
        Indent = true,
      };
      var serializer = new XmlSerializer(typeof(Manifest));

      var ms = new MemoryStream();
      using var xmlWriter = XmlWriter.Create(ms, xmlWriterSettings);
      serializer.Serialize(xmlWriter, this);
      return Encoding.UTF8.GetString(ms.GetBuffer());
    }
  }
}
