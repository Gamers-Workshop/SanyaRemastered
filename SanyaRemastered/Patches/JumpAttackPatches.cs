﻿using Exiled.API.Features;
using HarmonyLib;
using Mirror;
using SanyaRemastered.Functions;
using UnityEngine;

namespace SanyaRemastered.Patches
{
	/*[HarmonyPatch(typeof(AnimationController), nameof(AnimationController.UserCode_CmdChangeSpeedState))]
	public static class JumpAttackPatch
	{
		public static void Prefix(AnimationController __instance, byte newState)
		{
			if (!SanyaRemastered.Instance.Config.JumpingKickAttack || __instance.curAnim != 2 || __instance._curMoveState == newState || newState != 1 || !__instance.ccm.IsHuman())
				return;

			Log.Debug($"Attack:{__instance.ccm._hub.nicknameSync.MyNick}", SanyaRemastered.Instance.Config.IsDebugged);
			var player = Player.Get(__instance.gameObject);
			if (player.SessionVariables.TryGetValue("sanya_kickattack_lasttime", out object time) && (NetworkTime.time - (double)time) < 0.5) return;
			foreach (var hits in Physics.RaycastAll(__instance.ccm._hub.PlayerCameraReference.position, __instance.ccm._hub.PlayerCameraReference.forward, 2f, 4))
			{
				var target = Player.Get(hits.transform.gameObject);
				if (target?.Id == player?.Id) continue;

				if (HitboxIdentity.CheckFriendlyFire(player.Team,target.Team) || ServerConsole.FriendlyFire && !target.IsGodModeEnabled)
				{
					Log.Debug($"Attack:{player.Nickname} -> {target.Nickname}", SanyaRemastered.Instance.Config.IsDebugged);
					target.ReferenceHub.falldamage.RpcDoSound();
					target.Hurt(3f, player, DamageTypes.Scp0492);
					player.SendHitmarker();
					Vector3 vec = __instance.ccm._hub.PlayerCameraReference.forward * 0.5f;
					Vector3 targetpos = new Vector3(target.Position.x + vec.x, target.Position.y, target.Position.z + vec.z);
					if (!Methods.IsStuck(targetpos))
						target.Position = targetpos; if (!player.SessionVariables.ContainsKey("sanya_kickattack_lasttime"))
						player.SessionVariables.Add("sanya_kickattack_lasttime", NetworkTime.time);
					else
						player.SessionVariables["sanya_kickattack_lasttime"] = NetworkTime.time;
				}
			}
		}
	}*/
}
