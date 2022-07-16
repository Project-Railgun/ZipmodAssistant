using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Wpf.Ui.Mvvm.Contracts;

namespace ZipmodAssistant.App.Services
{
  public class PageService : IPageService
  {
    private readonly IServiceProvider _serviceProvider;

    public PageService(IServiceProvider serviceProvider)
    {
      _serviceProvider = serviceProvider;
    }

    public T? GetPage<T>() where T : class => _serviceProvider.GetService<T>();

    public FrameworkElement? GetPage(Type pageType) => _serviceProvider.GetService(pageType) as FrameworkElement;
  }
}
