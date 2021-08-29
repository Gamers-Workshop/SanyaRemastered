using Exiled.API.Features;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SanyaRemastered.Patches
{
	[HarmonyPatch(typeof(FallDamage), nameof(FallDamage.OnTouchdown))]
	public static class AddFallDammageOnSCP
	{
		public static bool Prefix(FallDamage __instance)
		{
            try
            {
				if (SanyaRemastered.Instance.Config.ScpFallDamage.Contains(__instance._ccm.CurRole.roleId.ToString()) && SanyaRemastered.Instance.Config.ScpTakeFallDamage && __instance._ccm.CurRole.team == Team.SCP)
				{
					if (__instance._footstepSync != null)
					{
						__instance._footstepSync.RpcPlayLandingFootstep(true);
					}
					float num = __instance.damageOverDistance.Evaluate(__instance.PreviousHeight - __instance._ccm.transform.position.y);
					if (num <= 5f || __instance._ccm.NoclipEnabled || __instance._ccm.GodMode || __instance._pms.InSafeTime)
					{
						return false;
					}

					{
						if (__instance._hub.playerStats.Health - num <= 0f)
						{
							__instance.TargetAchieve(__instance._ccm.connectionToClient);
						}
						Vector3 position = __instance._ccm.transform.position;
						__instance.RpcDoSound();
						__instance._ccm.RpcPlaceBlood(position, 0, Mathf.Clamp(num / 30f, 0.8f, 2f));
						__instance._hub.playerStats.HurtPlayer(new PlayerStats.HitInfo(Mathf.Abs(num), "WORLD", DamageTypes.Falldown, 0,false), __instance._ccm.gameObject, true, true);
					}
				}
			}
			catch(System.Exception ex)
            {
				Log.Error("FallDamage" + ex);
            }
			return true;
		}
	}
}
