using ZipmodAssistant.Tarot.Enum;

namespace ZipmodAssistant.Tarot.Interfaces.Models
{
  public interface ICard
  {
    string Name { get; }
    FileInfo FileLocation { get; }
    CharacterSex Sex { get; }
    string Personality { get; }

    Task LoadAsync();
  }
}
