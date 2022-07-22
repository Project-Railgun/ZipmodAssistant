using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ZipmodAssistant.Api.Enums;
using ZipmodAssistant.Api.Interfaces.Models;
using ZipmodAssistant.App.Interfaces.Models;
using ZipmodAssistant.App.Interfaces.Services;
using ZipmodAssistant.Shared.Enums;

namespace ZipmodAssistant.App.ViewModels
{
  public class ProjectViewModel : INotifyPropertyChanged, IProjectConfiguration
  {
    private readonly IProjectService _projectService;
    private bool _hasChanges = false;
    private string? _filename;
    private bool _isBuilding = false;
    private int _buildProgress = 0;
    private string _inputDirectory = string.Empty;
    private string _outputDirectory = string.Empty;
    private string _cacheDirectory = string.Empty;
    private bool _randomizeCab = true;
    private bool _skipRenaming = false;
    private bool _skipCompression = false;
    private bool _skipCleanup = false;
    private bool _skipKnownMods = false;
    private bool _isKk = false;
    private bool _isKks = false;
    private bool _isEc = false;
    private bool _isAis = false;
    private bool _isHs2 = false;

    public event PropertyChangedEventHandler? PropertyChanged;

    [JsonIgnore]
    public string? Filename
    {
      get => _filename;
      set
      {
        _filename = value;
        OnPropertyChanged();
      }
    }

    [JsonIgnore]
    public bool IsPersisted => _filename != null;

    [JsonIgnore]
    public bool IsBuilding
    {
      get => _isBuilding;
      set
      {
        _isBuilding = value;
        OnPropertyChanged();
      }
    }

    [JsonIgnore]
    public int BuildProgress
    {
      get => _buildProgress;
      set
      {
        _buildProgress = value;
        OnPropertyChanged();
      }
    }

    public string InputDirectory
    {
      get => _inputDirectory;
      set
      {
        _inputDirectory = value;
        OnPropertyChanged();
      }
    }

    public string OutputDirectory
    {
      get => _outputDirectory;
      set
      {
        _outputDirectory = value;
        OnPropertyChanged();
      }
    }

    public string CacheDirectory
    {
      get => _cacheDirectory;
      set
      {
        _cacheDirectory = value;
        OnPropertyChanged();
      }
    }

    public bool RandomizeCab
    {
      get => _randomizeCab;
      set
      {
        _randomizeCab = value;
        OnPropertyChanged();
      }
    }

    public bool SkipRenaming
    {
      get => _skipRenaming;
      set
      {
        _skipRenaming = value;
        OnPropertyChanged();
      }
    }

    public bool SkipCompression
    {
      get => _skipCompression;
      set
      {
        _skipCompression = value;
        OnPropertyChanged();
      }
    }

    public bool SkipCleanup
    {
      get => _skipCleanup;
      set
      {
        _skipCleanup = value;
        OnPropertyChanged();
      }
    }

    public bool SkipKnownMods
    {
      get => _skipKnownMods;
      set
      {
        _skipKnownMods = value;
        OnPropertyChanged();
      }
    }

    public bool IsKk
    {
      get => _isKk;
      set
      {
        _isKk = value;
        OnPropertyChanged();
      }
    }

    public bool IsKks
    {
      get => _isKks;
      set
      {
        _isKks = value;
        OnPropertyChanged();
      }
    }

    public bool IsEc
    {
      get => _isEc;
      set
      {
        _isEc = value;
        OnPropertyChanged();
      }
    }

    public bool IsAis
    {
      get => _isAis;
      set
      {
        _isAis = value;
        OnPropertyChanged();
      }
    }

    public bool IsHs2
    {
      get => _isHs2;
      set
      {
        _isHs2 = value;
        OnPropertyChanged();
      }
    }

    [JsonIgnore]
    public IEnumerable<TargetGame> Games
    {
      get => new TargetGame?[]
      {
        IsKk ? TargetGame.Koikatu : null,
        IsKk ? TargetGame.KoikatsuParty : null,
        IsKk ? TargetGame.KoikatsuPartySpecialPatch : null,
        IsKks ? TargetGame.KoikatsuSunshine : null,
        IsEc ? TargetGame.EmotionCreators : null,
        IsAis ? TargetGame.AiSyoujyo : null,
        IsHs2 ? TargetGame.HoneySelect2 : null,
      }.Where(g => g != null).Cast<TargetGame>();
    }

    [JsonIgnore]
    public ObservableCollection<string> LogMessages { get; set; } = new();

    public ProjectViewModel(IProjectService projectService, ContainerViewModel containerViewModel)
    {
      _projectService = projectService;
      var currentProject = projectService.GetCurrentProject();
      if (currentProject != null)
      {
        _filename = currentProject.Filename;
        _inputDirectory = currentProject.InputDirectory;
        _outputDirectory = currentProject.OutputDirectory;
        _cacheDirectory = currentProject.CacheDirectory;
        _isKk = currentProject.Games.Contains(TargetGame.Koikatu) ||
          currentProject.Games.Contains(TargetGame.KoikatsuParty) ||
          currentProject.Games.Contains(TargetGame.KoikatsuPartySpecialPatch);
        _isKks = currentProject.Games.Contains(TargetGame.KoikatsuSunshine);
        _isEc = currentProject.Games.Contains(TargetGame.EmotionCreators);
        _isAis = currentProject.Games.Contains(TargetGame.AiSyoujyo);
        _isHs2 = currentProject.Games.Contains(TargetGame.HoneySelect2);
        _randomizeCab = currentProject.RandomizeCab;
        _skipCompression = currentProject.SkipCompression;
        _skipCleanup = currentProject.SkipCleanup;
        _skipKnownMods = currentProject.SkipKnownMods;
      }
      containerViewModel.Update();
    }

    public async Task SaveAsync()
    {
      _hasChanges = !await _projectService.SaveProjectAsync(this);
    }

    public void ValidateDirectoryConfiguration()
    {
      if (!Directory.Exists(InputDirectory))
      {
        throw new DirectoryNotFoundException(InputDirectory);
      }
    }

    public void ResetGames()
    {
      IsKk = false;
      IsKks = false;
      IsEc = false;
      IsAis = false;
      IsHs2 = false;
    }

    void OnPropertyChanged([CallerMemberName] string name = "") =>
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
  }
}
