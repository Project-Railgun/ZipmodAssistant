using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZipmodAssistant.Api.Enums;
using ZipmodAssistant.Api.Interfaces.Models;

namespace ZipmodAssistant.Api.Models
{
  public class BuildRepository : IBuildRepository
  {
    private readonly List<IRepositoryItem> _repositoryItems = new();

    public IZipmodConfiguration Configuration { get; set; }
    public string RootDirectory { get; set; }
    public string FileLocation { get; set; }
    public RepositoryItemType ItemType => RepositoryItemType.Repository;
    public byte[] Hash => Array.Empty<byte>();

    public BuildRepository(string rootDirectory, IZipmodConfiguration zipmodConfiguration)
    {
      RootDirectory = rootDirectory;
      FileLocation = string.Empty;
      Configuration = zipmodConfiguration;
    }

    public BuildRepository(string fileLocation)
    {
      FileLocation = fileLocation;
    }

    public async Task<bool> ProcessAsync(IBuildConfiguration buildConfiguration, IBuildRepository repository)
    {
      // this will only get called if the repository is a zipmod/zip
      return true;
    }

    public IEnumerator<IRepositoryItem> GetEnumerator() => _repositoryItems.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _repositoryItems.GetEnumerator();
  }
}
