using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ZipmodAssistant.App.Commands
{
  public abstract class KeyboardShortcutCommand : RoutedCommand
  {
    protected KeyboardShortcutCommand()
    {

    }

    protected KeyboardShortcutCommand(Key key, ModifierKeys modKeys)
    {
      AddInputGesture(key, modKeys);
    }

    protected void AddInputGesture(Key key, ModifierKeys modKeys)
    {
      InputGestures.Add(new KeyGesture(key, modKeys));
    }

    public abstract void Execute();
  }
}
