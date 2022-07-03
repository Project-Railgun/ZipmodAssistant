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
    IProcessResult MarkAsMalformed(IRepositoryItem item, IBuildConfiguration buildConfiguration, string reason);
    /// <summary>
    ///   Moves <paramref name="item"/> to OUTPUT/blacklisted and saves the blacklist
    ///   information to the database
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    IProcessResult MarkAsBlacklisted(IRepositoryItem item, IBuildConfiguration buildConfiguration);
    /// <summary>
    ///   Copies <paramref name="item"/> to the OUTPUT/original directory
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    IProcessResult CopyOriginal(IRepositoryItem item, IBuildConfiguration buildConfiguration);
    /// <summary>
    ///   Moves <paramref name="item"/> to OUTPUT/treated/[targetgame]/[modtype]
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    IProcessResult MarkAsCompleted(IRepositoryItem item, IBuildConfiguration buildConfiguration);
    /// <summary>
    ///   Moves <paramref name="item"/> to OUTPUT/skipped, documenting the reason
    ///   in the logger
    /// </summary>
    /// <param name="item"></param>
    /// <param name="reason"></param>
    /// <returns></returns>
    IProcessResult MarkAsSkipped(IRepositoryItem item, IBuildConfiguration buildConfiguration, string reason);
    /// <summary>
    ///   Creates a temporary file for <paramref name="item"/>
    /// </summary>
    /// <remarks>
    ///   The temporary file will not be committed until .MarkAsCompleted is called
    ///   on the same <see cref="IRepositoryItem"/>
    /// </remarks>
    /// <param name="item"></param>
    /// <returns>The name of the temporary file</returns>
    string ReserveCache(IRepositoryItem item, IBuildConfiguration buildConfiguration);
    /// <summary>
    ///   Creates a temporary file for <paramref name="item"/>
    /// </summary>
    /// <remarks>
    ///   The temporary file will not be committed until .MarkAsCompleted is called
    ///   on the same <see cref="IRepositoryItem"/>
    /// </remarks>
    /// <param name="item"></param>
    /// <returns>A filestream for the temporary file</returns>
    FileStream ReserveCacheFile(IRepositoryItem item, IBuildConfiguration buildConfiguration);
    /// <summary>
    ///   Creates a temporary file for <paramref name="item"/> and writes <paramref name="data"/>
    ///   before returning
    /// </summary>
    /// <remarks>
    ///   The temporary file will not be committed until .MarkAsCompleted is called
    ///   on the same <see cref="IRepositoryItem"/>
    /// </remarks>
    /// <param name="item"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    FileStream ReserveCacheFile(IRepositoryItem item, IBuildConfiguration buildConfiguration, byte[] data);
  }
}
