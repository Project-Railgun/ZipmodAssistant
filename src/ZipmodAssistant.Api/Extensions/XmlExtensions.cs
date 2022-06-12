using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;

namespace ZipmodAssistant.Api.Extensions
{
  public static class XmlExtensions
  {
    public static void Add(this XmlSchemaSet schemaSet, string schema)
    {
      using var stream = new MemoryStream(Encoding.UTF8.GetBytes(schema));
      var xmlSchema = XmlSchema.Read(stream, null);
      if (xmlSchema == null)
      {
        throw new ArgumentException("Invalid schema", nameof(schema));
      }
      schemaSet.Add(xmlSchema);
    }
  }
}
