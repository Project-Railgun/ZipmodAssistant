using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZipmodAssistant.Tarot.Models
{
  public class AiCard : BaseCard
  {
    public AiCard(FileInfo fileLocation) : base(fileLocation)
    {
    }

    public override async Task LoadAsync(BinaryReader reader)
    {
      throw new NotImplementedException();
    }
  }
}
