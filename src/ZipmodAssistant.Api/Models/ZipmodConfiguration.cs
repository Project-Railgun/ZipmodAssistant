using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZipmodAssistant.Api.Enums;
using ZipmodAssistant.Api.Interfaces.Models;
using ZipmodAssistant.Shared.Enums;

namespace ZipmodAssistant.Api.Models
{
  public class ZipmodConfiguration : IZipmodConfiguration
  {
    public string Guid { get; init; }

    public string? Name { get; init; }

    public string Author { get; init; }

    public Version Version { get; init; }

    public string Website { get; init; }

    public string Description { get; init; }

    public IEnumerable<TargetGame> Games { get; } = new List<TargetGame>();

    public bool RandomizeCab { get; set; }

    public bool SkipRenaming { get; set; }

    public bool SkipCompression { get; set; }

    public bool SkipCleanup { get; set; }
  }
}
