using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZipmodAssistant.Api.Enums;

namespace ZipmodAssistant.Api.Data.DataModels
{
  public class SessionResultEntry
  {
    [Key]
    public int Id { get; set; }
    public string ZipmodHash { get; set; }
    public string Filename { get; set; }
    public SessionResultType Type { get; set; }
    public DateTime CreatedAt { get; set; }
    public string SessionId { get; set; }
  }
}
