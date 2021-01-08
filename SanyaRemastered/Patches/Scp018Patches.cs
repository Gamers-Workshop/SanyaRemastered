﻿using Exiled.API.Features;
using Grenades;
using HarmonyLib;
using Mirror;
using SanyaRemastered;
using UnityEngine;

namespace SanyaRemastered.Patches
{
	[HarmonyPatch(typeof(Grenade), nameof(Grenade.NetworkthrowerTeam), MethodType.Setter)]
	public static class GrenadeThrowerPatch
	{
		public static void Prefix(Grenade __instance, ref Team value)
		{
			Log.Debug($"[GrenadeThrowerPatch] value:{value} isscp018:{__instance is Scp018Grenade}");

			if (SanyaRemastered.Instance.Config.Scp018FriendlyFire&& __instance is Scp018Grenade) value = Team.TUT;
		}
	}
	[HarmonyPatch(typeof(Scp018Grenade), nameof(Scp018Grenade.OnSpeedCollisionEnter))]
	public static class Scp018Patch
	{
		public static bool Prefix(Scp018Grenade __instance, Collision collision, float relativeSpeed)
		{
			Vector3 velocity = __instance.rb.velocity * __instance.bounceSpeedMultiplier;
			float num = __instance.topSpeedPerBounce[__instance.bounce];
			if (relativeSpeed > num)
			{
				__instance.rb.velocity = velocity.normalized * num;
				if (__instance.actionAllowed)
				{
					__instance.bounce = Mathf.Min(__instance.bounce + 1, __instance.topSpeedPerBounce.Length - 1);
				}
			}
			else
			{
				if (relativeSpeed > __instance.source.maxDistance)
				{
					__instance.source.maxDistance = relativeSpeed;
				}
				__instance.rb.velocity = velocity;
			}
			if (NetworkServer.active)
			{
				Collider collider = collision.collider;
				int num2 = 1 << collider.gameObject.layer;

				if (!SanyaRemastered.Instance.Config.Scp018CantDestroyObject)
				{
					if (num2 == __instance.layerGlass)
					{
						if (__instance.actionAllowed && relativeSpeed >= __instance.breakpointGlass)
						{
							__instance.cooldown = __instance.cooldownGlass;
							BreakableWindow component = collider.GetComponent<BreakableWindow>();
							if (component != null)
							{
								component.ServerDamageWindow(relativeSpeed * __instance.damageGlass);
							}
						}
					}
					else if (num2 == __instance.layerDoor)
					{
						if (relativeSpeed >= __instance.breakpointDoor)
						{
							__instance.cooldown = __instance.cooldownDoor;
							/*Door componentInParent = collider.GetComponentInParent<Door>();
							if (componentInParent != null && !componentInParent.GrenadesResistant)
							{
								componentInParent.DestroyDoor(b: true);
							}*/
						}
					}
				}

				if ((num2 == __instance.layerHitbox || num2 == __instance.layerIgnoreRaycast) && __instance.actionAllowed && relativeSpeed >= __instance.breakpointHurt)
				{
					__instance.cooldown = __instance.cooldownHurt;
					ReferenceHub componentInParent2 = collider.GetComponentInParent<ReferenceHub>();
					if (componentInParent2 != null && (ServerConsole.FriendlyFire || componentInParent2.gameObject == __instance.thrower.gameObject || componentInParent2.weaponManager.GetShootPermission(__instance.throwerTeam)))
					{
						float num3 = relativeSpeed * __instance.damageHurt * SanyaRemastered.Instance.Config.Scp018DamageMultiplier;

						//componentInParent2.playerStats.ccm.CurClass != RoleType.Scp106 && 
						if (componentInParent2.playerStats.ccm.Classes.SafeGet(componentInParent2.playerStats.ccm.CurClass).team == Team.SCP)
						{
							num3 *= __instance.damageScpMultiplier;
						}

						componentInParent2.playerStats.HurtPlayer(new PlayerStats.HitInfo(num3, __instance.logName, DamageTypes.Grenade, Player.Dictionary[__instance.throwerGameObject.gameObject].Id), componentInParent2.playerStats.gameObject);
					}
				}

				if (__instance.bounce >= __instance.topSpeedPerBounce.Length - 1 && relativeSpeed >= num && !__instance.hasHitMaxSpeed)
				{
					__instance.NetworkfuseTime = NetworkTime.time + 10.0;
					__instance.hasHitMaxSpeed = true;
				}
			}
			//__instance.OnSpeedCollisionEnter(collision, relativeSpeed);
			return false;
		}
	}
}
