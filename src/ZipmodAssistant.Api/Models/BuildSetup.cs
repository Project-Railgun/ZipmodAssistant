using ZipmodAssistant.Api.Data;
using ZipmodAssistant.Api.Interfaces.Models;

namespace ZipmodAssistant.Api.Models
{
  public class BuildSetup : List<IZipmod>, IBuildSetup
  {
    public IBuildConfiguration Configuration { get; set; }

    public BuildSetup(IBuildConfiguration configuration)
    {
      Configuration = configuration;
    }
  }
}
