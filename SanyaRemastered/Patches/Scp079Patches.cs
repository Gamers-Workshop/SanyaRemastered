using SanyaRemastered.Functions;
using System.Collections.Generic;
using UnityEngine;
using MEC;
using SanyaRemastered.Data;
using HarmonyLib;
using Exiled.API.Features;
using Player = Exiled.API.Features.Player;
using System.Linq;
using CustomPlayerEffects;
using SanyaRemastered;
using Interactables.Interobjects.DoorUtils;
using Exiled.API.Enums;
using Interactables.Interobjects;

namespace SanyaRemastered.Patches
{
	[HarmonyPatch(typeof(Scp079PlayerScript), nameof(Scp079PlayerScript.Start))]
	public static class Scp079ManaPatch
	{
		public static void Postfix(Scp079PlayerScript __instance)
		{
			foreach (Scp079PlayerScript.Ability079 ability in __instance.abilities)
				if (SanyaRemastered.Instance.Config.Scp079ManaCost.TryGetValue(ability.label, out var value))
					ability.mana = value;
		}
	}

	//SCP-079Extend Sprint 
	[HarmonyPatch(typeof(Scp079PlayerScript), nameof(Scp079PlayerScript.UserCode_CmdInteract))]
	public static class Scp079InteractPatch
	{
		public static bool Prefix(Scp079PlayerScript __instance, Command079 command ,string args,GameObject target)
		{
			try
			{
				if (!SanyaRemastered.Instance.Config.Scp079ExtendEnabled) return true;

				var player = Player.Dictionary[__instance.gameObject];
				Log.Debug($"[Scp079InteractPatch] {player.IsExmode()} -> {command} args {args}");

				if (!player.IsExmode()) return true;

				if (command == Command079.Lockdown)
				{
					if (player.ReferenceHub.scp079PlayerScript.Network_curLvl + 1 < SanyaRemastered.Instance.Config.Scp079ExLevelGaz)
					{
						player.ReferenceHub.GetComponent<SanyaRemasteredComponent>().AddHudCenterDownText(Subtitles.Extend079NoLevel, 10);
						return false;
					}
					else if (player.ReferenceHub.scp079PlayerScript.Mana < SanyaRemastered.Instance.Config.Scp079ExCostGaz)
					{
						player.ReferenceHub.GetComponent<SanyaRemasteredComponent>().AddHudCenterDownText(Subtitles.Extend079NoEnergy, 10);
						return false;
					}
					else if (player.ReferenceHub.scp079PlayerScript.Mana >= SanyaRemastered.Instance.Config.Scp079ExCostGaz)
					{
						//no break
					}
					else
					{
						Log.Error("ERROR");
						return false;
					}
					Room room = player.CurrentRoom;
					List<Door> doors = new List<Door>();
					HashSet<Scp079Interactable> scp079Interactables = Scp079Interactable.InteractablesByRoomId[player.ReferenceHub.scp079PlayerScript.CurrentRoom.UniqueId];

					foreach (Scp079Interactable scp079Interactable in scp079Interactables)
					{
						if (!(scp079Interactable == null))
						{
							Scp079Interactable.InteractableType type = scp079Interactable.type;
							IDamageableDoor damageableDoor;
							if (type != Scp079Interactable.InteractableType.Door)
							{
								if (type == Scp079Interactable.InteractableType.Lockdown)
								{
									continue;
								}
							}
							else if (scp079Interactable.TryGetComponent(out DoorVariant doorVariant2) && (damageableDoor = (doorVariant2 as IDamageableDoor)) != null && damageableDoor.IsDestroyed)
							{
								player.ReferenceHub.GetComponent<SanyaRemasteredComponent>().AddHudCenterDownText(Subtitles.Extend079GazFail, 10);
								return false;
							}
						}
					}
					foreach (Scp079Interactable scp079Interactable in scp079Interactables)
					{
						scp079Interactable.TryGetComponent(out DoorVariant doorVariant);
						doors.Add(Door.Get(doorVariant));
					}
					if (room == null)
					{
						player.ReferenceHub.GetComponent<SanyaRemasteredComponent>().AddHudCenterDownText(Subtitles.Extend079GazFail, 10);
						return false;
					}
					foreach (var blackroom in SanyaRemastered.Instance.Config.GazBlacklistRooms)
					{
						if (room.Type.ToString().ToLower().Contains(blackroom.ToLower()))
						{
							player.ReferenceHub.GetComponent<SanyaRemasteredComponent>().AddHudCenterDownText(Subtitles.Extend079GazFail, 10);
							return false;
						}
					}
					player.ReferenceHub.GetComponent<SanyaRemasteredComponent>().AddHudCenterDownText(Subtitles.Extend079SuccessGaz, 10);
					if (!player.IsStaffBypassEnabled && !player.IsBypassModeEnabled) player.ReferenceHub.scp079PlayerScript.Mana -= SanyaRemastered.Instance.Config.Scp079ExCostGaz;
					Timing.RunCoroutine(GasRoom(room, player.ReferenceHub, doors), Segment.FixedUpdate);
					return false;
				}
				else if (command == Command079.Door)
				{
					__instance.RpcNotEnoughMana(SanyaRemastered.Instance.Config.Scp079ExtendCostDoorbeep, __instance.Mana);
					{
						{
							if (player.ReferenceHub.scp079PlayerScript.Lvl + 1 < SanyaRemastered.Instance.Config.Scp079ExtendLevelDoorbeep)
							{
								player.ReferenceHub.GetComponent<SanyaRemasteredComponent>().AddHudCenterDownText(Subtitles.Extend079NoLevel, 10);
								return false;
							}
							else if (player.ReferenceHub.scp079PlayerScript.Mana < SanyaRemastered.Instance.Config.Scp079ExtendCostDoorbeep)
							{
								player.ReferenceHub.GetComponent<SanyaRemasteredComponent>().AddHudCenterDownText(Subtitles.Extend079NoEnergy, 10);
								return false;
							}
							else if (player.ReferenceHub.scp079PlayerScript.Mana >= SanyaRemastered.Instance.Config.Scp079ExtendCostDoorbeep)
							{
								//no break
							}
							else
							{
								Log.Error("ERROR");
								return false;
							}
						}
						var door = target.GetComponent<DoorVariant>();
						if (door != null && door.syncInterval <= 0f)
						{

							door.syncInterval = 0.5f;
							if (!player.IsStaffBypassEnabled && !player.IsBypassModeEnabled) __instance.Mana -= SanyaRemastered.Instance.Config.Scp079ExtendCostDoorbeep;
						}
						return false;
					}
				}
				else if (command == Command079.Speaker && args == "EZ_Intercom")
				{
					if (player.ReferenceHub.scp079PlayerScript.Lvl + 1 < SanyaRemastered.Instance.Config.Scp079ExtendBlackoutIntercom)
					{
						player.ReferenceHub.GetComponent<SanyaRemasteredComponent>().AddHudCenterDownText(Subtitles.Extend079NoLevel, 10);
						return false;
					}
					else if (player.ReferenceHub.scp079PlayerScript.Mana < SanyaRemastered.Instance.Config.Scp079ExtendCostBlackoutIntercom)
					{
						player.ReferenceHub.GetComponent<SanyaRemasteredComponent>().AddHudCenterDownText(Subtitles.Extend079NoEnergy, 10);
						return false;
					}
					player.CurrentRoom.TurnOffLights(30f);
					if (!player.IsStaffBypassEnabled && !player.IsBypassModeEnabled) __instance.Mana -= SanyaRemastered.Instance.Config.Scp079ExtendCostBlackoutIntercom;
					player.ReferenceHub.GetComponent<SanyaRemasteredComponent>().AddHudCenterDownText(Subtitles.Extend079SuccessBlackoutIntercom, 10);
					return false;
				}
				return true;
			}
			catch (System.Exception ex)
            {
				Log.Error("[Scp079PlayerScript.UserCode_CmdInteract]" + ex);
				return true;
			}
		}
		private static IEnumerator<float> GasRoom(Room room, ReferenceHub scp,List<Door> doors)
		{
			DiscordLog.DiscordLog.Instance.LOG += $":biohazard: Gazage de la salle : {room.Type}";

			foreach (var door in doors)
			{
				door.Base.NetworkTargetState = true;
				door.Base.ServerChangeLock(DoorLockReason.Isolation,true);
			}
			for (int i = SanyaRemastered.Instance.Config.GasDuration; i > 0f; i--)
			{
				foreach (var player in Player.List)
				{
					if (player.CurrentRoom != null && player.CurrentRoom == room)
					{
						if (SanyaRemastered.Instance.Config.CassieSubtitle)
						{
							player.ReferenceHub.GetComponent<SanyaRemasteredComponent>().AddHudCenterDownText(Subtitles.ExtendGazWarn.Replace("{1}", i.ToString()).Replace("{s}", $"{(i <= 1 ? "" : "s")}"),2);
						}
						room.Color = FlickerableLightController.DefaultWarheadColor;
					}
				}
				yield return Timing.WaitForSeconds(0.5f);
				room.ResetColor();
				yield return Timing.WaitForSeconds(0.5f);
			}
			foreach (var door in doors)
			{
				door.Base.NetworkTargetState = false;
				door.Base.ServerChangeLock(DoorLockReason.Isolation, true);
			}
			foreach (var player in Player.List.Where((p) => p.Role != RoleType.None))
			{
				if (player.Team != Team.SCP && player.CurrentRoom != null && player.CurrentRoom == room)
				{
					player.Broadcast(5, Subtitles.ExtendGazActive, Broadcast.BroadcastFlags.Normal);
				}
			}
			for (int i = 0; i < SanyaRemastered.Instance.Config.TimerWaitGas * 2; i++)
			{
				foreach (var player in Player.List)
				{
					if (player.Team != Team.SCP && player.Role != RoleType.Spectator && player.CurrentRoom == room)
					{
						player.ReferenceHub.playerEffectsController.EnableEffect<Disabled>();
						player.ReferenceHub.playerEffectsController.EnableEffect<Asphyxiated>();
						player.ReferenceHub.playerEffectsController.EnableEffect<Poisoned>();
						player.ReferenceHub.playerStats.HurtPlayer(new PlayerStats.HitInfo(5, "GAS", DamageTypes.Poison, 0,true), player.GameObject);
						if (player.Role == RoleType.Spectator)
						{
							scp.scp079PlayerScript.AddExperience(SanyaRemastered.Instance.Config.GasExpGain);
							player.ReferenceHub.GetComponent<SanyaRemasteredComponent>().AddHudCenterDownText("Vous étes mort par le gazage de SCP-079", 20);
						}
					}
				}
				yield return Timing.WaitForSeconds(0.5f);
			}
			foreach (var door in doors)
			{
				door.Base.ServerChangeLock(DoorLockReason.Isolation, false);
			}
		}
	}
}