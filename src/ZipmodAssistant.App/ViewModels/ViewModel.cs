using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ZipmodAssistant.App.ViewModels
{
  public abstract class ViewModel : INotifyPropertyChanged
  {
    private ObservableCollection<string> _logMessages = BindCollectionLimit(new());

    public virtual ObservableCollection<string> LogMessages
    {
      get => _logMessages;
      set
      {
        _logMessages = BindCollectionLimit(value);
        OnPropertyChanged();
      }
    }
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName]string name = "") =>
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    protected static ObservableCollection<string> BindCollectionLimit(ObservableCollection<string> value)
    {
      
      return value;
    }
  }
}
