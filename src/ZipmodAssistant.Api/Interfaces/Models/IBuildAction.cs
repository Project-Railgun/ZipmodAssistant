namespace ZipmodAssistant.Api.Interfaces.Models
{
  public interface IBuildAction
  {
    IRepositoryItem Target { get; set; }
    DateTime StartedAt { get; set; }
    DateTime FinishedAt { get; set; }
    string Description { get; set; }
  }
}
