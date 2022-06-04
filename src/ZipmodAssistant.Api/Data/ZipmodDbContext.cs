using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZipmodAssistant.Api.Data
{
  public class ZipmodDbContext : DbContext
  {
    public ZipmodDbContext(DbContextOptions<ZipmodDbContext> options) : base(options)
    {
      Database.EnsureCreated();
    }
  }
}
