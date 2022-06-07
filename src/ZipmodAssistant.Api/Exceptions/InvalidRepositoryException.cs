namespace ZipmodAssistant.Api.Exceptions
{
  public class InvalidRepositoryException : Exception
  {
    public InvalidRepositoryException(string location) : base($"Invalid repository: {location}") { }
    public InvalidRepositoryException(string location, Exception innerException) : base($"Invalid repository: {location}", innerException) { }
  }
}
