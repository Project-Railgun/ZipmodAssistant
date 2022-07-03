using ZipmodAssistant.Api.Data;
using ZipmodAssistant.Api.Interfaces.Models;

namespace ZipmodAssistant.Api.Models
{
  public class BuildRepository : List<IRepositoryItem>, IBuildRepository
  {
    public IBuildConfiguration Configuration { get; set; }

    private readonly ZipmodDbContext _dbContext;

    public BuildRepository(IBuildConfiguration configuration, ZipmodDbContext dbContext)
    {
      Configuration = configuration;
      _dbContext = dbContext;
    }
  }
}
