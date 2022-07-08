using System.Collections.Concurrent;
using System.IO.Compression;
using System.Text.RegularExpressions;
using ZipmodAssistant.Api.Interfaces.Models;
using ZipmodAssistant.Api.Interfaces.Services;
using ZipmodAssistant.Api.Models;
using ZipmodAssistant.Api.Utilities;

namespace ZipmodAssistant.Api.Services
{
  public class OutputService : IOutputService
  {
    private static readonly Regex _validNameRegex = new(@"^\[(.+)\]\s?(.+)\s?v([0-9]+(?:\.[0-9]+)?(?:\.[0-9]+))?\.zipmod$");

    private readonly ILoggerService _logger;

    public OutputService(ILoggerService logger)
    {
      _logger = logger;
    }

    public IProcessResult CopyOriginal(IZipmod item, IBuildConfiguration buildConfiguration)
    {
      var directory = Path.Join(buildConfiguration.OutputDirectory, "original");
      Directory.CreateDirectory(directory);
      var newLocation = Path.Join(directory, GetItemFilename(item));
      File.Copy(item.FileInfo.FullName, newLocation, true);
      _logger.Log($"Copied original to {newLocation}");
      return new SuccessProcessResult(item);
    }

    public IProcessResult MarkAsBlacklisted(IZipmod item, IBuildConfiguration buildConfiguration)
    {
      _logger.Log($"{item.FileInfo.Name} marked as black-listed");
      return new SuccessProcessResult(item);
    }

    public IProcessResult MarkAsCompleted(IZipmod item, IBuildConfiguration buildConfiguration)
    {
      var directory = Path.Join(buildConfiguration.OutputDirectory, "treated");
      Directory.CreateDirectory(directory);
      // compile the folder into a zip
      return new SuccessProcessResult(item);
    }

    public IProcessResult MarkAsMalformed(IZipmod item, IBuildConfiguration buildConfiguration, string reason)
    {
      var directory = Path.Join(buildConfiguration.OutputDirectory, "malformed");
      Directory.CreateDirectory(directory);
      var newLocation = Path.Join(directory, item.FileInfo.Name);
      item.FileInfo.CopyTo(newLocation, true);
      _logger.Log($"{item.FileInfo.Name} malformed ({reason})");
      return new SuccessProcessResult(item);
    }

    public IProcessResult MarkAsSkipped(IZipmod item, IBuildConfiguration buildConfiguration, string reason)
    {
      var directory = Path.Join(buildConfiguration.OutputDirectory, "skipped");
      Directory.CreateDirectory(directory);
      var newLocation = Path.Join(directory, item.FileInfo.Name);
      item.FileInfo.CopyTo(newLocation, true);
      _logger.Log($"{item.FileInfo.Name} skipped ({reason})");
      return new SuccessProcessResult(item);
    }

    static string GetItemFilename(IZipmod item)
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
