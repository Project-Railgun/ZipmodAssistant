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
    public event PropertyChangedEventHandler? PropertyChanged;
    public BuildConfigurationModel BuildConfiguration { get; set; } = new();
    public DirectoryConfigurationModel DirectoryConfiguration { get; set; } = new();
    public ObservableCollection<string> LogMessages { get; set; } = new();

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
  }
}
