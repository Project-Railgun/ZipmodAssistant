using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZipmodAssistant.Api.Interfaces.Models
{
  public interface IBuildRepository : IEnumerable<IRepositoryItem>, IRepositoryItem
  {
    IZipmodConfiguration Configuration { get; set; }
    string RootDirectory { get; set; }
  }
}
