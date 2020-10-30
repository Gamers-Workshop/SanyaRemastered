using Exiled.API.Features;
using HarmonyLib;
using Hints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanyaRemastered.Patches
{
    class HintDisplayPatches
    {
		[HarmonyPatch(typeof(HintDisplay), nameof(HintDisplay.Show))]
		public static class HintPreventPatch
		{
			public static void Prefix(HintDisplay __instance, Hint hint)
			{
				if (hint.GetType() == typeof(TranslationHint))
				{
					Log.Debug($"[HintPreventPatch] TranslationHint Detect:{Player.Get(__instance.gameObject).Nickname}", SanyaPlugin.SanyaPlugin.Instance.Config.IsDebugged);
					return;
				}
				if (hint._effects != null && hint._effects.Length > 0)
				{
					Log.Debug($"[HintPreventPatch] HintEffects Detect:{Player.Get(__instance.gameObject).Nickname}", SanyaPlugin.SanyaPlugin.Instance.Config.IsDebugged);
					return;
				}
				Log.Debug($"[HintPreventPatch] Allow:{Player.Get(__instance.gameObject).Nickname}", SanyaPlugin.SanyaPlugin.Instance.Config.IsDebugged);
				return;
			}
		}
	}
}
