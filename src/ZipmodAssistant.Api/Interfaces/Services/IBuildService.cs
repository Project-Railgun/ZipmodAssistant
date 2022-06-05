using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZipmodAssistant.Api.Interfaces.Models;

namespace ZipmodAssistant.Api.Interfaces.Services
{
  public interface IBuildService
  {
    Task<bool> CommitBuildActionAsync(IBuildAction buildAction);

  }
}
