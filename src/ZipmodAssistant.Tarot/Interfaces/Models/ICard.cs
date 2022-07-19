using ZipmodAssistant.Tarot.Enum;

namespace ZipmodAssistant.Tarot.Interfaces.Models
{
  public interface ICard : IDisposable
  {
    string Name { get; }
    FileInfo FileLocation { get; }
    CharacterSex Sex { get; }
    string Personality { get; }

    Task LoadAsync(BinaryReader reader);
  }
}
