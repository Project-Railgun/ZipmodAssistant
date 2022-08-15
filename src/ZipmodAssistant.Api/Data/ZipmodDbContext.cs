using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
    public DbSet<PriorZipmodEntry> PriorZipmodEntries { get; set; }
    public DbSet<SessionResultEntry> SessionResultEntries { get; set; }

    private readonly ILogger<ZipmodDbContext> _logger;

    public ZipmodDbContext(DbContextOptions<ZipmodDbContext> options, ILogger<ZipmodDbContext> logger) : base(options)
    {
      _logger = logger;
      try
      {
        Database.EnsureCreated();
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "An error occured building the database");
      }
    }
  }
}
