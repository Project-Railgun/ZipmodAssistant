using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ZipmodAssistant.Api.Data;
using ZipmodAssistant.Api.Data.DataModels;
using ZipmodAssistant.Api.Enums;
using ZipmodAssistant.Api.Exceptions;
using ZipmodAssistant.Api.Extensions;
using ZipmodAssistant.Api.Interfaces.Models;
using ZipmodAssistant.Api.Interfaces.Services;

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
