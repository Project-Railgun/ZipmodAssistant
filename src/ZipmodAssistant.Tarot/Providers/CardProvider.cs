﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZipmodAssistant.Shared.Enums;
using ZipmodAssistant.Tarot.Interfaces.Models;
using ZipmodAssistant.Tarot.Interfaces.Providers;
using ZipmodAssistant.Tarot.Models;
using ZipmodAssistant.Tarot.Utilities;

namespace ZipmodAssistant.Tarot.Providers
{
  public class CardProvider : ICardProvider
  {
    private readonly Dictionary<string, ICard> _cardCache = new();

    public Task<ICard?> TryReadCardAsync(string filename) => TryReadCardAsync(new FileInfo(filename));

    public async Task<ICard?> TryReadCardAsync(FileInfo fileInfo)
    {
      if (_cardCache.ContainsKey(fileInfo.FullName))
      {
        return _cardCache[fileInfo.FullName];
      }
      try
      {
        using var stream = fileInfo.OpenRead();
        var imageStream = new MemoryStream();
        var dataStream = new MemoryStream();
        // TODO: compress everything up until IEND
        await CardUtilities.ReadDataToBuffersAsync(stream, imageStream, dataStream);
        // DO NOT DISPOSE OF HERE
        var additionalContentReader = new BinaryReader(dataStream);

        // product number. TODO: use somewhere?
        additionalContentReader.ReadInt32();
        var targetGame = GameUtilities.GetTargetGameFromMarker(additionalContentReader.ReadString());

        ICard card = targetGame switch
        {
          TargetGame.Koikatu => new KoikatsuCard(fileInfo),
          TargetGame.KoikatsuParty => new KoikatsuCard(fileInfo),
          TargetGame.KoikatsuPartySpecialPatch => new KoikatsuCard(fileInfo),
          TargetGame.KoikatsuSunshine => new KoikatsuSunshineCard(fileInfo),
          TargetGame.EmotionCreators => new EmotionCard(fileInfo),
          TargetGame.AiSyoujyo => new AiCard(fileInfo),
          _ => throw new ArgumentOutOfRangeException($"Game type {targetGame} not supported"),
        };
        await card.LoadAsync(additionalContentReader);
        _cardCache[fileInfo.FullName] = card;
        return card;
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex);
        return null;
      }
    }

    public async Task<FileInfo?> TryWriteCardAsync(string fileLocation, ICard card)
    {
      throw new NotImplementedException();
    }
  }
}
