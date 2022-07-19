using MessagePack;
using ZipmodAssistant.Tarot.Enum;
using ZipmodAssistant.Tarot.Interfaces.Models;

namespace ZipmodAssistant.Tarot.Models
{
  public abstract class BaseCard : ICard
  {
    [MessagePackObject]
    public class Header
    {
      [Key("lstInfo")]
      public List<Info> Infos { get; set; } = new();

      public Info? SearchInfo(string name) => Infos.Find(info => info.Name == name);

      [MessagePackObject]
      public record Info(
        [property: Key("name")] string Name,
        [property: Key("version")] string Version,
        [property: Key("pos")] long Position,
        [property: Key("size")] long Size);
    }

    protected const string PARAMETER_INFO_BLOCKNAME = "Parameter";

    public virtual string Name { get; protected set; }
    public FileInfo FileLocation { get; protected set; }
    public virtual CharacterSex Sex { get; protected set; } = CharacterSex.Unknown;
    public virtual string Personality { get; protected set; } = string.Empty;

    protected virtual bool IsDisposed { get; set; }

    protected BaseCard(FileInfo fileLocation)
    {
      FileLocation = fileLocation;
      Name = fileLocation.Name[..(fileLocation.Name.LastIndexOf('.') - 1)];
    }

    public abstract Task LoadAsync(BinaryReader reader);

    public virtual void Dispose()
    {
      GC.SuppressFinalize(this);
    }
  }
}
