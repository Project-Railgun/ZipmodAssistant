using ZipmodAssistant.Tarot.Enum;
using ZipmodAssistant.Tarot.Interfaces.Models;

namespace ZipmodAssistant.Tarot.Models
{
  public abstract class BaseCard : ICard
  {
    public string Name { get; protected set; }
    public FileInfo FileLocation { get; protected set; }
    public CharacterSex Sex { get; protected set; } = CharacterSex.Unknown;
    public string Personality { get; protected set; } = string.Empty;

    protected BaseCard(FileInfo fileLocation)
    {
      FileLocation = fileLocation;
      Name = fileLocation.Name[..(fileLocation.Name.LastIndexOf('.') - 1)];
    }

    public abstract Task LoadAsync();
  }
}
