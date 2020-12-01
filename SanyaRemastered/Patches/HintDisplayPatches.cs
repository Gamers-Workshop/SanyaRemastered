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
				if (!SanyaRemastered.Instance.Config.ExHudEnabled) return ;

				if (hint.GetType() == typeof(TranslationHint))
					return ;

				if (hint._effects != null && hint._effects.Length > 0)
					return ;

				return ;
			}
		}
	}
}
