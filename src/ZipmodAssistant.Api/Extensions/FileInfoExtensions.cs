namespace ZipmodAssistant.Api.Extensions
{
  public static class FileInfoExtensions
  {
    public static string NameWithoutExtension(this FileInfo fileInfo) =>
      fileInfo.Name[..fileInfo.Name.LastIndexOf('.')];

    public static void CopyTo(this DirectoryInfo directoryInfo, string destination) => CopyTo(directoryInfo, destination, false);

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
  }
}
