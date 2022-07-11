using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZipmodAssistant.Tarot.Utilities
{
  public static class CardUtilities
  {
    private static readonly byte[] _endChunk = { 0x49, 0x45, 0x4E, 0x44, 0xAE, 0x42, 0x60, 0x82 };

    private class EmptyStream : Stream
    {
      public override bool CanRead => false;

      public override bool CanSeek => true;

      public override bool CanWrite => true;

      public override long Length => 0;

      public override long Position
      {
        get => 0;
        set
        {
          // do nothing
        }
      }

      public override void Flush() { }

      public override int Read(byte[] buffer, int offset, int count) => 0;

      public override long Seek(long offset, SeekOrigin origin) => 0;

      public override void SetLength(long value) { }

      public override void Write(byte[] buffer, int offset, int count) { }
    }

    public static async Task<BinaryReader> ReadAdditionalBytesAsync(Stream stream)
    {
      using var imageStream = new EmptyStream();
      var dataStream = new MemoryStream();
      await ReadDataToBuffersAsync(stream, imageStream, dataStream);
      return new BinaryReader(dataStream);
    }

    public static async Task ReadDataToBuffersAsync(Stream inputStream, [NotNull]Stream imageStream, [NotNull]Stream dataStream)
    {
      bool isReadingChunks;
      // contains the image information
      var imageInfoBuffer = new byte[8];
      await inputStream.ReadAsync(imageInfoBuffer);
      await imageStream.WriteAsync(imageInfoBuffer);
      do
      {
        var lengthInfoBuffer = new byte[4];
        await inputStream.ReadAsync(lengthInfoBuffer);
        var length = BinaryPrimitives.ReadInt32BigEndian(lengthInfoBuffer);
        // length should be 1024, but there might be cases where it's not

        var chunkDataInfoBuffer = new byte[4];
        await inputStream.ReadAsync(chunkDataInfoBuffer);
        await imageStream.WriteAsync(chunkDataInfoBuffer);

        var chunkDataBuffer = new byte[length];
        await inputStream.ReadAsync(chunkDataBuffer);
        await imageStream.WriteAsync(chunkDataBuffer);

        var crcBuffer = new byte[4];
        await inputStream.ReadAsync(crcBuffer);
        await imageStream.WriteAsync(crcBuffer);

        // at this point, all data has been written to the image stream. Now we only check if we can end
        // and start on the data stream
        int i;
        for (i = _endChunk.Length - 1; i >= 0; i--)
        {
          if (chunkDataBuffer[^i] != _endChunk[i])
          {
            break;
          }
        }
        isReadingChunks = i > 0;
      }
      while (isReadingChunks);

      await inputStream.CopyToAsync(dataStream);
    }

    public static void ReadDataToBuffers(Stream inputStream, [NotNull]Stream imageStream, [NotNull]Stream dataStream)
    {
      bool isReadingChunks;
      var imageInfoBuffer = new byte[8];
      inputStream.Read(imageInfoBuffer);
      imageStream.Write(imageInfoBuffer);
      do
      {
        var lengthInfoBuffer = new byte[4];
        inputStream.Read(lengthInfoBuffer);
        var length = BinaryPrimitives.ReadInt32BigEndian(lengthInfoBuffer);
        // length should be 1024, but there might be cases where it's not

        var chunkDataInfoBuffer = new byte[4];
        inputStream.Read(chunkDataInfoBuffer);
        imageStream.Write(chunkDataInfoBuffer);

        var chunkDataBuffer = new byte[length];
        inputStream.Read(chunkDataBuffer);
        imageStream.Write(chunkDataBuffer);

        var crcBuffer = new byte[4];
        inputStream.Read(crcBuffer);
        imageStream.Write(crcBuffer);

        int i;
        for (i = _endChunk.Length - 1; i >= 0; i--)
        {
          if (chunkDataBuffer[^i] != _endChunk[i])
          {
            break;
          }
        }
        isReadingChunks = i > 0;
      }
      while (isReadingChunks);

      inputStream.CopyTo(dataStream);
    }
  }
}
