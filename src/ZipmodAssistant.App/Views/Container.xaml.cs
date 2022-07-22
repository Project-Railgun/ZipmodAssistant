using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls.Interfaces;
using Wpf.Ui.Mvvm.Contracts;
using ZipmodAssistant.App.Commands;
using ZipmodAssistant.App.Interfaces.Services;
using ZipmodAssistant.App.ViewModels;
using ZipmodAssistant.App.Views.Pages;

namespace ZipmodAssistant.App.Views
{
  /// <summary>
  /// Interaction logic for Container.xaml
  /// </summary>
  public partial class Container : INavigationWindow
  {
    public static OpenProjectCommand OpenProjectCommandBinding = new();

    private readonly IPageService _pageService;
    private readonly IThemeService _themeService;
    private readonly ITaskBarService _taskBarService;
    private readonly INavigationService _navigationService;
    private readonly IProjectService _projectService;

    ContainerViewModel ViewModel => (ContainerViewModel)DataContext;

    public Container(
      IPageService pageService,
      IThemeService themeService,
      ITaskBarService taskBarService,
      INavigationService navigationService,
      IProjectService projectService,
      ContainerViewModel viewModel)
    {
      DataContext = viewModel;
      _pageService = pageService;
      _themeService = themeService;
      _taskBarService = taskBarService;
      _navigationService = navigationService;
      InitializeComponent();
      _navigationService.SetNavigationControl(Navigation);
      SetPageService(_pageService);
      _themeService.SetTheme(ThemeType.Dark);
      Loaded += (_, _) => OnLoad();
      _projectService = projectService;
    }

    void ExitClicked(object sender, RoutedEventArgs e)
    {
      base.OnClosed(e);
      Application.Current.Shutdown();
    }

    public Frame GetFrame() => RootFrame;

    public INavigation GetNavigation() => Navigation;

    public bool Navigate<T>() => Navigate(typeof(T));

    public bool Navigate(Type pageType) => Navigation.Navigate(pageType);

    public void SetPageService(IPageService pageService) => Navigation.PageService = pageService;

    public void ShowWindow() => Show();

    public void CloseWindow() => Close();

    void OnLoad()
    {
      Dispatcher.Invoke(() =>
      {
        Navigate<HomePage>();

        _taskBarService.SetState(this, Wpf.Ui.TaskBar.TaskBarProgressState.None);
      });
    }

    async void HandleNewProjectMenuClicked(object sender, EventArgs e)
    {
      await _projectService.CreateNewProjectAsync();
      Navigate<ProjectPage>();
    }

    async void HandleOpenProjectMenuClicked(object sender, EventArgs e)
    {
      
    }
  }
}
