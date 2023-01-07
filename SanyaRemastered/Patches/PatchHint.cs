using Exiled.API.Features;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanyaRemastered.Patches
{
    [HarmonyPatch(typeof(Player), nameof(Player.ShowHint), new[] { typeof(string), typeof(float) })]
    public static class PatchHint
    {
        public static bool Prefix(Player __instance, string message, float duration = 3)
        {
            if (SanyaRemastered.Instance.Config.ExHudEnabled && __instance.GameObject.TryGetComponent(out SanyaRemasteredComponent sanyaRemastered))
            {
                sanyaRemastered.AddHudCenterDownText(message, (ulong)duration);
                return false;
            }
            return true;
        }
    }
}
