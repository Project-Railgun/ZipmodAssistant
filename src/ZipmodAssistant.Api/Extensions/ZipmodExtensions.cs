using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZipmodAssistant.Api.Enums;
using ZipmodAssistant.Api.Interfaces.Models;

namespace ZipmodAssistant.Api.Extensions
{
  internal static class ZipmodExtensions
  {
    public static string GetPath(this IZipmod zipmod, string path) => Path.Join(zipmod.WorkingDirectory, path);

    public static ZipmodType GetZipmodType(this IZipmod zipmod)
    {
      if (Directory.Exists(zipmod.GetPath("abdata\\list\\characustom")))
        return ZipmodType.Game;
      if (Directory.Exists(zipmod.GetPath("abdata\\studio\\info")) &&
        Directory.GetFiles(zipmod.GetPath("abdata\\studio\\info")).Any(f => new FileInfo(f).Name.StartsWith("ItemList_")))
        return ZipmodType.Studio;
      if (Directory.Exists(zipmod.GetPath("abdata\\map\\list\\mapinfo")))
        return ZipmodType.MapGame;
      if (Directory.Exists(zipmod.GetPath("abdata\\studio\\info\\kPlug")) &&
        Directory.GetFiles(zipmod.GetPath("abdata\\studio\\info\\kPlug")).Any(f => new FileInfo(f).Name.StartsWith("Map_")))
        return ZipmodType.MapStudio;
      return ZipmodType.Unknown;
    }
  }
}
