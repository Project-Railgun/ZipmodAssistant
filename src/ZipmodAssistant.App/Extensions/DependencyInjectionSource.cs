using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace ZipmodAssistant.App.Extensions
{
  public class DependencyInjectionSource : MarkupExtension
  {
    public static IServiceProvider ServiceProvider { get; set; }

    public Type Type { get; set; }

    public override object ProvideValue(IServiceProvider serviceProvider) => ServiceProvider?.GetService(Type);
  }
}
