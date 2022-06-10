using MessagePack;
using System.ComponentModel;
using ZipmodAssistant.Tarot.Interfaces.Models;
using ZipmodAssistant.Tarot.Utilities;

namespace ZipmodAssistant.Tarot.Models
{
  [MessagePackObject]
  public class AdditionalData : IAdditionalData
  {
    [IgnoreMember]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public IList<string> RequiredPluginGuids => throw new NotImplementedException();
    [IgnoreMember]
    [TypeConverter(typeof(ListTypeConverter))]
    public IList<string> RequiredZipmodNames => throw new NotImplementedException();

    [Key(0)]
    public int Version { get; set; }
    [Key(1)]
    [TypeConverter(typeof(DictionaryTypeConverter<string, object>))]
    public IDictionary<string, object> Data { get; set; } = new Dictionary<string, object>();

    public override string ToString() => $"Version={Version}; Entries={Data.Count}";
  }
}
