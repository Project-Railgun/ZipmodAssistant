using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZipmodAssistant.Api.Data.DataModels;
using ZipmodAssistant.Api.Interfaces.Repositories;

namespace ZipmodAssistant.App.ViewModels
{
  public class HistoryViewModel : ViewModel
  {
    private readonly IZipmodRepository _repository;

    private IEnumerable<PriorZipmodEntry> _zipmods = Array.Empty<PriorZipmodEntry>();
    private bool _loading = true;

    public IEnumerable<PriorZipmodEntry> Zipmods
    {
      get => _zipmods;
      set
      {
        _zipmods = value;
        OnPropertyChanged();
      }
    }
    public bool Loading
    {
      get => _loading;
      private set
      {
        _loading = value;
        OnPropertyChanged();
      }
    }

    public HistoryViewModel(IZipmodRepository zipmodRepository)
    {
      _repository = zipmodRepository;
    }

    public async Task LoadZipmodsAsync()
    {
      Loading = true;
      Zipmods = await _repository.GetZipmodsAsync();
      Loading = false;
    }

    public async Task SetCanSkipAsync(string zipmodGuid, bool canSkip)
    {
      var result = await _repository.SetCanSkipZipmodAsync(zipmodGuid, canSkip);
      if (!result)
      {
        throw new ArgumentException("Zipmod not found with that GUID", nameof(zipmodGuid));
      }
    }

    public async Task DeleteModFromHistoryAsync(string zipmodGuid)
    {
      var result = await _repository.RemoveZipmodAsync(zipmodGuid);
      if (!result)
      {
        throw new ArgumentException("Zipmod not found with that GUID", nameof(zipmodGuid));
      }
      Zipmods = Zipmods.Where(zipmod => zipmod.Guid != zipmodGuid);
    }

    public async Task DeleteAllAsync()
    {
      var guids = _zipmods.Select(zipmod => zipmod.Guid).ToArray();
      await _repository.RemoveZipmodsAsync(guids);
      Zipmods = Array.Empty<PriorZipmodEntry>();
    }
  }
}
