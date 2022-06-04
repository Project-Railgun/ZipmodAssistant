using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using ZipmodAssistant.Api.Interfaces.Models;
using ZipmodAssistant.Api.Interfaces.Services;

namespace ZipmodAssistant.Api.Services
{
  public class ManifestService : IManifestService
  {
    public Task<IZipmodConfiguration> ReadConfigurationFromManifestAsync(XmlDocument manifest)
    {
      throw new NotImplementedException();
    }

    public Task<IZipmodConfiguration> ReadConfigurationFromManifestAsync(string location)
    {
      throw new NotImplementedException();
    }

    public Task<bool> ValidateManifestAsync(XmlDocument manifest)
    {
      throw new NotImplementedException();
    }

    public Task<bool> ValidateManifestAsync(string location)
    {
      throw new NotImplementedException();
    }

    public Task<XmlDocument> WriteConfigurationToManifestAsync(IZipmodConfiguration zipmodConfiguration)
    {
      throw new NotImplementedException();
    }

    private const string manifestSchema = """
      <?xml version="1.0" encoding="utf-8">
      <xs:schema
        attributeFormDefault="unqualified"
        elementFormDefault="qualified"
        xmlns:xs="http://www.w3.org/2001/XMLSchema">
        <xs:element name="manifest">
          <xs:complexType>
            <xs:sequence>
              <xs:element type="xs:string" name="guid" />
              <xs:element type="xs:string" name="name" />
              <xs:element type="xs:string" name="version" />
              <xs:element type="xs:string" name="author" />
              <xs:element type="xs:string" name="description" />
              <xs:element type="xs:string" name="website" />
              <xs:element type="xs:string" name="game" maxOccurs="unbound" minOccurs="0" />
            </xs:sequence>
            <xs:attribute type="xs:byte" name="schema-ver" />
          </xs:complexType>
        </xs:element>
      </xs:schema>
      """;
  }
}
