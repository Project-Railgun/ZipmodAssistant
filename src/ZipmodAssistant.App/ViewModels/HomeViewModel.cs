using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ZipmodAssistant.Api.Enums;
using ZipmodAssistant.App.Models;

namespace ZipmodAssistant.App.ViewModels
{
  public class HomeViewModel : INotifyPropertyChanged
  {
    private bool _isBuilding = false;
    private int _buildProgress = 0;

    public event PropertyChangedEventHandler? PropertyChanged;
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
    public BuildConfigurationModel BuildConfiguration { get; set; } = new();
    public DirectoryConfigurationModel DirectoryConfiguration { get; set; } = new();
    public ObservableCollection<string> LogMessages { get; set; } = new()
    {
      "8:25:20 PM: Initiated logging: 220525.log",
      "8:25:20 PM: ZipmodHelper v2.0.0"
    };

    public HomeViewModel()
    {
      BuildConfiguration.PropertyChanged += OnPropertyChanged;
      DirectoryConfiguration.PropertyChanged += OnPropertyChanged;
    }

    public void ValidateDirectoryConfiguration()
    {
      if (!Directory.Exists(DirectoryConfiguration.InputDirectory))
      {
        throw new DirectoryNotFoundException(DirectoryConfiguration.InputDirectory);
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
