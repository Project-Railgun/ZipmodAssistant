using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ZipmodAssistant.App.ViewModels
{
  public class HomeViewModel : ViewModel
  {
    private bool _isProjectNameInputVisible = false;
    private string _projectName = string.Empty;

    public bool IsProjectNameInputVisible
    {
      get => _isProjectNameInputVisible;
      set
      {
        _isProjectNameInputVisible = value;
        OnPropertyChanged();
      }
    }

    public string ProjectName
    {
      get => _projectName;
      set
      {
        _projectName = value;
        OnPropertyChanged();
      }
    }
  }
}
