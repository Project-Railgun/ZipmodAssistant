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
    private static readonly byte[] _pngSignature = new byte[]
    {
      0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A,
    };

    private static Dictionary<string, bool> _hasIEndDataCache = new();

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

    public static async Task<bool> ContainsDataAfterIEndAsync(string filename)
    {
      if (_hasIEndDataCache.TryGetValue(filename, out var hasIEndData))
      {
        return hasIEndData;
      }
      using var inputStream = File.OpenRead(filename);
      bool isReadingChunks;
      // contains the image information
      var imageInfoBuffer = new byte[8];
      await inputStream.ReadAsync(imageInfoBuffer);
      for (var i = 0; i < _pngSignature.Length; i++)
      {
        if (imageInfoBuffer[i] != _pngSignature[i])
        {
          throw new BadImageFormatException("PNG file header invalid, ensure it's not corrupted");
        }
      }
      do
      {
        var lengthInfoBuffer = new byte[4];
        await inputStream.ReadAsync(lengthInfoBuffer);
        var length = BinaryPrimitives.ReadInt32BigEndian(lengthInfoBuffer);
        // length should be 8196, but there might be cases where it's not

        var chunkDataInfoBuffer = new byte[4];
        await inputStream.ReadAsync(chunkDataInfoBuffer);

        var chunkDataBuffer = new byte[length];
        await inputStream.ReadAsync(chunkDataBuffer);

        var crcBuffer = new byte[4];
        await inputStream.ReadAsync(crcBuffer);

        // at this point, all data has been written to the image stream. Now we only check if we can end
        // and start on the data stream

        isReadingChunks = length > 0;
      }
      while (isReadingChunks);
      var result = inputStream.Position == inputStream.Length - 1;
      _hasIEndDataCache.Add(filename, result);
      return result;
    }

    public static async Task ReadDataToBuffersAsync(Stream inputStream, [NotNull]Stream imageStream, [NotNull]Stream dataStream)
    {
      bool isReadingChunks;
      // contains the image information
      var imageInfoBuffer = new byte[8];
      await inputStream.ReadAsync(imageInfoBuffer);
      for (var i = 0; i < _pngSignature.Length; i++)
      {
        if (imageInfoBuffer[i] != _pngSignature[i])
        {
          throw new BadImageFormatException("PNG file header invalid, ensure it's not corrupted");
        }
      }
      await imageStream.WriteAsync(imageInfoBuffer);
      do
      {
        var lengthInfoBuffer = new byte[4];
        await inputStream.ReadAsync(lengthInfoBuffer);
        var length = BinaryPrimitives.ReadInt32BigEndian(lengthInfoBuffer);
        await imageStream.WriteAsync(lengthInfoBuffer);
        // length should be 8196, but there might be cases where it's not

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

        isReadingChunks = length > 0;
      }
      while (isReadingChunks);

      await inputStream.CopyToAsync(dataStream);
      imageStream.Seek(0, SeekOrigin.Begin);
      dataStream.Seek(0, SeekOrigin.Begin);
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

        isReadingChunks = length > 0;
      }
      while (isReadingChunks);

      inputStream.CopyTo(dataStream);
    }
  }
}
