using Exiled.API.Features;
using HarmonyLib;
using Mirror;
using PlayerStatsSystem;
using System;

namespace SanyaRemastered.Patches
{
    [HarmonyPatch(typeof(RoundSummary), nameof(RoundSummary.RoundInProgress))]
    public static class RoundInProgressFix
    {
        public static bool Prefix(RoundSummary __instance, ref bool __result)
        {
            try
            {
                __result = ReferenceHub.TryGetHostHub(out ReferenceHub hub) && hub.characterClassManager.RoundStarted && !RoundSummary.singleton._roundEnded;
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
            return false;
        }
    }
}