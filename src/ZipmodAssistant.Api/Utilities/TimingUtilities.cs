using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZipmodAssistant.Api.Utilities
{
  public static class TimingUtilities
  {
    public static TimeSpan Time(Action action)
    {
      var startTime = DateTime.Now;
      action.Invoke();
      return DateTime.Now - startTime;
    }

    public static async Task<TimeSpan> TimeAsync(Func<Task> task)
    {
      var startTime = DateTime.Now;
      await task.Invoke();
      return DateTime.Now - startTime;
    }
  }
}
