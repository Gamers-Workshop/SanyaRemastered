using Exiled.API.Features;
using HarmonyLib;
using UnityEngine;

namespace SanyaRemastered.Patches
{
    [HarmonyPatch(typeof(RagdollManager), nameof(RagdollManager.SpawnRagdoll))]
    public static class PreventRagdollPatch
    {
        public static bool Prefix(RagdollManager __instance, Vector3 pos, Quaternion rot, Vector3 velocity, int classId, PlayerStats.HitInfo ragdollInfo, ref bool allowRecall, string ownerID, ref string ownerNick, int playerId)
        {
            if (SanyaRemastered.Instance.Config.Scp106RemoveRagdoll && ragdollInfo.GetDamageType() == DamageTypes.Scp106
                || SanyaRemastered.Instance.Config.Scp096RemoveRagdoll && ragdollInfo.GetDamageType() == DamageTypes.Scp096) return false;

            if (SanyaRemastered.Instance.Config.Scp049Real && ragdollInfo.GetDamageType() != DamageTypes.Scp049) allowRecall = false;

            if (SanyaRemastered.Instance.Config.TeslaDestroyName && ragdollInfo.GetDamageType() == DamageTypes.Tesla) ownerNick = "inconue";
            return true;
        }
    }
}