using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZipmodAssistant.Api.Interfaces.Services;
using ZipmodAssistant.App.Interfaces.Models;
using ZipmodAssistant.App.Interfaces.Services;
using ZipmodAssistant.App.Models;

namespace ZipmodAssistant.App.Services
{
  public class ProjectService : IProjectService
  {
    public const string ProjectExtension = "zaproj";

    private readonly ILogger<IProjectService> _logger;
    private IProjectConfiguration? _currentProject;

    public ProjectService(ILogger<IProjectService> logger)
    {
      _logger = logger;
    }

    public async Task<IProjectConfiguration> CreateNewProjectAsync()
    {
      await Task.CompletedTask;
      _currentProject = new ProjectConfiguration();
      return _currentProject;
    }

    public async Task<IProjectConfiguration> CreateNewProjectAsync(string directory, string name)
    {
      if (!Directory.Exists(directory))
      {
        throw new ArgumentException("Directory not found", nameof(directory));
      }

      try
      {
        _currentProject = new ProjectConfiguration
        {
          Filename = Path.Join(directory, $"{name}.{ProjectExtension}"),
        };
        await _currentProject.SaveAsync();
        return _currentProject;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "An error occured when creating the project {name}", name);
        throw;
      }
    }

    public IProjectConfiguration? GetCurrentProject() => _currentProject;

    public async Task<IProjectConfiguration> LoadProjectAsync(string projectFilename)
    {
      try
      {
        _currentProject = await ProjectConfiguration.LoadAsync(projectFilename);
        return _currentProject;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "An error occured when loading {filename}", projectFilename);
        throw;
      }
    }

    public async Task<bool> SaveProjectAsync(IProjectConfiguration configuration, string? directory = null, string? name = null)
    {
      if (configuration.Filename == null && directory == null)
      {
        throw new ArgumentNullException(nameof(directory));
      }

      if (directory != null && name == null)
      {
        throw new ArgumentNullException(nameof(name));
      }

      try
      {
        var saveLocation = configuration.Filename ?? $"{Path.Join(directory, name)}.{ProjectExtension}";
        configuration.Filename = saveLocation;
        await configuration.SaveAsync();
        return true;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "An error occured when saving {filename}", configuration.Filename);
        return false;
      }
    }
  }
}
