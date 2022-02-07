using HarmonyLib;
using NorthwoodLib.Pools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using static HarmonyLib.AccessTools;

namespace SanyaRemastered.Patches
{
    [HarmonyPatch(typeof(PlayableScps.Scp096), nameof(PlayableScps.Scp096.OnDamage))]
    public static class Scp096CancelEnrageByDamage
    {
        public static bool Prefix(PlayableScps.Scp096 __instance)
        {
            if (SanyaRemastered.Instance.Config.Scp096Real) return false;
            return true;
        }
    }
}
