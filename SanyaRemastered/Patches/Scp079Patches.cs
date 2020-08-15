using SanyaPlugin.Functions;
using System.Collections.Generic;
using UnityEngine;
using MEC;
using SanyaRemastered.Data;
using Respawning;
using HarmonyLib;
using Exiled.API.Features;
using Player = Exiled.API.Features.Player;
using System.Linq;
using CustomPlayerEffects;

[HarmonyPatch(typeof(Scp079PlayerScript), nameof(Scp079PlayerScript.Start))]
public static class Scp079ManaPatch
{
	public static void Postfix(Scp079PlayerScript __instance)
	{
		foreach (Scp079PlayerScript.Ability079 ability in __instance.abilities)
		{
			switch (ability.label)
			{
				case "Camera Switch":
					ability.mana = SanyaPlugin.SanyaPlugin.Instance.Config.Scp079CostCamera;
					break;
				case "Door Lock":
					ability.mana = SanyaPlugin.SanyaPlugin.Instance.Config.Scp079CostLock;
					break;
				case "Door Lock Start":
					ability.mana = SanyaPlugin.SanyaPlugin.Instance.Config.Scp079CostLockStart;
					break;
				case "Door Lock Minimum":
					ability.mana = SanyaPlugin.SanyaPlugin.Instance.Config.Scp079RequiredLockStart;
					break;
				case "Door Interaction DEFAULT":
					ability.mana = SanyaPlugin.SanyaPlugin.Instance.Config.Scp079CostDoorDefault;
					break;
				case "Door Interaction CONT_LVL_1":
					ability.mana = SanyaPlugin.SanyaPlugin.Instance.Config.Scp079CostDoorContlv1;
					break;
				case "Door Interaction CONT_LVL_2":
					ability.mana = SanyaPlugin.SanyaPlugin.Instance.Config.Scp079CostDoorContlv2;
					break;
				case "Door Interaction CONT_LVL_3":
					ability.mana = SanyaPlugin.SanyaPlugin.Instance.Config.Scp079CostDoorContlv3;
					break;
				case "Door Interaction ARMORY_LVL_1":
					ability.mana = SanyaPlugin.SanyaPlugin.Instance.Config.Scp079CostDoorArmlv1;
					break;
				case "Door Interaction ARMORY_LVL_2":
					ability.mana = SanyaPlugin.SanyaPlugin.Instance.Config.Scp079CostDoorArmlv2;
					break;
				case "Door Interaction ARMORY_LVL_3":
					ability.mana = SanyaPlugin.SanyaPlugin.Instance.Config.Scp079CostDoorArmlv3;
					break;
				case "Door Interaction EXIT_ACC":
					ability.mana = SanyaPlugin.SanyaPlugin.Instance.Config.Scp079CostDoorGate;
					break;
				case "Door Interaction INCOM_ACC":
					ability.mana = SanyaPlugin.SanyaPlugin.Instance.Config.Scp079CostDoorIntercom;
					break;
				case "Door Interaction CHCKPOINT_ACC":
					ability.mana = SanyaPlugin.SanyaPlugin.Instance.Config.Scp079CostDoorCheckpoint;
					break;
				case "Room Lockdown":
					ability.mana = SanyaPlugin.SanyaPlugin.Instance.Config.Scp079CostLockDown;
					break;
				case "Tesla Gate Burst":
					ability.mana = SanyaPlugin.SanyaPlugin.Instance.Config.Scp079CostTesla;
					break;
				case "Elevator Teleport":
					ability.mana = SanyaPlugin.SanyaPlugin.Instance.Config.Scp079CostElevatorTeleport;
					break;
				case "Elevator Use":
					ability.mana = SanyaPlugin.SanyaPlugin.Instance.Config.Scp079CostElevatorUse;
					break;
				case "Speaker Start":
					ability.mana = SanyaPlugin.SanyaPlugin.Instance.Config.Scp079CostSpeakerStart;
					break;
				case "Speaker Update":
					ability.mana = SanyaPlugin.SanyaPlugin.Instance.Config.Scp079CostSpeakerUpdate;
					break;
			}
		}
	}
}

