using SharpCompress.Common;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using ZipmodAssistant.Api.Exceptions;
using ZipmodAssistant.Api.Extensions;
using ZipmodAssistant.Api.Interfaces.Models;
using ZipmodAssistant.Shared.Enums;

namespace ZipmodAssistant.Api.Models
{
  public class Manifest : IManifest
  {
    public string FileLocation { get; set; } = string.Empty;
    public string Hash { get; set; } = string.Empty;
    public string Guid { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = "0.0.0";
    public string Author { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string[] Website { get; set; } = Array.Empty<string>();
    public string[] Games { get; set; } = Array.Empty<string>();
    public string? MaterialEditorData { get; set; }

    private List<string> _additionalData = new();

    public static Task<Manifest> ReadFromFileAsync(string filename) => ReadFromStreamAsync(File.OpenRead(filename));

    public static async Task<Manifest> ReadFromStreamAsync(Stream _stream)
    {
      // this is creating two buffers so that a hash can be made of the manifest. ZipStreams don't
      // allow seeking so we have to make due with this
      byte[] buffer;
      try
      {
        buffer = new byte[_stream.Length];
      }
      catch (NotSupportedException)
      {
        // this should be a reasonable amount, hopefully not too many are using this but it should be fine
        using var rentedBuffer = MemoryPool<byte>.Shared.Rent(1024 * 64);
        buffer = rentedBuffer.Memory.ToArray();
      }
      var bytesRead = await _stream.ReadAsync(buffer);
      var xmlStream = new MemoryStream(buffer[..bytesRead]);
      //var serializer = new XmlSerializer(typeof(Manifest));
      //var deserialized = serializer.Deserialize(stream);
      //if (deserialized is Manifest manifest)
      //{
      //  var streamReader = new StreamReader(stream);

      //  streamReader.BaseStream.Seek(0, SeekOrigin.Begin);
      //  using var md5 = MD5.Create();
      //  var streamHash = await md5.ComputeHashAsync(streamReader.BaseStream);
      //  manifest.Hash = Convert.ToBase64String(streamHash);
      //  return manifest;
      //}

      var manifest = new Manifest();
      var games = new List<string>();
      var websites = new List<string>();
      var xmlReaderSettings = new XmlReaderSettings { Async = true };
      using (var reader = XmlReader.Create(xmlStream, xmlReaderSettings))
      {
        while (await reader.ReadAsync())
        {
          switch (reader.NodeType)
          {
            case XmlNodeType.Element:
              if (string.IsNullOrWhiteSpace(reader.Name))
              {
                break;
              }
              if (reader.Name == "manifest")
              {
                // ignore any attributes for now
                await reader.MoveToContentAsync();
                break;
              }
              if (reader.Name == "manifest")
              {
                await reader.MoveToContentAsync();
              }
              else if (reader.Name == "guid")
              {
                manifest.Guid = TrimEmpty(await reader.ReadElementContentAsStringAsync());
              }
              else if (reader.Name == "name")
              {
                manifest.Name = TrimEmpty(await reader.ReadElementContentAsStringAsync());
              }
              else if (reader.Name == "version")
              {
                manifest.Version = ReadVersionFromString(await reader.ReadElementContentAsStringAsync());
              }
              else if (reader.Name == "author")
              {
                manifest.Author = TrimEmpty(await reader.ReadElementContentAsStringAsync());
              }
              else if (reader.Name == "description")
              {
                manifest.Description = TrimEmpty(await reader.ReadElementContentAsStringAsync());
              }
              else if (reader.Name == "website")
              {
                websites.Add(await reader.ReadElementContentAsStringAsync());
              }
              else if (reader.Name == "game")
              {
                games.Add(await reader.ReadElementContentAsStringAsync());
              }
              else if (reader.Name == "MaterialEditor")
              {
                manifest.MaterialEditorData = await reader.ReadOuterXmlAsync();
              }
              else
              {
                manifest._additionalData.Add(await reader.ReadOuterXmlAsync());
              }
              break;
            case XmlNodeType.EndElement:
              break;
            default:
              break;
          }
        }
      }
      manifest.Games = games.ToArray();
      manifest.Website = websites.ToArray();
      using (var hashReader = new StreamReader(xmlStream))
      {
        hashReader.BaseStream.Seek(0, SeekOrigin.Begin);
        using var md5 = MD5.Create();
        var streamHash = await md5.ComputeHashAsync(hashReader.BaseStream);
        manifest.Hash = Convert.ToBase64String(streamHash);
      }
      
      if (string.IsNullOrEmpty(manifest.Guid))
      {
        throw new MalformedManifestException("Invalid manifest");
      }

      return manifest;
    }

    public override string ToString()
    {
      var xmlWriterSettings = new XmlWriterSettings
      {
        Indent = true,
      };
      using var ms = new MemoryStream();
      using var xmlWriter = XmlWriter.Create(ms, xmlWriterSettings);
      xmlWriter.WriteComment("Generated with ZipmodAssistant");
      xmlWriter.WriteStartElement("manifest");
      xmlWriter.WriteAttributeString("schema-ver", "1");
      xmlWriter.WriteElementString("guid", Guid);
      xmlWriter.WriteElementString("name", Name);
      xmlWriter.WriteElementString("version", Version);
      xmlWriter.WriteElementString("author", Author);
      xmlWriter.WriteElementString("description", Description);
      foreach (var game in Games)
      {
        xmlWriter.WriteElementString("game", game);
      }
      foreach (var website in Website)
      {
        xmlWriter.WriteElementString("website", website);
      }
      if (MaterialEditorData != null)
      {
        xmlWriter.WriteRaw(Environment.NewLine + '\t');
        xmlWriter.WriteRaw(MaterialEditorData);
        xmlWriter.WriteRaw(Environment.NewLine);
      }
      xmlWriter.WriteRaw(Environment.NewLine + '\t');
      foreach (var entry in _additionalData)
      {
        xmlWriter.WriteRaw(entry);
      }
      xmlWriter.WriteRaw(Environment.NewLine);
      xmlWriter.WriteEndElement();
      xmlWriter.Flush();

      return Encoding.UTF8.GetString(ms.GetBuffer());
    }

    static string TrimEmpty(string input) => input.Replace("\n", "").Trim();

    static string ReadVersionFromString(string value)
    {
      var version = new int[] { 0, 0, 0 };
      var versionSegments = value.Split('.');
      for (var i = 0; i < Math.Min(versionSegments.Length, 3); i++)
      {
        var versionSegment = versionSegments[i];
        if (int.TryParse(versionSegment.StartsWith('v') ? versionSegment[1..] : versionSegment, out var segint))
        {
          version[i] = segint;
        }
        else
        {
          version[i] = 0;
        }
      }

      return string.Join(".", version);
    }
  }
}
