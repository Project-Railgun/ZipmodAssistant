namespace ZipmodAssistant.Api.Extensions
{
  public static class FileInfoExtensions
  {
    public static string NameWithoutExtension(this FileInfo fileInfo) => fileInfo.Name[..fileInfo.Name.LastIndexOf('.')];
  }
}
