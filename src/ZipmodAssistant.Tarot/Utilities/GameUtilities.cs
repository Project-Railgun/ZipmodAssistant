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
  }
}
