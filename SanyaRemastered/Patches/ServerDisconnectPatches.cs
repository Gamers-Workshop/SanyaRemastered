using Exiled.API.Features;
using HarmonyLib;
using Mirror;
using PlayerStatsSystem;
using System;

namespace SanyaRemastered.Patches
{
    [HarmonyPatch(typeof(CustomNetworkManager), nameof(CustomNetworkManager.OnServerDisconnect))]
    public static class ServerDisconnectPatches
    {
        public static void Prefix(CustomNetworkManager __instance, NetworkConnection conn)
        {
            try
            {
                if (__instance._disconnectDrop)
                {
                    NetworkIdentity identity = conn.identity;
                    if (identity is not null && ReferenceHub.TryGetHubNetID(identity.netId, out ReferenceHub referenceHub))
                    {
                        referenceHub.playerStats.DealDamage(new CustomReasonDamageHandler("Disconect"));
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return;
            }
		}
    }
}