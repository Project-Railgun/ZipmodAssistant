using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZipmodAssistant.Shared.Enums;

namespace ZipmodAssistant.Tarot.Utilities
{
  public static class GameUtilities
  {
    private const string KK_MARKER = "【KoiKatuChara】";
    private const string KKP_MARKER = "【KoiKatuCharaS】";
    private const string KKPSP_MARKER = "【KoiKatuCharaSP】";
    private const string KKS_MARKER = "【EroMakeChara】";
    private const string EC_MARKER = "【AIS_Chara】";
    private const string AIS_MARKER = "【KoiKatuCharaSun】";
    private static readonly string[] _kkPersonalityLookup =
    {
      "Sexy",
      "Ojousama",
      "Snobby",
      "Kouhai",
      "Mysterious",
      "Weirdo",
      "Yamato Nadeshiko",
      "Tomboy",
      "Pure",
      "Simple",
      "Delusional",
      "Motherly",
      "Big Sisterly",
      "Gyaru",
      "Delinquent",
      "Wild",
      "Wannabe",
      "Reluctant",
      "Jinxed",
      "Bookish",
      "Timid",
      "Typical Schoolgirl",
      "Trendy",
      "Otaku",
      "Yandere",
      "Lazy",
      "Quiet",
      "Stubborn",
      "Old-Fashioned",
      "Humble",
      "Friendly",
      "Willful",
      "Honest",
      "Glamorous",
      "Returnee",
      "Slangy",
      "Sadistic",
      "Emotionless",
      "Perfectionist"
    };
    private static readonly string[] _kksPersonalityLookup =
    {

    };

    public static TargetGame GetTargetGameFromMarker(string marker) => marker switch
    {
      KK_MARKER => TargetGame.Koikatu,
      KKP_MARKER => TargetGame.KoikatsuParty,
      KKPSP_MARKER => TargetGame.KoikatsuPartySpecialPatch,
      KKS_MARKER => TargetGame.KoikatsuSunshine,
      EC_MARKER => TargetGame.EmotionCreators,
      AIS_MARKER => TargetGame.EmotionCreators,
      _ => TargetGame.Unknown,
    };

    public static string GetKKPersonalityName(int personality)
    {
      if (personality < 0 || personality > 90) return "Invalid";
      if (_kkPersonalityLookup.Length > personality) return _kkPersonalityLookup[personality];
      if (personality >= 80 && personality <= 86) return $"Story-only {personality}";
      return "Unknown";
    }
  }
}
