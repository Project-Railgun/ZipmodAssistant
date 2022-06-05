using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ZipmodAssistant.App.Extensions
{
  public static class PageExtensions
  {
    public static T GetService<T>(this Page page)
    {
      var serviceProvider = DependencyInjectionSource.ServiceProvider;
      return serviceProvider.GetService<T>();
    }
  }
}
