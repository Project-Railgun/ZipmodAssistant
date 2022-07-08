using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZipmodAssistant.Api.Interfaces.Models
{
  /// <summary>
  ///   The base that all results inherit from. This is solely for logging
  ///   purposes
  /// </summary>
  public interface IProcessResult
  {
    /// <summary>
    ///   The <see cref="IZipmod"/> this result originated from
    /// </summary>
    IZipmod Target { get; }
  }
}
