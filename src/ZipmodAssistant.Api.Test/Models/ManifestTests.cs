using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZipmodAssistant.Api.Models;

namespace ZipmodAssistant.Api.Test.Models
{
  public class ManifestTests
  {
    private const string _validManifestXml = """
      <?xml version="1.0" encoding="utf-8"?>
      <manifest schema-ver="1">
        <guid>test-zipmod</guid>
        <name>test</name>
        <version>0.0.1</version>
        <author>ItsSpyce</author>
        <description>Test manifest XML</description>
        <website>https://github.com/ItsSpyce</website>
        <game>kk</game>
        <game>KoikatsuSunshine</game>
      </manifest>
      """;

    [SetUp]
    public void Setup()
    {

    }

    [Test]
    public async Task ReadFromStream_Success()
    {
      using var manifestStream = new MemoryStream(Encoding.UTF8.GetBytes(_validManifestXml));
      var manifest = await Manifest.ReadFromStreamAsync(manifestStream);
      Assert.Multiple(() =>
      {
        Assert.That(manifest.Guid, Is.EqualTo("test-zipmod"));
        Assert.That(manifest.Name, Is.EqualTo("test"));
        Assert.That(manifest.Version, Is.EqualTo("0.0.1"));
        Assert.That(manifest.Games, Has.Length.EqualTo(2));
      });
    }

    [Test]
    public async Task ManifestToString_Success()
    {
      using var manifestStream = new MemoryStream(Encoding.UTF8.GetBytes(_validManifestXml));
      var manifest = await Manifest.ReadFromStreamAsync(manifestStream);
      Assert.That(manifest.ToString(), Is.EqualTo(_validManifestXml));
    }
  }
}
