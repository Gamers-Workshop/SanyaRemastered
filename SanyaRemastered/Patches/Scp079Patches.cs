using CustomPlayerEffects;
using Exiled.API.Features;
using HarmonyLib;
using Interactables.Interobjects.DoorUtils;
using MEC;
using PlayerStatsSystem;
using SanyaRemastered.Data;
using SanyaRemastered.Functions;
using System;
using System.Collections.Generic;
using UnityEngine;
using Player = Exiled.API.Features.Player;

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
        public static bool Prefix(Scp079PlayerScript __instance, Command079 command, string args, GameObject target)
        {
            try
            {
                if (!SanyaRemastered.Instance.Config.Scp079ExtendEnabled) return true;
                __instance.RefreshCurrentRoom();
                var player = Player.Dictionary[__instance.gameObject];
                Log.Debug($"[Scp079InteractPatch] {player.IsExmode()} -> {command} Room {__instance.CurrentRoom.Name}", SanyaRemastered.Instance.Config.IsDebugged);

                if (!player.IsExmode()) return true;

                if (command == Command079.Lockdown)
                {
                    DateTime time = DateTime.Now;
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
                    Room room = player.CurrentRoom;
                    List<DoorVariant> doors = new List<DoorVariant>();
                    HashSet<Scp079Interactable> scp079Interactables = Scp079Interactable.InteractablesByRoomId[__instance.CurrentRoom.UniqueId];
                    foreach (Scp079Interactable scp079Interactable in scp079Interactables)
                    {
                        if (!(scp079Interactable == null))
                        {
                            IDamageableDoor damageableDoor;
                            if (scp079Interactable.type != Scp079Interactable.InteractableType.Door)
                            {
                                continue;
                            }
                            else if (scp079Interactable.TryGetComponent(out DoorVariant doorVariant) && (damageableDoor = (doorVariant as IDamageableDoor)) != null && damageableDoor.IsDestroyed)
                            {
                                player.ReferenceHub.GetComponent<SanyaRemasteredComponent>().AddHudCenterDownText(Subtitles.Extend079GazFail, 10);
                                return false;
                            }
                            else
                            {
                                doors.Add(doorVariant);
                            }
                        }
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
                    if (player.TryGetSessionVariable("scp079_gas_waiting", out DateTime dateTime) && time < dateTime)
                    {
                        int TimeToWaiting = (int)(dateTime - time).TotalSeconds;
                        player.ReferenceHub.GetComponent<SanyaRemasteredComponent>().AddHudCenterDownText(Subtitles.Extend079GazCooldown.Replace("{0}", TimeToWaiting.ToString()).Replace("{s}", TimeToWaiting > 1 ? "s" : ""), 10);
                        return false;
                    }
                    else
                    {
                        player.SessionVariables.Remove("scp079_gas_waiting");
                        player.SessionVariables.Add("scp079_gas_waiting", DateTime.Now.AddSeconds(SanyaRemastered.Instance.Config.GasWaitingTime));
                    }
                    player.ReferenceHub.GetComponent<SanyaRemasteredComponent>().AddHudCenterDownText(Subtitles.Extend079SuccessGaz, 10);
                    if (!player.IsStaffBypassEnabled && !player.IsBypassModeEnabled) player.ReferenceHub.scp079PlayerScript.Mana -= SanyaRemastered.Instance.Config.Scp079ExCostGaz;
                    Timing.RunCoroutine(GasRoom(room, player.ReferenceHub, doors), Segment.FixedUpdate);
                    DiscordLog.DiscordLog.Instance.LOG += $":biohazard: Gazage de la salle : {room.Type}";
                    return false;
                }
                else if (command == Command079.Door)
                {
                    __instance.RpcNotEnoughMana(SanyaRemastered.Instance.Config.Scp079ExtendCostDoorbeep, __instance.Mana);
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
                        var door = target.GetComponent<DoorVariant>();
                        if (door != null && door.syncInterval <= 0f)
                        {

                            door.syncInterval = 0.5f;
                            if (!player.IsStaffBypassEnabled && !player.IsBypassModeEnabled) __instance.Mana -= SanyaRemastered.Instance.Config.Scp079ExtendCostDoorbeep;
                            door.PermissionsDenied(player.ReferenceHub, 1);
                            DoorEvents.TriggerAction(door, DoorAction.AccessDenied, player.ReferenceHub);
                        }
                        return false;
                    }
                }
                else if (command == Command079.Speaker && __instance.CurrentRoom.Name == MapGeneration.RoomName.EzIntercom)
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
                    if (AlphaWarheadController.Host.inProgress)
                    {
                        return false;
                    }

                    if (__instance.CurrentRoom != null)
                    {
                        foreach (FlickerableLightController flickerableLightController in __instance.CurrentRoom.GetComponentsInChildren<FlickerableLightController>())
                        {
                            if (flickerableLightController != null)
                            {
                                flickerableLightController.ServerFlickerLights(30f);
                            }
                        }
                        if (!player.IsStaffBypassEnabled && !player.IsBypassModeEnabled) __instance.Mana -= SanyaRemastered.Instance.Config.Scp079ExtendCostBlackoutIntercom;
                        player.ReferenceHub.GetComponent<SanyaRemasteredComponent>().AddHudCenterDownText(Subtitles.Extend079SuccessBlackoutIntercom, 10);
                        return false;
                    }
                }
                return true;
            }
            catch (System.Exception ex)
            {
                Log.Error("[Scp079PlayerScript.UserCode_CmdInteract] " + ex);
                return true;
            }
        }
        private static IEnumerator<float> GasRoom(Room room, ReferenceHub scp, List<DoorVariant> doors)
        {
            foreach (var door in doors)
            {
                door.ServerChangeLock(DoorLockReason.Isolation, true);
                door.NetworkTargetState = true;
            }
            for (int i = SanyaRemastered.Instance.Config.GasDuration; i > 0f; i--)
            {
                foreach (var player in room.Players)
                {
                    Methods.PlayAmbientSound(7,player);
                    if (SanyaRemastered.Instance.Config.CassieSubtitle)
                    {
                        player.ReferenceHub.GetComponent<SanyaRemasteredComponent>().AddHudCenterDownText(Subtitles.ExtendGazWarn.Replace("{1}", i.ToString()).Replace("{s}", $"{(i <= 1 ? "" : "s")}"), 2);
                    }
                }
                room.Color = FlickerableLightController.DefaultWarheadColor;
                yield return Timing.WaitForSeconds(0.5f);
                room.ResetColor();
                yield return Timing.WaitForSeconds(0.5f);
            }
            room.Color = FlickerableLightController.DefaultWarheadColor;
            foreach (var door in doors)
            {
                door.NetworkTargetState = false;
            }
            if (SanyaRemastered.Instance.Config.CassieSubtitle)
            {
                foreach (var player in room.Players)
                {
                    if (player.IsAlive)
                    {
                        player.ReferenceHub.GetComponent<SanyaRemasteredComponent>().AddHudCenterDownText(Subtitles.ExtendGazActive, 5);
                    }
                }
            }
            for (int i = 0; i < SanyaRemastered.Instance.Config.GasTimerWait * 2; i++)
            {
                foreach (var player in room.Players)
                {
                    if (player.IsAlive && player.Role != RoleType.Scp079)
                    {
                        player.ReferenceHub.playerEffectsController.EnableEffect<Disabled>(10);
                        player.ReferenceHub.playerEffectsController.EnableEffect<Asphyxiated>(10);
                        player.ReferenceHub.playerEffectsController.EnableEffect<Poisoned>(10);
                        player.ReferenceHub.playerStats.DealDamage(new CustomReasonDamageHandler("GAS.", 10));
                        if (player.Role == RoleType.Spectator)
                        {
                            scp.scp079PlayerScript.AddExperience(SanyaRemastered.Instance.Config.GasExpGain);
                            player.ReferenceHub.GetComponent<SanyaRemasteredComponent>().AddHudCenterDownText("Vous étes mort par le gazage de SCP-079", 20);
                        }
                    }
                }
                yield return Timing.WaitForSeconds(0.5f);
            }
            room.ResetColor();
            foreach (var door in doors)
            {
                door.ServerChangeLock(DoorLockReason.Isolation, false);
                door.NetworkTargetState = true;
            }
        }
    }
}