﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZipmodAssistant.Api.Enums;

namespace ZipmodAssistant.Api.Interfaces.Models
{
  public interface IRepositoryItem
  {
    RepositoryItemType ItemType { get; }
    FileInfo FileInfo { get; }
    byte[] Hash { get; }

    Task<bool> ProcessAsync(IBuildConfiguration buildConfiguration, IBuildRepository repository);
  }
}
