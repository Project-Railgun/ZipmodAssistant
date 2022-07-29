using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZipmodAssistant.Api.Data.DataModels
{
  public class PriorZipmodEntry
  {
    [Key]
    public string Hash { get; set; }
    public string Guid { get; set; }
    public bool CanSkip { get; set; }
    public string Version { get; set; }
  }
}
