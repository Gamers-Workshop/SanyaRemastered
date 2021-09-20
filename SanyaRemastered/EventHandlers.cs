using CustomPlayerEffects;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Pickups;
using InventorySystem.Items.ThrowableProjectiles;
using LightContainmentZoneDecontamination;
using LiteNetLib.Utils;
using MapGeneration.Distributors;
using MEC;
using Mirror;
using PlayableScps;
using Respawning;
using SanyaRemastered.Data;
using SanyaRemastered.Functions;
using SanyaRemastered.Patches;
using Scp914;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

namespace SanyaRemastered
{
    public class EventHandlers
    {
        public EventHandlers(SanyaRemastered plugin) => this.plugin = plugin;
        internal readonly SanyaRemastered plugin;
        internal List<CoroutineHandle> roundCoroutines = new List<CoroutineHandle>();
        internal bool loaded = false;



        /** Update **/
        internal IEnumerator<float> EverySecond()
        {
            while (true)
            {
                try
                {
                    //SCP-079's Radar Humain
                    if (plugin.Config.Scp079ExtendEnabled && plugin.Config.Scp079spot && plugin.Config.Scp079ExtendLevelSpot > 0)
                    {
                        List<Player> foundplayers = new List<Player>();
                        var scp079 = Scp079PlayerScript.instances.Count != 0 ? Player.Get(Scp079PlayerScript.instances.First().gameObject) : null;
                        string message = string.Empty;
                        if (scp079 != null && scp079.IsExmode() && last079cam != scp079.Camera)
                        {
                            foreach (var player in Player.List.Where(x => x.Team != Team.RIP && x.Team != Team.SCP))
                            {
                                if (player.ReferenceHub.characterClassManager.IsHuman() && scp079.CurrentRoom != null && scp079.CurrentRoom.Players.Contains(player))
                                {
                                    last079cam = scp079.Camera;
                                    foundplayers.Add(player);
                                    message = $"<color=#bbee00><size=25>SCP-079が{player.ReferenceHub.characterClassManager.CurRole.fullName}を発見した\n場所：{player.CurrentRoom.Type}</color></size>\n";
                                    break;
                                }
                            }
                        }

                        if (!string.IsNullOrEmpty(message))
                        {
                            foreach (var scp in Player.Get(Team.SCP))
                            {
                                scp.ReferenceHub.GetComponent<SanyaRemasteredComponent>().AddHudCenterDownText(message, 5);
                            }
                        }
                    }
                    if (plugin.Config.PainEffectStart > 0)
                    {
                        foreach (Player player in Player.List)
                        {
                            if (player.IsHuman() && (100f - (player.ReferenceHub.playerStats.GetHealthPercent() * 100f) <= SanyaRemastered.Instance.Config.PainEffectStart))
                            {
                                player.EnableEffect<Disabled>(1.2f);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Error($"[EverySecond] {e}");
                }
                //Chaque seconde
                yield return Timing.WaitForSeconds(1f);
            }
        }
        internal IEnumerator<float> Every30minute()
        {
            yield return Timing.WaitForSeconds(30f);
            while (true)
            {
                try
                {
                    MemoryMetrics metrics = Ram.MemoryService.CurrentTotalMetrics;
                    DiscordLog.DiscordLog.Instance.LOGStaff += $"Total Ram Usage: {metrics.Used / 1024:0.##}/{metrics.Total / 1024:0.##} Go [{((metrics.Used / metrics.Total) * 100):0.##}%]\n";
                    double slRamUsage = Ram.MemoryService.CurrentProcessRamUsage;
                    DiscordLog.DiscordLog.Instance.LOGStaff += $"SL Ram Usage: {slRamUsage / 1024:0.##}/{metrics.Total / 1024:0.##} Go [{((slRamUsage / metrics.Total) * 100):0.##}%]\n";
                    if (Player.List.Count() == 0)
                    {
                        if (plugin.Config.RamRestartNoPlayer < slRamUsage / 1024 && plugin.Config.RamRestartNoPlayer > 0)
                        {
                            DiscordLog.DiscordLog.Instance.LOGStaff += $"**The Ram exceed the limit NP**:\n";
                            RoundCoroutines.Add(Timing.RunCoroutine(Coroutines.RestartServer(), Segment.RealtimeUpdate));
                        }
                    }
                    else
                    {
                        if (plugin.Config.RamRestartWithPlayer < slRamUsage / 1024 && plugin.Config.RamRestartWithPlayer > 0)
                        {
                            DiscordLog.DiscordLog.Instance.LOGStaff += $"**The Ram exceed the limit WP**:\n";
                            RoundCoroutines.Add(Timing.RunCoroutine(Coroutines.RestartServer(), Segment.RealtimeUpdate));
                        }
                    }

                }
                catch (Exception e)
                {
                    Log.Error($"[Every30minutes] {e}");
                }
                //Chaque 30 Minutes
                yield return Timing.WaitForSeconds(1800f);
            }
        }
        /** Flag Params **/
        private readonly int grenade_pickup_mask = 1049088;
        public bool StopRespawn = false;
        public List<Vector3> DecalList = new List<Vector3>();
        public List<Dictionary<Player, int>> Scp914Effect = new List<Dictionary<Player, int>>();
        //private List<GameObject> PlayerHasEnrage096InPocket = new List<GameObject>();

        /** RoundVar **/
        private FlickerableLightController flickerableLightController = null;
        internal bool IsEnableBlackout = false;
        private uint playerlistnetid = 0;
        private Vector3 nextRespawnPos = Vector3.zero;
        private Camera079 last079cam = null;

        internal List<CoroutineHandle> RoundCoroutines { get => roundCoroutines; set => roundCoroutines = value; }

        public void OnWaintingForPlayers()
        {
            if (SanyaRemastered.Instance.Config.RamInfo)
            {
                RoundCoroutines.Add(Timing.RunCoroutine(Every30minute(), Segment.RealtimeUpdate));
            }
            loaded = true;
            RoundCoroutines.Add(Timing.RunCoroutine(EverySecond(), Segment.FixedUpdate));

            DecalList.Clear();
            Coroutines.isAirBombGoing = false;
            Coroutines.isActuallyBombGoing = false;
            Coroutines.AirBombWait = 0;

            flickerableLightController = UnityEngine.Object.FindObjectOfType<FlickerableLightController>();

            last079cam = null;

            if (SanyaRemastered.Instance.Config.TeslaRange != 5.5f)
            {
                foreach (var tesla in UnityEngine.Object.FindObjectsOfType<TeslaGate>())
                {
                    tesla.sizeOfTrigger = SanyaRemastered.Instance.Config.TeslaRange;
                }
            }
            if (plugin.Config.DisablePlayerLists)
            {
                foreach (var identity in UnityEngine.Object.FindObjectsOfType<NetworkIdentity>())
                {
                    if (identity.name == "PlayerList")
                    {
                        playerlistnetid = identity.netId;
                    }
                }
            }
            if (plugin.Config.Scp096Real)
            {
                foreach (var cp in UnityEngine.Object.FindObjectsOfType<CheckpointDoor>())
                {
                    if (cp.TryGetComponent(out DoorNametagExtension name))
                    {
                        foreach (var door in cp._subDoors)
                        {
                            if (door is BreakableDoor dr)
                            {
                                dr._remainingHealth = 750f;
                                dr._ignoredDamageSources |= DoorDamageType.Scp096;
                                dr._ignoredDamageSources |= DoorDamageType.ServerCommand;
                                dr._ignoredDamageSources |= DoorDamageType.Grenade;
                            }
                        }
                    }
                }
            }
            if (plugin.Config.AddDoorsOnSurface)
            {
                Vector3 DoorScale = new Vector3(1f, 1f, 1.8f);
                var LCZprefab = UnityEngine.Object.FindObjectsOfType<MapGeneration.DoorSpawnpoint>().First(x => x.TargetPrefab.name.Contains("LCZ"));
                var EZprefab = UnityEngine.Object.FindObjectsOfType<MapGeneration.DoorSpawnpoint>().First(x => x.TargetPrefab.name.Contains("EZ"));
                var HCZprefab = UnityEngine.Object.FindObjectsOfType<MapGeneration.DoorSpawnpoint>().First(x => x.TargetPrefab.name.Contains("HCZ"));

                // Couloir spawn Chaos
                var door1 = UnityEngine.Object.Instantiate(LCZprefab.TargetPrefab, new UnityEngine.Vector3(14.425f, 995.2f, -43.525f), Quaternion.Euler(Vector3.zero));
                var door2 = UnityEngine.Object.Instantiate(LCZprefab.TargetPrefab, new UnityEngine.Vector3(14.425f, 995.2f, -23.2f), Quaternion.Euler(Vector3.zero));
                // Exit
                var door3 = UnityEngine.Object.Instantiate(EZprefab.TargetPrefab, new UnityEngine.Vector3(176.2f, 983.24f, 35.23f), Quaternion.Euler(Vector3.up * 180f));
                var door4 = UnityEngine.Object.Instantiate(EZprefab.TargetPrefab, new UnityEngine.Vector3(174.4f, 983.24f, 29.1f), Quaternion.Euler(Vector3.up * 90f));

                door1.transform.localScale = DoorScale;
                door2.transform.localScale = DoorScale;

                door3.transform.localScale = DoorScale;
                door4.transform.localScale = DoorScale;

                /*//Spawn Main du serpent 
                var door5 = UnityEngine.Object.Instantiate(HCZprefab.TargetPrefab, new UnityEngine.Vector3(1.15f, 1000f, 4.8f), Quaternion.Euler(Vector3.up * 0f));
                (door5 as BreakableDoor)._ignoredDamageSources |= DoorDamageType.Grenade;
                var door6 = UnityEngine.Object.Instantiate(HCZprefab.TargetPrefab, new UnityEngine.Vector3(-1.27f, 1000f, 4.8f), Quaternion.Euler(Vector3.up * 360f));
                (door6 as BreakableDoor)._ignoredDamageSources |= DoorDamageType.Grenade;

                door5.gameObject.AddComponent<DoorNametagExtension>().UpdateName("GATE_EX_R");
                door6.gameObject.AddComponent<DoorNametagExtension>().UpdateName("GATE_EX_L");*/

                NetworkServer.Spawn(door1.gameObject);
                NetworkServer.Spawn(door2.gameObject);
                NetworkServer.Spawn(door3.gameObject);
                NetworkServer.Spawn(door4.gameObject);
                /*NetworkServer.Spawn(door5.gameObject);
                NetworkServer.Spawn(door6.gameObject);*/
                try
                {
                    /*foreach (var identity in UnityEngine.Object.FindObjectsOfType<NetworkIdentity>())
                    {
                        if (identity.name == "Gate")
                        {
                            identity.gameObject.transform.position = new UnityEngine.Vector3(14.425f, 995.2f, -23.2f);
                            ObjectDestroyMessage objectDestroyMessage = new ObjectDestroyMessage
                            {
                                netId = identity.netId
                            };
                            foreach (var ply in Player.List)
                            {
                                ply.Connection.Send(objectDestroyMessage, 0);
                                typeof(NetworkServer).GetMethod("SendSpawnMessage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).Invoke(null, new object[] { identity, ply.Connection });
                            }
                        }
                    }
                    foreach (KeyValuePair<uint,NetworkIdentity> Test in Mirror.NetworkIdentity.spawned)
                    {
                        Log.Info($"NetworkIdentity Spawned Key; {Test.Key} {Test.Value.gameObject} AssetId {Test.Value.assetId}");
                        if (Test.Value.gameObject.ToString().Contains("Door"))
                            GameObject.Destroy(Test.Value.gameObject);
                    }*/

                }
                catch (Exception e)
                {
                    Log.Error("Sanya Plugin [Spawn Tesla]" + e);
                }

            }
            Log.Info($"[OnWaintingForPlayers] Waiting for Players...");
        }

        public void OnRoundStart()
        {
            Log.Info($"[OnRoundStart] Round Start!");
        }

        public void OnRoundEnd(RoundEndedEventArgs ev)
        {
            Log.Info($"[OnRoundEnd] Round Ended.{ev.TimeToRestart}");

            if (SanyaRemastered.Instance.Config.GodmodeAfterEndround)
            {
                foreach (Player player in Player.List)
                {
                    ReferenceHub referenceHub = player.ReferenceHub;
                    referenceHub.characterClassManager.GodMode = true;
                }
            }
            Coroutines.isAirBombGoing = false;
        }

        public void OnRoundRestart()
        {
            Log.Info($"[OnRoundRestart] Restarting...");
            foreach (var player in Player.List)
                if (player.GameObject.TryGetComponent<SanyaRemasteredComponent>(out var comp))
                    UnityEngine.Object.Destroy(comp);
            SanyaRemasteredComponent._scplists.Clear();

            foreach (var cor in RoundCoroutines)
                Timing.KillCoroutines(cor);
            RoundCoroutines.Clear();

            RoundSummary.singleton.RoundEnded = true;
        }
        public void OnTeamRespawn(RespawningTeamEventArgs ev)
        {
            if (SanyaRemastered.Instance.Config.IsDebugged) Log.Debug($"[OnTeamRespawn] Queues:{ev.Players.Count} IsCI:{ev.NextKnownTeam == SpawnableTeamType.ChaosInsurgency} MaxAmount:{ev.MaximumRespawnAmount}");

            if (SanyaRemastered.Instance.Config.StopRespawnAfterDetonated && AlphaWarheadController.Host.detonated)
                ev.Players.Clear();
            else if (SanyaRemastered.Instance.Config.GodmodeAfterEndround && !RoundSummary.RoundInProgress())
                ev.Players.Clear();
            else if (Coroutines.isAirBombGoing && Coroutines.AirBombWait < 60)
                ev.Players.Clear();
            else if (StopRespawn)
                ev.Players.Clear();
        }
        public void OnWarheadStart(StartingEventArgs ev)
        {
            if (SanyaRemastered.Instance.Config.IsDebugged) Log.Debug($"[OnWarheadStart] {ev.Player?.Nickname}");

            if (SanyaRemastered.Instance.Config.CassieSubtitle)
            {
                bool isresumed = AlphaWarheadController._resumeScenario != -1;
                double left = isresumed ? AlphaWarheadController.Host.timeToDetonation : AlphaWarheadController.Host.timeToDetonation - 4;
                double count = Math.Truncate(left / 10.0) * 10.0;

                if (!isresumed)
                {
                    Methods.SendSubtitle(Subtitles.AlphaWarheadStart.Replace("{0}", count.ToString()), 15);
                }
                else
                {
                    Methods.SendSubtitle(Subtitles.AlphaWarheadResume.Replace("{0}", count.ToString()), 10);
                }
            }
        }

        public void OnWarheadCancel(StoppingEventArgs ev)
        {
            Log.Debug($"[OnWarheadCancel] {ev.Player?.Nickname}");

            if (AlphaWarheadController.Host._isLocked) return;

            if (SanyaRemastered.Instance.Config.CassieSubtitle)
            {
                Methods.SendSubtitle(Subtitles.AlphaWarheadCancel, 7);
            }

            if (SanyaRemastered.Instance.Config.CloseDoorsOnNukecancel)
            {
                if (plugin.Config.CloseDoorsOnNukecancel)
                    foreach (var door in Map.Doors)
                        if (door.Base.NetworkActiveLocks == (ushort)DoorLockReason.Warhead)
                            door.Base.NetworkTargetState = false;
            }
        }

        public void OnDetonated()
        {
            if (SanyaRemastered.Instance.Config.IsDebugged) Log.Debug($"[OnDetonated] Detonated:{RoundSummary.roundTime / 60:00}:{RoundSummary.roundTime % 60:00}");

            if (SanyaRemastered.Instance.Config.OutsidezoneTerminationTimeAfterNuke >= 0)
            {
                RoundCoroutines.Add(Timing.RunCoroutine(Coroutines.AirSupportBomb(false, SanyaRemastered.Instance.Config.OutsidezoneTerminationTimeAfterNuke), Segment.FixedUpdate));
            }
        }
        public void OnAnnounceDecont(AnnouncingDecontaminationEventArgs ev)
        {
            if (SanyaRemastered.Instance.Config.IsDebugged) Log.Debug($"[OnAnnounceDecont] {ev.Id} {DecontaminationController.Singleton._stopUpdating}");

            if (SanyaRemastered.Instance.Config.CassieSubtitle)
                switch (ev.Id)
                {
                    case 0:
                        {
                            foreach (Player player in Player.List.Where((p) => p.CurrentRoom.Zone == ZoneType.LightContainment))
                            {
                                player.ClearBroadcasts();
                                player.Broadcast(20, Subtitles.DecontaminationInit);
                            }
                            break;
                        }
                    case 1:
                        {
                            foreach (Player player in Player.List.Where((p) => p.CurrentRoom.Zone == ZoneType.LightContainment))
                            {
                                player.ClearBroadcasts();
                                player.Broadcast(20, Subtitles.DecontaminationMinutesCount.Replace("{0}", "10"));
                            }
                            break;
                        }
                    case 2:
                        {
                            foreach (Player player in Player.List.Where((p) => p.CurrentRoom.Zone == ZoneType.LightContainment))
                            {
                                player.ClearBroadcasts();
                                player.Broadcast(20, Subtitles.DecontaminationMinutesCount.Replace("{0}", "5"));
                            }
                            break;
                        }
                    case 3:
                        {
                            foreach (Player player in Player.List.Where((p) => p.CurrentRoom.Zone == ZoneType.LightContainment))
                            {
                                player.ClearBroadcasts();
                                player.Broadcast(20, Subtitles.DecontaminationMinutesCount.Replace("{0}", "1"));
                            }
                            break;
                        }
                    case 4:
                        {
                            foreach (Player player in Player.List.Where((p) => p.CurrentRoom.Zone == ZoneType.LightContainment))
                            {
                                player.ClearBroadcasts();
                                player.Broadcast(45, Subtitles.Decontamination30s.Replace("{0}", "10"));
                            }
                            break;
                        }
                    case 5:
                        {
                            //no announce
                            break;
                        }
                    case 6:
                        {
                            Methods.SendSubtitle(Subtitles.DecontaminationLockdown, 15);
                            break;
                        }
                }
        }

        public void OnAnnounceNtf(AnnouncingNtfEntranceEventArgs ev)
        {
            if (SanyaRemastered.Instance.Config.CassieSubtitle)
            {
                int SCPCount = 0;
                foreach (Player i in Player.List)
                {
                    if (i.Team == Team.SCP && i.Role != RoleType.Scp0492)
                    {
                        SCPCount++;
                    }
                }

                if (SCPCount > 0)
                {
                    Methods.SendSubtitle(Subtitles.MTFRespawnSCPs.Replace("{0}", $"{ev.UnitName}-{ev.UnitNumber}").Replace("{1}", SCPCount.ToString()), 30);
                }
                else
                {
                    Methods.SendSubtitle(Subtitles.MTFRespawnNOSCPs.Replace("{0}", $"{ev.UnitName}-{ev.UnitNumber}"), 30);
                }
            }
        }
        public void OnExplodingGrenade(ExplodingGrenadeEventArgs ev)
        {
            if (SanyaRemastered.Instance.Config.GrenadeEffect)
            {
                foreach (var ply in Player.List)
                {
                    var dis = Vector3.Distance(ev.Grenade.transform.position, ply.Position);
                    if (dis < 15)
                    {
                        ply.ReferenceHub.playerEffectsController.EnableEffect<Deafened>(20f / dis, true);
                    }
                }
            }
        }
        public void OnGeneratorFinish(GeneratorActivatedEventArgs ev)
        {
            if (SanyaRemastered.Instance.Config.IsDebugged) Log.Debug($"[OnGeneratorFinish] {ev.Generator.gameObject.GetComponent<Room>()?.Name}");
            //if (SanyaRemastered.Instance.Config.GeneratorFinishLock) ev.Generator.Network_flags = (byte)Scp079Generator.GeneratorFlags.Unlocked;

            int curgen = Map.ActivatedGenerators + 1;
            if (SanyaRemastered.Instance.Config.CassieSubtitle)
            {
                if (curgen < 3)
                {
                    Methods.SendSubtitle(Subtitles.GeneratorFinish.Replace("{0}", curgen.ToString()).Replace("{s}", curgen == 1 ? "" : "s"), 10);
                }
                else
                {
                    Methods.SendSubtitle(Subtitles.GeneratorComplete, 20);
                }
            }
        }
        public void OnPreAuth(PreAuthenticatingEventArgs ev)
        {
            if (SanyaRemastered.Instance.Config.IsDebugged) Log.Debug($"[OnPreAuth] {ev.Request.RemoteEndPoint.Address}:{ev.UserId}");
        }

        public void OnPlayerVerified(VerifiedEventArgs ev)
        {
            if (ev.Player.ReferenceHub.characterClassManager.IsHost) return;
            Log.Info($"[OnPlayerJoin] {ev.Player.Nickname} ({ev.Player.ReferenceHub.queryProcessor._ipAddress}:{ev.Player.UserId})");

            if (plugin.Config.DisablePlayerLists && playerlistnetid > 0)
            {
                ObjectDestroyMessage objectDestroyMessage = new ObjectDestroyMessage
                {
                    netId = playerlistnetid
                };
                ev.Player.Connection.Send(objectDestroyMessage, 0);
            }
            if (!string.IsNullOrWhiteSpace(plugin.Config.BoxMessageOnJoin))
            {
                ev.Player.OpenReportWindow(plugin.Config.BoxMessageOnJoin);
            }
            //Component
            if (!ev.Player.GameObject.TryGetComponent<SanyaRemasteredComponent>(out _))
                ev.Player.GameObject.AddComponent<SanyaRemasteredComponent>();
        }

        public void OnPlayerDestroying(DestroyingEventArgs ev)
        {
            if (ev.Player.IsHost) return;
            if (SanyaRemastered.Instance.Config.IsDebugged) Log.Debug($"[OnPlayerLeave] {ev.Player.Nickname} ({ev.Player.ReferenceHub.queryProcessor._ipAddress}:{ev.Player.UserId})");
        }

        public void OnPlayerSetClass(ChangingRoleEventArgs ev)
        {
            if (ev.Player.IsHost) return;
            if (SanyaRemastered.Instance.Config.IsDebugged) Log.Debug($"[OnPlayerSetClass] {ev.Player.Nickname} -> {ev.NewRole}");

            if (SanyaRemastered.Instance.Config.Scp079ExtendEnabled && ev.NewRole == RoleType.Scp079)
            {
                RoundCoroutines.Add(Timing.CallDelayed(5f, () => ev.Player.ReferenceHub.GetComponent<SanyaRemasteredComponent>().AddHudCenterDownText(Subtitles.Extend079First, 10)));
            }
            if (SanyaRemastered.Instance.Config.Scp049_add_time_res_success && ev.NewRole == RoleType.Scp0492)
            {
                foreach (Player Exiledspec in Player.List.Where((p) => p.Role == RoleType.Spectator))
                    Methods.AddDeathTimeForScp049(Exiledspec.ReferenceHub);
            }

            if (ev.Reason == SpawnReason.Escaped && plugin.Config.Scp096Real)
            {
                bool IsAnTarget = false;
                foreach (Player scp096 in Player.List.Where(x=> x.Role == RoleType.Scp096))
                    if (scp096.CurrentScp is PlayableScps.Scp096 Scp096 && Scp096.HasTarget(ev.Player.ReferenceHub))
                        IsAnTarget = true;
                if (IsAnTarget)
                {
                    ev.NewRole = RoleType.Spectator;
                    ev.Player.ReferenceHub.GetComponent<SanyaRemasteredComponent>().AddHudBottomText("Vous avez été abatue a la sortie car vous avez vue le visage de SCP-096", 10);
                }
            }
        }


        public void OnPlayerSpawn(SpawningEventArgs ev)
        {
            if (ev.Player.IsHost) return;
            if (SanyaRemastered.Instance.Config.IsDebugged) Log.Debug($"[OnPlayerSpawn] {ev.Player.Nickname} -{ev.RoleType}-> {ev.Position}");
            ev.Player.ReferenceHub.fpc.staminaController.RemainingStamina += 1;
            if (ev.Player.Role == (RoleType.Scp93953 | RoleType.Scp93989))
                ev.Player.Scale = new Vector3(SanyaRemastered.Instance.Config.Scp939Size, SanyaRemastered.Instance.Config.Scp939Size, SanyaRemastered.Instance.Config.Scp939Size);
            if (plugin.Config.RandomRespawnPosPercent > 0
                && ev.Player.ReferenceHub.characterClassManager._prevId == RoleType.Spectator
                && (ev.RoleType.GetTeam() == Team.MTF || ev.RoleType.GetTeam() == Team.CHI)
                && nextRespawnPos != Vector3.zero)
            {
                ev.Position = nextRespawnPos;
            }

            if (SanyaRemastered.Instance.Config.Scp106slow && ev.Player.Role == RoleType.Scp106
                || SanyaRemastered.Instance.Config.Scp939slow && ev.Player.Role == (RoleType.Scp93953 | RoleType.Scp93989))
            {
                ev.Player.ReferenceHub.playerEffectsController.EnableEffect<Disabled>();
            }
        }
        public void OnPlayerHurt(HurtingEventArgs ev)
        {
            if (ev.Target.IsHost || ev.Target.Role == RoleType.Spectator || ev.Target.ReferenceHub.characterClassManager.GodMode || ev.Target.ReferenceHub.characterClassManager.SpawnProtected || !ev.IsAllowed) return;
            if (SanyaRemastered.Instance.Config.IsDebugged) Log.Debug($"[OnPlayerHurt:Before] {ev.Attacker?.Nickname}[{ev.Attacker?.Role}] -{ev.HitInformation.Attacker}({ev.HitInformation.Amount})-> {ev.Target?.Nickname}[{ev.Target?.Role}]");
            
            if (ev.Target == null) return;

            if (ev.DamageType != DamageTypes.Nuke
                && ev.DamageType != DamageTypes.Decont
                && ev.DamageType != DamageTypes.Wall
                && ev.DamageType != DamageTypes.Tesla
                && ev.DamageType != DamageTypes.Scp207)
            {
                //GrenadeHitmark
                if (ev.Attacker != null)
                    if (SanyaRemastered.Instance.Config.HitmarkGrenade
                    && ev.DamageType == DamageTypes.Grenade
                    && ev.Target.UserId != ev.Attacker.UserId)
                    {
                        ev.Attacker.SendHitmarker();
                    }

                //USPMultiplier
                if (ev.DamageType == DamageTypes.Com18)
                {
                    if (ev.Target.ReferenceHub.characterClassManager.IsAnyScp())
                    {
                        ev.Amount *= SanyaRemastered.Instance.Config.UspDamageMultiplierScp;
                    }
                    else
                    {
                        ev.Amount *= SanyaRemastered.Instance.Config.UspDamageMultiplierHuman;
                        ev.Target.ReferenceHub.playerEffectsController.EnableEffect<Disabled>(5f);
                    }
                }

                //SCPsMultiplicator
                if (ev.Target.IsScp
                    && ev.DamageType != DamageTypes.Contain
                    && ev.DamageType != DamageTypes.Decont
                    && ev.DamageType != DamageTypes.Flying
                    && ev.DamageType != DamageTypes.Poison
                    && ev.DamageType != DamageTypes.Recontainment)
                {
                    if (SanyaRemastered.Instance.Config.ScpDamageMultiplicator.TryGetValue(ev.Target.Role, out float AmmountDamage) && AmmountDamage != 1)
                    {
                        ev.Amount *= AmmountDamage;
                    }
                    if (ev.Target.Role == RoleType.Scp106)
                    {
                        if (ev.DamageType == DamageTypes.Grenade) ev.Amount *= SanyaRemastered.Instance.Config.Scp106GrenadeMultiplicator;
                        if (ev.DamageType != DamageTypes.MicroHID && ev.DamageType != DamageTypes.Tesla)
                            ev.Amount *= SanyaRemastered.Instance.Config.Scp106DamageMultiplicator;
                    }
                }
            }

            if (SanyaRemastered.Instance.Config.IsDebugged) Log.Debug($"[OnPlayerHurt:After] {ev.Attacker?.Nickname}[{ev.Attacker?.Role}] -{ev.HitInformation.Attacker}({ev.HitInformation.Amount})-> {ev.Target?.Nickname}[{ev.Target?.Role}]");
        }

        public void OnDied(DiedEventArgs ev)
        {
            if (ev.Target.IsHost || ev.Target.Role == RoleType.Spectator || ev.Target.ReferenceHub.characterClassManager.GodMode || ev.Target.ReferenceHub.characterClassManager.SpawnProtected) return;
            if (SanyaRemastered.Instance.Config.IsDebugged) Log.Debug($"[OnPlayerDeath] {ev.Killer?.Nickname}[{ev.Killer?.Role}] -{ev.HitInformations.Tool.Name}-> {ev.Target?.Nickname}[{ev.Target?.Role}]");
            if (SanyaRemastered.Instance.Config.Scp939Size != 1)
            {
                ev.Target.Scale = Vector3.one;
            }
            if (ev.Killer == null) return;

            if (SanyaRemastered.Instance.Config.ScpRecoveryAmount.TryGetValue(ev.HitInformations.Tool.Name, out int Heal) && Heal > 0)
            {
                ev.Killer.ReferenceHub.playerStats.HealHPAmount(Heal);
            }

            if (SanyaRemastered.Instance.Config.HitmarkKilled
                && ev.Killer.Team != Team.SCP
                && !string.IsNullOrEmpty(ev.Killer.UserId)
                && ev.Killer.UserId != ev.Target.UserId)
            {
                ev.Killer.GameObject.GetComponent<Hitmarker>().Trigger(3);
            }

            if (SanyaRemastered.Instance.Config.CassieSubtitle
                && ev.Target.Team == Team.SCP
                && ev.HitInformations.Attacker != "DISCONNECT")
            {
                string fullname = CharacterClassManager._staticClasses.Get(ev.Target.Role).fullName;
                string str;
                if (ev.Target.Role != RoleType.Scp0492)
                    switch (ev.HitInformations.Tool.Name)
                    {
                        case "TESLA":
                            {
                                str = Subtitles.SCPDeathTesla.Replace("{0}", fullname);
                                break;
                            }
                        case "NUKE":
                            {
                                str = Subtitles.SCPDeathWarhead.Replace("{0}", fullname);
                                break;
                            }
                        case "DECONT":
                            {
                                str = Subtitles.SCPDeathDecont.Replace("{0}", fullname);
                                break;
                            }
                        default:
                            {
                                Log.Debug($"[CheckTeam] ply:{ev.Target.Id} kill:{ev.Killer.Id} plyid:{ev.HitInformations.PlayerId} killteam:{ev.Killer.Team}", SanyaRemastered.Instance.Config.IsDebugged);
                                switch (ev.Killer.Team)
                                {
                                    case Team.CDP:
                                        {
                                            str = Subtitles.SCPDeathTerminated.Replace("{0}", fullname).Replace("{1}", "un classe-D");
                                            break;
                                        }
                                    case Team.CHI:
                                        {
                                            str = Subtitles.SCPDeathTerminated.Replace("{0}", fullname).Replace("{1}", "l'insurection du chaos");
                                            break;
                                        }
                                    case Team.RSC:
                                        {
                                            str = Subtitles.SCPDeathTerminated.Replace("{0}", fullname).Replace("{1}", "un scientifique");
                                            break;
                                        }
                                    case Team.MTF:
                                        {
                                            string unit = ev.Killer.ReferenceHub.characterClassManager.CurUnitName;
                                            str = Subtitles.SCPDeathContainedMTF.Replace("{0}", fullname).Replace("{1}", unit);
                                            break;
                                        }
                                    default:
                                        {
                                            str = Subtitles.SCPDeathUnknown.Replace("{0}", fullname);
                                            break;
                                        }
                                }
                                break;
                            }
                    }
                else
                {
                    str = "{-1}";
                }
                int count = 0;
                bool isFound079 = false;
                bool isForced = false;
                foreach (var i in Player.List)
                {
                    if (ev.Target.UserId == i.UserId) continue;
                    if (i.Team == Team.SCP) count++;
                    if (i.Role == RoleType.Scp079) isFound079 = true;
                }

                Log.Debug($"[Check079] SCPs:{count} isFound079:{isFound079} totalvol:{Map.ActivatedGenerators}",SanyaRemastered.Instance.Config.IsDebugged);
                if (count == 1
                    && isFound079
                    && Map.ActivatedGenerators < 2
                    && ev.HitInformations.Tool != DamageTypes.Nuke)
                {
                    isForced = true;
                    str = str.Replace("{-1}", "\nTout les SCP ont été sécurisé.\nLa séquence de reconfinement de SCP-079 a commencé\nLa Heavy Containement Zone vas surcharger dans t-moins 1 minutes.");
                }
                else
                {
                    str = str.Replace("{-1}", string.Empty);
                }
                if (!string.IsNullOrWhiteSpace(str))
                    Methods.SendSubtitle(str, (ushort)(isForced ? 30 : 10));
            }

            if (ev.HitInformations.Tool == DamageTypes.Decont || ev.HitInformations.Tool == DamageTypes.Nuke)
            {
                ev.Target.Inventory.UserInventory.Items.Clear();
            }
        }
        public void OnEscapingPocketDimension(EscapingPocketDimensionEventArgs ev)
        {
            /*if (SanyaRemastered.Instance.Config.Scp096Real && PlayerHasEnrage096InPocket.Contains(ev.Player.GameObject))
            {
                foreach (Player player in Player.List.Where(x=>x.Role == RoleType.Scp096))
                    if (player.CurrentScp is PlayableScps.Scp096 Scp096)
                    {
                        Scp096.Enrage();
                        Scp096.AddTarget(ev.Player.GameObject);
                    }
            }*/
        }
        public void OnPocketDimDeath(FailingEscapePocketDimensionEventArgs ev)
        {
            if (SanyaRemastered.Instance.Config.IsDebugged) Log.Debug($"[OnPocketDimDeath] {ev.Player.Nickname}");
            /*if (PlayerHasEnrage096InPocket.Contains(ev.Player.GameObject))
                PlayerHasEnrage096InPocket.Remove(ev.Player.GameObject);*/
            List<Player> Scp106 = Player.List.Where(x => x.Role == RoleType.Scp106).ToList();
            foreach (Player player in Scp106)
            {
                player.SendHitmarker();
            }
            if (SanyaRemastered.Instance.Config.ScpRecoveryAmount.TryGetValue("Scp106" , out int heal) && heal > 0)
            {
                foreach (Player player in Scp106)
                {
                    player.ReferenceHub.playerStats.HealHPAmount(heal);
                }
            }
        }

        public void OnPlayerItemUsed(UsedItemEventArgs ev)
        {
            if (SanyaRemastered.Instance.Config.IsDebugged) Log.Debug($"[OnPlayerUsedMedicalItem] {ev.Player.Nickname} -> {ev.Item}");

            if (ev.Item.Base.ItemTypeId == ItemType.Medkit)
            {
                ev.Player.ReferenceHub.playerEffectsController.DisableEffect<Hemorrhage>();
                ev.Player.ReferenceHub.playerEffectsController.DisableEffect<Bleeding>();
                ev.Player.ReferenceHub.playerEffectsController.DisableEffect<Burned>();
            }
            if (ev.Item.Base.ItemTypeId == ItemType.Adrenaline)
            {
                ev.Player.ReferenceHub.playerEffectsController.DisableEffect<Panic>();
                ev.Player.ReferenceHub.playerEffectsController.DisableEffect<Disabled>();
            }
            if (ev.Item.Base.ItemTypeId == ItemType.SCP500)
            {
                ev.Player.ReferenceHub.playerEffectsController.DisableEffect<Asphyxiated>();
                ev.Player.ReferenceHub.playerEffectsController.DisableEffect<Hemorrhage>();
                ev.Player.ReferenceHub.playerEffectsController.DisableEffect<Bleeding>();
                ev.Player.ReferenceHub.playerEffectsController.DisableEffect<Disabled>();
                ev.Player.ReferenceHub.playerEffectsController.DisableEffect<Burned>();
                ev.Player.ReferenceHub.playerEffectsController.DisableEffect<Poisoned>();
                ev.Player.ReferenceHub.playerEffectsController.DisableEffect<Amnesia>();
                ev.Player.ReferenceHub.playerEffectsController.DisableEffect<Deafened>();
            }
        }

        public void OnPlayerTriggerTesla(TriggeringTeslaEventArgs ev)
        {
            Log.Debug($"[OnPlayerTriggerTesla] {ev.IsInHurtingRange}:{ev.Player.Nickname}", SanyaRemastered.Instance.Config.IsDebugged);
        }

        public void OnPlayerDoorInteract(InteractingDoorEventArgs ev)
        {
            if (SanyaRemastered.Instance.Config.IsDebugged) Log.Debug($"[OnPlayerDoorInteract] {ev.Player.Nickname}:{ev.Door?.Nametag}");

            if (plugin.Config.InventoryKeycardActivation && ev.Player.Team != Team.SCP && !ev.Player.IsBypassModeEnabled)
            {
                ev.IsAllowed = false;

                DoorLockMode lockMode = DoorLockUtils.GetMode((DoorLockReason)ev.Door.Base.ActiveLocks);
                if (ev.IsAllowed
                || ((ev.Door is IDamageableDoor damageableDoor) && damageableDoor.IsDestroyed)
                || (ev.Door.Base.NetworkTargetState && !lockMode.HasFlagFast(DoorLockMode.CanClose))
                || (!ev.Door.Base.NetworkTargetState && !lockMode.HasFlagFast(DoorLockMode.CanOpen))
                || lockMode == DoorLockMode.FullLock) return;

                if (ev.Door.RequiredPermissions.RequiredPermissions == Interactables.Interobjects.DoorUtils.KeycardPermissions.None)
                    ev.IsAllowed = true;

                /*foreach (var item in ev.Player.Inventory.UserInventory.Items.Where(x => x.Value.ItemTypeId.ToString().Contains("Keycard")))
                {
                    Item[] itemlist = UnityEngine.Object.FindObjectOfType<InventorySystem.Inventory>().item;

                    Item gameItem = Array.Find(itemlist, i => i.id == item.id);

                    // Relevant for items whose type was not found
                    if (gameItem == null)
                        continue;

                    Keycard.Permissions itemPerms = Keycard.ToTruthyPermissions(gameItem.permissions);

                    if (itemPerms.HasFlagFast(ev.Door.RequiredPermissions, ev.Door.RequiredPermissions.RequireAll))
                    {
                        ev.IsAllowed = true;
                    }
                }*/
            }
            if (plugin.Config.AddDoorsOnSurface && ev.Door.Base.TryGetComponent<DoorNametagExtension>(out var nametag))
            {
                if (nametag._nametag.Contains("GATE_EX_"))
                {
                    bool flagL = DoorNametagExtension.NamedDoors["GATE_EX_L"].TargetDoor.AllowInteracting(ev.Player.ReferenceHub, 0);
                    bool flagR = DoorNametagExtension.NamedDoors["GATE_EX_R"].TargetDoor.AllowInteracting(ev.Player.ReferenceHub, 0);
                    if (flagL && flagR)
                        if (nametag._nametag == "GATE_EX_L")
                            DoorNametagExtension.NamedDoors["GATE_EX_R"].TargetDoor.NetworkTargetState = !DoorNametagExtension.NamedDoors["GATE_EX_R"].TargetDoor.TargetState;
                        else
                            DoorNametagExtension.NamedDoors["GATE_EX_L"].TargetDoor.NetworkTargetState = !DoorNametagExtension.NamedDoors["GATE_EX_L"].TargetDoor.TargetState;
                    else
                        ev.IsAllowed = false;
                }
            }
        }

        public void OnPlayerLockerInteract(InteractingLockerEventArgs ev)
        {
            if (SanyaRemastered.Instance.Config.IsDebugged) Log.Debug($"[OnPlayerLockerInteract] {ev.Player.Nickname}:{ev.Locker.name}");
            if (SanyaRemastered.Instance.Config.InventoryKeycardActivation && ev.Player.Team != Team.SCP && !ev.Player.IsBypassModeEnabled)
            {
                /*foreach (var item in ev.Player.Inventory.UserInventory.Items)
                {
                    if (ev.Player.Inventory.GetItemByID(item.Value).permissions.Contains("PEDESTAL_ACC"))
                    {
                        ev.IsAllowed = true;
                    }
                }*/
            }
        }
        public void OnInteractingElevator(InteractingElevatorEventArgs ev)
        {
            if (SanyaRemastered.Instance.Config.IsDebugged) Log.Debug($"[OnInteractingElevator] Player : {ev.Player}  Name : {ev.Elevator.GetType().Name}");
            if (SanyaRemastered.Instance.Config.ScpCantInteract && SanyaRemastered.Instance.Config.ScpCantInteractList.TryGetValue("UseElevator", out List<RoleType> roles))
            {
                if (roles.Contains(ev.Player.Role))
                    ev.IsAllowed = false;
            }
        }
        public void OnIntercomSpeaking(IntercomSpeakingEventArgs ev)
        {
            if (!SanyaRemastered.Instance.Config.IntercomBrokenOnBlackout) return;

            Room RoomIntercom = Map.Rooms.First(x => x.Type == RoomType.EzIntercom);
            if (RoomIntercom.LightsOff)
            {
                ev.IsAllowed = false;
            }
        }
        public void OnShoot(ShootingEventArgs ev)
        {
            if (SanyaRemastered.Instance.Config.IsDebugged) Log.Debug($"[OnShoot] {ev.Shooter.Nickname} -{ev.ShotPosition}-> {Player.Get(ev.TargetNetId)?.Nickname}");


            if (ev.ShotPosition != Vector3.zero
                && Physics.Linecast(ev.Shooter.Position, ev.ShotPosition, out RaycastHit raycastHit, grenade_pickup_mask))
            {
                if (SanyaRemastered.Instance.Config.Item_shoot_move)
                {
                    var pickup = raycastHit.transform.GetComponentInParent<ItemPickupBase>();
                    Log.Info($"pickup is null ? {pickup != null} : pickup.Rb is null ?{pickup.Rb != null}");
                    if (pickup != null && pickup.Rb != null)
                    {
                        Log.Info($"Force ? {Vector3.Distance(ev.ShotPosition, ev.Shooter.Position) * 2} : Weight ?{pickup.Info.Weight}");
                        pickup.Rb.AddExplosionForce((Vector3.Distance(ev.ShotPosition, ev.Shooter.Position) * 2) / pickup.Info.Weight, ev.Shooter.Position, 500f, 3f, ForceMode.Impulse);
                    }
                }

                if (SanyaRemastered.Instance.Config.Grenade_shoot_fuse)
                {
                    var fraggrenade = raycastHit.transform.GetComponentInParent<ExplosionGrenade>();
                    if (fraggrenade != null)
                    {
                        fraggrenade._fuseTime = 0.1f;
                    }
                    var Flashgrenade = raycastHit.transform.GetComponentInParent<FlashbangGrenade>();
                    if (Flashgrenade != null)
                    {
                        Flashgrenade._fuseTime = 0.1f;
                    }
                }
                if (SanyaRemastered.Instance.Config.OpenDoorOnShoot)
                {
                    var door = raycastHit.transform.GetComponentInParent<DoorVariant>();
                    if (door != null)
                    {
                        DoorLockMode lockMode = DoorLockUtils.GetMode((DoorLockReason)door.ActiveLocks);

                        if (((door is IDamageableDoor damageableDoor) && damageableDoor.IsDestroyed)
                        || (door.NetworkTargetState && !lockMode.HasFlagFast(DoorLockMode.CanClose))
                        || (!door.NetworkTargetState && !lockMode.HasFlagFast(DoorLockMode.CanOpen))
                        || lockMode == DoorLockMode.FullLock
                        || door.NetworkTargetState && door.GetExactState() != 1f || !door.NetworkTargetState && door.GetExactState() != 0f
                        ) return;

                        if (door.RequiredPermissions.RequiredPermissions == Interactables.Interobjects.DoorUtils.KeycardPermissions.None && !(door is PryableDoor))
                        {
                            door.NetworkTargetState = !door.NetworkTargetState;
                        }
                    }
                }
                if (SanyaRemastered.Instance.Config.Scp096Real)
                {
                    Player target = Player.Get(ev.TargetNetId);
                    if (target != null && target.Role == RoleType.Scp096)
                    {
                        ev.IsAllowed = false;
                    }
                }
                if (ev.Shooter.SessionVariables.ContainsKey("InfAmmo"))
                {
                    FirearmStatus firearm = ev.Shooter.CurrentItem.Base.GetComponent<Firearm>().Status;
                    ev.Shooter.CurrentItem.Base.GetComponent<Firearm>().Status = new FirearmStatus((byte)(firearm.Ammo + 1), firearm.Flags, firearm.Attachments);
                }
            }
        }
        public void OnSyncingData(SyncingDataEventArgs ev)
        {
            if (ev.Player == null || ev.Player.IsHost || !ev.Player.ReferenceHub.Ready || ev.Player.ReferenceHub.animationController.curAnim == ev.CurrentAnimation) return;

            if (SanyaRemastered.Instance.Config.Scp079ExtendEnabled && ev.Player.Role == RoleType.Scp079)
            {
                if (ev.CurrentAnimation == 1)
                    ev.Player.ReferenceHub.GetComponent<SanyaRemasteredComponent>().AddHudCenterDownText(Subtitles.ExtendEnabled, 3);
                else
                    ev.Player.ReferenceHub.GetComponent<SanyaRemasteredComponent>().AddHudCenterDownText(Subtitles.ExtendDisabled, 3);
            }

            if (SanyaRemastered.Instance.Config.StaminaLostJump > 0f
                && ev.CurrentAnimation == 2
                && ev.Player.ReferenceHub.characterClassManager.IsHuman()
                && !ev.Player.ReferenceHub.fpc.staminaController._invigorated.IsEnabled
                && !ev.Player.ReferenceHub.fpc.staminaController._scp207.IsEnabled
                )
            {
                ev.Player.ReferenceHub.fpc.staminaController.RemainingStamina -= SanyaRemastered.Instance.Config.StaminaLostJump;
                ev.Player.ReferenceHub.fpc.staminaController._regenerationTimer = 0f;
            }
            if (SanyaRemastered.Instance.Config.StaminaEffect)
            {
                if (ev.Player.ReferenceHub.fpc.staminaController.RemainingStamina <= 0f
                    && ev.Player.ReferenceHub.characterClassManager.IsHuman()
                    && !ev.Player.ReferenceHub.fpc.staminaController._invigorated.IsEnabled
                    && !ev.Player.ReferenceHub.fpc.staminaController._scp207.IsEnabled
                    )
                {
                    ev.Player.ReferenceHub.playerEffectsController.EnableEffect<Disabled>(1f);
                }
            }
        }



        public void OnActivatingWarheadPanel(ActivatingWarheadPanelEventArgs ev)
        {
            if (SanyaRemastered.Instance.Config.IsDebugged) Log.Debug($"[OnActivatingWarheadPanel] Nickname : {ev.Player.Nickname}  Allowed : {ev.IsAllowed}");


            if (plugin.Config.InventoryKeycardActivation && !ev.Player.IsBypassModeEnabled && ev.Player.Team != Team.SCP)
            {
                /*foreach (var item in ev.Player.Inventory.UserInventory.Items.Where(x => x.Value.ItemTypeId.ToString().Contains("Keycard")))
                {
                    Item[] itemlist = UnityEngine.Object.FindObjectOfType<Inventory>().availableItems;

                    Item gameItem = Array.Find(itemlist, i => i.id == item.Value);

                    // Relevant for items whose type was not found
                    if (gameItem == null)
                        continue;

                    Keycard.Permissions itemPerms = Keycard.ToTruthyPermissions(gameItem.permissions);

                    if (itemPerms.HasFlagFast(Keycard.Permissions.ContainmentLevelThree, false))
                    {
                        ev.IsAllowed = true;
                    }
                }*/
            }
            var outsite = UnityEngine.Object.FindObjectOfType<AlphaWarheadOutsitePanel>();
            if (SanyaRemastered.Instance.Config.Nukecapclose && outsite.keycardEntered)
            {
                Timing.RunCoroutine(Coroutines.CloseNukeCap(), Segment.FixedUpdate);
            }
            else if (outsite.keycardEntered)
            {
                ev.IsAllowed = false;
            }
        }
        
        public void OnGeneratorUnlock(UnlockingGeneratorEventArgs ev)
        {
            /*if (plugin.Config.InventoryKeycardActivation && !ev.Player.IsBypassModeEnabled && ev.Player.Team != Team.SCP)
            {
                foreach (var item in ev.Player.Inventory.UserInventory.Items)
                {
                    if (ev.Player.Inventory.GetItemByID(item.Value.ItemTypeId).permissions.Contains("ARMORY_LVL_2"))
                    {
                        ev.IsAllowed = true;
                    }
                }
            }*/

            if (ev.IsAllowed && SanyaRemastered.Instance.Config.GeneratorUnlockOpen)
            {
                ev.Generator._doorToggleCooldownTime = 2f;
                ev.Generator.Network_flags = (byte)Scp079Generator.GeneratorFlags.Open;
            }
        }
        public void OnStoppingGenerator(StoppingGeneratorEventArgs ev)
        {
            if (SanyaRemastered.Instance.Config.IsDebugged) Log.Debug($"[OnEjectingGeneratorTablet] {ev.Player.Nickname} -> UnkNow Beceause EXILED Connard");
        }
        public void OnGeneratorOpen(OpeningGeneratorEventArgs ev)
        {
            if (SanyaRemastered.Instance.Config.IsDebugged) Log.Debug($"[OnGeneratorOpen] {ev.Player.Nickname} -> {ev.Generator.gameObject.GetComponent<Room>()?.Name}");
            if (ev.Generator.Engaged && SanyaRemastered.Instance.Config.GeneratorFinishLock)
            {
                ev.IsAllowed = false;
            }
        }

        public void OnGeneratorClose(ClosingGeneratorEventArgs ev)
        {
            if (SanyaRemastered.Instance.Config.IsDebugged) Log.Debug($"[OnGeneratorClose] {ev.Player.Nickname} -> {ev.Generator.gameObject.GetComponent<Room>()?.Name}");
        }

        public void OnActivatingGenerator(ActivatingGeneratorEventArgs ev)
        {
            if (SanyaRemastered.Instance.Config.IsDebugged) Log.Debug($"[OnGeneratorInsert] {ev.Player.Nickname} -> Unknow Exiled Connard");
        }




        public void OnHandcuffing(HandcuffingEventArgs ev)
        {
            /*if (SanyaRemastered.Instance.Config.Scp049Real)
            {
                ReferenceHub.GetHub(ev.Target.GameObject)..NetworkCufferId = ev.Cuffer.ReferenceHub.queryProcessor.PlayerId;
            }*/
        }
        public void On079LevelGain(GainingLevelEventArgs ev)
        {
            if (SanyaRemastered.Instance.Config.IsDebugged) Log.Debug($"[On079LevelGain] {ev.Player.Nickname} : {ev.NewLevel}");

            if (SanyaRemastered.Instance.Config.Scp079ExtendEnabled)
            {
                switch (ev.NewLevel)
                {
                    case 1:
                        ev.Player.ReferenceHub.GetComponent<SanyaRemasteredComponent>().AddHudCenterDownText(Subtitles.Extend079Lv2, 10);
                        break;
                    case 2:
                        ev.Player.ReferenceHub.GetComponent<SanyaRemasteredComponent>().AddHudCenterDownText(Subtitles.Extend079Lv3, 10);
                        break;
                    case 3:
                        ev.Player.ReferenceHub.GetComponent<SanyaRemasteredComponent>().AddHudCenterDownText(Subtitles.Extend079Lv4, 10);
                        break;
                    case 4:
                        ev.Player.ReferenceHub.GetComponent<SanyaRemasteredComponent>().AddHudCenterDownText(Subtitles.Extend079Lv5, 10);
                        break;
                }
            }
        }
        public void On106MakePortal(CreatingPortalEventArgs ev)
        {
            if (SanyaRemastered.Instance.Config.IsDebugged) Log.Debug($"[On106MakePortal] {ev.Player.Nickname}:{ev.Position}");
        }

        public void On106Teleport(TeleportingEventArgs ev)
        {
            if (SanyaRemastered.Instance.Config.IsDebugged) Log.Debug($"[On106Teleport] {ev.Player.Nickname}:{ev.PortalPosition}");
        }
        public void On914UpgradingPlayer(UpgradingPlayerEventArgs ev)
        {
            if (SanyaRemastered.Instance.Config.Scp914Effect)
            {
                switch (ev.KnobSetting)
                {
                    case Scp914KnobSetting.Rough:
                        {
                            var Death = new PlayerStats.HitInfo(99999, "Scp914", DamageTypes.RagdollLess, 0, true);
                            ev.Player.ReferenceHub.playerStats.HurtPlayer(Death, ev.Player.ReferenceHub.gameObject);
                            if (ev.Player.Team != Team.SCP)
                                ev.Player.ReferenceHub.GetComponent<SanyaRemasteredComponent>().AddHudCenterDownText("Un cadavre gravement mutilé a été trouvé à l'intérieur de SCP-914. Le sujet a évidemment été affiné par le SCP-914 sur le réglage Rough.", 30);
                        }
                        break;
                    case Scp914KnobSetting.Coarse:
                        {
                            if (ev.Player.Role == RoleType.Scp93953 || ev.Player.Role == RoleType.Scp93989)
                            {
                                var Death = new PlayerStats.HitInfo(99999, "Scp914", DamageTypes.RagdollLess, 0, true);
                                ev.Player.ReferenceHub.playerStats.HurtPlayer(Death, ev.Player.ReferenceHub.gameObject);
                            }
                            if (ev.Player.Team != Team.SCP)
                            {
                                var Hit = new PlayerStats.HitInfo(70, "Scp914", DamageTypes.RagdollLess, 0, true);
                                ev.Player.ReferenceHub.playerStats.HurtPlayer(Hit, ev.Player.ReferenceHub.gameObject);
                                ev.Player.ReferenceHub.playerEffectsController.GetEffect<Hemorrhage>();
                                ev.Player.ReferenceHub.playerEffectsController.GetEffect<Bleeding>();
                                ev.Player.ReferenceHub.playerEffectsController.GetEffect<Disabled>();
                                ev.Player.ReferenceHub.GetComponent<SanyaRemasteredComponent>().AddHudCenterDownText("Vous remarquez d'innombrables petites incisions dans votre corps.", 10);
                            }
                        }
                        break;
                    case Scp914KnobSetting.OneToOne:
                        {
                            if (ev.Player.Role == RoleType.Scp93953)
                            {
                                var Health = ev.Player.Health;
                                ev.Player.SetRole(RoleType.Scp93989);
                                ev.Player.ReferenceHub.playerStats.HurtPlayer(new PlayerStats.HitInfo(ev.Player.MaxHealth - Health, "Scp914", DamageTypes.RagdollLess, 0, true), ev.Player.ReferenceHub.gameObject);
                                break;
                            }
                            else if (ev.Player.Role == RoleType.Scp93989)
                            {
                                var Health = ev.Player.Health;
                                ev.Player.SetRole(RoleType.Scp93953);
                                ev.Player.ReferenceHub.playerStats.HurtPlayer(new PlayerStats.HitInfo(ev.Player.MaxHealth - Health, "Scp914", DamageTypes.RagdollLess, 0, true), ev.Player.ReferenceHub.gameObject);
                                break;
                            }
                            else if (ev.Player.Role != RoleType.Scp106)
                            {
                                if (ev.Player.Scale.y < 0)
                                {
                                    ev.Player.Scale = new Vector3(ev.Player.Scale.x, -ev.Player.Scale.y, ev.Player.Scale.z);
                                }
                                else if (ev.Player.Scale.z < 0 && ev.Player.Scale.z < 0)
                                {
                                    ev.Player.Scale = new Vector3(ev.Player.Scale.x, -ev.Player.Scale.y, -ev.Player.Scale.z);
                                }
                                else
                                {
                                    ev.Player.Scale = new Vector3(-ev.Player.Scale.x, -ev.Player.Scale.y, -ev.Player.Scale.z);
                                }
                            }
                        }
                        break;
                    case Scp914KnobSetting.Fine:
                        {
                            ev.Player.EnableEffect<Scp207>();
                            ev.Player.ChangeEffectIntensity<Scp207>(4);

                            Timing.CallDelayed(60, () =>
                            {
                                ev.Player.ReferenceHub.playerStats.HurtPlayer(new PlayerStats.HitInfo(914914, "Scp914", DamageTypes.Scp207, 0, true), ev.Player.ReferenceHub.gameObject);
                                ev.Player.ReferenceHub.GetComponent<SanyaRemasteredComponent>().AddHudCenterDownText("Vous étes mort d'un arret cardiaque", 30);
                            });
                        }
                        break;
                    case Scp914KnobSetting.VeryFine:
                        {
                            ev.Player.EnableEffect<Scp207>();
                            ev.Player.ChangeEffectIntensity<Scp207>(4);
                            Timing.CallDelayed(5, () => {
                                ev.Player.ReferenceHub.playerStats.HurtPlayer(new PlayerStats.HitInfo(914914, "Scp914", DamageTypes.Scp096, 0, true), ev.Player.ReferenceHub.gameObject);
                                ev.Player.ReferenceHub.GetComponent<SanyaRemasteredComponent>().AddHudCenterDownText("L'analyse chimique de la substance a l'intérieur de SCP-914 reste non concluante.", 30);
                            });
                        }
                        break;
                }
            }
        }
        public void On096AddingTarget(AddingTargetEventArgs ev)
        {
            if (SanyaRemastered.Instance.Config.Scp096Real)
            {
                ev.EnrageTimeToAdd = 0;
            }
        }
        public void On096Enraging(EnragingEventArgs ev)
        {
            if (SanyaRemastered.Instance.Config.Scp096Real)
            {
                ev.Scp096.EnrageTimeLeft = -960;
            }
        }
        public void On096CalmingDown(CalmingDownEventArgs ev)
        {
            /*foreach (ReferenceHub Ref in ev.Scp096._targets.ToList().Where(x => Player.Get(x.playerId).IsInPocketDimension))
            {
                PlayerHasEnrage096InPocket.Add(Ref.gameObject);
                ev.Scp096._targets.Remove(Ref);
            }*/
            if (SanyaRemastered.Instance.Config.Scp096Real && ev.Scp096._targets.ToList().Count != 0)
            {
                 ev.IsAllowed = false;
                ev.Scp096.PlayerState = Scp096PlayerState.Docile;
            }
        }
        public void On049FinishingRecall(FinishingRecallEventArgs ev)
        {
            if (SanyaRemastered.Instance.Config.Scp0492effect)
            {
                ev.Target.ReferenceHub.playerEffectsController.EnableEffect<Ensnared>(3f);
                ev.Target.ReferenceHub.playerEffectsController.EnableEffect<Deafened>(5f);
                ev.Target.ReferenceHub.playerEffectsController.EnableEffect<Blinded>(3f);
                ev.Target.ReferenceHub.playerEffectsController.EnableEffect<Amnesia>(5f);
                ev.Target.ReferenceHub.playerEffectsController.EnableEffect<Flashed>(0.2f);
            }
        }
        public void On049StartingRecall(StartingRecallEventArgs ev)
        {
            /*if (ev.Target.GameObject.GetComponent<PlayerStats>().lastHitInfo.GetDamageType() == DamageTypes.Scp049 && SanyaRemastered.Instance.Config.Scp049Real)
            {
                ev.Scp049.ReferenceHub.GetComponent<SanyaRemasteredComponent>().AddHudCenterDownText("Vous ne pouvez pas soigné se corps",5);
                ev.IsAllowed = false;
            }*/
        }
    }
}