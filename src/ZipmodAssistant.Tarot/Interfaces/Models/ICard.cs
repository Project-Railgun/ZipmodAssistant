using ZipmodAssistant.Tarot.Enum;

namespace ZipmodAssistant.Tarot.Interfaces.Models
{
  public interface ICard
  {
    string Name { get; }
    FileInfo FileLocation { get; }
    CharacterSex Sex { get; }
    string Personality { get; }
    Stream ImageStream { get; }
    Stream DataStream { get; }

    Task LoadAsync(BinaryReader reader);
  }
}
