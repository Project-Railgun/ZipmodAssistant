using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ZipmodAssistant.Api.Enums;
using ZipmodAssistant.Api.Interfaces.Models;
using ZipmodAssistant.Shared.Enums;

namespace ZipmodAssistant.App.ViewModels
{
  public class HomeViewModel : INotifyPropertyChanged, IBuildConfiguration
  {
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
    private ObservableCollection<TargetGame> _gameTags = new();

    public event PropertyChangedEventHandler? PropertyChanged;

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

    public ObservableCollection<TargetGame> GameTags
    {
      get => _gameTags;
      set
      {
        _gameTags = value;
        OnPropertyChanged();
        value.CollectionChanged += (sender, args) => OnPropertyChanged();
      }
    }

    [JsonIgnore]
    public ObservableCollection<string> LogMessages { get; set; } = new();

    public HomeViewModel()
    {

    }

    public void ValidateDirectoryConfiguration()
    {
      if (!Directory.Exists(InputDirectory))
      {
        throw new DirectoryNotFoundException(InputDirectory);
      }
    }

    void OnPropertyChanged(object sender, PropertyChangedEventArgs args)
    {
      PropertyChanged?.Invoke(sender, args);
    }

    void OnPropertyChanged([CallerMemberName] string name = "")
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
  }
}
