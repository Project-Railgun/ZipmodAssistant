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
    Task<ICard?> TryReadCardAsync(string filename);
    Task<FileInfo?> TryWriteCardAsync(string filename, ICard card);
  }
}
