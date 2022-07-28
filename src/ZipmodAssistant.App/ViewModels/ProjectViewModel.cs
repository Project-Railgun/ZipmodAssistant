﻿using System;
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
using ZipmodAssistant.Api.Interfaces.Services;
using ZipmodAssistant.App.Interfaces.Models;
using ZipmodAssistant.App.Interfaces.Services;
using ZipmodAssistant.App.Models;
using ZipmodAssistant.Shared.Enums;

namespace ZipmodAssistant.App.ViewModels
{
  public class ProjectViewModel : ViewModel, IProjectConfiguration
  {
    private readonly IProjectService _projectService;
    private readonly ILoggerService _logger;
    private readonly IProjectConfiguration _project;

    private bool _hasChanges = false;
    private bool _isBuilding = false;
    private int _buildProgress = 0;

    public string? Filename
    {
      get => _project.Filename;
      set
      {
        _project.Filename = value;
        OnPropertyChanged();
      }
    }

    public bool IsPersisted => Filename != null;

    public bool IsBuilding
    {
      get => _isBuilding;
      set
      {
        _isBuilding = value;
        OnPropertyChanged();
      }
    }

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
      get => _project.InputDirectory;
      set
      {
        _project.InputDirectory = value;
        OnPropertyChanged();
      }
    }

    public string OutputDirectory
    {
      get => _project.OutputDirectory;
      set
      {
        _project.OutputDirectory = value;
        OnPropertyChanged();
      }
    }

    public string CacheDirectory
    {
      get => _project.CacheDirectory;
      set
      {
        _project.CacheDirectory = value;
        OnPropertyChanged();
      }
    }

    public bool RandomizeCab
    {
      get => _project.RandomizeCab;
      set
      {
        _project.RandomizeCab = value;
        OnPropertyChanged();
      }
    }

    public bool SkipRenaming
    {
      get => _project.SkipRenaming;
      set
      {
        _project.SkipRenaming = value;
        OnPropertyChanged();
      }
    }

    public bool SkipCompression
    {
      get => _project.SkipCompression;
      set
      {
        _project.SkipCompression = value;
        OnPropertyChanged();
      }
    }

    public bool SkipCleanup
    {
      get => _project.SkipCleanup;
      set
      {
        _project.SkipCleanup = value;
        OnPropertyChanged();
      }
    }

    public bool SkipKnownMods
    {
      get => _project.SkipKnownMods;
      set
      {
        _project.SkipKnownMods = value;
        OnPropertyChanged();
      }
    }

    public bool IsKk
    {
      get => _project.Games.Contains(TargetGame.Koikatu) ||
        _project.Games.Contains(TargetGame.KoikatsuParty) ||
        _project.Games.Contains(TargetGame.KoikatsuPartySpecialPatch);
      set
      {
        if (TryUpdateGame(
          value,
          TargetGame.Koikatu,
          TargetGame.KoikatsuParty,
          TargetGame.KoikatsuPartySpecialPatch))
          OnPropertyChanged();
      }
    }

    public bool IsKks
    {
      get => _project.Games.Contains(TargetGame.KoikatsuSunshine);
      set
      {
        if (TryUpdateGame(value, TargetGame.KoikatsuSunshine))
          OnPropertyChanged();
      }
    }

    public bool IsEc
    {
      get => _project.Games.Contains(TargetGame.EmotionCreators);
      set
      {
        if (TryUpdateGame(value, TargetGame.EmotionCreators))
          OnPropertyChanged();
      }
    }

    public bool IsAis
    {
      get => _project.Games.Contains(TargetGame.AiSyoujyo);
      set
      {
        if (TryUpdateGame(value, TargetGame.AiSyoujyo))
          OnPropertyChanged();
      }
    }

    public bool IsHs2
    {
      get => _project.Games.Contains(TargetGame.HoneySelect2);
      set
      {
        if (TryUpdateGame(value, TargetGame.HoneySelect2))
          OnPropertyChanged();
      }
    }

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
      set
      {
        _project.Games = value;
        OnPropertyChanged();
      }
    }

    public ObservableCollection<string> LogMessages { get; set; } = new();

    public ProjectViewModel(IProjectService projectService, ILoggerService logger)
    {
      _projectService = projectService;
      _logger = logger;
      _project = projectService.GetCurrentProject();
      PropertyChanged += (_, _) => _hasChanges = true;
      if (_project != null)
      {
        PropertyChanged += async (_, _) => await SaveAsync();
      }
      else
      {
        _project = new ProjectConfiguration
        {
          InputDirectory = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "Input"),
          OutputDirectory = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "Output"),
          CacheDirectory = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "Cache"),
          RandomizeCab = true,
          Games = new List<TargetGame>(),
        };
      }
    }

    public async Task SaveAsync()
    {
      if (!_hasChanges) return;
      await _project.SaveAsync();
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

    bool TryUpdateGame(bool value, params TargetGame[] gameList)
    {
      if (_project.Games is List<TargetGame> games)
      {
        if (value)
          games.AddRange(gameList);
        else
          games.RemoveAll(game => gameList.Contains(game));
        return true;
      }
      return false;
    }
  }
}