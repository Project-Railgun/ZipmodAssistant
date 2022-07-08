using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZipmodAssistant.Api.Interfaces.Models;

namespace ZipmodAssistant.Api.Interfaces.Services
{
  /// <summary>
  ///   Handles moving files around to the appropriate directories
  /// </summary>
  public interface IOutputService
  {
    /// <summary>
    ///   Moves <paramref name="item"/> to OUTPUT/malformed, documenting the reason
    ///   in the logger
    /// </summary>
    /// <param name="item"></param>
    /// <param name="reason"></param>
    /// <returns></returns>
    IProcessResult MarkAsMalformed(IZipmod item, IBuildConfiguration buildConfiguration, string reason);
    /// <summary>
    ///   Moves <paramref name="item"/> to OUTPUT/blacklisted and saves the blacklist
    ///   information to the database
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    IProcessResult MarkAsBlacklisted(IZipmod item, IBuildConfiguration buildConfiguration);
    /// <summary>
    ///   Copies <paramref name="item"/> to the OUTPUT/original directory
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    IProcessResult CopyOriginal(IZipmod item, IBuildConfiguration buildConfiguration);
    /// <summary>
    ///   Moves <paramref name="item"/> to OUTPUT/treated/[targetgame]/[modtype]
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    IProcessResult MarkAsCompleted(IZipmod item, IBuildConfiguration buildConfiguration);
    /// <summary>
    ///   Moves <paramref name="item"/> to OUTPUT/skipped, documenting the reason
    ///   in the logger
    /// </summary>
    /// <param name="item"></param>
    /// <param name="reason"></param>
    /// <returns></returns>
    IProcessResult MarkAsSkipped(IZipmod item, IBuildConfiguration buildConfiguration, string reason);
  }
}
