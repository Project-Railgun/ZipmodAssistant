using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZipmodAssistant.Api.Enums;
using ZipmodAssistant.Api.Interfaces.Services;

namespace ZipmodAssistant.Api.Interfaces.Models
{
  /// <summary>
  ///   Represents any valid file inside of a <see cref="IBuildRepository"/>
  /// </summary>
  public interface IRepositoryItem
  {
    /// <summary>
    ///   The file type for the item
    /// </summary>
    RepositoryItemType ItemType { get; }
    /// <summary>
    ///   The file this item is from
    /// </summary>
    FileInfo FileInfo { get; }
    /// <summary>
    ///   A base64 encoded MD5 hash of the item. Optional
    /// </summary>
    string Hash { get; }

    /// <summary>
    ///   Processes and validates the file
    /// </summary>
    /// <param name="buildConfig">The build configuration to use - typically owned by the <see cref="IBuildRepository"/></param>
    /// <param name="output">The output service that controls the movement between cache and output</param>
    /// <returns></returns>
    Task<IProcessResult> ProcessAsync(IBuildConfiguration buildConfig, IOutputService output);
  }
}
