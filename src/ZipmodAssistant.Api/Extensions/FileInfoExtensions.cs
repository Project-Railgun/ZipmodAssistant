﻿namespace ZipmodAssistant.Api.Extensions
{
  public static class FileInfoExtensions
  {
    public static string NameWithoutExtension(this FileInfo fileInfo) =>
      fileInfo.Name[..fileInfo.Name.LastIndexOf('.')];

    public static void CopyTo(this DirectoryInfo directoryInfo, string destination) =>
      CopyTo(directoryInfo, destination, false);

    public static void CopyTo(this DirectoryInfo directoryInfo, string destination, bool overwrite)
    {
      foreach (var directory in directoryInfo.GetDirectories("*", SearchOption.AllDirectories))
      {
        Directory.CreateDirectory(Path.Join(destination, directory.FullName.Replace(directoryInfo.FullName, null)));
      }
      foreach (var file in directoryInfo.GetFiles("*.*", SearchOption.AllDirectories))
      {
        file.CopyTo(Path.Join(destination, file.FullName.Replace(directoryInfo.FullName, null)), overwrite);
      }
    }

    public static void MoveToSafely(this FileInfo fileInfo, string destination) =>
      MoveToSafely(fileInfo, destination, false);

    public static void MoveToSafely(this FileInfo fileInfo, string destination, bool overwrite)
    {
      var target = new FileInfo(destination);
      target.Directory.Create();
      fileInfo.MoveTo(destination, overwrite);
    }

    public static void CopyTo(this FileInfo fileInfo, IEnumerable<string> directories) =>
      CopyTo(fileInfo, directories, false);

    public static void CopyTo(this FileInfo fileInfo, IEnumerable<string> directories, bool overwrite)
    {
      foreach (var directory in directories)
      {
        if (!Directory.Exists(directory))
        {
          Directory.CreateDirectory(directory);
        }
        fileInfo.CopyTo(Path.Join(directory, fileInfo.Name), overwrite);
      }
    }
  }
}
