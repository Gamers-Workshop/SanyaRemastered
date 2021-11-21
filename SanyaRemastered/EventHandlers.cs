using CustomPlayerEffects;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Pickups;
using InventorySystem.Items.Radio;
using InventorySystem.Items.ThrowableProjectiles;
using LightContainmentZoneDecontamination;
using MapGeneration.Distributors;
using MEC;
using Mirror;
using PlayableScps;
using Respawning;
using SanyaRemastered.Data;
using SanyaRemastered.Functions;
using Scp914;
using System;
using System.Collections.Generic;
using System.Linq;
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
        //private List<GameObject> PlayerHasEnrage096InPocket = new List<GameObject>();

        /** RoundVar **/
        private uint playerlistnetid = 0;
        private Vector3 nextRespawnPos = Vector3.zero;

        internal List<CoroutineHandle> RoundCoroutines { get => roundCoroutines; set => roundCoroutines = value; }

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
            Log.Debug($"[OnAnnounceDecont] {ev.Id} {DecontaminationController.Singleton._stopUpdating}", SanyaRemastered.Instance.Config.IsDebugged);

            if (SanyaRemastered.Instance.Config.CassieSubtitle)
                switch (ev.Id)
                {
                    case 0:
                        {
                            foreach (Player player in Player.List)
                            {
                                if (player.CurrentRoom.Zone == ZoneType.LightContainment)
                                    player.Broadcast(20, Subtitles.DecontaminationInit);
                            }
                            break;
                        }
                    case 1:
                        {
                            foreach (Player player in Player.List)
                            {
                                if (player.CurrentRoom.Zone == ZoneType.LightContainment)
                                    player.Broadcast(20, Subtitles.DecontaminationMinutesCount.Replace("{0}", "10"));
                            }
                            break;
                        }
                    case 2:
                        {
                            foreach (Player player in Player.List)
                            {
                                if (player.CurrentRoom.Zone == ZoneType.LightContainment)
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
                        pickup.Rb.AddExplosionForce((2.5f / (pickup.Info.Weight + 1))+2, ev.Owner.Position, 500f, 3f, ForceMode.Impulse);
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

        public void OnPreAuth(PreAuthenticatingEventArgs ev)
        {
            Log.Debug($"[OnPreAuth] {ev.Request.RemoteEndPoint.Address}:{ev.UserId}", SanyaRemastered.Instance.Config.IsDebugged);
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
            Log.Debug($"[OnPlayerLeave] {ev.Player.Nickname} ({ev.Player.ReferenceHub.queryProcessor._ipAddress}:{ev.Player.UserId})", SanyaRemastered.Instance.Config.IsDebugged);
        }

        public void OnPlayerSetClass(ChangingRoleEventArgs ev)
        {
            if (ev.Player.IsHost) return;
            Log.Debug($"[OnPlayerSetClass] {ev.Player.Nickname} -{ev.Reason}> {ev.NewRole}", SanyaRemastered.Instance.Config.IsDebugged);
            if (ev.Player.GameObject.TryGetComponent<ContainScpComponent>(out var comp1))
                UnityEngine.Object.Destroy(comp1);
            if (ev.Player.GameObject.TryGetComponent<Scp914Effect>(out var comp2))
                UnityEngine.Object.Destroy(comp2);
            if (SanyaRemastered.Instance.Config.Scp079ExtendEnabled && ev.NewRole == RoleType.Scp079)
            {
                RoundCoroutines.Add(Timing.CallDelayed(5f, () => 
                { 
                    ev.Player.ReferenceHub.GetComponent<SanyaRemasteredComponent>().AddHudCenterDownText(Subtitles.Extend079First, 20);
                    ev.Player.ResetInventory(new List<ItemType> {ItemType.KeycardJanitor, ItemType.KeycardScientist, ItemType.GunCOM15, ItemType.GunShotgun,ItemType.Medkit,ItemType.GrenadeFlash});
                }));
            }
            if (ev.Reason == SpawnReason.Escaped && plugin.Config.Scp096Real)
            {
                bool IsAnTarget = false;
                foreach (Player scp096 in Player.List.Where(x => x.Role == RoleType.Scp096))
                    if (scp096.CurrentScp is PlayableScps.Scp096 Scp096 && Scp096.HasTarget(ev.Player.ReferenceHub))
                        IsAnTarget = true;
                if (IsAnTarget)
                {
                    ev.NewRole = RoleType.Spectator;
                    ev.Player.ReferenceHub.GetComponent<SanyaRemasteredComponent>().AddHudCenterDownText("Vous avez été abatue a la sortie car vous avez vue le visage de SCP-096", 10);
                }
            }
        }


        public void OnPlayerSpawn(SpawningEventArgs ev)
        {
            if (ev.Player.IsHost) return;
            Log.Debug($"[OnPlayerSpawn] {ev.Player.Nickname} -{ev.RoleType}-> {ev.Position}", SanyaRemastered.Instance.Config.IsDebugged);
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
                ev.Player.EnableEffect(EffectType.Disabled);
            }
        }
        public void OnPlayerHurt(HurtingEventArgs ev)
        {
            if (ev.Target.IsHost || ev.Target.Role == RoleType.Spectator || ev.Target.ReferenceHub.characterClassManager.GodMode || ev.Target.ReferenceHub.characterClassManager.SpawnProtected || !ev.IsAllowed) return;
            Log.Debug($"[OnPlayerHurt:Before] {ev.Attacker?.Nickname}[{ev.Attacker?.Role}] -{ev.HitInformation.Attacker}({ev.HitInformation.Amount})-> {ev.Target?.Nickname}[{ev.Target?.Role}]", SanyaRemastered.Instance.Config.IsDebugged);

            if (ev.Target == null) return;
            if (ev.DamageType == DamageTypes.Scp207 && ev.Target.GameObject.TryGetComponent<Scp914Effect>(out var comp) && ev.HitInformation.Attacker != "SCP-914")
            {
                ev.Amount = 0;
            }
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
                    if (ev.Target.ArtificialHealth < ev.Amount && SanyaRemastered.Instance.Config.ScpDamageMultiplicator.TryGetValue(ev.Target.Role, out float AmmountDamage) && AmmountDamage != 1)
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

            Log.Debug($"[OnPlayerHurt:After] {ev.Attacker?.Nickname}[{ev.Attacker?.Role}] -{ev.HitInformation.Attacker}({ev.HitInformation.Amount})-> {ev.Target?.Nickname}[{ev.Target?.Role}]", SanyaRemastered.Instance.Config.IsDebugged);
        }

        public void OnDied(DiedEventArgs ev)
        {
            if (ev.Target.IsHost || ev.Target.Role == RoleType.Spectator || ev.Target.ReferenceHub.characterClassManager.GodMode || ev.Target.ReferenceHub.characterClassManager.SpawnProtected) return;
            Log.Debug($"[OnPlayerDeath] {ev.Killer?.Nickname}[{ev.Killer?.Role}] -{ev.HitInformations.Tool.Name}-> {ev.Target?.Nickname}[{ev.Target?.Role}]", SanyaRemastered.Instance.Config.IsDebugged);

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
                ev.Killer.SendHitmarker(3);
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

                Log.Debug($"[Check079] SCPs:{count} isFound079:{isFound079} Gen:{Map.ActivatedGenerators}", SanyaRemastered.Instance.Config.IsDebugged);
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
            Log.Debug($"[OnPocketDimDeath] {ev.Player.Nickname}", SanyaRemastered.Instance.Config.IsDebugged);
            /*if (PlayerHasEnrage096InPocket.Contains(ev.Player.GameObject))
                PlayerHasEnrage096InPocket.Remove(ev.Player.GameObject);*/
            List<Player> Scp106 = Player.List.Where(x => x.Role == RoleType.Scp106).ToList();
            foreach (Player player in Scp106)
            {
                player.SendHitmarker();
            }
            if (SanyaRemastered.Instance.Config.ScpRecoveryAmount.TryGetValue("Scp106", out int heal) && heal > 0)
            {
                foreach (Player player in Scp106)
                {
                    player.ReferenceHub.playerStats.HealHPAmount(heal);
                }
            }
        }
        public void OnThrowingItem(ThrowingItemEventArgs ev)
        {
            if (plugin.Config.Scp079ExtendEnabled && ev.Player.Role == RoleType.Scp079)
                ev.IsAllowed = false;
        }
        public void OnPlayerUsingItem(UsingItemEventArgs ev)
        {
            if (plugin.Config.Scp079ExtendEnabled && ev.Player.Role == RoleType.Scp079)
                ev.IsAllowed = false;
        }
        public void OnPlayerItemUsed(UsedItemEventArgs ev)
        {
            Log.Debug($"[OnPlayerUsedMedicalItem] {ev.Player.Nickname} -> {ev.Item.Type}", SanyaRemastered.Instance.Config.IsDebugged);
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
                ev.Player.DisableAllEffects();
                if (ev.Player.GameObject.TryGetComponent<Scp914Effect>(out var comp))
                    UnityEngine.Object.Destroy(comp);
            }
        }

        public void OnPlayerTriggerTesla(TriggeringTeslaEventArgs ev)
        {
            Log.Debug($"[OnPlayerTriggerTesla] {ev.IsInHurtingRange}:{ev.Player.Nickname}", SanyaRemastered.Instance.Config.IsDebugged);
            if (SanyaRemastered.Instance.Config.TeslaNoTriggerRadioPlayer && ev.Player.ReferenceHub.characterClassManager.IsHuman() && ev.Player.ReferenceHub.inventory.UserInventory.Items.Any(x => x.Value.ItemTypeId == ItemType.Radio && x.Value.GetComponent<RadioItem>().IsUsable))
                ev.IsTriggerable = false;
        }

        public void OnPlayerDoorInteract(InteractingDoorEventArgs ev)
        {
            Log.Debug($"[OnPlayerDoorInteract] {ev.Player.Nickname}:{ev.Door?.Nametag}", SanyaRemastered.Instance.Config.IsDebugged);
            if (plugin.Config.ContainCommand && ev.Player.Team == Team.SCP)
            {
                Methods.IsCanBeContain(ev.Player);
            }
            if (plugin.Config.ScpCantInteract)
            {
                if (!ev.Door.IsOpen && plugin.Config.ScpCantInteractList.TryGetValue("DoorInteractOpen", out var role) && role.Contains(ev.Player.Role))
                {
                    ev.IsAllowed = false;
                }
                if (ev.Door.IsOpen && plugin.Config.ScpCantInteractList.TryGetValue("DoorInteractClose", out var role2) && role2.Contains(ev.Player.Role))
                {
                    ev.IsAllowed = false;
                }
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
            Log.Debug($"[OnPlayerLockerInteract] {ev.Player.Nickname}:{ev.Locker.name}", SanyaRemastered.Instance.Config.IsDebugged);
        }
        public void OnInteractingElevator(InteractingElevatorEventArgs ev)
        {
            Log.Debug($"[OnInteractingElevator] Player : {ev.Player}  Name : {ev.Elevator.GetType().Name}", SanyaRemastered.Instance.Config.IsDebugged);
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
        public void OnShooting(ShootingEventArgs ev)
        {
            Log.Debug($"[OnShooting] {ev.Shooter.Nickname} -{ev.ShotPosition}-> {Player.Get(ev.TargetNetId)?.Nickname}", SanyaRemastered.Instance.Config.IsDebugged);
            if (SanyaRemastered.Instance.Config.Scp079ExtendEnabled && ev.Shooter.Role == RoleType.Scp079) 
                ev.IsAllowed = false;
            if (SanyaRemastered.Instance.Config.Scp096Real)
            {
                Player target = Player.Get(ev.TargetNetId);
                if (target != null && target.Role == RoleType.Scp096)
                {
                    ev.IsAllowed = false;
                }
            }
        }
        public void OnUsingMicroHIDEnergy(UsingMicroHIDEnergyEventArgs ev)
        {
            if (ev.Player.SessionVariables.ContainsKey("InfAmmo"))
            {
                ev.Drain = 0;
            }
            if (SanyaRemastered.Instance.Config.MicroHidNotActive.Contains(ev.Player.Role))
            {
                ev.IsAllowed = false;
            }
        }
        public void OnSyncingData(SyncingDataEventArgs ev)
        {
            if (ev.Player == null || ev.Player.IsHost || !ev.Player.ReferenceHub.Ready) return;

            if (SanyaRemastered.Instance.Config.Scp079ExtendEnabled && ev.Player.Role == RoleType.Scp079)
            {
                if (ev.CurrentAnimation == 1)
                    ev.Player.ReferenceHub.GetComponent<SanyaRemasteredComponent>().AddHudCenterDownText(Subtitles.ExtendEnabled, 3);
                else
                    ev.Player.ReferenceHub.GetComponent<SanyaRemasteredComponent>().AddHudCenterDownText(Subtitles.ExtendDisabled, 3);
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
            Log.Debug($"[OnActivatingWarheadPanel] Nickname : {ev.Player.Nickname}  Allowed : {ev.IsAllowed}", SanyaRemastered.Instance.Config.IsDebugged);

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
            if (ev.IsAllowed && SanyaRemastered.Instance.Config.GeneratorUnlockOpen) ev.Generator.ServerSetFlag(Scp079Generator.GeneratorFlags.Open, true);
        }
        public void OnStoppingGenerator(StoppingGeneratorEventArgs ev)
        {
            Log.Debug($"[OnStoppingGenerator] {ev.Player.Nickname} -> {ev.Generator.gameObject.GetComponent<Room>()?.Name}", SanyaRemastered.Instance.Config.IsDebugged);
        }
        public void OnGeneratorOpen(OpeningGeneratorEventArgs ev)
        {
            Log.Debug($"[OnGeneratorOpen] {ev.Player.Nickname} -> {ev.Generator.gameObject.GetComponent<Room>()?.Name}", SanyaRemastered.Instance.Config.IsDebugged);
            if (ev.Generator.Engaged && SanyaRemastered.Instance.Config.GeneratorFinishLock)
            {
                ev.IsAllowed = false;
            }
        }

        public void OnGeneratorClose(ClosingGeneratorEventArgs ev)
        {
            Log.Debug($"[OnGeneratorClose] {ev.Player.Nickname} -> {ev.Generator.gameObject.GetComponent<Room>()?.Name}", SanyaRemastered.Instance.Config.IsDebugged);
        }

        public void OnActivatingGenerator(ActivatingGeneratorEventArgs ev)
        {
            Log.Debug($"[OnActivatingGenerator] {ev.Player.Nickname} -> {ev.Generator.gameObject.GetComponent<Room>()?.Name}", SanyaRemastered.Instance.Config.IsDebugged);
            if (SanyaRemastered.Instance.Config.GeneratorActivatingClose) ev.Generator.ServerSetFlag(Scp079Generator.GeneratorFlags.Open, false);
        }

        public void OnHandcuffing(HandcuffingEventArgs ev)
        {
            Log.Debug($"[OnHandcuffing] {ev.Cuffer.Nickname} -> {ev.Target.Nickname}", SanyaRemastered.Instance.Config.IsDebugged);
            if (ev.Target.IsGodModeEnabled) ev.IsAllowed = false;
            /*if (SanyaRemastered.Instance.Config.Scp049Real)
            {
                ReferenceHub.GetHub(ev.Target.GameObject)..NetworkCufferId = ev.Cuffer.ReferenceHub.queryProcessor.PlayerId;
            }*/
        }
        public void OnProcessingHotkey(ProcessingHotkeyEventArgs ev)
        {
            Log.Debug($"[OnProcessingHotkey] {ev.Player.Nickname} -> {ev.Hotkey}", SanyaRemastered.Instance.Config.IsDebugged);

            if (plugin.Config.Scp079ExtendEnabled && ev.Player.Role == RoleType.Scp079)
            {
                Scp079PlayerScript scp079 = ev.Player.ReferenceHub.scp079PlayerScript;
                ev.IsAllowed = false;
                switch (ev.Hotkey)
                {
                    case HotkeyButton.Keycard:
                        {
                            if (scp079.Network_curLvl + 1 >= SanyaRemastered.Instance.Config.Scp079ExtendLevelFindscp)
                            {
                                List<Camera079> cams = new List<Camera079>();
                                foreach (var ply in Player.List)
                                {
                                    if (ply.Team == Team.SCP && ply.Role != RoleType.Scp079)
                                    {
                                        cams.AddRange(ply.Position.GetNearCams());
                                    }
                                }

                                Camera079 target;
                                if (cams.Count > 0)
                                {
                                    target = cams.GetRandomOne();
                                }
                                else break;

                                if (target != null)
                                {
                                    if (SanyaRemastered.Instance.Config.Scp079ExtendCostFindscp > scp079.Mana)
                                    {
                                        scp079.RpcNotEnoughMana(SanyaRemastered.Instance.Config.Scp079ExtendCostFindscp, scp079.Mana);
                                        ev.Player.ReferenceHub.GetComponent<SanyaRemasteredComponent>().AddHudCenterDownText(Subtitles.Extend079NoEnergy, 5);
                                        break;
                                    }

                                    scp079.RpcSwitchCamera(target.cameraId, false);
                                    scp079.Mana -= SanyaRemastered.Instance.Config.Scp079ExtendCostFindscp;
                                    scp079.currentCamera = target;
                                    break;
                                }
                            }
                            ev.Player.ReferenceHub.GetComponent<SanyaRemasteredComponent>().AddHudCenterDownText(Subtitles.Extend079NoLevel, 5);
                            break;
                        }
                    case HotkeyButton.PrimaryFirearm:
                        {

                            if (!ev.Player.SessionVariables.ContainsKey("scp079_advanced_mode"))
                                ev.Player.SessionVariables.Add("scp079_advanced_mode", null);
                            ev.Player.ReferenceHub.GetComponent<SanyaRemasteredComponent>().AddHudCenterDownText(Subtitles.ExtendEnabled, 5);
                            ev.Player.Inventory.ServerSelectItem(4);
                            break;
                        }
                    case HotkeyButton.SecondaryFirearm:
                        {
                            ev.Player.ReferenceHub.GetComponent<SanyaRemasteredComponent>().AddHudCenterDownText(Subtitles.ExtendDisabled, 5);
                            ev.Player.SessionVariables.Remove("scp079_advanced_mode");
                            ev.Player.Inventory.ServerSelectItem(0);
                            break;
                        }
                    case HotkeyButton.Medical:
                        {
                            if (scp079.Network_curLvl + 1 >= SanyaRemastered.Instance.Config.Scp079ExtendLevelFindscp)
                            {
                                List<Camera079> cams = new List<Camera079>();
                                foreach (var gen in Recontainer079.AllGenerators)
                                {
                                    if (gen.Activating)
                                    {
                                        cams.AddRange(gen.gameObject.transform.position.GetNearCams());
                                    }
                                }

                                Camera079 target;
                                if (cams.Count > 0)
                                {
                                    target = cams.GetRandomOne();
                                }
                                else break;

                                if (target != null)
                                {
                                    if (SanyaRemastered.Instance.Config.Scp079ExtendCostFindscp > scp079.Mana)
                                    {
                                        scp079.RpcNotEnoughMana(SanyaRemastered.Instance.Config.Scp079ExtendCostFindscp, scp079.Mana);
                                        ev.Player.ReferenceHub.GetComponent<SanyaRemasteredComponent>().AddHudCenterDownText(Subtitles.Extend079NoEnergy, 5);
                                        break;
                                    }

                                    scp079.RpcSwitchCamera(target.cameraId, false);
                                    scp079.Mana -= SanyaRemastered.Instance.Config.Scp079ExtendCostFindscp;
                                    scp079.currentCamera = target;
                                    break;
                                }
                            }
                            ev.Player.ReferenceHub.GetComponent<SanyaRemasteredComponent>().AddHudCenterDownText(Subtitles.Extend079NoLevel, 5);
                            break;
                        }
                }
            }
        }
        public void On079LevelGain(GainingLevelEventArgs ev)
        {
            Log.Debug($"[On079LevelGain] {ev.Player.Nickname} : {ev.NewLevel}", SanyaRemastered.Instance.Config.IsDebugged);

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
            Log.Debug($"[On106MakePortal] {ev.Player.Nickname}:{ev.Position}", SanyaRemastered.Instance.Config.IsDebugged);
        }

        public void On106Teleport(TeleportingEventArgs ev)
        {
            Log.Debug($"[On106Teleport] {ev.Player.Nickname}:{ev.PortalPosition}", SanyaRemastered.Instance.Config.IsDebugged);
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
                                ev.Player.SetRole(RoleType.Scp93989, lite: true);
                                break;
                            }
                            else if (ev.Player.Role == RoleType.Scp93989)
                            {
                                var Health = ev.Player.Health;
                                ev.Player.SetRole(RoleType.Scp93953, lite: true);
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
                            if (!ev.Player.GameObject.TryGetComponent<Scp914Effect>(out var Death))
                            {
                                var comp = ev.Player.GameObject.AddComponent<Scp914Effect>();
                                comp.TimerBeforeDeath = 60;
                            }
                            else
                            {
                                Death.TimerBeforeDeath /= 2;
                            }
                        }
                        break;
                    case Scp914KnobSetting.VeryFine:
                        {
                            if (!ev.Player.GameObject.TryGetComponent<Scp914Effect>(out _))
                            {
                                var comp = ev.Player.GameObject.AddComponent<Scp914Effect>();
                                ev.Player.ChangeRunningSpeed(100, false);
                                ev.Player.ChangeWalkingSpeed(100, false);
                                comp.TimerBeforeDeath = 5;
                            }
                        }
                        break;
                }
            }
        }
        public void On096AddingTarget(AddingTargetEventArgs ev)
        {
            if (SanyaRemastered.Instance.Config.Scp096Real)
            {
                ev.EnrageTimeToAdd = -18;
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
            }
            else if (SanyaRemastered.Instance.Config.Scp096Real)
            {
                ev.Scp096.EnrageTimeLeft = 0.3f;
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
    }
}