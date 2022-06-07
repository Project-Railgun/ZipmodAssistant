using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZipmodAssistant.Api.Data.DataModels
{
  public class ManifestHistoryEntry
  {
    [Key]
    public string Guid { get; set; }
    public byte[] Hash { get; set; }
    public bool IsBlackListed { get; set; }
    public string Version { get; set; }
  }
}
