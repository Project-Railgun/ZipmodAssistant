using MessagePack;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZipmodAssistant.Tarot.Enum;
using ZipmodAssistant.Tarot.Interfaces.Models;

namespace ZipmodAssistant.Tarot.Models
{
  public class KoikatsuCard : BaseCard
  {
    [MessagePackObject]
    [ReadOnly(true)]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public struct Parameters
    {
      [MessagePackObject]
      [TypeConverter(typeof(ExpandableObjectConverter))]
      public struct LikesAnswers
      {
        [Key("animal")]
        public bool Animals;
        [Key("eat")]
        public bool Eating;
        [Key("cook")]
        public bool Cooking;
        [Key("exercise")]
        public bool Exercising;
        [Key("study")]
        public bool Studying;
        [Key("fashionable")]
        public bool Fashion;
        [Key("blackCoffee")]
        public bool BlackCoffee;
        [Key("spicy")]
        public bool SpicyFood;
        [Key("sweet")]
        public bool SweetFood;
      }

      [MessagePackObject]
      [TypeConverter(typeof(ExpandableObjectConverter))]
      public struct SexualTastes
      {
        [Key("kiss")]
        public bool Kissing;
        [Key("aibu")]
        public bool Aibu;
        [Key("anal")]
        public bool Anal;
        [Key("massage")]
        public bool Massaging;
        [Key("notCondom")]
        public bool NoCondom;
      }

      [MessagePackObject]
      [TypeConverter (typeof(ExpandableObjectConverter))]
      public struct PersonalityTraits
      {
        [Key("hinnyo")]
        public bool Hinnyo;
        [Key("harapeko")]
        public bool Harapeko;
        [Key("donkan")]
        public bool Donkan;
        [Key("choroi")]
        public bool Choroi;
        [Key("bitch")]
        public bool Bitch;
        [Key("mutturi")]
        public bool Mutturi;
        [Key("dokusyo")]
        public bool Dokusyo;
        [Key("ongaku")]
        public bool Ongaku;
        [Key("kappatu")]
        public bool Kappatu;
        [Key("ukemi")]
        public bool Ukemi;
        [Key("friendly")]
        public bool Friendly;
        [Key("kireizuki")]
        public bool Kireizuki;
        [Key("taida")]
        public bool Taida;
        [Key("sinsyutu")]
        public bool Sinsyutu;
        [Key("hitori")]
        public bool Hitori;
        [Key("undo")]
        public bool Undo;
        [Key("majime")]
        public bool Majime;
        [Key("likeGirls")]
        public bool LikesGirls;
      }

      [Key("version")]
      public Version Version;
      [Key("sex")]
      public byte Sex;
      [Key("lastname")]
      public string LastName;
      [Key("firstname")]
      public string FirstName;
      [Key("nickname")]
      public string Nickname;
      [Key("callType")]
      public int CallType;
      [Key("personality")]
      public int Personality;
      [Key("bloodType")]
      public byte BloodType;
      [Key("birthMonth")]
      public byte BirthMonth;
      [Key("birthDay")]
      public byte BirthDay;
      [Key("clubAcitivities")]
      public byte ClubActivities;
      [Key("voiceRate")]
      public float VoiceRate;
      [Key("weakPoint")]
      public int WeakPoint;
      [Key("answer")]
      public LikesAnswers Likes;
      [Key("denial")]
      public SexualTastes Tastes;
      [Key("attribute")]
      public PersonalityTraits Traits;
      [Key("aggressive")]
      public int Aggressive;
      [Key("diligence")]
      public int Dilligence;
      [Key("kindness")]
      public int Kindness;

      public Parameters()
      {
        Version = (Version)_currentVersion.Clone();
        Sex = 0;
        LastName = string.Empty;
        FirstName = string.Empty;
        Nickname = string.Empty;
        CallType = -1;
        Personality = 0;
        BloodType = 0;
        BirthMonth = 1;
        BirthDay = 1;
        ClubActivities = 0;
        VoiceRate = 0.5f;
        WeakPoint = -1;
        Likes = new();
        Tastes = new();
        Traits = new();
        Aggressive = 0;
        Dilligence = 0;
        Kindness = 0;
      }
    }

    private static readonly Version _currentVersion = new("0.0.5");
    private const string EXTENDED_INFO_BLOCKNAME = "KKEx";

    public Parameters? CardParameters { get; private set; }
    public IDictionary<string, AdditionalData> AdditionalData { get; private set; } = new Dictionary<string, AdditionalData>();
    public override string Name =>
      CardParameters == null ? base.Name : $"{CardParameters?.FirstName} {CardParameters?.LastName}";
    public override CharacterSex Sex =>
      CardParameters == null ? CharacterSex.Unknown : CardParameters?.Sex == 0 ? CharacterSex.Male : CharacterSex.Female;

    public KoikatsuCard(FileInfo fileLocation) : base(fileLocation)
    {
    }

    public override Task LoadAsync(BinaryReader reader)
    {
      return Task.Run(() =>
      {
        // do some checks on this maybe?
        var loadVersion = new Version(reader.ReadString());
        var faceLength = reader.ReadInt32();
        if (faceLength > 0)
        {
          reader.BaseStream.Seek(faceLength, SeekOrigin.Current);
        }

        var headerLength = reader.ReadInt32();
        var headerData = reader.ReadBytes(headerLength);
        // somehow figure out a way to deserialize async without bloating memory with multiple streams
        var header = MessagePackSerializer.Deserialize<Header>(headerData);
        // no clue what this is doing but it's there
        reader.ReadInt64();
        var baseInfo = header.SearchInfo(PARAMETER_INFO_BLOCKNAME);
        if (baseInfo != null)
        {
          var headerVersion = new Version(baseInfo.Version);
          if (_currentVersion.CompareTo(headerVersion) >= 0)
          {
            reader.BaseStream.Seek(baseInfo.Position, SeekOrigin.Current);
            var parameterData = reader.ReadBytes((int)baseInfo.Size);
            CardParameters = MessagePackSerializer.Deserialize<Parameters>(parameterData);
          }
        }

        var extendedInfo = header.SearchInfo(EXTENDED_INFO_BLOCKNAME);
        if (extendedInfo != null)
        {
          reader.BaseStream.Seek(extendedInfo.Position, SeekOrigin.Current);
          var extendedData = reader.ReadBytes((int)extendedInfo.Size);
          AdditionalData = MessagePackSerializer.Deserialize<Dictionary<string, AdditionalData>>(extendedData);
        }
      });
    }
  }
}
