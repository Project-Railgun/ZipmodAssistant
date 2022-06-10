using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ZipmodAssistant.Api.Enums;
using ZipmodAssistant.Shared.Enums;

namespace ZipmodAssistant.App.Models
{
  public class BuildConfigurationModel : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler? PropertyChanged;

    private bool _randomizeCab = true;
    private bool _skipRenaming = false;
    private bool _skipCompression = false;
    private bool _skipJunkCleanup = false;
    private ObservableCollection<TargetGame> _gameTags = new();

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

    public bool SkipJunkCleanup
    {
      get => _skipJunkCleanup;
      set
      {
        _skipJunkCleanup = value;
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
      }
    }

    public BuildConfigurationModel()
    {
      _gameTags.CollectionChanged += OnGameTagsChanged;
    }

    void OnGameTagsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
      PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(nameof(GameTags)));
    }

    void OnPropertyChanged([CallerMemberName] string name = "")
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
  }
}
