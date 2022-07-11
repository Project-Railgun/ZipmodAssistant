using ZipmodAssistant.Api.Data;
using ZipmodAssistant.Api.Interfaces.Models;

namespace ZipmodAssistant.Api.Models
{
  public class BuildRepository : List<IZipmod>, IBuildRepository
  {
    public IBuildConfiguration Configuration { get; set; }

    public BuildRepository(IBuildConfiguration configuration)
    {
      Configuration = configuration;
    }
  }
}
