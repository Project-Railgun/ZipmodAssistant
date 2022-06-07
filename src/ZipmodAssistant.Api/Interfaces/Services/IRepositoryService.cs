using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZipmodAssistant.Api.Interfaces.Models;

namespace ZipmodAssistant.Api.Interfaces.Services
{
  public interface IRepositoryService
  {
    Task<IBuildRepository> GetRepositoryFromDirectoryAsync(string rootDirectory);
    Task ProcessRepositoryAsync(IBuildConfiguration buildAction, IBuildRepository repository);
  }
}
