using Exiled.API.Features;
using HarmonyLib;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerStatsSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SanyaRemastered.Patches
{
	[HarmonyPatch(typeof(FpcMotor), nameof(FpcMotor.UpdateGrounded))]
	public static class AddFallDammageOnSCP
	{
		public static bool Prefix(FpcMotor __instance,ref Vector3 moveDir, ref bool sendJump, float jumpSpeed)
		{
            try
            {
                if (__instance.WantsToJump)
                {
                    if (jumpSpeed > 0f)
                    {
                        moveDir.y = jumpSpeed;
                    }
                    __instance._requestedJump = false;
                    __instance.IsJumping = true;
                    sendJump = true;
                }
                else
                {
                    moveDir.y = -10f;
                    __instance.IsJumping = false;
                }
                if (__instance._maxFallSpeed > 14.5f && (__instance.Hub.GetTeam() is not Team.SCPs 
                    || SanyaRemastered.Instance.Config.ScpFallDamage.Contains(__instance.Hub.GetRoleId().ToString()) && SanyaRemastered.Instance.Config.ScpTakeFallDamage))
                {
                    __instance.ServerProcessFall(__instance._maxFallSpeed - 14.5f);
                }
                __instance._maxFallSpeed = 14.5f;
                return false;
            }
            catch (Exception ex)
            {
                Log.Error("FallDamage" + ex);
            }
			return true;
		}
	}
}