//SCP-079 
[HarmonyPatch(typeof(Scp079PlayerScript), nameof(Scp079PlayerScript.CallCmdSwitchCamera))]
public static class Scp079CameraPatch
{
	public static bool Prefix(Scp079PlayerScript __instance, ref ushort cameraId, bool lookatRotation)
	{
		if (!SanyaPlugin.SanyaPlugin.Instance.Config.Scp079ExtendEnabled) return true;

		Log.Debug($"[Scp079CameraPatch] {cameraId}:{lookatRotation}");

		if (__instance.GetComponent<AnimationController>().curAnim != 1) return true;

		if (__instance.curLvl + 1 >= SanyaPlugin.SanyaPlugin.Instance.Config.Scp079ExtendLevelFindscp)
		{
			List<Camera079> cams = new List<Camera079>();
			foreach (var ply in Player.List)
			{
				if (ply.Team == Team.SCP && ply.Role != RoleType.Scp079)
				{
					cams.AddRange(ply.ReferenceHub.GetNearCams());
				}
			}

			Camera079 target;
			if (cams.Count > 0)
			{
				target = cams.GetRandomOne();
			}
			else return true;

			if (target != null)
			{
				if (SanyaPlugin.SanyaPlugin.Instance.Config.Scp079ExtendCostFindscp > __instance.curMana)
				{
					__instance.RpcNotEnoughMana(SanyaPlugin.SanyaPlugin.Instance.Config.Scp079ExtendCostFindscp, __instance.curMana);
					return false;
				}

				__instance.RpcSwitchCamera(target.cameraId, lookatRotation);
				__instance.Mana -= SanyaPlugin.SanyaPlugin.Instance.Config.Scp079ExtendCostFindscp;
				__instance.currentCamera = target;
				return false;
			}
		}
		return true;
	}
}

