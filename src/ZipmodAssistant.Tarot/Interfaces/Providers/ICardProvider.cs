using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZipmodAssistant.Tarot.Interfaces.Models;

namespace ZipmodAssistant.Tarot.Interfaces.Providers
{
  public interface ICardProvider
  {
    Task<ICard?> TryReadCardAsync(FileInfo fileInfo);
    Task<FileInfo?> TryWriteCardAsync(string fileLocation, ICard card);
  }
}
