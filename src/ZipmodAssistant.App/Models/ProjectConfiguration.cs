using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ZipmodAssistant.App.Interfaces.Models;
using ZipmodAssistant.Shared.Enums;

namespace ZipmodAssistant.App.Models
{
  public class ProjectConfiguration : IProjectConfiguration
  {
    static readonly JsonSerializerOptions _serializerOptions = new()
    {
      WriteIndented = true,
    };

    [JsonIgnore]
    public string? Filename { get; set; }

    [JsonIgnore]
    public bool IsPersisted => Filename != null;

    public string InputDirectory { get; set; } = string.Empty;

    public string OutputDirectory { get; set; } = string.Empty;

    public string CacheDirectory { get; set; } = string.Empty;

    public bool RandomizeCab { get; set; }

    public bool SkipRenaming { get; set; }

    public bool SkipCompression { get; set; }

    public bool SkipCleanup { get; set; }

    public bool SkipKnownMods { get; set; }

    public IEnumerable<TargetGame> Games { get; set; } = new List<TargetGame>();

    public static async Task<ProjectConfiguration> LoadAsync(string filename)
    {
      using var fs = File.OpenRead(filename);
      var project = await JsonSerializer.DeserializeAsync<ProjectConfiguration>(fs, _serializerOptions);
      project.Filename = filename;
      return project;
    }

    public async Task SaveAsync()
    {
      if (Filename == null)
      {
        throw new Exception("Cannot save without filename set");
      }

      using var fs = File.Create(Filename);
      await JsonSerializer.SerializeAsync(fs, this, typeof(ProjectConfiguration), _serializerOptions);
    }
  }
}
