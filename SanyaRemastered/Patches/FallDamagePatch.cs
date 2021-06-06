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
        public static void Prefix(FallDamage __instance)
        {
            if (__instance._ccm.CurRole.roleId == (RoleType.Scp049 | RoleType.Scp0492 | RoleType.Scp93989 | RoleType.Scp93953) && SanyaRemastered.Instance.Config.ScpTakeFallDamage)
            {
                if (__instance._footstepSync != null)
                {
                    __instance._footstepSync.RpcPlayLandingFootstep(true);
                }
                float num = __instance.damageOverDistance.Evaluate(__instance.PreviousHeight - __instance.transform.position.y);
                if (num <= 5f || __instance._ccm.NoclipEnabled || __instance._ccm.GodMode || __instance._pms.InSafeTime)
                {
                    return;
                }
                __instance.RpcDoSound();
                __instance._ccm.RpcPlaceBlood(__instance.transform.position, 0, Mathf.Clamp(num / 30f, 0.8f, 2f));
                __instance._hub.playerStats.HurtPlayer(new PlayerStats.HitInfo(Mathf.Abs(num), "WORLD", DamageTypes.Falldown, 0), __instance.gameObject, true, true);
            }
        }
    }
}
