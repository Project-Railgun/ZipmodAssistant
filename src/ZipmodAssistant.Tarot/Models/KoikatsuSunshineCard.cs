using MessagePack;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZipmodAssistant.Tarot.Models
{
  public class KoikatsuSunshineCard : BaseCard
  {
    [MessagePackObject]
    [ReadOnly(true)]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public struct Parameters
    {

    }

    private static readonly Version _currentVersion = new("0.0.6");

    public Parameters? CardParameters { get; private set; }
    public IDictionary<string, AdditionalData> AdditionalData { get; private set; } = new Dictionary<string, AdditionalData>();
    

    public KoikatsuSunshineCard(FileInfo fileLocation) : base(fileLocation)
    {
    }

    public override async Task LoadAsync(BinaryReader reader)
    {
      
    }
  }
}
