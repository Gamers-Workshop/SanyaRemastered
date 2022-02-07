using HarmonyLib;
using Mirror;
using PlayableScps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SanyaRemastered.Patches
{
    [HarmonyPatch(typeof(Scp173), nameof(Scp173.UpdateBlink))]
    public static class Scp173Patches
    {
        public static void Postfix(Scp173 __instance)
        {
            if (SanyaRemastered.Instance.Config.Scp173Real)
            {
                __instance.BreakneckSpeedsActive = !__instance._wasSeen;
				__instance._breakneckSpeedsCooldownRemaining = 0;
			}
        }
    }
    [HarmonyPatch(typeof(Scp173), nameof(Scp173.ServerDoSnap))]
    public static class Scp173Patches2
    {
        public static bool Prefix(Scp173 __instance,NetworkConnection conn, PlayableScps.Messages.Scp173SnapMessage msg)
        {
            if (SanyaRemastered.Instance.Config.Scp173Real)
            {
				if (!ReferenceHub.TryGetHubNetID(conn.identity.netId, out ReferenceHub referenceHub))
				{
					return false;
				}
				if (!Scp173.TryGet173FromHub(referenceHub, out Scp173 scp))
				{
					return false;
				}
				if (msg.TargetHub == null || msg.TargetHub.characterClassManager.IsAnyScp())
				{
					return false;
				}
				if (Vector3.Distance(referenceHub.playerMovementSync.RealModelPosition, msg.TargetHub.playerMovementSync.RealModelPosition) > 3f)
				{
					return false;
				}
				scp.ServerKillPlayer(msg.TargetHub);
				return false;
			}
			return true;
		}
	}
}
