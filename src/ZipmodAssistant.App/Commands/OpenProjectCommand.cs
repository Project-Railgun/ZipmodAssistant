using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ZipmodAssistant.App.Commands
{
  
  public class OpenProjectCommand : KeyboardShortcutCommand
  {
    public OpenProjectCommand() : base(Key.O, ModifierKeys.Control)
    {

    }

    public override void Execute()
    {
      
    }
  }
}
