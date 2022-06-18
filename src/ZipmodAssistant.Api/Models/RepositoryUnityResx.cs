using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZipmodAssistant.Api.Enums;
using ZipmodAssistant.Api.Interfaces.Models;

namespace ZipmodAssistant.Api.Models
{
  public class RepositoryUnityResx : IRepositoryItem
  {
    public RepositoryItemType ItemType => RepositoryItemType.UnityResx;

    public FileInfo FileInfo { get; }

    public byte[] Hash => Array.Empty<byte>();

    public RepositoryUnityResx(FileInfo fileInfo)
    {
      FileInfo = fileInfo;
    }

    public async Task<IProcessResult> ProcessAsync(IBuildConfiguration buildConfiguration, IBuildRepository repository)
    {
      throw new NotImplementedException();
    }
  }
}
