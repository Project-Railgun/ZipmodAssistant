using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZipmodAssistant.Api.Data.DataModels
{
  public class SessionResultEntry
  {
    [Key]
    public int Id { get; set; }
    public string Filename { get; set; }
    public DateTime CreatedAt { get; set; }
  }
}
