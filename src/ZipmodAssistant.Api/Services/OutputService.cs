using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ZipmodAssistant.Api.Enums;
using ZipmodAssistant.Api.Interfaces.Models;
using ZipmodAssistant.Api.Interfaces.Services;
using ZipmodAssistant.Api.Models;
using ZipmodAssistant.Api.Utilities;

namespace ZipmodAssistant.Api.Services
{
  public class OutputService : IOutputService
  {
    private static readonly ConcurrentDictionary<string, FileStream> _tempFileStreams = new();
    private static readonly Regex _validNameRegex = new(@"^\[(.+)\]\s?(.+)\s?v([0-9]+(?:\.[0-9]+)?(?:\.[0-9]+))?\.zipmod$");

    private readonly ILoggerService _logger;

    public OutputService(ILoggerService logger)
    {
      _logger = logger;
    }

    public IProcessResult CopyOriginal(IRepositoryItem item, IBuildConfiguration buildConfiguration)
    {
      var directory = Path.Join(buildConfiguration.OutputDirectory, "original");
      Directory.CreateDirectory(directory);
      var newLocation = Path.Join(directory, GetItemFilename(item));
      File.Copy(item.FileInfo.FullName, newLocation, true);
      _logger.Log($"Copied original to {newLocation}");
      return new SuccessProcessResult(item);
    }

    public IProcessResult MarkAsBlacklisted(IRepositoryItem item, IBuildConfiguration buildConfiguration)
    {
      _logger.Log($"{item.FileInfo.Name} marked as black-listed");
      return new SuccessProcessResult(item);
    }

    public IProcessResult MarkAsCompleted(IRepositoryItem item, IBuildConfiguration buildConfiguration)
    {
      var directory = Path.Join(buildConfiguration.OutputDirectory, "treated");
      Directory.CreateDirectory(directory);
      var tempFilename = GetTempFilename(item, buildConfiguration);
      if (_tempFileStreams.Remove(tempFilename, out var tempFilestream))
      {
        // finalize stream and close
        tempFilestream.Dispose();
      }
      if (File.Exists(tempFilename))
      {
        File.Copy(tempFilename, Path.Join(directory, item.FileInfo.Name), true);
      }
      else
      {
        item.FileInfo.CopyTo(Path.Join(directory, item.FileInfo.Name), true);
      }
      return new SuccessProcessResult(item);
    }

    public IProcessResult MarkAsMalformed(IRepositoryItem item, IBuildConfiguration buildConfiguration, string reason)
    {
      var directory = Path.Join(buildConfiguration.OutputDirectory, "malformed");
      Directory.CreateDirectory(directory);
      var newLocation = Path.Join(directory, item.FileInfo.Name);
      item.FileInfo.CopyTo(newLocation, true);
      _logger.Log($"{item.FileInfo.Name} malformed ({reason})");
      return new SuccessProcessResult(item);
    }

    public IProcessResult MarkAsSkipped(IRepositoryItem item, IBuildConfiguration buildConfiguration, string reason)
    {
      var directory = Path.Join(buildConfiguration.OutputDirectory, "skipped");
      Directory.CreateDirectory(directory);
      var newLocation = Path.Join(directory, item.FileInfo.Name);
      item.FileInfo.CopyTo(newLocation, true);
      _logger.Log($"{item.FileInfo.Name} skipped ({reason})");
      return new SuccessProcessResult(item);
    }

    public string ReserveCache(IRepositoryItem item, IBuildConfiguration buildConfiguration)
    {
      var tempFilename = GetTempFilename(item, buildConfiguration);
      if (item.ItemType == RepositoryItemType.Repository)
      {
        if (Directory.Exists(tempFilename))
        {
          Directory.Delete(tempFilename, true);
        }
      }
      else
      {
        if (File.Exists(tempFilename))
        {
          throw new UnauthorizedAccessException($"{item.FileInfo.Name} already has a temporary file reservation");
        }
        File.Create(tempFilename).Close();
      }
      return tempFilename;
    }

    public FileStream ReserveCacheFile(IRepositoryItem item, IBuildConfiguration buildConfiguration)
    {
      if (item.ItemType == RepositoryItemType.Repository)
      {
        throw new ArgumentException("Cannot reserve file for zipmod", nameof(item));
      }
      var tempFilename = GetTempFilename(item, buildConfiguration);
      if (_tempFileStreams.TryGetValue(tempFilename, out var existentStream))
      {
        _logger.Log($"Attempted access on temporary file {existentStream.SafeFileHandle}");
        throw new UnauthorizedAccessException($"{item.FileInfo.Name} already has a file stream open");
      }
      var stream = File.Create(tempFilename);
      if (!_tempFileStreams.TryAdd(tempFilename, stream))
      {
        throw new Exception("Failed to add temp filestream to watching collection");
      }
      return stream;
    }

    public FileStream ReserveCacheFile(IRepositoryItem item, IBuildConfiguration buildConfiguration, byte[] data)
    {
      if (item.ItemType == RepositoryItemType.Repository)
      {
        throw new ArgumentException("Cannot reserve file for zipmod", nameof(item));
      }
      var cacheStream = ReserveCacheFile(item, buildConfiguration);
      if (data.Length > 0)
      {
        cacheStream.Write(data, 0, data.Length);
      }
      return cacheStream;
    }

    static string GetTempFilename(IRepositoryItem item, IBuildConfiguration buildConfiguration) =>
      Path.Combine(buildConfiguration.CacheDirectory, $"{item.FileInfo.GetHashCode()}.tmp");

    static string GetItemFilename(IRepositoryItem item)
    {
      if (item is RepositoryZipmod zipmodItem)
      {
        if (_validNameRegex.IsMatch(item.FileInfo.Name))
        {
          return item.FileInfo.Name;
        }
        if (zipmodItem.Manifest != null)
        {
          return TextUtilities.ResolveFilenameFromManifest(zipmodItem.Manifest);
        }
        throw new ArgumentNullException(nameof(zipmodItem.Manifest));
      }
      return item.FileInfo.Name;
    }
  }
}
