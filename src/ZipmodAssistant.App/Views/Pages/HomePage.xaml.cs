using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wpf.Ui.Mvvm.Contracts;
using ZipmodAssistant.App.Interfaces.Services;
using ZipmodAssistant.App.Services;
using ZipmodAssistant.App.ViewModels;

namespace ZipmodAssistant.App.Views.Pages
{
  /// <summary>
  /// Interaction logic for Home.xaml
  /// </summary>
  public partial class HomePage : Page
  {
    private readonly INavigationService _navigationService;
    private readonly IProjectService _projectService;
    private readonly FolderBrowserDialog _folderBrowserDialog = new()
    {
      ShowNewFolderButton = true,
    };
    private readonly OpenFileDialog _fileBrowserDialog = new()
    {
      Filter = $"ZipmodAssistant Project Files (*.{ProjectService.ProjectExtension})|*.{ProjectService.ProjectExtension}",
      Multiselect = false,
    };

    HomeViewModel ViewModel => (HomeViewModel)DataContext;

    public HomePage(HomeViewModel viewModel, INavigationService navigationService, IProjectService projectService)
    {
      DataContext = viewModel;
      _navigationService = navigationService;
      _projectService = projectService;
      InitializeComponent();
    }

    async void CreateNewProjectClicked(object sender, RoutedEventArgs e)
    {
      if (ViewModel.IsProjectNameInputVisible)
      {
        switch (_folderBrowserDialog.ShowDialog())
        {
          case DialogResult.OK:
            await _projectService.CreateNewProjectAsync(_folderBrowserDialog.SelectedPath, ViewModel.ProjectName);
            _navigationService.Navigate(typeof(ProjectPage));
            break;
          default:
            return;
        }
      }
      else
      {
        ViewModel.IsProjectNameInputVisible = true;
      }
    }

    async void OpenExistingProjectClicked(object sender, RoutedEventArgs e)
    {
      switch (_fileBrowserDialog.ShowDialog())
      {
        case DialogResult.OK:
          await _projectService.LoadProjectAsync(_fileBrowserDialog.FileName);
          _navigationService.Navigate(typeof(ProjectPage));
          break;
        default:
          return;
      }
    }
  }
}
