using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ZipmodAssistant.App.Models
{
  public class DirectoryConfigurationModel : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler? PropertyChanged;

    private string _inputDirectory = "";
    private string _outputDirectory = ".\\build";
    private string _cacheDirectory = ".\\cache";

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

    public void OnPropertyChanged([CallerMemberName] string name = "")
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
  }
}
