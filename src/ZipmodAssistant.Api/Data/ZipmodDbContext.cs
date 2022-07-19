using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZipmodAssistant.Api.Data.DataModels;

namespace ZipmodAssistant.Api.Data
{
  public class ZipmodDbContext : DbContext
  {
    public DbSet<ManifestHistoryEntry> ManifestHistoryEntries { get; set; }
    public DbSet<SessionResultEntry> SessionResultEntries { get; set; }

    public ZipmodDbContext(DbContextOptions<ZipmodDbContext> options) : base(options)
    {
      Database.EnsureCreated();
    }
  }
}
