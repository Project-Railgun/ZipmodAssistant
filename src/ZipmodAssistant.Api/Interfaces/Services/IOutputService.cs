using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZipmodAssistant.Api.Interfaces.Models;

namespace ZipmodAssistant.Api.Interfaces.Services
{
  public interface IOutputService
  {
    IProcessResult MarkAsMalformed(IRepositoryItem item, string reason);
    IProcessResult MarkAsBlacklisted(IRepositoryItem item);
    IProcessResult CopyOriginal(IRepositoryItem item);
    IProcessResult MarkAsCompleted(IRepositoryItem item);
    IProcessResult MarkAsSkipped(IRepositoryItem item, string reason);
    string ReserveCache(IRepositoryItem item);
  }
}
