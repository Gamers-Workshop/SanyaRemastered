﻿using SanyaPlugin.Functions;
using System.Collections.Generic;
using UnityEngine;
using MEC;
using SanyaRemastered.Data;
using Respawning;
using HarmonyLib;
using Exiled.API.Features;
using Player = Exiled.API.Features.Player;

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
					ability.mana = SanyaPlugin.SanyaPlugin.instance.Config.Scp079CostCamera;
					break;
				case "Door Lock":
					ability.mana = SanyaPlugin.SanyaPlugin.instance.Config.Scp079CostLock;
					break;
				case "Door Lock Start":
					ability.mana = SanyaPlugin.SanyaPlugin.instance.Config.Scp079CostLockStart;
					break;
				case "Door Lock Minimum":
					ability.mana = SanyaPlugin.SanyaPlugin.instance.Config.Scp079RequiredLockStart;
					break;
				case "Door Interaction DEFAULT":
					ability.mana = SanyaPlugin.SanyaPlugin.instance.Config.Scp079CostDoorDefault;
					break;
				case "Door Interaction CONT_LVL_1":
					ability.mana = SanyaPlugin.SanyaPlugin.instance.Config.Scp079CostDoorContlv1;
					break;
				case "Door Interaction CONT_LVL_2":
					ability.mana = SanyaPlugin.SanyaPlugin.instance.Config.Scp079CostDoorContlv2;
					break;
				case "Door Interaction CONT_LVL_3":
					ability.mana = SanyaPlugin.SanyaPlugin.instance.Config.Scp079CostDoorContlv3;
					break;
				case "Door Interaction ARMORY_LVL_1":
					ability.mana = SanyaPlugin.SanyaPlugin.instance.Config.Scp079CostDoorArmlv1;
					break;
				case "Door Interaction ARMORY_LVL_2":
					ability.mana = SanyaPlugin.SanyaPlugin.instance.Config.Scp079CostDoorArmlv2;
					break;
				case "Door Interaction ARMORY_LVL_3":
					ability.mana = SanyaPlugin.SanyaPlugin.instance.Config.Scp079CostDoorArmlv3;
					break;
				case "Door Interaction EXIT_ACC":
					ability.mana = SanyaPlugin.SanyaPlugin.instance.Config.Scp079CostDoorGate;
					break;
				case "Door Interaction INCOM_ACC":
					ability.mana = SanyaPlugin.SanyaPlugin.instance.Config.Scp079CostDoorIntercom;
					break;
				case "Door Interaction CHCKPOINT_ACC":
					ability.mana = SanyaPlugin.SanyaPlugin.instance.Config.Scp079CostDoorCheckpoint;
					break;
				case "Room Lockdown":
					ability.mana = SanyaPlugin.SanyaPlugin.instance.Config.Scp079CostLockDown;
					break;
				case "Tesla Gate Burst":
					ability.mana = SanyaPlugin.SanyaPlugin.instance.Config.Scp079CostTesla;
					break;
				case "Elevator Teleport":
					ability.mana = SanyaPlugin.SanyaPlugin.instance.Config.Scp079CostElevatorTeleport;
					break;
				case "Elevator Use":
					ability.mana = SanyaPlugin.SanyaPlugin.instance.Config.Scp079CostElevatorUse;
					break;
				case "Speaker Start":
					ability.mana = SanyaPlugin.SanyaPlugin.instance.Config.Scp079CostSpeakerStart;
					break;
				case "Speaker Update":
					ability.mana = SanyaPlugin.SanyaPlugin.instance.Config.Scp079CostSpeakerUpdate;
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
		if (!SanyaPlugin.SanyaPlugin.instance.Config.Scp079ExtendEnabled) return true;

		Log.Debug($"[Scp079CameraPatch] {cameraId}:{lookatRotation}");

		if (__instance.GetComponent<AnimationController>().curAnim != 1) return true;

		if (__instance.curLvl + 1 >= SanyaPlugin.SanyaPlugin.instance.Config.Scp079ExtendLevelFindscp)
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
				if (SanyaPlugin.SanyaPlugin.instance.Config.Scp079ExtendCostFindscp > __instance.curMana)
				{
					__instance.RpcNotEnoughMana(SanyaPlugin.SanyaPlugin.instance.Config.Scp079ExtendCostFindscp, __instance.curMana);
					return false;
				}

				__instance.RpcSwitchCamera(target.cameraId, lookatRotation);
				__instance.Mana -= SanyaPlugin.SanyaPlugin.instance.Config.Scp079ExtendCostFindscp;
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
		if (!SanyaPlugin.SanyaPlugin.instance.Config.Scp079ExtendEnabled) return true;

		var player = Player.Dictionary[__instance.gameObject];
		Log.Debug($"[Scp079InteractPatch] {player.ReferenceHub.animationController.curAnim} -> {command}");

		if (player.ReferenceHub.animationController.curAnim != 1) return true;
		
		if (command.Contains("LOCKDOWN:"))
		{
		__instance.RpcNotEnoughMana(SanyaPlugin.SanyaPlugin.instance.Config.scp079_ex_cost_gaz, __instance.curMana);
			foreach (GameObject Player in PlayerManager.players)
		{
			{
			if (player.ReferenceHub.scp079PlayerScript.NetworkcurLvl < SanyaPlugin.SanyaPlugin.instance.Config.scp079_ex_level_gaz - 1)
			{
				player.ReferenceHub.SendTextHint(Subtitles.Extend079NoLevel, 10);
				break;
			}
			else if (player.ReferenceHub.scp079PlayerScript.NetworkcurMana < SanyaPlugin.SanyaPlugin.instance.Config.scp079_ex_cost_gaz)
			{
				player.ReferenceHub.SendTextHint(Subtitles.Extend079NoEnergy, 10);
				break;
			}
			else if (player.ReferenceHub.scp079PlayerScript.NetworkcurMana >= SanyaPlugin.SanyaPlugin.instance.Config.scp079_ex_cost_gaz)
			{
				player.ReferenceHub.scp079PlayerScript.NetworkcurMana -= SanyaPlugin.SanyaPlugin.instance.Config.scp079_ex_cost_gaz;
			}
			else
			{
			Log.Error("else");
			break;
			}
			
			ReferenceHub hub = ReferenceHub.GetHub(Player);
					Player player2 = Exiled.API.Features.Player.Dictionary[hub.gameObject];
			Room room = SCP079room(hub);
			bool locked = false;

			foreach (Door door in room.Transform.GetComponentsInChildren<Door>())
					{
						if (!locked)
						locked = door.destroyed;
						if (!locked)
						locked = door.locked;
					}
		if (player2.CurrentRoom == SCP079room(ReferenceHub.GetHub(__instance.gameObject)))
			if (room == null || room.Name.StartsWith("EZ") || locked)

			{
				player.ReferenceHub.SendTextHint(Subtitles.Extend079GazFail, 10);
				break;
			}
		foreach (var blackroom in SanyaPlugin.SanyaPlugin.instance.Config.gazBlacklistRooms)
			{
				if (room.Name.ToLower().Contains(blackroom.ToLower()))
					{
						player.ReferenceHub.SendTextHint(Subtitles.Extend079GazFail, 10);
						break;
					}
			}
				player.ReferenceHub.SendTextHint(Subtitles.Extend079SuccessGaz, 10);
				Timing.RunCoroutine(GasRoom(room, player.ReferenceHub));
			}
		}
		return false;
	}
	else if (command.Contains("DOOR:"))
	{
			if (__instance.curLvl + 1 >= SanyaPlugin.SanyaPlugin.instance.Config.Scp079ExtendLevelDoorbeep)
			{
				if (SanyaPlugin.SanyaPlugin.instance.Config.Scp079ExtendLevelDoorbeep > __instance.curMana)
				{
					__instance.RpcNotEnoughMana(SanyaPlugin.SanyaPlugin.instance.Config.Scp079ExtendCostDoorbeep, __instance.curMana);
					return false;
				}
				var door = target.GetComponent<Door>();
				if (door != null && door.curCooldown <= 0f)
				{
					player.ReferenceHub.playerInteract.RpcDenied(target);
					door.curCooldown = 0.5f;
					__instance.Mana -= SanyaPlugin.SanyaPlugin.instance.Config.Scp079ExtendCostDoorbeep;
				}
				return false;
			}
		}
		return true;
	}

	private static IEnumerator<float> GasRoom(Room room, ReferenceHub scp)
	{
		string str = ".g4 ";
		for (int i = SanyaPlugin.SanyaPlugin.instance.Config.GasDuration; i > 0f; i--)
		{
			str += ". .g4 ";
		}

		RespawnEffectsController.PlayCassieAnnouncement(str, false, false);
		List<Door> doors = Exiled.API.Features.Map.Doors.FindAll((d) => Vector3.Distance(d.transform.position, room.Position) <= 20f);
		foreach (var item in doors)
		{
			item.NetworkisOpen = true;
			item.Networklocked = true;
		}
		
		for (int i = SanyaPlugin.SanyaPlugin.instance.Config.GasDuration; i > 0f; i--)
		{
			foreach (var ply in PlayerManager.players)
			{
				var player = Exiled.API.Features.Player.Dictionary[ply];
				if (player.Team != Team.SCP && player.CurrentRoom != null && player.CurrentRoom.Transform == room.Transform)
				{
					player.ClearBroadcasts();
					player.Broadcast(1, Subtitles.ExtendGazWarn.Replace("{1}", i.ToString()));
					RespawnEffectsController.PlayCassieAnnouncement(".g3", false, false);
				}
			}
			yield return Timing.WaitForSeconds(1f);
		}
		foreach (var item in doors)
		{
			item.NetworkisOpen = false;
			item.Networklocked = true;
		}
		foreach (var ply in PlayerManager.players)
		{
			var player = Exiled.API.Features.Player.Dictionary[ply];
			if (player.Team != Team.SCP && player.CurrentRoom != null && player.CurrentRoom.Transform == room.Transform)
			{
				player.Broadcast(5, Subtitles.ExtendGazActive, Broadcast.BroadcastFlags.Normal);
			}
		}
		for (int i = 0; i < SanyaPlugin.SanyaPlugin.instance.Config.TimerWaitGas * 2; i++)
		{
			foreach (var ply in PlayerManager.players)
			{
				var player = Exiled.API.Features.Player.Dictionary[ply];
				if (player.Team != Team.SCP && player.Role != RoleType.Spectator && player.CurrentRoom != null && player.CurrentRoom.Transform == room.Transform)
				{
					player.ReferenceHub.playerStats.HurtPlayer(new PlayerStats.HitInfo(20f, "GAS", DamageTypes.Poison, 0), player.GameObject);
					if (player.Role == RoleType.Spectator)
					{
						scp.scp079PlayerScript.AddExperience(SanyaPlugin.SanyaPlugin.instance.Config.GasExpGain);
					}
				}
			}
			yield return Timing.WaitForSeconds(0.5f);
		}
		foreach (var item in doors)
		{
			item.Networklocked = false;
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