﻿using Dissonance.Integrations.MirrorIgnorance;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Hints;
using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using InventorySystem;
using InventorySystem.Items;
using InventorySystem.Items.Firearms.BasicMessages;
using InventorySystem.Items.ThrowableProjectiles;
using MapGeneration.Distributors;
using MEC;
using Mirror;
using NorthwoodLib;
using NorthwoodLib.Pools;
using PlayableScps;
using RemoteAdmin;
using Respawning;
using SanyaRemastered.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace SanyaRemastered.Functions
{
    internal static class Coroutines
    {
        public static bool isAirBombGoing = false;
        public static bool isActuallyBombGoing = false;
        public static int AirBombWait = 0;
        public static IEnumerator<float> CloseNukeCap()
        {
            var outsite = UnityEngine.Object.FindObjectOfType<AlphaWarheadOutsitePanel>();
            yield return Timing.WaitForSeconds(0.1f);
            outsite.NetworkkeycardEntered = false;
            yield break;
        }
        public static IEnumerator<float> RestartServer()
        {
            yield return Timing.WaitForSeconds(30f);
            if (Player.List.Count() == 0)
            {
                ServerStatic.StopNextRound = ServerStatic.NextRoundAction.Restart;
                ServerConsole.AddOutputEntry(default(ServerOutput.ExitActionRestartEntry));
            }
            else
            {
                if (ServerStatic.StopNextRound == ServerStatic.NextRoundAction.Restart)
                {
                    ServerStatic.StopNextRound = ServerStatic.NextRoundAction.DoNothing;
                    ServerConsole.AddOutputEntry(default(ServerOutput.ExitActionResetEntry));
                }
                else
                {
                    ServerStatic.StopNextRound = ServerStatic.NextRoundAction.Restart;
                    ServerConsole.AddOutputEntry(default(ServerOutput.ExitActionRestartEntry));
                }
            }
        }
        public static IEnumerator<float> AirSupportBomb(bool stop, int timewait = 0, float TimeEnd = -1f)
        {
            AirBombWait = timewait;
            if (isAirBombGoing && stop)
            {
                isAirBombGoing = false;
                isActuallyBombGoing = false;
                RespawnEffectsController.PlayCassieAnnouncement($"The Outside Zone emergency termination sequence as been stop .", false, true);
                if (SanyaRemastered.Instance.Config.CassieSubtitle)
                {
                    Methods.SendSubtitle(Subtitles.AirbombStop, 10);
                }
                DiscordLog.DiscordLog.Instance.LOG += ":airplane_arriving: Arrêt  du bombardement\n";
                yield break;
            }
            else if (isAirBombGoing && !stop)
            {
                yield break;
            }
            DiscordLog.DiscordLog.Instance.LOG += $":airplane_departure: Départ du bombardement dans {AirBombWait / 60:00}min {AirBombWait % 60:00}sec\n";

            isAirBombGoing = true;
            while (AirBombWait > 0)
            {
                Log.Debug("Démarage AirSupport timewait");
                if (AirBombWait == 60f || AirBombWait == 120f || AirBombWait == 300f || AirBombWait == 600f || AirBombWait == 1800f || AirBombWait == 3600f)
                {
                    RespawnEffectsController.PlayCassieAnnouncement($"Alert . The Outside Zone emergency termination sequence activated in t minus {AirBombWait / 60} minutes .", false, true);
                    if (SanyaRemastered.Instance.Config.CassieSubtitle)
                    {
                        Methods.SendSubtitle(Subtitles.AirbombStartingWaitMinutes.Replace("{0}", (AirBombWait / 60).ToString()), 10);
                    }
                }
                else if (AirBombWait == 30f)
                {
                    RespawnEffectsController.PlayCassieAnnouncement($"Alert . The Outside Zone emergency termination sequence activated in t minus 30 seconds .", false, true);
                    if (SanyaRemastered.Instance.Config.CassieSubtitle)
                    {
                        Methods.SendSubtitle(Subtitles.AirbombStartingWait30s, 10);
                    }
                }
                else if (AirBombWait == 0)
                {
                    break;
                }
                if (!isAirBombGoing)
                {
                    RespawnEffectsController.PlayCassieAnnouncement($"The Outside Zone emergency termination sequence as been stop .", false, true);
                    if (SanyaRemastered.Instance.Config.CassieSubtitle)
                    {
                        Methods.SendSubtitle(Subtitles.AirbombStop, 10);
                    }
                    DiscordLog.DiscordLog.Instance.LOG += ":airplane_arriving: Arrêt  du bombardement\n";
                    Log.Info($"[AirSupportBomb] The AirBomb as stop");
                    yield break;
                }
                AirBombWait--;
                yield return Timing.WaitForSeconds(1);
            }
            if (isAirBombGoing)
            {
                isActuallyBombGoing = true;
                Log.Info($"[AirSupportBomb] booting...");
                DiscordLog.DiscordLog.Instance.LOG += ":airplane: Bombardement en cours\n";
                SanyaRemastered.Instance.Handlers.RoundCoroutines.Add(Timing.RunCoroutine(RepeatAirBombSound(), Segment.FixedUpdate));
                RespawnEffectsController.PlayCassieAnnouncement("danger . outside zone emergency termination sequence activated .", false, true);
                if (SanyaRemastered.Instance.Config.CassieSubtitle)
                {
                    Methods.SendSubtitle(Subtitles.AirbombStarting, 10);
                }
                yield return Timing.WaitForSeconds(5f);
                Log.Info($"[AirSupportBomb] charging...");
                {
                    int waitforready = 5;
                    while (waitforready >= 0)
                    {
                        Methods.PlayAmbientSound(7);
                        waitforready--;
                        if (!isAirBombGoing)
                        {
                            isActuallyBombGoing = false;
                        }
                        yield return Timing.WaitForSeconds(1f);
                    }
                }
                Log.Info($"[AirSupportBomb] throwing...");
                while (isAirBombGoing)
                {
                    List<Vector3> randampos = OutsideRandomAirbombPos.Load().OrderBy(x => Guid.NewGuid()).ToList();
                    foreach (var pos in randampos)
                    {
                        Methods.SpawnGrenade(pos, ItemType.GrenadeHE, 0.1f);
                        yield return Timing.WaitForSeconds(0.1f);
                    }
                    if (TimeEnd != -1)
                    {
                        if (TimeEnd <= Time.deltaTime)
                        {
                            isAirBombGoing = false;
                            Log.Info($"[AirSupportBomb] TimeBombing:{TimeEnd}");
                            break;
                        }
                    }
                    yield return Timing.WaitForSeconds(0.25f);
                }
                if (SanyaRemastered.Instance.Config.CassieSubtitle)
                    Methods.SendSubtitle(Subtitles.AirbombEnded, 10);
                RespawnEffectsController.PlayCassieAnnouncement("outside zone termination completed .", false, true);
                isActuallyBombGoing = false;
                DiscordLog.DiscordLog.Instance.LOG += ":airplane_arriving: Arrêt  du bombardement\n";
                Log.Info($"[AirSupportBomb] Ended.");
                yield break;
            }
        }
        public static IEnumerator<float> RepeatAirBombSound()
        {
            while (isActuallyBombGoing)
            {
                CommsHack.AudioAPI.API.PlayFileRaw("/home/scp/.config/EXILED/Configs/AudioAPI/Siren.raw", 0.1f);
                yield return Timing.WaitForSeconds(11);
            }
        }

        public static IEnumerator<float> Scp106CustomTeleport(Scp106PlayerScript scp106PlayerScript, Vector3 position)
        {
            if (!scp106PlayerScript.goingViaThePortal)
            {
                scp106PlayerScript.RpcTeleportAnimation();
                scp106PlayerScript.goingViaThePortal = true;
                yield return Timing.WaitForSeconds(3.5f);
                scp106PlayerScript._hub.playerMovementSync.OverridePosition(position, 0f, false);
                yield return Timing.WaitForSeconds(3.5f);
                if (AlphaWarheadController.Host.detonated && scp106PlayerScript.transform.position.y < 800f)
                    scp106PlayerScript._hub.playerStats.HurtPlayer(new PlayerStats.HitInfo(9000f, "WORLD", DamageTypes.Nuke, 0,false), scp106PlayerScript.gameObject, true);
                scp106PlayerScript.goingViaThePortal = false;
            }
        }
    }
    internal static class Methods
    {
        
        public static HttpClient httpClient = new HttpClient();
        public static void PlayFileRaw(string path, ushort id, float volume, bool _3d, Vector3 position) => PlayStream(File.OpenRead(path), id, volume, _3d, position);

        public static void PlayStream(Stream stream, ushort id, float volume, bool _3d, Vector3 position) => CommsHack.AudioAPI.API.PlayWithParams(stream, id, volume, _3d, position);
        public static void IsCanBeContain(Player player)
        {
            try 
            {
            if (SanyaRemastered.Instance.Config.ContainCommand && player.Team == Team.SCP)
            {
                switch (player.Role)
                {
                    case RoleType.Scp173:
                        {
                            if (Player.List.Any(x => x.Role == RoleType.Scp079))
                            {
                                return;
                            }

                            switch (player.CurrentRoom.Type)
                            {
                                case RoomType.Lcz914:
                                    {
                                        if (!Functions.Extensions.IsInTheBox(player.CurrentRoom.Transform.position - player.Position, 2.9f, -10.2f, 10.1f, -10.2f, 0, -5f, player.CurrentRoom.Transform.rotation.eulerAngles.y))
                                        {
                                            return;
                                        }
                                        var door = player.CurrentRoom.Doors.First(x => x.Nametag == "914");
                                        if (door.Base.GetExactState() == 0f)
                                        {
                                            if (!player.GameObject.TryGetComponent<ContainScpComponent>(out _))
                                            {
                                                var containScpComponent = player.GameObject.AddComponent<ContainScpComponent>();
                                                containScpComponent.doors.Add(door);
                                                containScpComponent.CassieAnnounceContain = "SCP 1 7 3 as been contained in the Containment chamber of SCP 9 1 4";
                                            }
                                            return;
                                        }
                                        return;

                                    }
                                    case RoomType.Lcz173:
                                    {
                                            if (!Functions.Extensions.IsInTheBox(player.CurrentRoom.Transform.position - player.Position, -16.4f, -30.2f, -5.2f, -16.7f, -16.8f, -22.3f, player.CurrentRoom.Transform.rotation.eulerAngles.y))
                                            {
                                                return;
                                            }
                                            var door = player.CurrentRoom.Doors.First(x => x.Nametag == "173_GATE");
                                            if (door.Base.GetExactState() == 0f && !door.Base.GetComponent<Timed173PryableDoor>()._stopwatch.IsRunning)
                                            {
                                                if (!player.GameObject.TryGetComponent<ContainScpComponent>(out _))
                                                {
                                                    var containScpComponent = player.GameObject.AddComponent<ContainScpComponent>();
                                                    containScpComponent.doors.Add(door);
                                                    containScpComponent.CassieAnnounceContain = "SCP 1 7 3 as been contained in there containment chamber";
                                                }
                                                return;
                                            }
                                            return;
                                    }
                                    case RoomType.Lcz012:
                                    {
                                        if (!Functions.Extensions.IsInTheBox(player.CurrentRoom.Transform.position - player.Position, 10.2f, -9.6f, 8.2f, 2.7f, 8f, -3f, player.CurrentRoom.Transform.rotation.eulerAngles.y)
                                            && !Functions.Extensions.IsInTheBox(player.CurrentRoom.Transform.position - player.Position, 9.8f, -8.9f, 7.8f, -10f, 8f, 2.5f, player.CurrentRoom.Transform.rotation.eulerAngles.y))
                                        {
                                            return;
                                        }
                                        var door = player.CurrentRoom.Doors.First(x => x.Nametag == "012");
                                        {
                                            if (door.Base.GetExactState() == 0f)
                                            {
                                                if (!player.GameObject.TryGetComponent<ContainScpComponent>(out _))
                                                {
                                                    var containScpComponent = player.GameObject.AddComponent<ContainScpComponent>();
                                                    containScpComponent.doors.Add(door);
                                                    containScpComponent.CassieAnnounceContain = "SCP 1 7 3 as been contained in the Containment chamber of SCP 0 1 2";
                                                }
                                                return;
                                            }
                                        }
                                        return;
                                    }
                                case RoomType.HczArmory:
                                    {
                                        if (!Functions.Extensions.IsInTheBox(player.CurrentRoom.Transform.position - player.Position, 0.1f, -5.6f, 2.9f, -2.8f, 0f, -5f, player.CurrentRoom.Transform.rotation.eulerAngles.y))
                                        {
                                            return;
                                        }
                                        var door = player.CurrentRoom.Doors.First(x => x.Nametag == "HCZ_ARMORY");
                                        {
                                            if (door.Base.GetExactState() == 0f)
                                            {
                                                if (!player.GameObject.TryGetComponent<ContainScpComponent>(out _))
                                                {
                                                    var containScpComponent = player.GameObject.AddComponent<ContainScpComponent>();
                                                    containScpComponent.doors.Add(door);
                                                    containScpComponent.CassieAnnounceContain = "SCP 1 7 3 as been contained in the Armory of Heavy containment Zone";
                                                }
                                                return;
                                            }
                                        }
                                        return;
                                    }
                                case RoomType.LczArmory:
                                    {
                                        if (!Functions.Extensions.IsInTheBox(player.CurrentRoom.Transform.position - player.Position, 1.2f, -9.5f, 6f, -7f, -1f, -10f, player.CurrentRoom.Transform.rotation.eulerAngles.y))
                                        {
                                            return;
                                        }
                                        var door = player.CurrentRoom.Doors.First(x => x.Nametag == "LCZ_ARMORY");
                                        if (door.Base.GetExactState() == 0f)
                                        {
                                            if (!player.GameObject.TryGetComponent<ContainScpComponent>(out _))
                                            {
                                                var containScpComponent = player.GameObject.AddComponent<ContainScpComponent>();
                                                containScpComponent.doors.Add(door);
                                                containScpComponent.CassieAnnounceContain = "SCP 1 7 3 as been contained in the Armory of Light Containment Zone";
                                            }
                                            return;
                                        }
                                        return;
                                    }
                                case RoomType.HczHid:
                                    {
                                        if (!Functions.Extensions.IsInTheBox(player.CurrentRoom.Transform.position - player.Position, 3.7f, -4.0f, 9.8f, 7.4f, 0f, -5f, player.CurrentRoom.Transform.rotation.eulerAngles.y))
                                        {
                                            return;
                                        }
                                        var door = player.CurrentRoom.Doors.First(x => x.Nametag == "HID");
                                        if (door.Base.GetExactState() == 0f)
                                        {
                                            if (!player.GameObject.TryGetComponent<ContainScpComponent>(out _))
                                            {
                                                var containScpComponent = player.GameObject.AddComponent<ContainScpComponent>();
                                                containScpComponent.doors.Add(door);
                                                containScpComponent.CassieAnnounceContain = "SCP 1 7 3 as been contained in the Storage of Micro H I D";
                                            }
                                        }
                                        return;
                                    }
                                case RoomType.Hcz049:
                                    {
                                        if (!Functions.Extensions.IsInTheBox(player.CurrentRoom.Transform.position - player.Position, -3f, -8.6f, -4.6f, -10.1f, -260f, -270f, player.CurrentRoom.Transform.rotation.eulerAngles.y))
                                        {
                                            return;
                                        }
                                        var door = player.CurrentRoom.Doors.First(x => x.Nametag == "049_ARMORY");
                                        if (door.Base.GetExactState() == 0f)
                                        {
                                            if (!player.GameObject.TryGetComponent<ContainScpComponent>(out _))
                                            {
                                                var containScpComponent = player.GameObject.AddComponent<ContainScpComponent>();
                                                containScpComponent.doors.Add(door);
                                                containScpComponent.CassieAnnounceContain = "SCP 1 7 3 as been contained in the Armory of SCP 0 4 9";
                                            }
                                            return;
                                        }
                                        return;
                                    }
                                case RoomType.Hcz106:
                                    {
                                        if (Functions.Extensions.IsInTheBox(player.CurrentRoom.Transform.position - player.Position, 9.6f, -24.5f, 30.8f, -1.9f, 20f, 10f, player.CurrentRoom.Transform.rotation.eulerAngles.y))
                                        {
                                            var door = player.CurrentRoom.Doors.First(x => x.Nametag == "106_BOTTOM");
                                            {
                                                if (door.Base.GetExactState() == 0f)
                                                {
                                                    if (!player.GameObject.TryGetComponent<ContainScpComponent>(out _))
                                                    {
                                                        var containScpComponent = player.GameObject.AddComponent<ContainScpComponent>();
                                                        containScpComponent.doors.Add(door);
                                                        containScpComponent.CassieAnnounceContain = "SCP 1 7 3 as been contained in the Containment chamber of SCP 1 0 6";
                                                    }
                                                    return;
                                                }
                                                else
                                                {
                                                    return;
                                                }
                                            }
                                        }
                                        if (Functions.Extensions.IsInTheBox(player.CurrentRoom.Transform.position - player.Position, -25.6f, -33.7f, 32f, -4.6f, 20f, -10f, player.CurrentRoom.Transform.rotation.eulerAngles.y))
                                        {
                                            if (Player.List.Any((p) => p.Role == RoleType.Scp106))
                                            {
                                                return;
                                            }
                                            var door1 = player.CurrentRoom.Doors.First(x => x.Nametag == "106_PRIMARY");
                                            var door2 = player.CurrentRoom.Doors.First(x => x.Nametag == "106_SECOND");
                                            if (door1.Base.GetExactState() == 0f && door2.Base.GetExactState() == 0f)
                                            {
                                                if (!player.GameObject.TryGetComponent<ContainScpComponent>(out _))
                                                {
                                                    var containScpComponent = player.GameObject.AddComponent<ContainScpComponent>();
                                                    containScpComponent.doors.Add(door1);
                                                    containScpComponent.doors.Add(door2);
                                                    containScpComponent.CassieAnnounceContain = "SCP 1 7 3 as been contained in the Containment chamber of SCP 1 0 6";
                                                }
                                                return;
                                            }
                                            else
                                            {
                                                return;
                                            }
                                        }
                                        return;
                                    }
                                case RoomType.Hcz079:
                                    {
                                        byte TEST = 0;
                                        if (!Functions.Extensions.IsInTheBox(player.CurrentRoom.Transform.position - player.Position, 10.3f, -8.2f, 22.5f, 5.2f, 10f, 0f, player.CurrentRoom.Transform.rotation.eulerAngles.y))
                                        {
                                            TEST = 1;
                                        }
                                        if (TEST != 1)
                                        {
                                            if (!Functions.Extensions.IsInTheBox(player.CurrentRoom.Transform.position - player.Position, -12.3f, -20.8f, 18.7f, -2.5f, 7f, 0f, player.CurrentRoom.Transform.rotation.eulerAngles.y))
                                            {
                                                TEST = 2;
                                            }
                                        }
                                        if (TEST == 1)
                                        {
                                            var door = player.CurrentRoom.Doors.First(x => x.Nametag == "079_SECOND");
                                            if (door.Base.GetExactState() == 0f)
                                            {
                                                if (!player.GameObject.TryGetComponent<ContainScpComponent>(out _))
                                                {
                                                    var containScpComponent = player.GameObject.AddComponent<ContainScpComponent>();
                                                    containScpComponent.doors.Add(door);
                                                    containScpComponent.CassieAnnounceContain = "SCP 1 7 3 as been contained in the Containment chamber of SCP 0 7 9";
                                                }
                                                return;
                                            }
                                            return;
                                        }
                                        if (TEST == 2)
                                        {
                                            var door = player.CurrentRoom.Doors.First(x => x.Nametag == "079_FIRST");
                                            {
                                                if (door.Base.GetExactState() == 0f)
                                                {
                                                    if (!player.GameObject.TryGetComponent<ContainScpComponent>(out _))
                                                    {
                                                        var containScpComponent = player.GameObject.AddComponent<ContainScpComponent>();
                                                        containScpComponent.doors.Add(door);
                                                        containScpComponent.CassieAnnounceContain = "SCP 1 7 3 as been contained in the Containment chamber of SCP 0 7 9";
                                                    }
                                                    return;
                                                }
                                                return;
                                            }
                                        }
                                        return;
                                    }
                                default:
                                    {
                                        return;
                                    }
                            }
                        }
                    case RoleType.Scp096:
                        {
                            Log.Debug($"096 state : {(player.ReferenceHub.scpsController.CurrentScp as PlayableScps.Scp096).PlayerState}", SanyaRemastered.Instance.Config.IsDebugged);
                            if (Scp096PlayerState.Docile != (player.ReferenceHub.scpsController.CurrentScp as PlayableScps.Scp096).PlayerState
                                && Scp096PlayerState.TryNotToCry != (player.ReferenceHub.scpsController.CurrentScp as PlayableScps.Scp096).PlayerState)
                            {
                                return;
                            }
                            if (player.CurrentRoom.Type == RoomType.Hcz096)
                            {
                                if (!Functions.Extensions.IsInTheBox(player.CurrentRoom.Transform.position - player.Position, 4.4f, 0.5f, 1.9f, -1.9f, 0f, -5f, player.CurrentRoom.Transform.rotation.eulerAngles.y))
                                {
                                    return;
                                }
                                var door = player.CurrentRoom.Doors.First(x => x.Nametag == "096");
                                if (door.Base.GetExactState() == 0f)
                                {
                                    if (!player.GameObject.TryGetComponent<ContainScpComponent>(out _))
                                    {
                                        var containScpComponent = player.GameObject.AddComponent<ContainScpComponent>();
                                        containScpComponent.doors.Add(door);
                                        containScpComponent.CassieAnnounceContain = "SCP 0 9 6 as been contained in there containment chamber";
                                    }
                                    return;
                                }
                                return;
                            }
                            else
                            {
                                return;
                            }
                        }
                    case RoleType.Scp049:
                        {
                            if (player.CurrentRoom.Type == RoomType.Hcz049)
                            {
                                if (!Functions.Extensions.IsInTheBox(player.CurrentRoom.Transform.position - player.Position, -3f, -8.6f, -4.6f, -10.1f, -260f, -270f, player.CurrentRoom.Transform.rotation.eulerAngles.y))
                                {
                                    return;
                                }
                                var door = player.CurrentRoom.Doors.First(x => x.Base is PryableDoor);
                                {
                                    if (door.Base.GetExactState() == 0f)
                                    {
                                        if (!player.GameObject.TryGetComponent<ContainScpComponent>(out _))
                                        {
                                            var containScpComponent = player.GameObject.AddComponent<ContainScpComponent>();
                                            containScpComponent.doors.Add(door);
                                            containScpComponent.CassieAnnounceContain = "SCP 0 4 9 as been contained in there containment chamber";
                                        }
                                        return;
                                    }
                                    else
                                    {
                                        return;
                                    }
                                }
                            }
                            else
                            {
                                return;
                            }
                        }
                    case RoleType.Scp93953:
                    case RoleType.Scp93989:
                        {
                            if (player.CurrentRoom.Type == RoomType.Hcz106)
                            {
                                if (!Functions.Extensions.IsInTheBox(player.CurrentRoom.Transform.position - player.Position, 9.6f, -24.4f, 30.8f, -1.9f, 20f, 13f, player.CurrentRoom.Transform.rotation.eulerAngles.y))
                                {
                                    return;
                                }
                                var door = player.CurrentRoom.Doors.First(x => x.Nametag == "106_BOTTOM");
                                {
                                    if (door.Base.GetExactState() == 0f)
                                    {
                                        if (!player.GameObject.TryGetComponent<ContainScpComponent>(out _))
                                        {
                                            var containScpComponent = player.GameObject.AddComponent<ContainScpComponent>();
                                            containScpComponent.doors.Add(door);
                                            containScpComponent.CassieAnnounceContain = "SCP 9 3 9 as been contained in the Containment Chamber of SCP 1 0 6";
                                        }
                                        return;

                                    }
                                    else
                                    {
                                        return;
                                    }
                                }
                            }
                            return;
                        }
                }
                return;
            }
            return;
            }
            catch (Exception ex)
            {
                Log.Error("Error in IsCanBeContain" + ex);
            }
        }

        public static void SpawnDummyModel(Vector3 position, Quaternion rotation, RoleType role, Vector3 scale)
        {
            GameObject obj = UnityEngine.Object.Instantiate(NetworkManager.singleton.spawnPrefabs.FirstOrDefault(p => p.gameObject.name == "Player"));
            CharacterClassManager ccm = obj.GetComponent<CharacterClassManager>();
            if (ccm == null)
                Log.Error("CCM is null, this can cause problems!");
            ccm.CurClass = role;
            ccm.GodMode = true;
            ccm.RefreshPlyModel(role);
            obj.GetComponent<NicknameSync>().Network_myNickSync = "Dummy";
            obj.GetComponent<QueryProcessor>().PlayerId = 9999;
            obj.GetComponent<QueryProcessor>().NetworkPlayerId = 9999;
            obj.transform.localScale = scale;
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            NetworkServer.Spawn(obj);
        }
        public static bool IsStuck(Vector3 pos)
        {
            bool result = false;
            foreach (Collider collider in Physics.OverlapBox(pos, new Vector3(0.4f, 1f, 0.4f), new Quaternion(0f, 0f, 0f, 0f)))
            {
                bool flag = collider.name.Contains("Hitbox") || collider.name.Contains("mixamorig") || collider.name.Equals("Player") || collider.name.Equals("PlyCenter") || collider.name.Equals("Antijumper");
                if (!flag)
                {
                    Log.Debug($"Detect:{collider.name}", SanyaRemastered.Instance.Config.IsDebugged);
                    result = true;
                }
            }
            return result;
        }
        public static void SpawnGrenade(Vector3 position, ItemType Grenade, float fusedur = -1, Player player = null)
        {
            try
            {
                if (Grenade == ItemType.GrenadeFlash)
                {
                    if (fusedur != -1)
                        new FlashGrenade(ItemType.GrenadeFlash, player) { FuseTime = fusedur }.SpawnActive(position, player);
                    else
                        new FlashGrenade(ItemType.GrenadeFlash, player).SpawnActive(position, player);
                }
                else if (fusedur != -1)
                    new ExplosiveGrenade(Grenade, player) { FuseTime = fusedur }.SpawnActive(position, player);
                else
                    new ExplosiveGrenade(Grenade, player).SpawnActive(position, player);
            }
            catch (Exception ex)
            {
                Log.Error($"[SpawnGrenade] Error: {ex}");
            }

        }
        public static int GetRandomIndexFromWeight(int[] list)
        {
            int sum = 0;

            foreach (int i in list)
            {
                if (i <= 0) continue;
                sum += i;
            }

            int random = UnityEngine.Random.Range(0, sum);
            for (int i = 0; i < list.Length; i++)
            {
                if (list[i] <= 0) continue;

                if (random < list[i])
                {
                    return i;
                }
                random -= list[i];
            }
            return -1;
        }
		public static void SendTextHintNotEffect(this Player player, string text, float time)
		{
			player.ReferenceHub.hints.Show(new TextHint(text, new HintParameter[] { new StringHintParameter(string.Empty) }, null, time));
		}

        public static void SendSubtitle(string text, ushort time, ReferenceHub target = null)
        {
            Broadcast brd = PlayerManager.localPlayer.GetComponent<Broadcast>();
            if (target != null)
            {
                brd.TargetClearElements(target.characterClassManager.connectionToClient);
                brd.TargetAddElement(target.characterClassManager.connectionToClient, text, time, Broadcast.BroadcastFlags.Normal);
            }
            else
            {
                brd.RpcClearElements();
                brd.RpcAddElement(text, time, Broadcast.BroadcastFlags.Normal);
            }
        }

        public static void PlayAmbientSound(int id)
        {
            PlayerManager.localPlayer.GetComponent<AmbientSoundPlayer>().RpcPlaySound(Mathf.Clamp(id, 0, 32));
        }

        public static void TargetSendRpc<T>(this ReferenceHub sendto, T target, string rpcName, NetworkWriter writer) where T : NetworkBehaviour
        {
            var msg = new RpcMessage
            {
                netId = target.netId,
                componentIndex = target.ComponentIndex,
                functionHash = target.GetType().FullName.GetStableHashCode() * 503 + rpcName.GetStableHashCode(),
                payload = writer.ToArraySegment()
            };
            sendto?.characterClassManager.connectionToClient.Send(msg, 0);
        }
        
        public static void MoveNetworkIdentityObject(NetworkIdentity identity, Vector3 pos)
        {
            identity.gameObject.transform.position = pos;
            ObjectDestroyMessage objectDestroyMessage = new ObjectDestroyMessage();
            objectDestroyMessage.netId = identity.netId;
            foreach (var ply in Player.List)
            {
                ply.Connection.Send(objectDestroyMessage, 0);
                typeof(NetworkServer).GetMethod("SendSpawnMessage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).Invoke(null, new object[] { identity, ply.Connection });
            }
        }
        public static bool CanLookToPlayer(this Camera079 camera, ReferenceHub player)
        {
            Player EPlayer = Player.Dictionary[player.gameObject];
            if (EPlayer.Role == RoleType.Spectator || EPlayer.Role == RoleType.Scp079 || EPlayer.Role == RoleType.None)
                return false;

            Vector3 vector = player.transform.position - camera.transform.position;
            float num = Vector3.Dot(camera.head.transform.forward, vector);

            return (num >= 0f && num * num / vector.sqrMagnitude > 0.4225f)
                && Physics.Raycast(camera.transform.position, vector, out RaycastHit raycastHit, 100f, -117407543)
                && raycastHit.transform.name == player.name;
        }

        public static int GetMTFTickets()
        {
            if (CustomLiteNetLib4MirrorTransport.DelayConnections) return -1;
            return RespawnTickets.Singleton.GetAvailableTickets(SpawnableTeamType.NineTailedFox);
        }

        public static int GetCITickets()
        {
            if (CustomLiteNetLib4MirrorTransport.DelayConnections) return -1;
            return RespawnTickets.Singleton.GetAvailableTickets(SpawnableTeamType.NineTailedFox);
        }

   

        // API, dont change
        public static int GetComponentIndex(NetworkIdentity identity, Type type)
        {
            return Array.FindIndex(identity.NetworkBehaviours, (x) => x.GetType() == type);
        }

        // API, dont change
        public static ulong GetDirtyBit(Type targetType, string PropertyName)
        {
            var bytecodes = targetType.GetProperty(PropertyName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)?.GetSetMethod().GetMethodBody().GetILAsByteArray();
            return bytecodes[Array.FindLastIndex(bytecodes, x => x == System.Reflection.Emit.OpCodes.Ldc_I8.Value) + 1];
        }

       
        public static void SendTargetRPCInternal(NetworkConnection conn, Type invokeClass, string rpcName, NetworkWriter writer, int channelId, uint netid, int componentindex)
        {
            if (!NetworkServer.active)
            {
                Debug.LogError("TargetRPC Function " + rpcName + " called on client.");
                return;
            }
            RpcMessage msg = new RpcMessage
            {
                netId = netid,
                componentIndex = componentindex,
                functionHash = GetMethodHash(invokeClass, rpcName),
                payload = writer.ToArraySegment()
            };
            conn.Send<RpcMessage>(msg, channelId);
        }
        private static int GetMethodHash(Type invokeClass, string methodName)
        {
            return invokeClass.FullName.GetStableHashCode() * 503 + methodName.GetStableHashCode();
        }

        public static GameObject SpawnDummy(RoleType role, Vector3 pos, Quaternion rot)
        {
            GameObject gameObject = UnityEngine.Object.Instantiate(NetworkManager.singleton.spawnPrefabs.FirstOrDefault(p => p.gameObject.name == "Player"));
            CharacterClassManager ccm = gameObject.GetComponent<CharacterClassManager>();
            ccm.CurClass = role;
            ccm.RefreshPlyModel();
            gameObject.GetComponent<NicknameSync>().Network_myNickSync = "Yamato";
            gameObject.GetComponent<QueryProcessor>().NetworkPlayerId = 9999;
            gameObject.transform.position = pos;
            gameObject.transform.rotation = rot;
            NetworkServer.Spawn(gameObject);
            return gameObject;
        }
    }
    internal static class Extensions
    {
        public static bool IsHuman(this Player player)
        {
            return player.Team != Team.SCP && player.Team != Team.RIP;
        }
        
        public static bool IsInTheBox(Vector3 posroom,float x1,float x2,float z1, float z2,float y1, float y2,float rotation)
        {
            Vector3 end;
            Vector3 end2;
            if (rotation == 0f)
            {
                end = new Vector3(x1, y1, z1);
                end2 = new Vector3(x2, y2, z2);
            }
            else if (rotation == 90f)
            {
                end = new Vector3(z1, y1, -x2);
                end2 = new Vector3(z2, y2, -x1);
            }
            else if (rotation == 180f)
            {
                end = new Vector3(-x2, y1, -z2);
                end2 = new Vector3(-x1, y2, -z1);
            }
            else
            {
                end = new Vector3(-z2, y1, x1);
                end2 = new Vector3(-z1, y2, x2);
            }
            if (SanyaRemastered.Instance.Config.IsDebugged)
            {

                Log.Debug(end2.x < posroom.x);
                Log.Debug(posroom.x < end.x);
                Log.Debug(end2.y < posroom.y);
                Log.Debug(posroom.y < end.y);
                Log.Debug(end2.z < posroom.z);
                Log.Debug(posroom.z < end.z);
            }
            if (end2.x < posroom.x && posroom.x < end.x && end2.y < posroom.y && posroom.y < end.y && end2.z < posroom.z && posroom.z < end.z)
            {
                return true;
            }
            return false;
        }
        public static void SendHitmarker(this Player player, float size = 1f) => Hitmarker.SendHitmarker(player.Connection, size);

        public static int GetHealthAmountPercent(this Player player)
        {
            return (int)(100f - (player.ReferenceHub.playerStats.GetHealthPercent() * 100f));
        }
        public static void OpenReportWindow(this Player player, string text)
        {
            player.ReferenceHub.GetComponent<GameConsoleTransmission>().SendToClient(player.Connection, "[REPORTING] " + text, "white");
        }
        public static IEnumerable<Camera079> GetNearCams(this Vector3 position)
        {
            foreach (var cam in Scp079PlayerScript.allCameras)
            {
                var dis = Vector3.Distance(position, cam.transform.position);
                if (dis <= 15f)
                {
                    yield return cam;
                }
            }
        }
        public static bool IsExmode(this Player player) => player.SessionVariables.ContainsKey("scp079_advanced_mode");

        public static bool IsList(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);
        }

        public static bool IsDictionary(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>);
        }

        public static Type GetListArgs(this Type type)
        {
            return type.GetGenericArguments()[0];
        }

        public static T GetRandomOne<T>(this List<T> list)
        {
            return list[UnityEngine.Random.Range(0, list.Count)];
        }

        public static T Random<T>(this IEnumerable<T> ie)
        {
            if (!ie.Any()) return default;
            return ie.ElementAt(SanyaRemastered.Instance.Random.Next(ie.Count()));
        }
        public static string FormatArguments(ArraySegment<string> sentence, int index)
        {
            StringBuilder SB = StringBuilderPool.Shared.Rent();
            foreach (string word in sentence.Segment(index))
            {
                SB.Append(word);
                SB.Append(" ");
            }
            string msg = SB.ToString();
            StringBuilderPool.Shared.Return(SB);
            return msg;
        }
    }
}