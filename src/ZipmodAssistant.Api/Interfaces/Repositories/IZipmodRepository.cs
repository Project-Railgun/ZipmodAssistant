using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZipmodAssistant.Api.Data.DataModels;
using ZipmodAssistant.Api.Interfaces.Models;

namespace ZipmodAssistant.Api.Interfaces.Repositories
{
  public interface IZipmodRepository
  {
    Task<bool> IsManifestInHistoryAsync(IManifest manifest);

    Task<bool> IsNewerVersionAvailableAsync(IZipmod zipmod);

    Task<bool> SetCanSkipZipmodAsync(string guid, bool canSkip);

    Task<bool> AddZipmodAsync(IZipmod zipmod);

    Task<bool> RemoveZipmodAsync(IZipmod zipmod);

    Task<bool> UpdateZipmodAsync(IZipmod zipmod);

    Task<IEnumerable<PriorZipmodEntry>> GetZipmodsAsync();
  }
}
