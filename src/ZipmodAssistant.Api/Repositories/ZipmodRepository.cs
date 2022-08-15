using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZipmodAssistant.Api.Data;
using ZipmodAssistant.Api.Data.DataModels;
using ZipmodAssistant.Api.Interfaces.Models;
using ZipmodAssistant.Api.Interfaces.Repositories;

namespace ZipmodAssistant.Api.Repositories
{
  public class ZipmodRepository : IZipmodRepository
  {
    private readonly IServiceProvider _serviceProvider;

    public ZipmodRepository(IServiceProvider serviceProvider)
    {
      _serviceProvider = serviceProvider;
    }

    public async Task<bool> AddZipmodAsync(IZipmod zipmod)
    {
      var _dbContext = _serviceProvider.GetService<ZipmodDbContext>();
      if (zipmod.Manifest == null || _dbContext.PriorZipmodEntries.Find(zipmod.Manifest.Guid) != null)
      {
        return false;
      }
      var entry = await _dbContext.PriorZipmodEntries.AddAsync(GetPriorZipmodEntryFromZipmod(zipmod));
      await _dbContext.SaveChangesAsync();
      return true;
    }

    public async Task<bool> IsManifestInHistoryAsync(IManifest manifest!!)
    {
      var _dbContext = _serviceProvider.GetService<ZipmodDbContext>();
      var entry = await _dbContext.PriorZipmodEntries.FindAsync(manifest.Hash);
      return entry != null;
    }

    public async Task<bool> IsNewerVersionAvailableAsync(IZipmod zipmod)
    {
      var _dbContext = _serviceProvider.GetService<ZipmodDbContext>();
      if (zipmod.Manifest == null || _dbContext.PriorZipmodEntries.Find(zipmod.Manifest.Guid) != null)
      {
        return false;
      }
      var entry = await _dbContext.PriorZipmodEntries.FindAsync(zipmod.Hash);
      if (entry == null)
      {
        return false;
      }
      var currentVersion = Version.Parse(zipmod.Manifest.Version);
      var lastVersion = Version.Parse(entry.Version);
      return lastVersion > currentVersion;
    }

    public async Task<bool> SetCanSkipZipmodAsync(IZipmod zipmod, bool canSkip)
    {
      var _dbContext = _serviceProvider.GetService<ZipmodDbContext>();
      if (zipmod.Manifest == null)
      {
        return false;
      }
      var entry = await _dbContext.PriorZipmodEntries.FindAsync(zipmod.Hash);
      if (entry == null)
      {
        return false;
      }
      entry.CanSkip = canSkip;
      _dbContext.PriorZipmodEntries.Update(entry);
      await _dbContext.SaveChangesAsync();
      return true;
    }

    public async Task<bool> RemoveZipmodAsync(IZipmod zipmod)
    {
      var _dbContext = _serviceProvider.GetService<ZipmodDbContext>();
      if (zipmod.Manifest == null)
      {
        return false;
      }
      var entry = await _dbContext.PriorZipmodEntries.FindAsync(zipmod.Hash);
      if (entry == null)
      {
        return false;
      }
      _dbContext.PriorZipmodEntries.Remove(entry);
      await _dbContext.SaveChangesAsync();
      return true;
    }

    public async Task<bool> UpdateZipmodAsync(IZipmod zipmod)
    {
      var _dbContext = _serviceProvider.GetService<ZipmodDbContext>();
      if (zipmod.Manifest == null)
      {
        return false;
      }
      _dbContext.PriorZipmodEntries.Update(GetPriorZipmodEntryFromZipmod(zipmod));
      await _dbContext.SaveChangesAsync();
      return true;
    }

    static PriorZipmodEntry GetPriorZipmodEntryFromZipmod(IZipmod zipmod) => new()
    {
      Guid = zipmod.Manifest.Guid,
      CanSkip = true,
      Version = zipmod.Manifest.Version,
    };
  }
}
