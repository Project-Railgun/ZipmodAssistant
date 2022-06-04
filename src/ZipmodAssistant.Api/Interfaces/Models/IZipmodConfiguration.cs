using Semver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZipmodAssistant.Api.Enums;

namespace ZipmodAssistant.Api.Interfaces.Models
{
  public interface IZipmodConfiguration
  {
    string Guid { get; }

    string? Name { get; }

    string Author { get; }

    Version Version { get; }
    
    string Website { get; }

    string Description { get; }

    IEnumerable<TargetGame> Games { get; }

    bool RandomizeCab { get; }

    bool SkipRenaming { get; }

    bool SkipCompression { get; }

    bool SkipCleanup { get; }
  }
}
