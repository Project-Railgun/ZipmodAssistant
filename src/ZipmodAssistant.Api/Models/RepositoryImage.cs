using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZipmodAssistant.Api.Enums;
using ZipmodAssistant.Api.Interfaces.Models;
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

    public async Task<bool> ProcessAsync(IBuildConfiguration buildConfiguration, IBuildRepository repository)
    {
      var card = await _cardProvider.TryReadCardAsync(FileInfo);
      return card != null;
    }
  }
}