//SCP-079Extend Sprint 
[HarmonyPatch(typeof(Scp079PlayerScript), nameof(Scp079PlayerScript.CallCmdInteract))]
public static class Scp079InteractPatch
{
	public static bool Prefix(Scp079PlayerScript __instance, ref string command, ref GameObject target)
	{
		if (!SanyaPlugin.SanyaPlugin.Instance.Config.Scp079ExtendEnabled) return true;

		var player = Player.Dictionary[__instance.gameObject];
		Log.Debug($"[Scp079InteractPatch] {player.ReferenceHub.animationController.curAnim} -> {command}");

		if (player.ReferenceHub.animationController.curAnim != 1) return true;
		
		if (command.Contains("LOCKDOWN:"))
		{
			foreach (GameObject Player in PlayerManager.players)
			{
				{
				if (player.ReferenceHub.scp079PlayerScript.curLvl + 2 < SanyaPlugin.SanyaPlugin.Instance.Config.Scp079ExLevelGaz)
				{
					player.ReferenceHub.SendTextHint(Subtitles.Extend079NoLevel, 10);
					break;
				}
				else if (player.ReferenceHub.scp079PlayerScript.curMana < SanyaPlugin.SanyaPlugin.Instance.Config.Scp079ExCostGaz)
				{
					player.ReferenceHub.SendTextHint(Subtitles.Extend079NoEnergy, 10);
					break;
				}
				else if (player.ReferenceHub.scp079PlayerScript.curMana >= SanyaPlugin.SanyaPlugin.Instance.Config.Scp079ExCostGaz)
				{
					//no break
				}
				else
				{
				Log.Error("ERROR");
				break;
				}
				ReferenceHub hub = ReferenceHub.GetHub(Player);
					Player player2 = Exiled.API.Features.Player.Dictionary[hub.gameObject];
				Room room = SCP079room(hub);
				bool locked = false;
				List<Door> doors = Map.Doors.Where((d) => Vector3.Distance(d.transform.position, room.Position) <= 11f).ToList();
						foreach (Door door in doors)
						{
						if (!locked &&
							( door.destroyed
							|| door.lockdown
							|| door.locked
							|| door._isLockedBy079
							|| door._wasLocked
							|| door.warheadlock
							|| door._checkpointLockOpen
							|| door._checkpointLockOpenDecont
							|| door._checkpointLockOpenWarhead))
							locked = true;
						}

					if (player2.CurrentRoom == SCP079room(ReferenceHub.GetHub(__instance.gameObject)))
				if (room == null || locked)
				{
					player.ReferenceHub.SendTextHint(Subtitles.Extend079GazFail, 10);
					break;
				}
			foreach (var blackroom in SanyaPlugin.SanyaPlugin.Instance.Config.GazBlacklistRooms)
				{
					if (room.Name.ToLower().Contains(blackroom.ToLower()))
					{
						player.ReferenceHub.SendTextHint(Subtitles.Extend079GazFail, 10);
						break;
					}
				}
					player.ReferenceHub.SendTextHint(Subtitles.Extend079SuccessGaz, 10);
					if (!player.IsStaffBypassEnabled && !player.IsBypassModeEnabled) player.ReferenceHub.scp079PlayerScript.curMana -= SanyaPlugin.SanyaPlugin.Instance.Config.Scp079ExCostGaz;
					Timing.RunCoroutine(GasRoom(room, player.ReferenceHub));
				}
			}
			return false;
		}
		else if (command.Contains("DOOR:"))
		{
			__instance.RpcNotEnoughMana(SanyaPlugin.SanyaPlugin.Instance.Config.Scp079ExtendCostDoorbeep, __instance.curMana);
			foreach (GameObject Player in PlayerManager.players)
			{
				{
					if (player.ReferenceHub.scp079PlayerScript.curLvl + 2 < SanyaPlugin.SanyaPlugin.Instance.Config.Scp079ExtendLevelDoorbeep)
					{
						player.ReferenceHub.SendTextHint(Subtitles.Extend079NoLevel, 10);
						break;
					}
					else if (player.ReferenceHub.scp079PlayerScript.curMana < SanyaPlugin.SanyaPlugin.Instance.Config.Scp079ExtendCostDoorbeep)
					{
						player.ReferenceHub.SendTextHint(Subtitles.Extend079NoEnergy, 10);
						break;
					}
					else if (player.ReferenceHub.scp079PlayerScript.curMana >= SanyaPlugin.SanyaPlugin.Instance.Config.Scp079ExtendCostDoorbeep)
					{
						//no break
					}
					else
					{
						Log.Error("ERROR");
						break;
					}
				}
				var door = target.GetComponent<Door>();
				if (door != null && door.curCooldown <= 0f)
				{
					player.ReferenceHub.playerInteract.CallRpcDenied(target);
					door.curCooldown = 0.5f;
					if (!player.IsStaffBypassEnabled && !player.IsBypassModeEnabled) __instance.Mana -= SanyaPlugin.SanyaPlugin.Instance.Config.Scp079ExtendCostDoorbeep;
				}
				return false;
			}
		}
		return true;
	}
private static IEnumerator<float> GasRoom(Room room, ReferenceHub scp)
	{
		List<Door> doors = Map.Doors.Where((d) => Vector3.Distance(d.transform.position, room.Position) <= 11f).ToList();
		foreach (var item in doors)
		{
			item.Networklocked = true;
			item.NetworkisOpen = true;
		}
		
		for (int i = SanyaPlugin.SanyaPlugin.Instance.Config.GasDuration; i > 0f; i -= 1)
		{
			foreach (var ply in PlayerManager.players)
			{
				var player = Exiled.API.Features.Player.Dictionary[ply];
				if (player.CurrentRoom != null && player.CurrentRoom.Transform == room.Transform)
				{
					if (SanyaPlugin.SanyaPlugin.Instance.Config.CassieSubtitle)
					{
						player.ClearBroadcasts();
						player.Broadcast(1, Subtitles.ExtendGazWarn.Replace("{1}", i.ToString()));
					}
					foreach (RespawnEffectsController respawnEffectsController in RespawnEffectsController.AllControllers)
					{
						if (respawnEffectsController != null)
						{
							Methods.RpcCassieAnnouncement(respawnEffectsController, player.ReferenceHub.characterClassManager.Connection," .g4 ", false, false);
						}
					}
				}
			}
			yield return Timing.WaitForSeconds(1);
		}
		foreach (var item in doors)
		{
			item.Networklocked = true;
			item.NetworkisOpen = false;
		}
		foreach (var ply in PlayerManager.players)
		{
			var player = Exiled.API.Features.Player.Dictionary[ply];
			if (player.Team != Team.SCP && player.CurrentRoom != null && player.CurrentRoom.Transform == room.Transform)
			{
				player.Broadcast(5, Subtitles.ExtendGazActive, Broadcast.BroadcastFlags.Normal);
			}
		}
		for (int i = 0; i < SanyaPlugin.SanyaPlugin.Instance.Config.TimerWaitGas * 2; i++)
		{
			foreach (var ply in PlayerManager.players)
			{
				var player = Exiled.API.Features.Player.Dictionary[ply];
				if (player.Team != Team.SCP && player.Role != RoleType.Spectator && player.CurrentRoom != null && player.CurrentRoom.Transform == room.Transform)
				{
					player.ReferenceHub.playerEffectsController.EnableEffect<Disabled>();
					player.ReferenceHub.playerEffectsController.EnableEffect<Asphyxiated>();
					player.ReferenceHub.playerEffectsController.EnableEffect<Poisoned>();
					player.ReferenceHub.playerStats.HurtPlayer(new PlayerStats.HitInfo(2.5f, "GAS", DamageTypes.Poison, 0), player.GameObject);
					if (player.Role == RoleType.Spectator)
					{
						scp.scp079PlayerScript.AddExperience(SanyaPlugin.SanyaPlugin.Instance.Config.GasExpGain);
						player.ReferenceHub.SendTextHint("Vous étes mort par le gazage de SCP-079", 20);
					}
				}
			}
			yield return Timing.WaitForSeconds(0.5f);
		}
		foreach (var item in doors)
		{
			item.Networklocked = false;
			item.NetworkisOpen = true;
		}
	}

	private static Room SCP079room(ReferenceHub player)
	{
		Vector3 playerPos = player.scp079PlayerScript.currentCamera.transform.position;
		Vector3 end = playerPos - new Vector3(0f, 10f, 0f);
		bool flag = Physics.Linecast(playerPos, end, out RaycastHit raycastHit, -84058629);

		if (!flag || raycastHit.transform == null)
			return null;

		Transform transform = raycastHit.transform;

		while (transform.parent != null && transform.parent.parent != null)
			transform = transform.parent;

		foreach (Room room in Exiled.API.Features.Map.Rooms)
			if (room.Position == transform.position)
				return room;

		return new Room(transform.name, transform, transform.position);
	}
}