using HarmonyLib;
using Mirror;
using Mirror.LiteNetLib4Mirror;
using PlayerStatsSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanyaRemastered.Patches
{
    [HarmonyPatch(typeof(CustomNetworkManager), nameof(CustomNetworkManager.OnServerDisconnect))]
    public static class ServerDisconnectPatches
    {
        public static void Prefix(CustomNetworkManager __instance, NetworkConnection conn)
        {
			if (__instance._disconnectDrop)
			{
				NetworkIdentity identity = conn.identity;
				ReferenceHub referenceHub;
				if (identity != null && ReferenceHub.TryGetHubNetID(identity.netId, out referenceHub))
				{
					referenceHub.playerStats.DealDamage(new CustomReasonDamageHandler("Disconect",-1,"SUCCESSFULLY TERMINATED . TERMINATION CAUSE UNSPECIFIED"));
				}
			}
		}
    }
}