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

    public async Task<IEnumerable<PriorZipmodEntry>> GetZipmodsAsync()
    {
      var dbContext = _serviceProvider.GetService<ZipmodDbContext>();
      var entries = dbContext.PriorZipmodEntries.ToList();
      return entries;
    }

    public async Task<bool> AddZipmodAsync(IZipmod zipmod)
    {
      var dbContext = _serviceProvider.GetService<ZipmodDbContext>();
      if (zipmod.Manifest == null || dbContext.PriorZipmodEntries.Find(zipmod.Manifest.Guid) != null)
      {
        return false;
      }
      await dbContext.PriorZipmodEntries.AddAsync(GetPriorZipmodEntryFromZipmod(zipmod));
      await dbContext.SaveChangesAsync();
      return true;
    }

    public async Task<bool> IsManifestInHistoryAsync(IManifest manifest)
    {
      var dbContext = _serviceProvider.GetService<ZipmodDbContext>();
      var entry = await dbContext.PriorZipmodEntries.FindAsync(manifest.Guid);
      return entry != null;
    }

    public async Task<bool> IsNewerVersionAvailableAsync(IZipmod zipmod)
    {
      var dbContext = _serviceProvider.GetService<ZipmodDbContext>();
      if (zipmod.Manifest == null || dbContext.PriorZipmodEntries.Find(zipmod.Manifest.Guid) != null)
      {
        return false;
      }
      var entry = await dbContext.PriorZipmodEntries.FindAsync(zipmod.Hash);
      if (entry == null)
      {
        return false;
      }
      var currentVersion = Version.Parse(zipmod.Manifest.Version);
      var lastVersion = Version.Parse(entry.Version);
      return lastVersion > currentVersion;
    }

    public async Task<bool> SetCanSkipZipmodAsync(string guid, bool canSkip)
    {
      var dbContext = _serviceProvider.GetService<ZipmodDbContext>();
      var entry = await dbContext.PriorZipmodEntries.FindAsync(guid);
      if (entry == null)
      {
        return false;
      }
      entry.CanSkip = canSkip;
      dbContext.PriorZipmodEntries.Update(entry);
      await dbContext.SaveChangesAsync();
      return true;
    }

    public async Task<bool> RemoveZipmodAsync(IZipmod zipmod)
    {
      var dbContext = _serviceProvider.GetService<ZipmodDbContext>();
      if (zipmod.Manifest == null)
      {
        return false;
      }
      var entry = await dbContext.PriorZipmodEntries.FindAsync(zipmod.Hash);
      if (entry == null)
      {
        return false;
      }
      dbContext.PriorZipmodEntries.Remove(entry);
      await dbContext.SaveChangesAsync();
      return true;
    }

    public async Task<bool> RemoveZipmodAsync(string guid)
    {
      var dbContext = _serviceProvider.GetService<ZipmodDbContext>();
      var entry = await dbContext.PriorZipmodEntries.FindAsync(guid);
      if (entry == null)
      {
        return false;
      }
      dbContext.PriorZipmodEntries.Remove(entry);
      await dbContext.SaveChangesAsync();
      return true;
    }

    public async Task<int> RemoveZipmodsAsync(params string[] guids)
    {
      var dbContext = _serviceProvider.GetService<ZipmodDbContext>();
      var toRemove = (await GetZipmodsAsync()).Where(zipmod => guids.Contains(zipmod.Guid));
      dbContext.PriorZipmodEntries.RemoveRange(toRemove);
      var deletedCount = await dbContext.SaveChangesAsync();
      return deletedCount;
    }

    public async Task<bool> UpdateZipmodAsync(IZipmod zipmod)
    {
      var dbContext = _serviceProvider.GetService<ZipmodDbContext>();
      if (zipmod.Manifest == null)
      {
        return false;
      }
      dbContext.PriorZipmodEntries.Update(GetPriorZipmodEntryFromZipmod(zipmod));
      await dbContext.SaveChangesAsync();
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
