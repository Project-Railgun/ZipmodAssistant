using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZipmodAssistant.Api.Interfaces.Models;

namespace ZipmodAssistant.Api.Models
{
  public abstract class ProcessResult : IProcessResult
  {
    public IZipmod Target { get; }

    protected ProcessResult(IZipmod target)
    {
      Target = target;
    }
  }

  public class SuccessProcessResult : ProcessResult
  {
    public SuccessProcessResult(IZipmod target) : base(target)
    {

    }
  }

  public class NonCardImageResult : ProcessResult
  {
    public string Filename { get; }

    public NonCardImageResult(IZipmod target, string filename) : base(target)
    {
      Filename = filename;
    }
  }

  public class MalformedItemResult : ProcessResult
  {
    public string Reason { get; }

    public MalformedItemResult(IZipmod target, string reason) : base(target)
    {
      Reason = reason;
    }
  }

  public class VersionMismatchResult : ProcessResult
  {
    public string Guid { get; }
    public string OldVersion { get; }
    public string NewVersion { get; }

    public VersionMismatchResult(IZipmod target, string guid, string oldVersion, string newVersion) : base(target)
    {
      Guid = guid;
      OldVersion = oldVersion;
      NewVersion = newVersion;
    }
  }

  public class NoChangeProcessResult : ProcessResult
  {
    public NoChangeProcessResult(IZipmod target) : base(target) { }
  }
}
