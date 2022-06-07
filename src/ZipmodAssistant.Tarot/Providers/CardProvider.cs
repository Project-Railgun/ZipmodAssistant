using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZipmodAssistant.Tarot.Interfaces.Models;
using ZipmodAssistant.Tarot.Interfaces.Providers;

namespace ZipmodAssistant.Tarot.Providers
{
  public class CardProvider : ICardProvider
  {
    public async Task<ICard?> TryReadCardAsync(FileInfo fileInfo)
    {
      try
      {
        using var stream = fileInfo.OpenRead();
        using var reader = new BinaryReader(stream);

        if (reader.BaseStream.Length == 0)
        {
          return null;
        }
        var header = reader.ReadBytes(8);
      }
      catch
      {
        return null;
      }
    }

    public async Task<FileInfo?> TryWriteCardAsync(string fileLocation, ICard card)
    {
      try
      {

      }
      catch
      {
        return null;
      }
    }
  }
}
