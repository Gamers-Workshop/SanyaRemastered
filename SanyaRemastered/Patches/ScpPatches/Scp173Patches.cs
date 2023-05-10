using HarmonyLib;
using Mirror;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.PlayableScps.Scp173;
using RelativePositioning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Utils.Networking;

namespace SanyaRemastered.Patches
{/*
    [HarmonyPatch(typeof(Scp173BreakneckSpeedsAbility), nameof(Scp173BreakneckSpeedsAbility.UpdateServerside))]
    public static class Scp173Patches
    {
        public static void Postfix(Scp173BreakneckSpeedsAbility __instance)
        {
            if (SanyaRemastered.Instance.Config.Scp173Real)
            {
                __instance.IsActive = !__instance._observersTracker.IsObserved;
				__instance.Cooldown.NextUse = 0;
			}
        }
    }
    //[HarmonyPatch(typeof(Scp173SnapAbility), nameof(Scp173SnapAbility))]
    public static class Scp173Patches2
    {
        public static bool Prefix(Scp173SnapAbility __instance, NetworkReader reader)
        {
            if (SanyaRemastered.Instance.Config.Scp173Real)
            {
                __instance.ServerProcessCmd(reader);
                __instance._targetHub = reader.ReadReferenceHub();
                if (__instance._observersTracker.IsObserved)
                {
                    return false;
                }
                if (__instance._targetHub == null || __instance._targetHub.roleManager.CurrentRole is not IFpcRole fpcRole)
                {
                    return false;
                }
                FirstPersonMovementModule fpcModule = __instance.ScpRole.FpcModule;
                FirstPersonMovementModule fpcModule2 = fpcRole.FpcModule;
                Transform playerCameraReference = __instance.Owner.PlayerCameraReference;
                Vector3 position = fpcModule2.Position;
                Vector3 position2 = fpcModule.Position;
                Quaternion rotation = playerCameraReference.rotation;
                fpcModule2.Position = fpcModule2.Tracer.GenerateBounds(0.4f, true).ClosestPoint(reader.ReadRelativePosition().Position);
                Bounds bounds = fpcModule.Tracer.GenerateBounds(0.1f, true);
                bounds.Encapsulate(fpcModule.Position + fpcModule.Motor.Velocity * 0.2f);
                fpcModule.Position = bounds.ClosestPoint(reader.ReadRelativePosition().Position);
                playerCameraReference.rotation = reader.ReadLowPrecisionQuaternion().Value;
                if (Scp173SnapAbility.TryHitTarget(playerCameraReference, out ReferenceHub referenceHub) && referenceHub.playerStats.DealDamage(__instance.ScpRole.DamageHandler))
                {
                    Hitmarker.SendHitmarker(__instance.Owner, 1f);
                    if (__instance.ScpRole.SubroutineModule.TryGetSubroutine(out Scp173AudioPlayer scp173AudioPlayer))
                    {
                        scp173AudioPlayer.ServerSendSound(Scp173AudioPlayer.Scp173SoundId.Snap);
                    }
                }
                fpcModule2.Position = position;
                fpcModule.Position = position2;
                playerCameraReference.rotation = rotation; 
                return false;
			}
			return true;
		}
	}*/
}
