using AdminToys;
using CustomPlayerEffects;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items.Pickups;
using InventorySystem.Items.ThrowableProjectiles;
using LightContainmentZoneDecontamination;
using MapGeneration;
using MapGeneration.Distributors;
using MEC;
using Mirror;
using SanyaRemastered.Data;
using SanyaRemastered.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SanyaRemastered.EventHandlers
{
    public class ServerHandlers
    {
        public ServerHandlers(SanyaRemastered plugin) => this.plugin = plugin;
        internal readonly SanyaRemastered plugin;
        internal List<CoroutineHandle> roundCoroutines = new List<CoroutineHandle>();
        internal bool loaded = false;


        /** RoundVar **/
        public uint playerlistnetid = 0;


        /** Update **/
        internal IEnumerator<float> EverySecond()
        {
            while (true)
            {
                try
                {
                    if (plugin.Config.PainEffectStart > 0)
                    {
                        foreach (Player player in Player.List)
                        {
                            if (player.IsHuman() && player.GetHealthAmountPercent() > SanyaRemastered.Instance.Config.PainEffectStart)
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
                    if (SanyaRemastered.Instance.Config.RamInfo)
                        DiscordLog.DiscordLog.Instance.LOGStaff += $"Total Ram Usage: {metrics.Used / 1024:0.##}/{metrics.Total / 1024:0.##} Go [{((metrics.Used / metrics.Total) * 100):0.##}%]\n";
                    double slRamUsage = Ram.MemoryService.CurrentProcessRamUsage;
                    if (SanyaRemastered.Instance.Config.RamInfo)
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

        public List<CoroutineHandle> RoundCoroutines { get => roundCoroutines; set => roundCoroutines = value; }

        public void OnWaintingForPlayers()
        {
            if (plugin.Config.RamRestartNoPlayer > 0 || plugin.Config.RamRestartWithPlayer > 0 || plugin.Config.RamInfo)
                RoundCoroutines.Add(Timing.RunCoroutine(Every30minute(), Segment.RealtimeUpdate));
            loaded = true;
            RoundCoroutines.Add(Timing.RunCoroutine(EverySecond(), Segment.FixedUpdate));

            DecalList.Clear();
            Coroutines.isAirBombGoing = false;
            Coroutines.isActuallyBombGoing = false;
            Coroutines.AirBombWait = 0;
            Server.Host.ReferenceHub.characterClassManager.NetworkCurClass = RoleType.Tutorial;
            Server.Host.Position = new Vector3(54.8f, 3000f, -44.9f);
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
                                dr._ignoredDamageSources &= ~DoorDamageType.Scp096;
                            }
                        }
                    }
                }
            }
            if (plugin.Config.GateClosingAuto)
            {
                DoorNametagExtension.NamedDoors.TryGetValue("GATE_A", out var GateA);
                var CheckpointA = GateA.TargetDoor.gameObject.GetComponent<PryableDoor>().gameObject.AddComponent<GateTimerClose>();
                DoorNametagExtension.NamedDoors.TryGetValue("GATE_B", out var GateB);
                var CheckpointB = GateB.TargetDoor.gameObject.GetComponent<PryableDoor>().gameObject.AddComponent<GateTimerClose>();
            }
            if (plugin.Config.AddDoorsOnSurface)
            {
                Vector3 DoorScale = new Vector3(1f, 1f, 1.8f);
                var LCZprefab = UnityEngine.Object.FindObjectsOfType<DoorSpawnpoint>().First(x => x.TargetPrefab.name.Contains("LCZ"));
                var EZprefab = UnityEngine.Object.FindObjectsOfType<DoorSpawnpoint>().First(x => x.TargetPrefab.name.Contains("EZ"));
                var HCZprefab = UnityEngine.Object.FindObjectsOfType<DoorSpawnpoint>().First(x => x.TargetPrefab.name.Contains("HCZ"));

                // Couloir spawn Chaos
                var door1 = UnityEngine.Object.Instantiate(LCZprefab.TargetPrefab, new Vector3(14.425f, 995.2f, -43.525f), Quaternion.Euler(Vector3.zero));
                var door2 = UnityEngine.Object.Instantiate(LCZprefab.TargetPrefab, new Vector3(14.425f, 995.2f, -23.2f), Quaternion.Euler(Vector3.zero));
                // Exit
                var door3 = UnityEngine.Object.Instantiate(EZprefab.TargetPrefab, new Vector3(176.2f, 983.24f, 35.23f), Quaternion.Euler(Vector3.up * 180f));
                var door4 = UnityEngine.Object.Instantiate(EZprefab.TargetPrefab, new Vector3(174.4f, 983.24f, 29.1f), Quaternion.Euler(Vector3.up * 90f));
                //Scale
                door1.transform.localScale = DoorScale;
                door2.transform.localScale = DoorScale;
                door3.transform.localScale = DoorScale;
                door4.transform.localScale = DoorScale;
                //name
                door3.gameObject.AddComponent<DoorNametagExtension>().UpdateName("EXIT_1");
                door4.gameObject.AddComponent<DoorNametagExtension>().UpdateName("EXIT_2");
                //spawn
                NetworkServer.Spawn(door1.gameObject);
                NetworkServer.Spawn(door2.gameObject);
                NetworkServer.Spawn(door3.gameObject);
                NetworkServer.Spawn(door4.gameObject);
            }
            if (plugin.Config.EditObjectsOnSurface)
            {
                //Prefabの確保
                var primitivePrefab = CustomNetworkManager.singleton.spawnPrefabs.First(x => x.name.Contains("Primitive"));
                var lightPrefab = CustomNetworkManager.singleton.spawnPrefabs.First(x => x.name.Contains("LightSource"));
                var stationPrefab = CustomNetworkManager.singleton.spawnPrefabs.First(x => x.name.Contains("Station"));

                //ElevatorA
                var station1 = UnityEngine.Object.Instantiate(stationPrefab, new Vector3(-0.15f, 1000f, 9.75f), Quaternion.Euler(Vector3.up * 180f));
                //En face l'ElevatorB
                var station2 = UnityEngine.Object.Instantiate(stationPrefab, new Vector3(86.69f, 987.2f, -70.85f), Quaternion.Euler(Vector3.up));
                //MTF
                var station3 = UnityEngine.Object.Instantiate(stationPrefab, new Vector3(147.9f, 992.77f, -46.2f), Quaternion.Euler(Vector3.up * 90f));
                //ElevatorB
                var station4 = UnityEngine.Object.Instantiate(stationPrefab, new Vector3(83f, 992.77f, -46.35f), Quaternion.Euler(Vector3.up * 90f));
                //CI
                var station5 = UnityEngine.Object.Instantiate(stationPrefab, new Vector3(10.37f, 987.5f, -47.5f), Quaternion.Euler(Vector3.up * 180f));
                //Surface
                var station6 = UnityEngine.Object.Instantiate(stationPrefab, new Vector3(56.5f, 1000f, -68.5f), Quaternion.Euler(Vector3.up * 270f));
                var station7 = UnityEngine.Object.Instantiate(stationPrefab, new Vector3(56.5f, 1000f, -71.85f), Quaternion.Euler(Vector3.up * 270f));

                //Light Nuke Red
                var light_nuke = UnityEngine.Object.Instantiate(lightPrefab.GetComponent<LightSourceToy>());
                light_nuke.transform.position = new Vector3(40.75f, 991f, -35.75f);
                light_nuke.NetworkLightRange = 4.5f;
                light_nuke.NetworkLightIntensity = 2f;
                light_nuke.NetworkLightColor = Color.red;

                //Lumiére dans le couloir de la GateA 
                var light_GateA1 = UnityEngine.Object.Instantiate(lightPrefab.GetComponent<LightSourceToy>());
                light_GateA1.transform.position = new Vector3(-1, 1005.2f, -37);
                light_GateA1.NetworkLightRange = 6f;
                light_GateA1.NetworkLightIntensity = 1f;
                light_GateA1.NetworkLightColor = Color.white;

                var light_GateA2 = UnityEngine.Object.Instantiate(lightPrefab.GetComponent<LightSourceToy>());
                light_GateA2.transform.position = new Vector3(-1, 1005.2f, -29.5f);
                light_GateA2.NetworkLightRange = 6f;
                light_GateA2.NetworkLightIntensity = 1f;
                light_GateA2.NetworkLightColor = Color.white;

                //SCP-106 Do not go on Container Of 106
                var room106 = Map.Rooms.First(x => x.Type == RoomType.Hcz106);

                var wall_106 = UnityEngine.Object.Instantiate(primitivePrefab.GetComponent<PrimitiveObjectToy>());
                wall_106.transform.SetParentAndOffset(room106.transform, new Vector3(7.2f, 2.6f, -14.3f));
                wall_106.transform.localScale = new Vector3(20, 5, 14);
                if (room106.transform.forward == Vector3.left || room106.transform.forward == Vector3.right)
                    wall_106.transform.rotation = Quaternion.Euler(Vector3.up * 90f);
                wall_106.UpdatePositionServer();
                wall_106.NetworkPrimitiveType = PrimitiveType.Cube;

                NetworkServer.Spawn(station1);
                NetworkServer.Spawn(station2);
                NetworkServer.Spawn(station3);
                NetworkServer.Spawn(station4);
                NetworkServer.Spawn(station5);
                NetworkServer.Spawn(station6);
                NetworkServer.Spawn(station7);

                NetworkServer.Spawn(light_nuke.gameObject);
                NetworkServer.Spawn(light_GateA1.gameObject);
                NetworkServer.Spawn(light_GateA2.gameObject);
            }
            Log.Info($"[OnWaintingForPlayers] Waiting for Players...");
        }

        public void OnRoundStart()
        {
            Timing.CallDelayed(1f, () =>
            {
                if (Player.Get(RoleType.Scp049).Count() > 0 && DoorNametagExtension.NamedDoors.TryGetValue("049_GATE", out DoorNametagExtension door))
                {
                    Door.Get(door.TargetDoor).IsOpen = true;
                }
            });
            Timing.CallDelayed(5f, () =>
            {
                if (Player.Get(RoleType.Scp049).Count() > 0 && DoorNametagExtension.NamedDoors.TryGetValue("049_GATE", out DoorNametagExtension door))
                {
                    Door.Get(door.TargetDoor).IsOpen = true;
                }
            });
            Log.Info($"[OnRoundStart] Round Start!");
        }

        public void OnRoundEnd(RoundEndedEventArgs ev)
        {
            Log.Info($"[OnRoundEnd] Round Ended.{ev.TimeToRestart}");

            if (SanyaRemastered.Instance.Config.GodmodeAfterEndround)
            {
                foreach (Player player in Player.List)
                {
                    player.ReferenceHub.characterClassManager.GodMode = true;
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
            foreach (var player in Player.List)
                if (player.GameObject.TryGetComponent<Scp914Effect>(out var comp))
                    UnityEngine.Object.Destroy(comp);
            foreach (var player in Player.List)
                if (player.GameObject.TryGetComponent<ContainScpComponent>(out var comp))
                    UnityEngine.Object.Destroy(comp);
            SanyaRemasteredComponent._scplists.Clear();

            foreach (var cor in RoundCoroutines)
                Timing.KillCoroutines(cor);
            RoundCoroutines.Clear();

            RoundSummary.singleton.RoundEnded = true;
        }
        public void OnTeamRespawn(RespawningTeamEventArgs ev)
        {
            Log.Debug($"[OnTeamRespawn] Queues:{ev.Players.Count} NextKnowTeam:{ev.NextKnownTeam} MaxAmount:{ev.MaximumRespawnAmount}", SanyaRemastered.Instance.Config.IsDebugged);

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
            Log.Debug($"[OnWarheadStart] {ev.Player?.Nickname}", SanyaRemastered.Instance.Config.IsDebugged);

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
                foreach (var door in Map.Doors)
                    if (door.Base.NetworkActiveLocks == (ushort)DoorLockReason.Warhead)
                        door.Base.NetworkTargetState = false;
            }
        }

        public void OnDetonated()
        {
            Log.Debug($"[OnDetonated] Detonated:{RoundSummary.roundTime / 60:00}:{RoundSummary.roundTime % 60:00}", SanyaRemastered.Instance.Config.IsDebugged);

            if (SanyaRemastered.Instance.Config.OutsidezoneTerminationTimeAfterNuke >= 0)
            {
                RoundCoroutines.Add(Timing.RunCoroutine(Coroutines.AirSupportBomb(false, SanyaRemastered.Instance.Config.OutsidezoneTerminationTimeAfterNuke), Segment.FixedUpdate));
            }
        }
        public void OnAnnounceDecont(AnnouncingDecontaminationEventArgs ev)
        {
            if (ev.Id != 6)
                Log.Debug($"[OnAnnounceDecont] {ev.Id} {DecontaminationController.Singleton._stopUpdating}", SanyaRemastered.Instance.Config.IsDebugged);

            if (SanyaRemastered.Instance.Config.CassieSubtitle)
                switch (ev.Id)
                {
                    case 0:
                        {
                            foreach (Player player in Player.List)
                            {
                                if (player?.CurrentRoom?.Zone == ZoneType.LightContainment)
                                    player.Broadcast(20, Subtitles.DecontaminationInit, shouldClearPrevious: true);
                            }
                            break;
                        }
                    case 1:
                        {
                            foreach (Player player in Player.List)
                            {
                                if (player?.CurrentRoom?.Zone == ZoneType.LightContainment)
                                    player.Broadcast(20, Subtitles.DecontaminationMinutesCount.Replace("{0}", "10"), shouldClearPrevious: true);
                            }
                            break;
                        }
                    case 2:
                        {
                            foreach (Player player in Player.List)
                            {
                                if (player?.CurrentRoom?.Zone == ZoneType.LightContainment)
                                    player.Broadcast(20, Subtitles.DecontaminationMinutesCount.Replace("{0}", "5"), shouldClearPrevious: true);
                            }
                            break;
                        }
                    case 3:
                        {
                            foreach (Player player in Player.List)
                                if (player?.CurrentRoom?.Zone == ZoneType.LightContainment)
                                {
                                    player.Broadcast(20, Subtitles.DecontaminationMinutesCount.Replace("{0}", "1"), shouldClearPrevious: true);
                                }
                            break;
                        }
                    case 4:
                        {
                            foreach (Player player in Player.List)
                                if (player?.CurrentRoom?.Zone == ZoneType.LightContainment)
                                {
                                    player.Broadcast(45, Subtitles.Decontamination30s.Replace("{0}", "10"), shouldClearPrevious: true);
                                }
                            break;
                        }
                    default:
                        break;

                }
        }
        public void OnDecontaminating(DecontaminatingEventArgs ev)
        {
            Log.Debug($"[OnDecontaminating]", SanyaRemastered.Instance.Config.IsDebugged);

            if (plugin.Config.CassieSubtitle)
                Methods.SendSubtitle(Subtitles.DecontaminationLockdown, 15);
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
            Log.Debug($"[OnGeneratorFinish] {ev.Generator.gameObject.GetComponent<Room>()?.Name}", SanyaRemastered.Instance.Config.IsDebugged);

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
            if (SanyaRemastered.Instance.Config.GeneratorFinishLock) ev.Generator.ServerSetFlag(Scp079Generator.GeneratorFlags.Open, false);


        }
        public void OnPlacingBulletHole(PlacingBulletHole ev)
        {
            if (ev.Position != Vector3.zero && Physics.Linecast(ev.Owner.Position, ev.Position, out RaycastHit raycastHit, grenade_pickup_mask))
            {
                if (raycastHit.transform.TryGetComponent<ItemPickupBase>(out var pickup))
                {
                    if (SanyaRemastered.Instance.Config.Grenade_shoot_fuse)
                    {
                        var thrownProjectile = pickup.transform.GetComponentInParent<ThrownProjectile>();
                        if (thrownProjectile != null && thrownProjectile.Info.ItemId != ItemType.SCP018)
                        {
                            var timeGrenade = raycastHit.transform.GetComponentInParent<TimeGrenade>();
                            if (timeGrenade != null)
                            {
                                timeGrenade.TargetTime = 0.1f;
                                goto finish;
                            }
                        }
                    }
                    if (SanyaRemastered.Instance.Config.Item_shoot_move)
                    {
                        pickup.Rb.AddExplosionForce((2.5f / (pickup.Info.Weight + 1)) + 4, ev.Owner.Position, 500f, 3f, ForceMode.Impulse);
                        goto finish;
                    }
                }

                if (SanyaRemastered.Instance.Config.OpenDoorOnShoot)
                {
                    var basicDoor = raycastHit.transform.GetComponentInParent<BasicDoor>();
                    if (basicDoor != null)
                    {
                        if ((basicDoor is IDamageableDoor damageableDoor) && damageableDoor.IsDestroyed) goto finish;
                        if (basicDoor.GetExactState() != 1f && basicDoor.GetExactState() != 0f) goto finish;
                        if (basicDoor.NetworkActiveLocks != 0) goto finish;
                        if (basicDoor.RequiredPermissions.RequiredPermissions == Interactables.Interobjects.DoorUtils.KeycardPermissions.None && !(basicDoor is PryableDoor))
                        {
                            basicDoor.ServerInteract(ev.Owner.ReferenceHub, 0);
                        }
                    }
                }
            finish:;
            }
        }
        public void OnDamagingWindow(DamagingWindowEventArgs ev)
        {
            if (plugin.Config.ContainCommand && Map.FindParentRoom(ev.Window.gameObject).Type == RoomType.Hcz049)
            {
                ev.Damage = 0;
            }
        }
    }
}
