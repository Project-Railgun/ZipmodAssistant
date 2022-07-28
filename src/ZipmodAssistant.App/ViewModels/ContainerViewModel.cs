using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ZipmodAssistant.App.Interfaces.Services;

namespace ZipmodAssistant.App.ViewModels
{
  public class ContainerViewModel : ViewModel
  {

    private readonly IProjectService _projectService;
    private bool _canSave = false;

    public bool CanSave
    {
      get => _canSave;
      set
      {
        _canSave = value;
        OnPropertyChanged();
      }
    }

    public ContainerViewModel(IProjectService projectService)
    {
      _projectService = projectService;
      _canSave = projectService.GetCurrentProject() != null;
    }

    public void Update()
    {
      _canSave = _projectService.GetCurrentProject() != null;
    }
  }
}
