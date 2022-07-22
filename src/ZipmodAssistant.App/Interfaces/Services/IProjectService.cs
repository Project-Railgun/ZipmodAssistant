using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZipmodAssistant.App.Interfaces.Models;

namespace ZipmodAssistant.App.Interfaces.Services
{
  public interface IProjectService
  {
    Task<IProjectConfiguration> CreateNewProjectAsync();
    Task<IProjectConfiguration> CreateNewProjectAsync(string directory, string name);
    IProjectConfiguration? GetCurrentProject();
    Task<IProjectConfiguration> LoadProjectAsync(string projectFilename);
    Task<bool> SaveProjectAsync(IProjectConfiguration configuration, string? directory = null, string? name = null);
  }
}
