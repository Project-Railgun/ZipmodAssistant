using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZipmodAssistant.Api.Enums;
using ZipmodAssistant.Api.Interfaces.Models;
using ZipmodAssistant.Api.Interfaces.Services;
using ZipmodAssistant.Tarot.Interfaces.Providers;

namespace ZipmodAssistant.Api.Models
{
  public class RepositoryImage : IRepositoryItem
  {
    public RepositoryItemType ItemType => RepositoryItemType.Image;

    public FileInfo FileInfo { get; }

    public byte[] Hash => Array.Empty<byte>();

    private readonly ICardProvider _cardProvider;

    public RepositoryImage(FileInfo fileInfo, ICardProvider cardProvider)
    {
      FileInfo = fileInfo;
      _cardProvider = cardProvider;
    }

    public async Task<IProcessResult> ProcessAsync(IOutputService output, IBuildRepository repository)
    {
      var card = await _cardProvider.TryReadCardAsync(FileInfo);
      if (card == null)
      {
        return output.MarkAsSkipped(this, "No data after IEND");
      }

      return output.MarkAsCompleted(this);
    }
  }
}
