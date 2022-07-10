using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZipmodAssistant.Api.Enums;
using ZipmodAssistant.Api.Extensions;
using ZipmodAssistant.Api.Interfaces.Models;
using ZipmodAssistant.Api.Interfaces.Services;

namespace ZipmodAssistant.Api.Models
{
  public class Zipmod : IZipmod
  {
    public string WorkingDirectory { get; private set; }

    public FileInfo FileInfo { get; }

    public IManifest Manifest { get; }

    public string Hash { get; }

    public Zipmod(FileInfo fileInfo, string workingDirectory, Manifest manifest)
    {
      FileInfo = fileInfo;
      WorkingDirectory = workingDirectory;
      Manifest = manifest;
      Hash = manifest.Hash;
    }
  }
}
