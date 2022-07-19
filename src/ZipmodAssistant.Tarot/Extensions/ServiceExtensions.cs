using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZipmodAssistant.Tarot.Interfaces.Providers;
using ZipmodAssistant.Tarot.Providers;

namespace ZipmodAssistant.Tarot.Extensions
{
  public static class ServiceExtensions
  {
    public static IServiceCollection AddTarot(this IServiceCollection services) =>
      services.AddScoped<ICardProvider, CardProvider>();
  }
}
