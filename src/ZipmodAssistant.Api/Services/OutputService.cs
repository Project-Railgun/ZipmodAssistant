using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZipmodAssistant.Api.Interfaces.Models;
using ZipmodAssistant.Api.Interfaces.Services;
using ZipmodAssistant.Api.Models;

namespace ZipmodAssistant.Api.Services
{
  public class OutputService : IOutputService
  {
    private static readonly Dictionary<string, FileStream> _tempFileStreams = new();

    private readonly IBuildConfiguration _buildConfiguration;
    private readonly ILoggerService _logger;

    public OutputService(IBuildConfiguration buildConfiguration, ILoggerService logger)
    {
      _buildConfiguration = buildConfiguration;
      _logger = logger;
    }

    public IProcessResult CopyOriginal(IRepositoryItem item)
    {
      throw new NotImplementedException();
    }

    public IProcessResult MarkAsBlacklisted(IRepositoryItem item)
    {
      throw new NotImplementedException();
    }

    public IProcessResult MarkAsCompleted(IRepositoryItem item)
    {
      var tempFilename = GetTempFilename(item);
      if (_tempFileStreams.ContainsKey(tempFilename))
      {
        // finalize stream and close
      }
      else if (File.Exists(tempFilename))
      {
        // same as above, but we didn't create a stream for it
      }
      else
      {
        // do a straight copy to output
      }
      return new SuccessProcessResult(item);
    }

    public IProcessResult MarkAsMalformed(IRepositoryItem item, string reason)
    {
      throw new NotImplementedException();
    }

    public IProcessResult MarkAsSkipped(IRepositoryItem item, string reason)
    {
      throw new NotImplementedException();
    }

    public string ReserveCache(IRepositoryItem item)
    {
      var tempFilename = GetTempFilename(item);
      if (File.Exists(tempFilename))
      {
        throw new UnauthorizedAccessException($"{item.FileInfo.Name} already has a temporary file reservation");
      }
      File.Create(tempFilename).Close();
      return tempFilename;
    }

    public FileStream ReserveCacheFile(IRepositoryItem item)
    {
      var tempFilename = GetTempFilename(item);
      if (_tempFileStreams.TryGetValue(tempFilename, out var existentStream))
      {
        _logger.Log($"Attempted access on temporary file {existentStream.SafeFileHandle}");
        throw new UnauthorizedAccessException($"{item.FileInfo.Name} already has a file stream open");
      }
      var stream = File.Create(tempFilename);
      _tempFileStreams.Add(tempFilename, stream);
      return stream;
    }

    public FileStream ReserveCacheFile(IRepositoryItem item, byte[] data)
    {
      var cacheStream = ReserveCacheFile(item);
      if (data.Length > 0)
      {
        cacheStream.Write(data, 0, data.Length);
      }
      return cacheStream;
    }

    string GetTempFilename(IRepositoryItem item) =>
      Path.Combine(_buildConfiguration.CacheDirectory, $"{item.FileInfo.GetHashCode()}.tmp");
  }
}
