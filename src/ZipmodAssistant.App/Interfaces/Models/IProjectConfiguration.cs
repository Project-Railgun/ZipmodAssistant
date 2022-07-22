using System.Threading.Tasks;
using ZipmodAssistant.Api.Interfaces.Models;

namespace ZipmodAssistant.App.Interfaces.Models
{
  public interface IProjectConfiguration : IBuildConfiguration
  {
    string? Filename { get; set; }
    bool IsPersisted { get; }

    Task SaveAsync();
  }
}
