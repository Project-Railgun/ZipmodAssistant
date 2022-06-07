using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZipmodAssistant.Api.Interfaces.Services
{
  public interface IBuildService
  {
    Task RandomizeCabAsync();
    Task RenameFilesAsync();
    Task CompressFilesAsync();
    Task CleanupRepositoryAsync();
  }
}
