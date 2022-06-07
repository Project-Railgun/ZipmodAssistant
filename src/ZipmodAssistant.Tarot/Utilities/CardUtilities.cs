using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZipmodAssistant.Tarot.Utils
{
  public static class CardUtilities
  {
    private static readonly byte[] _endChunk = { 0x49, 0x45, 0x4E, 0x44, 0xAE, 0x42, 0x60, 0x82 };

    public static async Task<byte[]> ReadAdditionalBytesAsync(Stream stream)
    {
      // first iterate through actual chunks
      // get rid of the first 8 bytes in the reader
      stream.Position += 8;
      var isReadingChunks = true;
      do
      {
        // TODO: is binaryprimitives faster than just regular bit shifting?
        var lengthData = new byte[4];
        await stream.ReadAsync(lengthData);
        var length = BinaryPrimitives.ReadInt32BigEndian(lengthData);
        // remove chunk type
        stream.Position += 4;
        var chunkData = new byte[length];
        await stream.ReadAsync(chunkData);
        // remove CRC
        stream.Position += 4;
        // TODO: I'm tired as hell right now so verify this
        if (chunkData[^1] == _endChunk[^1] && chunkData[^(_endChunk.Length - 1)] == _endChunk[0])
        {
          // chances are we're at IEND, verify by checking the rest
          isReadingChunks = false;
        }
      }
      while (isReadingChunks);

      var buffer = new MemoryStream();
      // haven't tested, not sure if it respects the position during copy
      await stream.CopyToAsync(buffer);
      return buffer.ToArray();
    }
  }
}
