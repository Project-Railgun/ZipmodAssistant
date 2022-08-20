using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ZipmodAssistant.App.Logging
{
  public class InMemorySink : ILogEventSink
  {
    private const int MAX_QUEUE_LENGTH = 200;
    private readonly IFormatProvider? _formatProvider;

    private static readonly ConcurrentQueue<string> _events = new();
    private static readonly ConcurrentBag<Action<IEnumerable<string>>> _subscriptions = new();

    public InMemorySink(IFormatProvider? formatProvider)
    {
      _formatProvider = formatProvider;
    }

    public void Emit(LogEvent logEvent)
    {
      var message = logEvent.RenderMessage(_formatProvider);
      AddEventToQueue(message);
    }

    static void AddEventToQueue(string message)
    {
      if (_events.Count == MAX_QUEUE_LENGTH)
      {
        _events.TryDequeue(out var _);
      }
      if (_subscriptions.IsEmpty)
      {
        _events.Enqueue(message);
      }
      else
      {
        Parallel.ForEach(_subscriptions, (subscriber) =>
        {
          subscriber(new string[] { message });
        });
      }
    }

    public static IEnumerable<string> Subscribe(Action<IEnumerable<string>> onEmit)
    {
      var currentItems = _events.ToArray();
      _events.Clear();
      _subscriptions.Add(onEmit);
      return currentItems;
    }
  }
}
