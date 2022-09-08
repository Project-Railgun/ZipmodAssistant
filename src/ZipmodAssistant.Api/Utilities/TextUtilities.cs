using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ZipmodAssistant.Api.Enums;
using ZipmodAssistant.Api.Interfaces.Models;
using ZipmodAssistant.Shared.Enums;

namespace ZipmodAssistant.Api.Utilities
{
  internal static class TextUtilities
  {
    static readonly Regex _specialCharsRegex = new Regex(@"[!@#$%^&*\(\)=+\[\]{}\\\|,<.>/?;:'""]");

    private static readonly Dictionary<TargetGame, string[]> _gameAliases = new()
    {
      { TargetGame.Koikatu, new [] { "koikatsu", "kk" } },
      { TargetGame.KoikatsuSunshine, new [] { "koikatsusunshine", "kks" } },
      { TargetGame.EmotionCreators, new [] { "emotioncreators, ec" } },
      { TargetGame.AiSyoujyo, new [] { "ai girl", "ais" } },
      { TargetGame.HoneySelect2, new [] { "honeyselect2", "hs2" } },
    };

    public static bool TryParseGame(string? name, out TargetGame game)
    {
      if (!string.IsNullOrEmpty(name))
      {
        var formattedName = name.ToLower();
        foreach (var gameAlias in _gameAliases)
        {
          if (gameAlias.Value.Contains(formattedName))
          {
            game = gameAlias.Key;
            return true;
          }
        }
      }
      game = TargetGame.Unknown;
      return false;
    }

    public static string ResolveFilenameFromManifest(IManifest manifest)
    {
      var nameToUse = _specialCharsRegex.Replace(manifest.Name ?? manifest.Guid, "");
      return $"[{manifest.Author}] {nameToUse} {(manifest.Version.StartsWith('v') ? "" : 'v')}{manifest.Version}.zipmod";
    }
  }
}
