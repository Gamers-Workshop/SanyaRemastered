using CustomPlayerEffects;
using Exiled.API.Features;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanyaRemastered.Patches
{
    [HarmonyPatch(typeof(PocketDimensionGenerator), nameof(PocketDimensionGenerator.RandomizeTeleports))]
    public static class PocketDimensionFix
    {
        public static bool Prefix()
        {
            return !Player.List.Any(x => x.IsInPocketDimension && x.IsEffectActive<Corroding>());
		}
    }
}
