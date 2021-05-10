using CustomPlayerEffects;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using Grenades;
using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using LightContainmentZoneDecontamination;
using LiteNetLib.Utils;
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

        /** Infosender **/
        private readonly UdpClient udpClient = new UdpClient();
        internal Task sendertask;

        /** AuthChecker **/
        internal const byte BypassFlags = (1 << 1) | (1 << 3);
        internal static readonly NetDataReader reader = new NetDataReader();
        internal static readonly NetDataWriter writer = new NetDataWriter();
        internal static readonly Dictionary<string, string> kickedbyChecker = new Dictionary<string, string>();

        /** Update **/
        internal IEnumerator<float> EverySecond()
        {
            while (true)
            {
                try
                {
                    //ItemCleanup
                    if (SanyaRemastered.Instance.Config.ItemCleanup > 0)
                    {
                        List<GameObject> nowitems = null;

                        foreach (var i in ItemCleanupPatch.items)
                        {
                            if (Time.time - i.Value > SanyaRemastered.Instance.Config.ItemCleanup && i.Key != null)
                            {
                                if (nowitems == null) nowitems = new List<GameObject>();
                                if (SanyaRemastered.Instance.Config.IsDebugged) Log.Debug($"[ItemCleanupPatch] Cleanup:{i.Key.transform.position} {Time.time - i.Value} > {SanyaRemastered.Instance.Config.ItemCleanup}");
                                nowitems.Add(i.Key);
                            }
                        }

                        if (nowitems != null)
                        {
                            foreach (var x in nowitems)
                            {
                                ItemCleanupPatch.items.Remove(x);
                                NetworkServer.Destroy(x);
                            }
                        }
                    }
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

        /** Flag Params **/
        private readonly int grenade_pickup_mask = 1049088;
        private int prevMaxAHP = 0;
        public bool StopRespawn = false;
        public List<Vector3> DecalList = new List<Vector3>();

        /** RoundVar **/
        private FlickerableLightController flickerableLightController = null;
        internal bool IsEnableBlackout = false;
        private uint playerlistnetid = 0;
        private Vector3 nextRespawnPos = Vector3.zero;
        private Camera079 last079cam = null;

        internal List<CoroutineHandle> RoundCoroutines { get => roundCoroutines; set => roundCoroutines = value; }

        public void OnWaintingForPlayers()
        {
            loaded = true;

            RoundCoroutines.Add(Timing.RunCoroutine(EverySecond(), Segment.FixedUpdate));

            DecalList.Clear();
            ItemCleanupPatch.items.Clear();
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

            if (plugin.Config.AddDoorsOnSurface)
            {
                var LCZprefab = UnityEngine.Object.FindObjectsOfType<MapGeneration.DoorSpawnpoint>().First(x => x.TargetPrefab.name.Contains("LCZ"));
                var EZprefab = UnityEngine.Object.FindObjectsOfType<MapGeneration.DoorSpawnpoint>().First(x => x.TargetPrefab.name.Contains("EZ"));
                var HCZprefab = UnityEngine.Object.FindObjectsOfType<MapGeneration.DoorSpawnpoint>().First(x => x.TargetPrefab.name.Contains("HCZ"));
                // Couloir spawn Chaos
                var door1 = UnityEngine.Object.Instantiate(LCZprefab.TargetPrefab, new UnityEngine.Vector3(14.425f, 995.2f, -43.525f), Quaternion.Euler(Vector3.zero));
                var door2 = UnityEngine.Object.Instantiate(LCZprefab.TargetPrefab, new UnityEngine.Vector3(14.425f, 995.2f, -23.25f), Quaternion.Euler(Vector3.zero));
                // Exit
                var door3 = UnityEngine.Object.Instantiate(EZprefab.TargetPrefab, new UnityEngine.Vector3(176.2f, 983.24f, 35.23f), Quaternion.Euler(Vector3.up * 180f));
                var door4 = UnityEngine.Object.Instantiate(EZprefab.TargetPrefab, new UnityEngine.Vector3(174.4f, 983.24f, 29.1f), Quaternion.Euler(Vector3.up * 90f));

                //Spawn Main du serpent 
                var door5 = UnityEngine.Object.Instantiate(HCZprefab.TargetPrefab, new UnityEngine.Vector3(1.15f, 1000f, 4.8f), Quaternion.Euler(Vector3.up * 0f));
                (door5 as BreakableDoor)._ignoredDamageSources |= DoorDamageType.Grenade;
                var door6 = UnityEngine.Object.Instantiate(HCZprefab.TargetPrefab, new UnityEngine.Vector3(-1.27f, 1000f, 4.8f), Quaternion.Euler(Vector3.up * 360f));
                (door6 as BreakableDoor)._ignoredDamageSources |= DoorDamageType.Grenade;

                door5.gameObject.AddComponent<DoorNametagExtension>().UpdateName("GATE_EX_R");
                door6.gameObject.AddComponent<DoorNametagExtension>().UpdateName("GATE_EX_L");

                NetworkServer.Spawn(door1.gameObject);
                NetworkServer.Spawn(door2.gameObject);
                NetworkServer.Spawn(door3.gameObject);
                NetworkServer.Spawn(door4.gameObject);
                NetworkServer.Spawn(door5.gameObject);
                NetworkServer.Spawn(door6.gameObject);
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

            foreach (var cor in RoundCoroutines)
                Timing.KillCoroutines(cor);
            RoundCoroutines.Clear();

            RoundSummary.singleton._roundEnded = true;
        }

        public void OnWarheadStart(StartingEventArgs ev)
        {
            if (SanyaRemastered.Instance.Config.IsDebugged) Log.Debug($"[OnWarheadStart] {ev.Player?.Nickname}");

            if (SanyaRemastered.Instance.Config.Scp049_2DontOpenDoorAnd106 && (ev.Player.Role == RoleType.Scp0492 || ev.Player.Role == RoleType.Scp106))
            {
                ev.IsAllowed = false;
            }
            if (SanyaRemastered.Instance.Config.Scp939And096DontOpenlockerAndGenerator && (ev.Player.Role == RoleType.Scp93953 || ev.Player.Role == RoleType.Scp93989 || ev.Player.Role == RoleType.Scp096))
            {
                ev.IsAllowed = false;
            }
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
                        if (door.NetworkActiveLocks == (ushort)DoorLockReason.Warhead)
                            door.NetworkTargetState = false;
            }
        }

        public void OnDetonated()
        {
            if (SanyaRemastered.Instance.Config.IsDebugged) Log.Debug($"[OnDetonated] Detonated:{RoundSummary.roundTime / 60:00}:{RoundSummary.roundTime % 60:00}");

            if (SanyaRemastered.Instance.Config.OutsidezoneTerminationTimeAfterNuke >= 0)
            {
                RoundCoroutines.Add(Timing.RunCoroutine(Coroutines.AirSupportBomb(false, SanyaRemastered.Instance.Config.OutsidezoneTerminationTimeAfterNuke)));
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

        public void OnPreAuth(PreAuthenticatingEventArgs ev)
        {
            if (SanyaRemastered.Instance.Config.IsDebugged) Log.Debug($"[OnPreAuth] {ev.Request.RemoteEndPoint.Address}:{ev.UserId}");
        }

        public void OnPlayerVerified(VerifiedEventArgs ev)
        {
            if (ev.Player.ReferenceHub.characterClassManager.IsHost) return;
            Log.Info($"[OnPlayerJoin] {ev.Player.Nickname} ({ev.Player.ReferenceHub.queryProcessor._ipAddress}:{ev.Player.UserId})");

            if (kickedbyChecker.TryGetValue(ev.Player.UserId, out var reason))
            {
                string reasonMessage = string.Empty;
                if (reason == "steam")
                    reasonMessage = Subtitles.LimitedKickMessage;

                ServerConsole.Disconnect(ev.Player.ReferenceHub.characterClassManager.connectionToClient, reasonMessage);
                kickedbyChecker.Remove(ev.Player.UserId);
                return;
            }
            if (plugin.Config.DisablePlayerLists && playerlistnetid > 0)
            {
                ObjectDestroyMessage objectDestroyMessage = new ObjectDestroyMessage
                {
                    netId = playerlistnetid
                };
                ev.Player.Connection.Send(objectDestroyMessage, 0);
            }

            //MuteFixer
            foreach (var player in Player.List)
                if (player.IsMuted)
                    player.ReferenceHub.characterClassManager.SetDirtyBit(2uL);
            //Component
            ev.Player.GameObject.AddComponent<SanyaRemasteredComponent>();
        }

        public void OnPlayerDestroying(DestroyingEventArgs ev)
        {
            if (ev.Player.IsHost) return;
            if (SanyaRemastered.Instance.Config.IsDebugged) Log.Debug($"[OnPlayerLeave] {ev.Player.Nickname} ({ev.Player.ReferenceHub.queryProcessor._ipAddress}:{ev.Player.UserId})");
            if (SanyaRemasteredComponent._scplists.Contains(ev.Player))
                SanyaRemasteredComponent._scplists.Remove(ev.Player);
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
            //Scp939Extend
            if (ev.NewRole.Is939())
            {
                if (prevMaxAHP == 0) prevMaxAHP = ev.Player.ReferenceHub.playerStats.maxArtificialHealth;
                ev.Player.ReferenceHub.playerStats.NetworkmaxArtificialHealth = 0;
                ev.Player.ReferenceHub.playerStats.NetworkartificialHpDecay = 0f;
                ev.Player.ReferenceHub.playerStats.NetworkartificialNormalRatio = 1f;
            }
            else if (ev.Player.ReferenceHub.characterClassManager._prevId.Is939())
            {
                ev.Player.ReferenceHub.playerStats.NetworkmaxArtificialHealth = this.prevMaxAHP;
                ev.Player.ReferenceHub.playerStats.NetworkartificialHpDecay = 0.75f;
                ev.Player.ReferenceHub.playerStats.NetworkartificialNormalRatio = 0.7f;
            }
        }


        public void OnPlayerSpawn(SpawningEventArgs ev)
        {
            if (ev.Player.IsHost) return;
            if (SanyaRemastered.Instance.Config.IsDebugged) Log.Debug($"[OnPlayerSpawn] {ev.Player.Nickname} -{ev.RoleType}-> {ev.Position}");
            ev.Player.ReferenceHub.fpc.staminaController.RemainingStamina += 1;
            if (ev.Player.Role == (RoleType.Scp93953 | RoleType.Scp93989))
                ev.Player.Scale = new Vector3(SanyaRemastered.Instance.Config.Scp939Size, SanyaRemastered.Instance.Config.Scp939Size, SanyaRemastered.Instance.Config.Scp939Size);
            if (SanyaRemastered.Instance.Config.Scp0492effect && ev.Player.Role == RoleType.Scp0492)
            {
                ev.Player.ReferenceHub.playerEffectsController.EnableEffect<Ensnared>(3f);
                ev.Player.ReferenceHub.playerEffectsController.EnableEffect<Deafened>(5f);
                ev.Player.ReferenceHub.playerEffectsController.EnableEffect<Blinded>(3f);
                ev.Player.ReferenceHub.playerEffectsController.EnableEffect<Amnesia>(5f);
                ev.Player.ReferenceHub.playerEffectsController.EnableEffect<Flashed>(0.2f);
            }
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
            if (SanyaRemastered.Instance.Config.IsDebugged) Log.Debug($"[OnPlayerHurt:Before] {ev.Attacker?.Nickname}[{ev.Attacker?.Role}] -{ev.HitInformations.GetDamageName()}({ev.HitInformations.Amount})-> {ev.Target?.Nickname}[{ev.Target?.Role}]");

            if (ev.Attacker == null) return;

            if (ev.DamageType != DamageTypes.Nuke
                && ev.DamageType != DamageTypes.Decont
                && ev.DamageType != DamageTypes.Wall
                && ev.DamageType != DamageTypes.Tesla
                && ev.DamageType != DamageTypes.Scp207)
            {
                //GrenadeHitmark
                if (SanyaRemastered.Instance.Config.HitmarkGrenade
                    && ev.DamageType == DamageTypes.Grenade
                    && ev.Target.UserId != ev.Attacker.UserId)
                {
                    ev.Attacker.GameObject.GetComponent<Hitmarker>().Trigger();
                }

                //USPMultiplier
                if (ev.DamageType == DamageTypes.Usp)
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
                    if (SanyaRemastered.Instance.Config.ScpDamageMultiplicator.TryGetValue(ev.Target.Role, out float AmmountDamage))
                    {
                        ev.Amount *= AmmountDamage;
                    }
                    if (ev.Target.Role == RoleType.Scp106)
                    {
                        if (ev.DamageType == DamageTypes.Grenade) ev.Amount *= SanyaRemastered.Instance.Config.Scp106GrenadeMultiplicator;
                        if (ev.DamageType != DamageTypes.MicroHid
                            && ev.DamageType != DamageTypes.Tesla)
                            ev.Amount *= SanyaRemastered.Instance.Config.Scp106DamageMultiplicator;
                    }
                }
            }

            if (SanyaRemastered.Instance.Config.IsDebugged) Log.Debug($"[OnPlayerHurt:After] {ev.Attacker?.Nickname}[{ev.Attacker?.Role}] -{ev.HitInformations.GetDamageName()}({ev.HitInformations.Amount})-> {ev.Target?.Nickname}[{ev.Target?.Role}]");
        }

        public void OnDied(DiedEventArgs ev)
        {
            if (ev.Target.IsHost || ev.Target.Role == RoleType.Spectator || ev.Target.ReferenceHub.characterClassManager.GodMode || ev.Target.ReferenceHub.characterClassManager.SpawnProtected) return;
            if (SanyaRemastered.Instance.Config.IsDebugged) Log.Debug($"[OnPlayerDeath] {ev.Killer?.Nickname}[{ev.Killer?.Role}] -{ev.HitInformations.GetDamageName()}-> {ev.Target?.Nickname}[{ev.Target?.Role}]");
            if (SanyaRemastered.Instance.Config.Scp939Size != 1)
            {
                ev.Target.Scale = new Vector3(1, 1, 1);
            }
            if (ev.Killer == null) return;

            if (SanyaRemastered.Instance.Config.ScpRecoveryAmount.TryGetValue(ev.HitInformations.GetDamageType(), out int Heal))
            {
                ev.Killer.ReferenceHub.playerStats.HealHPAmount(Heal);
            }

            if (SanyaRemastered.Instance.Config.HitmarkKilled
                && ev.Killer.Team != Team.SCP
                && !string.IsNullOrEmpty(ev.Killer.UserId)
                && ev.Killer.UserId != ev.Target.UserId)
            {
                ev.Killer.GameObject.GetComponent<Hitmarker>().Trigger(2);
            }

            if (SanyaRemastered.Instance.Config.CassieSubtitle
                && ev.Target.Team == Team.SCP
                && ev.HitInformations.Attacker != "DISCONNECT")
            {
                string fullname = CharacterClassManager._staticClasses.Get(ev.Target.Role).fullName;
                string str;
                if (ev.Target.Role != RoleType.Scp0492)
                    switch (ev.HitInformations.GetDamageType().name)
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
                                Team killerTeam = ev.Killer.Team;
                                foreach (Player Exiledi in Player.List)
                                {
                                    if (Exiledi.ReferenceHub.queryProcessor.PlayerId == ev.HitInformations.PlayerId)
                                    {
                                        killerTeam = Exiledi.Team;
                                    }
                                }
                                if (SanyaRemastered.Instance.Config.IsDebugged) Log.Debug($"[CheckTeam] ply:{ev.Target.ReferenceHub.queryProcessor.PlayerId} kill:{ev.Killer.ReferenceHub.queryProcessor.PlayerId} plyid:{ev.HitInformations.PlayerId} killteam:{killerTeam}");
                                switch (killerTeam)
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

                if (SanyaRemastered.Instance.Config.IsDebugged) Log.Debug($"[Check079] SCPs:{count} isFound079:{isFound079} totalvol:{Generator079.mainGenerator.totalVoltage} forced:{Generator079.mainGenerator.forcedOvercharge}");
                if (count == 1
                    && isFound079
                    && Generator079.mainGenerator.totalVoltage < 4
                    && !Generator079.mainGenerator.forcedOvercharge
                    && ev.HitInformations.GetDamageType() != DamageTypes.Nuke)
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

            if (ev.HitInformations.GetDamageType() == DamageTypes.Decont || ev.HitInformations.GetDamageType() == DamageTypes.Nuke)
            {
                ev.Target.Inventory.Clear();
            }
        }
        public void OnPocketDimDeath(FailingEscapePocketDimensionEventArgs ev)
        {
            if (SanyaRemastered.Instance.Config.IsDebugged) Log.Debug($"[OnPocketDimDeath] {ev.Player.Nickname}");

            if (SanyaRemastered.Instance.Config.ScpRecoveryAmount.TryGetValue(DamageTypes.Scp106 , out int heal))
            {
                foreach (Player player in Player.List.Where(x => x.Role == RoleType.Scp106))
                {
                    player.ReferenceHub.playerStats.HealHPAmount(heal);
                    player.GameObject.GetComponent<Hitmarker>().Trigger(2);
                }
            }
        }

        public void OnPlayerUsedMedicalItem(UsedMedicalItemEventArgs ev)
        {
            if (SanyaRemastered.Instance.Config.IsDebugged) Log.Debug($"[OnPlayerUsedMedicalItem] {ev.Player.Nickname} -> {ev.Item}");

            if (ev.Item == ItemType.Medkit)
            {
                ev.Player.ReferenceHub.playerEffectsController.DisableEffect<Hemorrhage>();
                ev.Player.ReferenceHub.playerEffectsController.DisableEffect<Bleeding>();
                ev.Player.ReferenceHub.playerEffectsController.DisableEffect<Burned>();
            }
            if (ev.Item == ItemType.Adrenaline)
            {
                ev.Player.ReferenceHub.playerEffectsController.DisableEffect<Panic>();
                ev.Player.ReferenceHub.playerEffectsController.DisableEffect<Disabled>();
            }
            if (ev.Item == ItemType.SCP500)
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
            if (SanyaRemastered.Instance.Config.TeslaTriggerableTeams.Count == 0
                || SanyaRemastered.Instance.Config.TeslaTriggerableTeamsParsed.Contains(ev.Player.Team))
            {
                if (SanyaRemastered.Instance.Config.TeslaNoTriggerableDisarmed || ev.Player.CufferId == -1)
                {
                    ev.IsTriggerable = true;
                }
                else
                {
                    ev.IsTriggerable = false;
                }
            }
            else
            {
                ev.IsTriggerable = false;
            }
        }

        public void OnPlayerDoorInteract(InteractingDoorEventArgs ev)
        {
            if (SanyaRemastered.Instance.Config.IsDebugged) Log.Debug($"[OnPlayerDoorInteract] {ev.Player.Nickname}:{ev.Door.name}");


            if (SanyaRemastered.Instance.Config.Scp049_2DontOpenDoorAnd106 && (ev.Player.Role == RoleType.Scp0492 || ev.Player.Role == RoleType.Scp106))
            {
                ev.IsAllowed = false;
            }
            if (plugin.Config.InventoryKeycardActivation && ev.Player.Team != Team.SCP && !ev.Player.IsBypassModeEnabled)
            {
                ev.IsAllowed = false;

                DoorLockMode lockMode = DoorLockUtils.GetMode((DoorLockReason)ev.Door.ActiveLocks);
                if (ev.IsAllowed
                || ((ev.Door is IDamageableDoor damageableDoor) && damageableDoor.IsDestroyed)
                || (ev.Door.NetworkTargetState && !lockMode.HasFlagFast(DoorLockMode.CanClose))
                || (!ev.Door.NetworkTargetState && !lockMode.HasFlagFast(DoorLockMode.CanOpen))
                || lockMode == DoorLockMode.FullLock) return;

                if (ev.Door.RequiredPermissions.RequiredPermissions.ToTruthyPermissions() == Keycard.Permissions.None)
                    ev.IsAllowed = true;

                foreach (var item in ev.Player.Inventory.items.Where(x => x.id.IsKeycard()))
                {
                    Item[] itemlist = UnityEngine.Object.FindObjectOfType<Inventory>().availableItems;

                    Item gameItem = Array.Find(itemlist, i => i.id == item.id);

                    // Relevant for items whose type was not found
                    if (gameItem == null)
                        continue;

                    Keycard.Permissions itemPerms = Keycard.ToTruthyPermissions(gameItem.permissions);

                    if (itemPerms.HasFlagFast(ev.Door.RequiredPermissions.RequiredPermissions.ToTruthyPermissions(), ev.Door.RequiredPermissions.RequireAll))
                    {
                        ev.IsAllowed = true;
                    }
                }
            }
            if (plugin.Config.AddDoorsOnSurface && ev.Door.TryGetComponent<DoorNametagExtension>(out var nametag))
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
                foreach (var item in ev.Player.Inventory.items)
                {
                    if (ev.Player.Inventory.GetItemByID(item.id).permissions.Contains("PEDESTAL_ACC"))
                    {
                        ev.IsAllowed = true;
                    }
                }
            }
            if (SanyaRemastered.Instance.Config.Scp049_2DontOpenDoorAnd106 && (ev.Player.Role == RoleType.Scp0492 || ev.Player.Role == RoleType.Scp106))
            {
                ev.IsAllowed = false;
            }
            if (SanyaRemastered.Instance.Config.Scp939And096DontOpenlockerAndGenerator && (ev.Player.Role == RoleType.Scp93953 || ev.Player.Role == RoleType.Scp93989 || ev.Player.Role == RoleType.Scp096))
            {
                ev.IsAllowed = false;
            }
        }
        public void OnIntercomSpeaking(IntercomSpeakingEventArgs ev)
        {
            if (!SanyaRemastered.Instance.Config.IntercomBrokenOnBlackout) return;

            Room RoomIntercom = Map.Rooms.Where(x => x.Type == RoomType.EzIntercom).Single();
            if (RoomIntercom.LightsOff)
            {
                ev.IsAllowed = false;
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
                && !ev.Player.ReferenceHub.fpc.staminaController._invigorated.Enabled
                && !ev.Player.ReferenceHub.fpc.staminaController._scp207.Enabled
                )
            {
                ev.Player.ReferenceHub.fpc.staminaController.RemainingStamina -= SanyaRemastered.Instance.Config.StaminaLostJump;
                ev.Player.ReferenceHub.fpc.staminaController._regenerationTimer = 0f;
            }
            if (SanyaRemastered.Instance.Config.StaminaEffect)
            {
                if (ev.Player.ReferenceHub.fpc.staminaController.RemainingStamina <= 0f
                    && ev.Player.ReferenceHub.characterClassManager.IsHuman()
                    && !ev.Player.ReferenceHub.fpc.staminaController._invigorated.Enabled
                    && !ev.Player.ReferenceHub.fpc.staminaController._scp207.Enabled
                    )
                {
                    ev.Player.ReferenceHub.playerEffectsController.EnableEffect<Disabled>(1f);
                }
            }
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

            if (plugin.Config.RandomRespawnPosPercent > 0)
            {
                int randomnum = UnityEngine.Random.Range(0, 100);
                Log.Debug($"[RandomRespawnPos] Check:{randomnum}>{plugin.Config.RandomRespawnPosPercent}", SanyaRemastered.Instance.Config.IsDebugged);
                if (randomnum > plugin.Config.RandomRespawnPosPercent && !Warhead.IsDetonated)
                {
                    List<Vector3> poslist = new List<Vector3>
                    {
                        Exiled.API.Extensions.Role.GetRandomSpawnPoint(RoleType.Scp049),
                        Exiled.API.Extensions.Role.GetRandomSpawnPoint(RoleType.Scp93953),
                    };

                    if (!Map.IsLCZDecontaminated && DecontaminationController.Singleton._nextPhase < 4)
                    {
                        poslist.Add(Map.Rooms.First(x => x.Type == Exiled.API.Enums.RoomType.LczArmory).Position);

                        foreach (var itempos in RandomItemSpawner.singleton.posIds)
                        {
                            if (itempos.posID == "RandomPistol" && itempos.position.position.y > 0.5f && itempos.position.position.y < 0.7f)
                            {
                                poslist.Add(new Vector3(itempos.position.position.x, itempos.position.position.y, itempos.position.position.z));
                            }
                            else if (itempos.posID == "toilet_keycard" && itempos.position.position.y > 1.25f && itempos.position.position.y < 1.35f)
                            {
                                poslist.Add(new Vector3(itempos.position.position.x, itempos.position.position.y - 0.5f, itempos.position.position.z));
                            }
                        }
                    }

                    foreach (GameObject roomid in GameObject.FindGameObjectsWithTag("RoomID"))
                    {
                        Rid rid = roomid.GetComponent<Rid>();
                        if (rid != null && (rid.id == "LCZ_ARMORY" || rid.id == "Shelter"))
                        {
                            poslist.Add(roomid.transform.position);
                        }
                    }

                    foreach (var i in poslist)
                    {
                        Log.Debug($"[RandomRespawnPos] TargetLists:{i}", SanyaRemastered.Instance.Config.IsDebugged);
                    }

                    int randomnumlast = UnityEngine.Random.Range(0, poslist.Count);
                    nextRespawnPos = new Vector3(poslist[randomnumlast].x, poslist[randomnumlast].y + 2, poslist[randomnumlast].z);

                    Log.Info($"[RandomRespawnPos] Determined:{nextRespawnPos}");
                }
                else
                {
                    nextRespawnPos = Vector3.zero;
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
        public void OnActivatingWarheadPanel(ActivatingWarheadPanelEventArgs ev)
        {
            if (SanyaRemastered.Instance.Config.IsDebugged) Log.Debug($"[OnActivatingWarheadPanel] Nickname : {ev.Player.Nickname}  Allowed : {ev.IsAllowed}");


            if (plugin.Config.InventoryKeycardActivation && !ev.Player.IsBypassModeEnabled && ev.Player.Team != Team.SCP)
            {
                foreach (var item in ev.Player.Inventory.items.Where(x => x.id.IsKeycard()))
                {
                    Item[] itemlist = UnityEngine.Object.FindObjectOfType<Inventory>().availableItems;

                    Item gameItem = Array.Find(itemlist, i => i.id == item.id);

                    // Relevant for items whose type was not found
                    if (gameItem == null)
                        continue;

                    Keycard.Permissions itemPerms = Keycard.ToTruthyPermissions(gameItem.permissions);

                    if (itemPerms.HasFlagFast(Keycard.Permissions.ContainmentLevelThree, false))
                    {
                        ev.IsAllowed = true;
                    }
                }
            }
            var outsite = UnityEngine.Object.FindObjectOfType<AlphaWarheadOutsitePanel>();
            if (SanyaRemastered.Instance.Config.Nukecapclose && outsite.keycardEntered)
            {
                Timing.RunCoroutine(Coroutines.CloseNukeCap());
            }
            else if (outsite.keycardEntered)
            {
                ev.IsAllowed = false;
            }
        }
        public void OnGeneratorUnlock(UnlockingGeneratorEventArgs ev)
        {
            if (plugin.Config.InventoryKeycardActivation && !ev.Player.IsBypassModeEnabled && ev.Player.Team != Team.SCP)
            {
                foreach (var item in ev.Player.Inventory.items)
                {
                    if (ev.Player.Inventory.GetItemByID(item.id).permissions.Contains("ARMORY_LVL_2"))
                    {
                        ev.IsAllowed = true;
                    }
                }
            }

            if (ev.IsAllowed && SanyaRemastered.Instance.Config.GeneratorUnlockOpen)
            {
                ev.Generator._doorAnimationCooldown = 2f;
                ev.Generator.NetworkisDoorOpen = true;
                ev.Generator.RpcDoSound(true);
            }
        }

        public void OnGeneratorOpen(OpeningGeneratorEventArgs ev)
        {
            if (SanyaRemastered.Instance.Config.IsDebugged) Log.Debug($"[OnGeneratorOpen] {ev.Player.Nickname} -> {ev.Generator.CurRoom}");
            if (ev.Generator.prevFinish && SanyaRemastered.Instance.Config.GeneratorFinishLock)
            {
                ev.IsAllowed = false;
            }
            if (SanyaRemastered.Instance.Config.Scp049_2DontOpenDoorAnd106 && (ev.Player.Role == RoleType.Scp0492 || ev.Player.Role == RoleType.Scp106))
            {
                ev.IsAllowed = false;
            }
            if (SanyaRemastered.Instance.Config.Scp939And096DontOpenlockerAndGenerator && (ev.Player.Role == RoleType.Scp93953 || ev.Player.Role == RoleType.Scp93989 || ev.Player.Role == RoleType.Scp096))
            {
                ev.IsAllowed = false;
            }
        }

        public void OnGeneratorClose(ClosingGeneratorEventArgs ev)
        {
            if (SanyaRemastered.Instance.Config.IsDebugged) Log.Debug($"[OnGeneratorClose] {ev.Player.Nickname} -> {ev.Generator.CurRoom}");
            if (ev.IsAllowed && ev.Generator.isTabletConnected && SanyaRemastered.Instance.Config.GeneratorActivatingClose)
            {
                ev.Generator._doorAnimationCooldown = 2f;
                ev.Generator.NetworkisDoorOpen = false;
                ev.Generator.RpcDoSound(false);
            }
            if (SanyaRemastered.Instance.Config.Scp049_2DontOpenDoorAnd106 && (ev.Player.Role == RoleType.Scp0492 || ev.Player.Role == RoleType.Scp106))
            {
                ev.IsAllowed = false;
            }
            if (SanyaRemastered.Instance.Config.Scp939And096DontOpenlockerAndGenerator && (ev.Player.Role == RoleType.Scp93953 || ev.Player.Role == RoleType.Scp93989 || ev.Player.Role == RoleType.Scp096))
            {
                ev.IsAllowed = false;
            }
        }
        public void OnEjectingGeneratorTablet(EjectingGeneratorTabletEventArgs ev)
        {
            if (SanyaRemastered.Instance.Config.IsDebugged) Log.Debug($"[OnEjectingGeneratorTablet] {ev.Player.Nickname} -> {ev.Generator.CurRoom}");
            if (SanyaRemastered.Instance.Config.Scp049_2DontOpenDoorAnd106 && (ev.Player.Role == RoleType.Scp0492 || ev.Player.Role == RoleType.Scp106))
            {
                ev.IsAllowed = false;
            }
            if (SanyaRemastered.Instance.Config.Scp939And096DontOpenlockerAndGenerator && (ev.Player.Role == RoleType.Scp93953 || ev.Player.Role == RoleType.Scp93989 || ev.Player.Role == RoleType.Scp096))
            {
                ev.IsAllowed = false;
            }
        }
        public void OnGeneratorInsert(InsertingGeneratorTabletEventArgs ev)
        {
            if (SanyaRemastered.Instance.Config.IsDebugged) Log.Debug($"[OnGeneratorInsert] {ev.Player.Nickname} -> {ev.Generator.CurRoom}");
        }

        public void OnGeneratorFinish(GeneratorActivatedEventArgs ev)
        {
            if (SanyaRemastered.Instance.Config.IsDebugged) Log.Debug($"[OnGeneratorFinish] {ev.Generator.CurRoom}");
            if (SanyaRemastered.Instance.Config.GeneratorFinishLock) ev.Generator.NetworkisDoorOpen = false;

            int curgen = Generator079.mainGenerator.NetworktotalVoltage + 1;
            if (SanyaRemastered.Instance.Config.CassieSubtitle && !Generator079.mainGenerator.forcedOvercharge)
            {
                if (curgen < 5)
                {
                    Methods.SendSubtitle(Subtitles.GeneratorFinish.Replace("{0}", curgen.ToString()).Replace("{s}", curgen == 1 ? "" : "s"), 10);
                }
                else
                {
                    Methods.SendSubtitle(Subtitles.GeneratorComplete, 20);
                }
            }
        }
        public void OnInteractingElevator(InteractingElevatorEventArgs ev)
        {
            if (SanyaRemastered.Instance.Config.IsDebugged) Log.Debug($"[OnInteractingElevator] Player : {ev.Player}  Name : {ev.Elevator.GetType().Name}");
            if (SanyaRemastered.Instance.Config.Scp049_2DontOpenDoorAnd106 && (ev.Player.Role == RoleType.Scp0492))
            {
                ev.IsAllowed = false;
            }
        }
        public void OnPlacingDecal(PlacingDecalEventArgs ev)
        {
            if (SanyaRemastered.Instance.Config.IsDebugged) Log.Debug($"[OnPlacingDecal] position : {ev.Position} Owner: {ev.Owner} Type: {ev.Type}");
            if (SanyaRemastered.Instance.Config.Coroding106 && ev.Type == 6)
            {
                DecalList.Add(ev.Position);
            }
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
                        ev.Player.ReferenceHub.GetComponent<SanyaRemasteredComponent>().AddHudCenterDownText(Subtitles.Extend079Lv4, 10);
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
        public void On914Upgrade(UpgradingItemsEventArgs ev)
        {
            if (SanyaRemastered.Instance.Config.IsDebugged) Log.Debug($"[On914Upgrade] {ev.KnobSetting} Players:{ev.Players.Count} Items:{ev.Items.Count}");
            if (SanyaRemastered.Instance.Config.Scp914Effect)
            {
                switch (ev.KnobSetting)
                {
                    case Scp914Knob.Rough:
                        foreach (var player in ev.Players)
                        {
                            var Death = new PlayerStats.HitInfo(99999, "Scp-914", DamageTypes.RagdollLess, 0);
                            player.ReferenceHub.playerStats.HurtPlayer(Death, player.ReferenceHub.gameObject);
                            if (player.Team != Team.SCP)
                                player.ReferenceHub.GetComponent<SanyaRemasteredComponent>().AddHudCenterDownText("Un cadavre gravement mutilé a été trouvé à l'intérieur de SCP-914. Le sujet a évidemment été affiné par le SCP-914 sur le réglage Rough.", 30);
                        }
                        break;
                    case Scp914Knob.Coarse:
                        foreach (var player in ev.Players)
                        {
                            if (player.Role == RoleType.Scp93953 || player.Role == RoleType.Scp93989)
                            {
                                var Death = new PlayerStats.HitInfo(99999, "Scp-914", DamageTypes.RagdollLess, 0);
                                player.ReferenceHub.playerStats.HurtPlayer(Death, player.ReferenceHub.gameObject);
                            }
                            if (player.Team != Team.SCP)
                            {
                                var Hit = new PlayerStats.HitInfo(70, "Scp-914", DamageTypes.RagdollLess, 0);
                                player.ReferenceHub.playerStats.HurtPlayer(Hit, player.ReferenceHub.gameObject);
                                player.ReferenceHub.playerEffectsController.GetEffect<Hemorrhage>();
                                player.ReferenceHub.playerEffectsController.GetEffect<Bleeding>();
                                player.ReferenceHub.playerEffectsController.GetEffect<Disabled>();
                                player.ReferenceHub.GetComponent<SanyaRemasteredComponent>().AddHudCenterDownText("Vous remarquez d'innombrables petites incisions dans votre corps.", 10);
                            }
                        }
                        break;
                    case Scp914Knob.OneToOne:
                        foreach (var player in ev.Players)
                        {
                            if (player.Role == RoleType.Scp93953)
                            {
                                var Health = player.Health;
                                player.SetRole(RoleType.Scp93989);
                                var Hit = new PlayerStats.HitInfo(player.MaxHealth - Health, "Scp-914", DamageTypes.RagdollLess, 0);
                                player.ReferenceHub.playerStats.HurtPlayer(Hit, player.ReferenceHub.gameObject);
                                break;
                            }
                            if (player.Role == RoleType.Scp93989)
                            {
                                var Health = player.Health;
                                player.SetRole(RoleType.Scp93953);
                                var Hit = new PlayerStats.HitInfo(player.MaxHealth - Health, "Scp-914", DamageTypes.RagdollLess, 0);
                                player.ReferenceHub.playerStats.HurtPlayer(Hit, player.ReferenceHub.gameObject);
                                break;
                            }
                            if (player.Team != Team.SCP)
                            {
                                {

                                }
                            }
                        }
                        break;
                    case Scp914Knob.Fine:
                        foreach (var player in ev.Players)
                        {
                            player.ReferenceHub.fpc.effectScp207.Intensity = 20;
                        }
                        break;
                    case Scp914Knob.VeryFine:
                        foreach (var player in ev.Players)
                        {
                            var Death = new PlayerStats.HitInfo(99999, "Scp-914", DamageTypes.Scp096, 0);
                            player.ReferenceHub.playerStats.HurtPlayer(Death, player.ReferenceHub.gameObject);
                            player.ReferenceHub.GetComponent<SanyaRemasteredComponent>().AddHudCenterDownText("L'analyse chimique de la substance a l'intérieur de SCP-914 reste non concluante.", 30);
                        }
                        break;
                }
            }
        }
        public void OnCalmingDown(CalmingDownEventArgs ev)
        {
            if (SanyaRemastered.Instance.Config.IsDebugged) Log.Debug($"[On106Teleport] {ev.Player.Nickname}");
            if (SanyaRemastered.Instance.Config.Scp096Real)
            {
                ev.IsAllowed = false;
            }
        }
        public void OnChangingItem(ChangingItemEventArgs ev)
        {
            if (ev.Player.ReferenceHub.weaponManager._reloadingWeapon == ev.Player.ReferenceHub.weaponManager.curWeapon
                && ev.Player.ReferenceHub.weaponManager._reloadingWeapon != -100)
            {
                ev.Player.ReferenceHub.weaponManager._reloadingWeapon = -100;
                ev.Player.ReferenceHub.weaponManager._reloadCooldown = -1f;
            }
        }
        public void OnShoot(ShootingEventArgs ev)
        {
            if (SanyaRemastered.Instance.Config.IsDebugged) Log.Debug($"[OnShoot] {ev.Shooter.Nickname} -{ev.Position}-> {ev.Target?.name}");

            if (SanyaRemastered.Instance.Config.StaminaLostLogicer > 0f
                  && ev.Shooter.ReferenceHub.characterClassManager.IsHuman()
                  && ItemType.GunLogicer == ev.Shooter.CurrentItem.id
                  && ev.IsAllowed
                  && !ev.Shooter.ReferenceHub.fpc.staminaController._invigorated.Enabled
                  && !ev.Shooter.ReferenceHub.fpc.staminaController._scp207.Enabled)

            {
                ev.Shooter.ReferenceHub.fpc.staminaController.RemainingStamina -= SanyaRemastered.Instance.Config.StaminaLostLogicer;
                ev.Shooter.ReferenceHub.fpc.staminaController._regenerationTimer = 0f;
            }

            if (ev.Position != Vector3.zero
                && Physics.Linecast(ev.Shooter.Position, ev.Position, out RaycastHit raycastHit, grenade_pickup_mask))
            {
                if (SanyaRemastered.Instance.Config.Item_shoot_move)
                {
                    var pickup = raycastHit.transform.GetComponentInParent<Pickup>();
                    if (pickup != null && pickup.Rb != null)
                    {
                        pickup.Rb.AddExplosionForce(Vector3.Distance(ev.Position, ev.Shooter.Position), ev.Shooter.Position, 500f, 3f, ForceMode.Impulse);
                    }
                }

                if (SanyaRemastered.Instance.Config.Grenade_shoot_fuse)
                {
                    var fraggrenade = raycastHit.transform.GetComponentInParent<FragGrenade>();
                    if (fraggrenade != null)
                    {
                        fraggrenade.NetworkfuseTime = 0.1f;
                    }
                    var Flashgrenade = raycastHit.transform.GetComponentInParent<FlashGrenade>();
                    if (Flashgrenade != null)
                    {
                        Flashgrenade.NetworkfuseTime = 0.1f;
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

                        if (door.RequiredPermissions.RequiredPermissions.ToTruthyPermissions() == Keycard.Permissions.None && !(door is PryableDoor))
                        {
                            door.NetworkTargetState = !door.NetworkTargetState;
                        }
                    }
                }
                if (SanyaRemastered.Instance.Config.Scp096Real)
                {
                    var player = raycastHit.transform.GetComponentInParent<Player>();
                    if (player.Role == RoleType.Scp096)
                    {
                        ev.IsAllowed = false;
                    }
                }
            }
        }
        public void OnCommand(SendingConsoleCommandEventArgs ev)
        {
            string[] args = ev.Arguments.ToArray();
            if (SanyaRemastered.Instance.Config.IsDebugged) Log.Debug($"[OnCommand] Player : {ev.Player} command:{ev.Name} args:{args.Length}");
            string effort = $"{ev.Name} ";
            foreach (string s in ev.Arguments)
                effort += $"{s} ";

            args = effort.Split(' ');
            if (SanyaRemastered.Instance.Config.ContainCommand && ev.Player.Team == Team.SCP && args[0] == "contain")
            {
                switch (ev.Player.Role)
                {
                    case RoleType.Scp173:
                        {
                            foreach (var ply in Player.List)
                            {
                                if (ply.Role == RoleType.Scp079)
                                {
                                    ev.Player.SendConsoleMessage("SCP-079 est toujours présent", "default");
                                    ply.ReferenceHub.BroadcastMessage($"SCP-173 a fait la commande .contain dans la salle {ev.Player.CurrentRoom.Name}");
                                    return;
                                }
                            }
                            switch (ev.Player.CurrentRoom.Name)
                            {
                                case "LCZ_914 (14)":
                                    {
                                        bool success = false;
                                        {
                                            Vector3 end;
                                            Vector3 end2;
                                            var posroom = ev.Player.CurrentRoom.Transform.position - ev.Player.Position;
                                            var x1 = 2.9f;
                                            var x2 = -10.2f;
                                            var z1 = 10.1f;
                                            var z2 = -10.2f;
                                            var y1 = 0f;
                                            var y2 = -5f;
                                            if (ev.Player.CurrentRoom.Transform.rotation.eulerAngles.y == 0f)
                                            {
                                                end = new Vector3(x1, y1, z1);
                                                end2 = new Vector3(x2, y2, z2);
                                            }
                                            else if (ev.Player.CurrentRoom.Transform.rotation.eulerAngles.y == 90f)
                                            {
                                                end = new Vector3(z1, y1, -x2);
                                                end2 = new Vector3(z2, y2, -x1);
                                            }
                                            else if (ev.Player.CurrentRoom.Transform.rotation.eulerAngles.y == 180f)
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
                                                if (SanyaRemastered.Instance.Config.IsDebugged)
                                                {
                                                    Log.Info(end2.x < posroom.x);
                                                    Log.Info(posroom.x < end.x);
                                                    Log.Info(end2.y < posroom.y);
                                                    Log.Info(posroom.y < end.y);
                                                    Log.Info(end2.z < posroom.z);
                                                    Log.Info(posroom.z < end.z);
                                                }
                                            }
                                            if (end2.x < posroom.x && posroom.x < end.x && end2.y < posroom.y && posroom.y < end.y && end2.z < posroom.z && posroom.z < end.z)
                                            {

                                            }
                                            else
                                            {
                                                ev.Player.SendConsoleMessage("Tu doit étre dans ton confinement", "red");
                                                break;
                                            }
                                        }
                                        foreach (var door in Map.Doors)
                                        {
                                            if (door.name.Equals("914"))
                                                if (!door.NetworkTargetState && door.GetExactState() == 0f)
                                                {
                                                    door.ActiveLocks = (ushort)DoorLockReason.SpecialDoorFeature;
                                                    success = true;
                                                }
                                        }
                                        if (success)
                                        {
                                            ev.Player.SetRole(RoleType.Spectator);
                                            RespawnEffectsController.PlayCassieAnnouncement("SCP 1 7 3 as been contained in the Containment chamber of SCP 9 1 4", true, true);
                                            ev.Player.SendConsoleMessage("173 room 049", "default");
                                        }
                                        else
                                        {
                                            ev.Player.SendConsoleMessage("La gate n'est pas fermer", "default");
                                        }
                                        break;
                                    }
                                case "LCZ_012 (12)":
                                    {
                                        int TEST = 0;
                                        bool success = false;
                                        {
                                            Vector3 end;
                                            Vector3 end2;
                                            var posroom = ev.Player.CurrentRoom.Transform.position - ev.Player.Position;
                                            var x1 = 10.2f;
                                            var x2 = -9.6f;
                                            var z1 = 8.2f;
                                            var z2 = 2.7f;
                                            var y1 = 8f;
                                            var y2 = -3f;
                                            if (ev.Player.CurrentRoom.Transform.rotation.eulerAngles.y == 0f)
                                            {
                                                end = new Vector3(x1, y1, z1);
                                                end2 = new Vector3(x2, y2, z2);
                                            }
                                            else if (ev.Player.CurrentRoom.Transform.rotation.eulerAngles.y == 90f)
                                            {
                                                end = new Vector3(z1, y1, -x2);
                                                end2 = new Vector3(z2, y2, -x1);
                                            }
                                            else if (ev.Player.CurrentRoom.Transform.rotation.eulerAngles.y == 180f)
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
                                                Log.Info(end2.x < posroom.x);
                                                Log.Info(posroom.x < end.x);
                                                Log.Info(end2.y < posroom.y);
                                                Log.Info(posroom.y < end.y);
                                                Log.Info(end2.z < posroom.z);
                                                Log.Info(posroom.z < end.z);
                                            }
                                            if (end2.x < posroom.x && posroom.x < end.x && end2.y < posroom.y && posroom.y < end.y && end2.z < posroom.z && posroom.z < end.z)
                                            {
                                                TEST = 1;
                                            }
                                        }
                                        if (TEST != 1)
                                        {
                                            Vector3 end;
                                            Vector3 end2;
                                            var posroom = ev.Player.CurrentRoom.Transform.position - ev.Player.Position;
                                            var x1 = 9.8f;
                                            var x2 = -8.9f;
                                            var z1 = 7.8f;
                                            var z2 = -10f;
                                            var y1 = 8f;
                                            var y2 = 2.5f;
                                            if (ev.Player.CurrentRoom.Transform.rotation.eulerAngles.y == 0f)
                                            {
                                                end = new Vector3(x1, y1, z1);
                                                end2 = new Vector3(x2, y2, z2);
                                            }
                                            else if (ev.Player.CurrentRoom.Transform.rotation.eulerAngles.y == 90f)
                                            {
                                                end = new Vector3(z1, y1, -x2);
                                                end2 = new Vector3(z2, y2, -x1);
                                            }
                                            else if (ev.Player.CurrentRoom.Transform.rotation.eulerAngles.y == 180f)
                                            {
                                                end = new Vector3(-x2, y1, -z2);
                                                end2 = new Vector3(-x1, y2, -z1);
                                            }
                                            else
                                            {
                                                end = new Vector3(-z2, y1, x1);
                                                end2 = new Vector3(-z1, y2, x2);
                                            }
                                            Log.Info(end2.x < posroom.x);
                                            Log.Info(posroom.x < end.x);
                                            Log.Info(end2.y < posroom.y);
                                            Log.Info(posroom.y < end.y);
                                            Log.Info(end2.z < posroom.z);
                                            Log.Info(posroom.z < end.z);
                                            if (end2.x < posroom.x && posroom.x < end.x && end2.y < posroom.y && posroom.y < end.y && end2.z < posroom.z && posroom.z < end.z)
                                            {
                                                ev.Player.SendConsoleMessage("Tu n'est pas coincé", "default");
                                                TEST = 1;
                                            }
                                        }
                                        if (TEST == 0)
                                        {
                                            break;
                                        }
                                        foreach (var door in Map.Doors)
                                        {
                                            if (door.name.Equals("012"))
                                                if (!door.NetworkTargetState && door.GetExactState() == 0f)
                                                {
                                                    door.ActiveLocks = (ushort)DoorLockReason.SpecialDoorFeature;
                                                    success = true;
                                                }
                                        }
                                        if (success)
                                        {
                                            ev.Player.SetRole(RoleType.Spectator);
                                            RespawnEffectsController.PlayCassieAnnouncement("SCP 1 7 3 as been contained in the Containment chamber of SCP 0 1 2", true, true);
                                            ev.Player.SendConsoleMessage("173 room 049", "default");
                                        }
                                        else
                                        {
                                            ev.Player.SendConsoleMessage("La gate n'est pas fermer", "default");
                                        }
                                        break;
                                    }
                                case "HCZ_Room3ar":
                                    {
                                        bool success = false;
                                        {
                                            Vector3 end;
                                            Vector3 end2;
                                            var posroom = ev.Player.CurrentRoom.Transform.position - ev.Player.Position;
                                            var x1 = 0.1f;
                                            var x2 = -5.6f;
                                            var z1 = 2.9f;
                                            var z2 = -2.8f;
                                            var y1 = 0f;
                                            var y2 = -5f;
                                            if (ev.Player.CurrentRoom.Transform.rotation.eulerAngles.y == 0f)
                                            {
                                                end = new Vector3(x1, y1, z1);
                                                end2 = new Vector3(x2, y2, z2);
                                            }
                                            else if (ev.Player.CurrentRoom.Transform.rotation.eulerAngles.y == 90f)
                                            {
                                                end = new Vector3(z1, y1, -x2);
                                                end2 = new Vector3(z2, y2, -x1);
                                            }
                                            else if (ev.Player.CurrentRoom.Transform.rotation.eulerAngles.y == 180f)
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
                                                Log.Info(end2.x < posroom.x);
                                                Log.Info(posroom.x < end.x);
                                                Log.Info(end2.y < posroom.y);
                                                Log.Info(posroom.y < end.y);
                                                Log.Info(end2.z < posroom.z);
                                                Log.Info(posroom.z < end.z);
                                            }
                                            if (end2.x < posroom.x && posroom.x < end.x && end2.y < posroom.y && posroom.y < end.y && end2.z < posroom.z && posroom.z < end.z)
                                            {

                                            }
                                            else
                                            {
                                                ev.Player.SendConsoleMessage("Tu doit étre dans ton confinement", "red");
                                                break;
                                            }
                                        }
                                        foreach (var door in Map.Doors)
                                        {
                                            if (door.name.Equals("HCZ_ARMORY"))
                                                if (!door.NetworkTargetState && door.GetExactState() == 0f)
                                                {
                                                    door.ActiveLocks = (ushort)DoorLockReason.SpecialDoorFeature;
                                                    success = true;
                                                }
                                        }
                                        if (success)
                                        {
                                            ev.Player.SetRole(RoleType.Spectator);
                                            RespawnEffectsController.PlayCassieAnnouncement("SCP 1 7 3 as been contained in the Armory of Heavy containment Zone", true, true);
                                            ev.Player.SendConsoleMessage("Armory HCZ room 049", "default");
                                        }
                                        else
                                        {
                                            ev.Player.SendConsoleMessage("La gate n'est pas fermer", "default");
                                        }
                                        break;
                                    }
                                case "LCZ_Armory":
                                    {
                                        bool success = false;
                                        {
                                            Vector3 end;
                                            Vector3 end2;
                                            var posroom = ev.Player.CurrentRoom.Transform.position - ev.Player.Position;
                                            var x1 = 1.2f;
                                            var x2 = -9.5f;
                                            var z1 = 6f;
                                            var z2 = -7f;
                                            var y1 = -1f;
                                            var y2 = -10f;
                                            if (ev.Player.CurrentRoom.Transform.rotation.eulerAngles.y == 0f)
                                            {
                                                end = new Vector3(x1, y1, z1);
                                                end2 = new Vector3(x2, y2, z2);
                                            }
                                            else if (ev.Player.CurrentRoom.Transform.rotation.eulerAngles.y == 90f)
                                            {
                                                end = new Vector3(z1, y1, -x2);
                                                end2 = new Vector3(z2, y2, -x1);
                                            }
                                            else if (ev.Player.CurrentRoom.Transform.rotation.eulerAngles.y == 180f)
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
                                                Log.Info(end2.x < posroom.x);
                                                Log.Info(posroom.x < end.x);
                                                Log.Info(end2.y < posroom.y);
                                                Log.Info(posroom.y < end.y);
                                                Log.Info(end2.z < posroom.z);
                                                Log.Info(posroom.z < end.z);
                                            }
                                            if (end2.x < posroom.x && posroom.x < end.x && end2.y < posroom.y && posroom.y < end.y && end2.z < posroom.z && posroom.z < end.z)
                                            {

                                            }
                                            else
                                            {
                                                ev.Player.SendConsoleMessage("Tu doit étre dans ton confinement", "red");
                                                break;
                                            }
                                        }
                                        foreach (var door in Map.Doors)
                                        {
                                            if (door.name.Equals("LCZ_ARMORY"))
                                                if (!door.NetworkTargetState && door.GetExactState() == 0f)
                                                {
                                                    door.ActiveLocks = (ushort)DoorLockReason.SpecialDoorFeature;
                                                    success = true;
                                                }
                                        }
                                        if (success)
                                        {
                                            ev.Player.SetRole(RoleType.Spectator);
                                            RespawnEffectsController.PlayCassieAnnouncement("SCP 1 7 3 as been contained in the Armory of Light Containment Zone", true, true);
                                            ev.Player.SendConsoleMessage("Armory LCZ room 049", "default");
                                        }
                                        else
                                        {
                                            ev.Player.SendConsoleMessage("La gate n'est pas fermer", "default");
                                        }
                                        break;
                                    }
                                case "HCZ_Nuke":
                                    {
                                        bool success = false;
                                        {
                                            Vector3 end;
                                            Vector3 end2;
                                            var posroom = ev.Player.CurrentRoom.Transform.position - ev.Player.Position;
                                            var x1 = 7.5f;
                                            var x2 = 0f;
                                            var z1 = -15.4f;
                                            var z2 = -20.4f;
                                            var y1 = -400;
                                            var y2 = -420f;
                                            if (ev.Player.CurrentRoom.Transform.rotation.eulerAngles.y == 0f)
                                            {
                                                end = new Vector3(x1, y1, z1);
                                                end2 = new Vector3(x2, y2, z2);
                                            }
                                            else if (ev.Player.CurrentRoom.Transform.rotation.eulerAngles.y == 90f)
                                            {
                                                end = new Vector3(z1, y1, -x2);
                                                end2 = new Vector3(z2, y2, -x1);
                                            }
                                            else if (ev.Player.CurrentRoom.Transform.rotation.eulerAngles.y == 180f)
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
                                                Log.Info(end2.x < posroom.x);
                                                Log.Info(posroom.x < end.x);
                                                Log.Info(end2.y < posroom.y);
                                                Log.Info(posroom.y < end.y);
                                                Log.Info(end2.z < posroom.z);
                                                Log.Info(posroom.z < end.z);
                                            }

                                            if (end2.x < posroom.x && posroom.x < end.x && end2.y < posroom.y && posroom.y < end.y && end2.z < posroom.z && posroom.z < end.z)
                                            {

                                            }
                                            else
                                            {
                                                ev.Player.SendConsoleMessage("Tu doit étre confiné", "red");
                                                break;
                                            }
                                        }
                                        foreach (var door in Map.Doors)
                                        {
                                            if (door.name.Equals("NUKE_ARMORY"))
                                                if (!door.NetworkTargetState && door.GetExactState() == 0f)
                                                {
                                                    door.ActiveLocks = (ushort)DoorLockReason.SpecialDoorFeature;
                                                    success = true;
                                                }
                                        }
                                        if (success)
                                        {
                                            if (ev.Player.Position.y < -600) break;
                                            ev.Player.SetRole(RoleType.Spectator);
                                            RespawnEffectsController.PlayCassieAnnouncement("SCP 1 7 3 as been contained in the Armory of NATO_A Warhead", true, true);
                                            ev.Player.SendConsoleMessage("173 room 049", "default");
                                        }
                                        else
                                        {
                                            ev.Player.SendConsoleMessage("La gate n'est pas fermer", "default");
                                        }
                                        break;
                                    }
                                case "HCZ_Hid":
                                    {
                                        bool success = false;
                                        {
                                            Vector3 end;
                                            Vector3 end2;
                                            var posroom = ev.Player.CurrentRoom.Transform.position - ev.Player.Position;
                                            var x1 = 3.7f;
                                            var x2 = -4.0f;
                                            var z1 = 9.8f;
                                            var z2 = 7.4f;
                                            var y1 = 0f;
                                            var y2 = -5f;
                                            if (ev.Player.CurrentRoom.Transform.rotation.eulerAngles.y == 0f)
                                            {
                                                end = new Vector3(x1, y1, z1);
                                                end2 = new Vector3(x2, y2, z2);
                                            }
                                            else if (ev.Player.CurrentRoom.Transform.rotation.eulerAngles.y == 90f)
                                            {
                                                end = new Vector3(z1, y1, -x2);
                                                end2 = new Vector3(z2, y2, -x1);
                                            }
                                            else if (ev.Player.CurrentRoom.Transform.rotation.eulerAngles.y == 180f)
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
                                                Log.Info(end2.x < posroom.x);
                                                Log.Info(posroom.x < end.x);
                                                Log.Info(end2.y < posroom.y);
                                                Log.Info(posroom.y < end.y);
                                                Log.Info(end2.z < posroom.z);
                                                Log.Info(posroom.z < end.z);
                                            }
                                            if (end2.x < posroom.x && posroom.x < end.x && end2.y < posroom.y && posroom.y < end.y && end2.z < posroom.z && posroom.z < end.z)
                                            {

                                            }
                                            else
                                            {
                                                ev.Player.SendConsoleMessage("Tu doit étre dans ton confinement", "red");
                                                break;
                                            }
                                        }
                                        foreach (var door in Map.Doors)
                                        {
                                            if (door.name.Equals("HID") && door.RequiredPermissions.RequiredPermissions.ToTruthyPermissions() == Keycard.Permissions.ArmoryLevelThree)
                                                if (!door.NetworkTargetState && door.GetExactState() == 0f)
                                                {
                                                    door.ActiveLocks = (ushort)DoorLockReason.SpecialDoorFeature;
                                                    success = true;
                                                }
                                        }
                                        if (success)
                                        {
                                            ev.Player.SetRole(RoleType.Spectator);
                                            RespawnEffectsController.PlayCassieAnnouncement("SCP 1 7 3 as been contained in the Storage of Micro H I D", true, true);
                                            ev.Player.SendConsoleMessage("HID room 049", "default");
                                        }
                                        else
                                        {
                                            ev.Player.SendConsoleMessage("La gate n'est pas fermer", "default");
                                        }
                                        break;
                                    }
                                case "HCZ_049":
                                    {
                                        bool success = false;
                                        {
                                            Vector3 end;
                                            Vector3 end2;
                                            var posroom = ev.Player.CurrentRoom.Transform.position - ev.Player.Position;
                                            var x1 = -3f;
                                            var x2 = -8.6f;
                                            var z1 = -4.6f;
                                            var z2 = -10.1f;
                                            var y1 = -260f;
                                            var y2 = -270f;
                                            if (ev.Player.CurrentRoom.Transform.rotation.eulerAngles.y == 0f)
                                            {
                                                end = new Vector3(x1, y1, z1);
                                                end2 = new Vector3(x2, y2, z2);
                                            }
                                            else if (ev.Player.CurrentRoom.Transform.rotation.eulerAngles.y == 90f)
                                            {
                                                end = new Vector3(z1, y1, -x2);
                                                end2 = new Vector3(z2, y2, -x1);
                                            }
                                            else if (ev.Player.CurrentRoom.Transform.rotation.eulerAngles.y == 180f)
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
                                                Log.Info(end2.x < posroom.x);
                                                Log.Info(posroom.x < end.x);
                                                Log.Info(end2.y < posroom.y);
                                                Log.Info(posroom.y < end.y);
                                                Log.Info(end2.z < posroom.z);
                                                Log.Info(posroom.z < end.z);
                                            }
                                            if (end2.x < posroom.x && posroom.x < end.x && end2.y < posroom.y && posroom.y < end.y && end2.z < posroom.z && posroom.z < end.z)
                                            {

                                            }
                                            else
                                            {
                                                ev.Player.SendConsoleMessage("Tu doit étre dans ton confinement", "red");
                                                break;
                                            }
                                        }
                                        foreach (var door in Map.Doors)
                                        {
                                            if (door.name.Equals("049_ARMORY"))
                                                if (!door.NetworkTargetState && door.GetExactState() == 0f)
                                                {
                                                    door.ActiveLocks = (ushort)DoorLockReason.SpecialDoorFeature;
                                                    success = true;
                                                }
                                        }
                                        if (success)
                                        {
                                            ev.Player.SetRole(RoleType.Spectator);
                                            RespawnEffectsController.PlayCassieAnnouncement("SCP 1 7 3 as been contained in the Armory of SCP 0 4 9", true, true);
                                            ev.Player.SendConsoleMessage("173 room 049", "default");
                                        }
                                        else
                                        {
                                            ev.Player.SendConsoleMessage("La gate n'est pas fermer", "default");
                                        }
                                        break;
                                    }
                                case "HCZ_106":
                                    {
                                        int TEST = 0;
                                        bool success = false;
                                        {
                                            Vector3 end;
                                            Vector3 end2;
                                            var posroom = ev.Player.CurrentRoom.Transform.position - ev.Player.Position;
                                            var x1 = 9.6f;
                                            var x2 = -24.4f;
                                            var z1 = 30.8f;
                                            var z2 = -1.9f;
                                            var y1 = 20f;
                                            var y2 = 13f;
                                            if (ev.Player.CurrentRoom.Transform.rotation.eulerAngles.y == 0f)
                                            {
                                                end = new Vector3(x1, y1, z1);
                                                end2 = new Vector3(x2, y2, z2);
                                            }
                                            else if (ev.Player.CurrentRoom.Transform.rotation.eulerAngles.y == 90f)
                                            {
                                                end = new Vector3(z1, y1, -x2);
                                                end2 = new Vector3(z2, y2, -x1);
                                            }
                                            else if (ev.Player.CurrentRoom.Transform.rotation.eulerAngles.y == 180f)
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
                                                Log.Info(end2.x < posroom.x);
                                                Log.Info(posroom.x < end.x);
                                                Log.Info(end2.y < posroom.y);
                                                Log.Info(posroom.y < end.y);
                                                Log.Info(end2.z < posroom.z);
                                                Log.Info(posroom.z < end.z);
                                            }
                                            if (end2.x < posroom.x && posroom.x < end.x && end2.y < posroom.y && posroom.y < end.y && end2.z < posroom.z && posroom.z < end.z)
                                            {
                                                TEST = 1;
                                            }
                                        }
                                        if (TEST != 1)
                                        {
                                            {
                                                foreach (var ply in Player.List.Where((p) => p.Role == RoleType.Scp106))
                                                {
                                                    ev.Player.SendConsoleMessage("Tu ne peux pas te faire reconfiner ici car SCP-106 n'est pas confiné", "default");
                                                    return;
                                                }
                                                {
                                                    Vector3 end;
                                                    Vector3 end2;
                                                    var posroom = ev.Player.CurrentRoom.Transform.position - ev.Player.Position;
                                                    var x1 = -25.6f;
                                                    var x2 = -33.7f;
                                                    var z1 = 32f;
                                                    var z2 = -4.6f;
                                                    var y1 = 20f;
                                                    var y2 = -10f;
                                                    if (ev.Player.CurrentRoom.Transform.rotation.eulerAngles.y == 0f)
                                                    {
                                                        end = new Vector3(x1, y1, z1);
                                                        end2 = new Vector3(x2, y2, z2);
                                                    }
                                                    else if (ev.Player.CurrentRoom.Transform.rotation.eulerAngles.y == 90f)
                                                    {
                                                        end = new Vector3(z1, y1, -x2);
                                                        end2 = new Vector3(z2, y2, -x1);
                                                    }
                                                    else if (ev.Player.CurrentRoom.Transform.rotation.eulerAngles.y == 180f)
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
                                                        Log.Info(end2.x < posroom.x);
                                                        Log.Info(posroom.x < end.x);
                                                        Log.Info(end2.y < posroom.y);
                                                        Log.Info(posroom.y < end.y);
                                                        Log.Info(end2.z < posroom.z);
                                                        Log.Info(posroom.z < end.z);
                                                    }
                                                    if (end2.x < posroom.x && posroom.x < end.x && end2.y < posroom.y && posroom.y < end.y && end2.z < posroom.z && posroom.z < end.z)
                                                    {
                                                        TEST = 2;
                                                    }
                                                }
                                            }
                                        }
                                        if (TEST == 1)
                                        {
                                            foreach (var door in Map.Doors)
                                            {
                                                if (door.name.Equals("106_BOTTOM"))
                                                {
                                                    if (!door.NetworkTargetState && door.GetExactState() == 0f)
                                                    {
                                                        door.ActiveLocks = (ushort)DoorLockReason.SpecialDoorFeature;
                                                        success = true;
                                                    }
                                                }
                                            }
                                        }
                                        if (TEST == 2)
                                        {
                                            foreach (var door in Map.Doors)
                                            {
                                                if (door.name.Equals("106_PRIMARY") || door.name.Equals("106_SECONDARY"))
                                                {
                                                    if (!door.NetworkTargetState && door.GetExactState() == 0f)
                                                    {
                                                        door.ActiveLocks = (ushort)DoorLockReason.SpecialDoorFeature;
                                                        success = true;
                                                    }
                                                    else
                                                    {
                                                        success = false;
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                        if (success)
                                        {
                                            ev.Player.SetRole(RoleType.Spectator);
                                            RespawnEffectsController.PlayCassieAnnouncement("SCP 1 7 3 as been contained in Containment chamber of SCP 1 0 6", true, true);
                                            ev.Player.SendConsoleMessage("173 room 049", "default");
                                        }
                                        else
                                        {
                                            ev.Player.SendConsoleMessage("La gate n'est pas fermer", "default");
                                        }
                                        break;
                                    }
                                case "HCZ_079":
                                    {
                                        bool success = false;
                                        int TEST = 0;
                                        {
                                            Vector3 end;
                                            Vector3 end2;
                                            var posroom = ev.Player.CurrentRoom.Transform.position - ev.Player.Position;
                                            var x1 = 10.3f;
                                            var x2 = -8.2f;
                                            var z1 = 22.5f;
                                            var z2 = 5.2f;
                                            var y1 = 10f;
                                            var y2 = 0f;
                                            if (ev.Player.CurrentRoom.Transform.rotation.eulerAngles.y == 0f)
                                            {
                                                end = new Vector3(x1, y1, z1);
                                                end2 = new Vector3(x2, y2, z2);
                                            }
                                            else if (ev.Player.CurrentRoom.Transform.rotation.eulerAngles.y == 90f)
                                            {
                                                end = new Vector3(z1, y1, -x2);
                                                end2 = new Vector3(z2, y2, -x1);
                                            }
                                            else if (ev.Player.CurrentRoom.Transform.rotation.eulerAngles.y == 180f)
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
                                                Log.Info(end2.x < posroom.x);
                                                Log.Info(posroom.x < end.x);
                                                Log.Info(end2.y < posroom.y);
                                                Log.Info(posroom.y < end.y);
                                                Log.Info(end2.z < posroom.z);
                                                Log.Info(posroom.z < end.z);
                                            }
                                            if (end2.x < posroom.x && posroom.x < end.x && end2.y < posroom.y && posroom.y < end.y && end2.z < posroom.z && posroom.z < end.z)
                                            {
                                                TEST = 1;
                                            }
                                        }
                                        if (TEST != 1)
                                        {
                                            Vector3 end;
                                            Vector3 end2;
                                            var posroom = ev.Player.CurrentRoom.Transform.position - ev.Player.Position;
                                            var x1 = -12.3f;
                                            var x2 = -20.8f;
                                            var z1 = 18.7f;
                                            var z2 = -2.5f;
                                            var y1 = 7f;
                                            var y2 = 0f;
                                            if (ev.Player.CurrentRoom.Transform.rotation.eulerAngles.y == 0f)
                                            {
                                                end = new Vector3(x1, y1, z1);
                                                end2 = new Vector3(x2, y2, z2);
                                            }
                                            else if (ev.Player.CurrentRoom.Transform.rotation.eulerAngles.y == 90f)
                                            {
                                                end = new Vector3(z1, y1, -x2);
                                                end2 = new Vector3(z2, y2, -x1);
                                            }
                                            else if (ev.Player.CurrentRoom.Transform.rotation.eulerAngles.y == 180f)
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
                                                Log.Info(end2.x < posroom.x);
                                                Log.Info(posroom.x < end.x);
                                                Log.Info(end2.y < posroom.y);
                                                Log.Info(posroom.y < end.y);
                                                Log.Info(end2.z < posroom.z);
                                                Log.Info(posroom.z < end.z);
                                            }
                                            if (end2.x < posroom.x && posroom.x < end.x && end2.y < posroom.y && posroom.y < end.y && end2.z < posroom.z && posroom.z < end.z)
                                            {
                                                TEST = 2;
                                            }
                                        }
                                        if (TEST == 1)
                                        {
                                            foreach (var door in Map.Doors)
                                            {
                                                if (door.name.Equals("079_SECOND"))
                                                    if (!door.NetworkTargetState && door.GetExactState() == 0f)
                                                    {
                                                        door.ActiveLocks = (ushort)DoorLockReason.SpecialDoorFeature;
                                                        success = true;
                                                    }
                                            }
                                        }
                                        if (TEST == 2)
                                        {
                                            foreach (var door in Map.Doors)
                                            {
                                                if (door.name.Equals("079_FIRST") && TEST == 2)
                                                    if (!door.NetworkTargetState && door.GetExactState() == 0f)
                                                    {
                                                        door.ActiveLocks = (ushort)DoorLockReason.SpecialDoorFeature;
                                                        success = true;
                                                    }
                                            }
                                        }
                                        if (success)
                                        {
                                            ev.Player.SetRole(RoleType.Spectator);
                                            RespawnEffectsController.PlayCassieAnnouncement("SCP 1 7 3 as been contained in the Containment chamber of SCP 0 7 9", true, true);
                                            ev.Player.SendConsoleMessage("173 room 049", "default");
                                        }
                                        if (TEST == 0)
                                        {
                                            ev.Player.SendConsoleMessage("Tu doit étre confiné", "red");
                                        }
                                        else
                                        {
                                            ev.Player.SendConsoleMessage("La gate n'est pas fermer", "default");
                                        }
                                        break;
                                    }
                                default:
                                    {
                                        break;
                                    }
                            }
                            break;
                        }
                    case RoleType.Scp096:
                        {
                            if (SanyaRemastered.Instance.Config.IsDebugged) Log.Info($"096 state : {(ev.Player.ReferenceHub.scpsController.CurrentScp as PlayableScps.Scp096).PlayerState}");
                            if (Scp096PlayerState.Docile != (ev.Player.ReferenceHub.scpsController.CurrentScp as PlayableScps.Scp096).PlayerState
                                && Scp096PlayerState.TryNotToCry != (ev.Player.ReferenceHub.scpsController.CurrentScp as PlayableScps.Scp096).PlayerState)
                            {
                                ev.Player.SendConsoleMessage("NON MEC VAS TUER LES GENS IL Doivent pas te reconf si t'es trigger", "red");
                                break;
                            }
                            if (ev.Player.CurrentRoom.Name.Equals("HCZ_457"))
                            {
                                bool success = false;
                                {
                                    Vector3 end;
                                    Vector3 end2;
                                    var posroom = ev.Player.CurrentRoom.Transform.position - ev.Player.Position;
                                    var x1 = 4.4f;
                                    var x2 = 0.5f;
                                    var z1 = 1.9f;
                                    var z2 = -1.9f;
                                    var y1 = 0f;
                                    var y2 = -5f;
                                    if (ev.Player.CurrentRoom.Transform.rotation.eulerAngles.y == 0f)
                                    {
                                        end = new Vector3(x1, y1, z1);
                                        end2 = new Vector3(x2, y2, z2);
                                    }
                                    else if (ev.Player.CurrentRoom.Transform.rotation.eulerAngles.y == 90f)
                                    {
                                        end = new Vector3(z1, y1, -x2);
                                        end2 = new Vector3(z2, y2, -x1);
                                    }
                                    else if (ev.Player.CurrentRoom.Transform.rotation.eulerAngles.y == 180f)
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
                                        Log.Info(end2.x < posroom.x);
                                        Log.Info(posroom.x < end.x);
                                        Log.Info(end2.y < posroom.y);
                                        Log.Info(posroom.y < end.y);
                                        Log.Info(end2.z < posroom.z);
                                        Log.Info(posroom.z < end.z);
                                    }
                                    if (end2.x < posroom.x && posroom.x < end.x && end2.y < posroom.y && posroom.y < end.y && end2.z < posroom.z && posroom.z < end.z)
                                    {

                                    }
                                    else
                                    {
                                        ev.Player.SendConsoleMessage("Tu doit étre dans ton confinement (au centre)", "red");
                                        break;
                                    }
                                }
                                foreach (var door in Map.Doors)
                                {
                                    if (door.name.Equals("096"))
                                        if (!door.NetworkTargetState && door.GetExactState() == 0f)
                                        {
                                            door.ActiveLocks = (ushort)DoorLockReason.SpecialDoorFeature;
                                            success = true;
                                        }
                                }
                                if (success)
                                {
                                    ev.Player.SetRole(RoleType.Spectator);
                                    RespawnEffectsController.PlayCassieAnnouncement("SCP 0 9 6 as been contained in there containment chamber", true, true);
                                    ev.Player.SendConsoleMessage("096 room 096", "default");
                                }
                                else
                                {
                                    ev.Player.SendConsoleMessage("La gate n'est pas fermer", "default");
                                }
                            }
                            else if (ev.Player.CurrentRoom.Name.Equals("HCZ_Room3ar"))
                            {
                                bool success = false;
                                {
                                    Vector3 end;
                                    Vector3 end2;
                                    var posroom = ev.Player.CurrentRoom.Transform.position - ev.Player.Position;
                                    var x1 = 0.1f;
                                    var x2 = -5.6f;
                                    var z1 = 2.9f;
                                    var z2 = -2.8f;
                                    var y1 = 0f;
                                    var y2 = -5f;
                                    if (ev.Player.CurrentRoom.Transform.rotation.eulerAngles.y == 0f)
                                    {
                                        end = new Vector3(x1, y1, z1);
                                        end2 = new Vector3(x2, y2, z2);
                                    }
                                    else if (ev.Player.CurrentRoom.Transform.rotation.eulerAngles.y == 90f)
                                    {
                                        end = new Vector3(z1, y1, -x2);
                                        end2 = new Vector3(z2, y2, -x1);
                                    }
                                    else if (ev.Player.CurrentRoom.Transform.rotation.eulerAngles.y == 180f)
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
                                        Log.Info(end2.x < posroom.x);
                                        Log.Info(posroom.x < end.x);
                                        Log.Info(end2.y < posroom.y);
                                        Log.Info(posroom.y < end.y);
                                        Log.Info(end2.z < posroom.z);
                                        Log.Info(posroom.z < end.z);
                                    }

                                    if (end2.x < posroom.x && posroom.x < end.x && end2.y < posroom.y && posroom.y < end.y && end2.z < posroom.z && posroom.z < end.z)
                                    {

                                    }
                                    else
                                    {
                                        ev.Player.SendConsoleMessage("Tu doit étre confiné", "red");
                                        break;
                                    }
                                }
                                foreach (var door in Map.Doors)
                                {
                                    if (door.name.Equals("HCZ_ARMORY"))
                                        if (!door.NetworkTargetState && door.GetExactState() == 0f)
                                        {
                                            door.ActiveLocks = (ushort)DoorLockReason.SpecialDoorFeature;
                                            success = true;
                                        }
                                }
                                if (success)
                                {
                                    ev.Player.SetRole(RoleType.Spectator);
                                    RespawnEffectsController.PlayCassieAnnouncement("SCP 0 9 6 as been contained in the Armory of Heavy Containment Zone", true, true);
                                    ev.Player.SendConsoleMessage("096 room nuke", "default");
                                }
                                else
                                {
                                    ev.Player.SendConsoleMessage("La gate n'est pas fermer", "default");
                                }
                            }
                            else
                            {
                                ev.Player.SendConsoleMessage("Tu n'est pas confiné", "default");
                            }
                            break;
                        }
                    case RoleType.Scp049:
                        {
                            if (ev.Player.CurrentRoom.Name.Equals("HCZ_049"))
                            {
                                bool success = false;
                                {
                                    Vector3 end;
                                    Vector3 end2;
                                    var posroom = ev.Player.CurrentRoom.Transform.position - ev.Player.Position;
                                    var x1 = 9.2f;
                                    var x2 = -9.3f;
                                    var z1 = -11.6f;
                                    var z2 = -16.5f;
                                    var y1 = -250f;
                                    var y2 = -275f;
                                    if (ev.Player.CurrentRoom.Transform.rotation.eulerAngles.y == 0f)
                                    {
                                        end = new Vector3(x1, y1, z1);
                                        end2 = new Vector3(x2, y2, z2);
                                    }
                                    else if (ev.Player.CurrentRoom.Transform.rotation.eulerAngles.y == 90f)
                                    {
                                        end = new Vector3(z1, y1, -x2);
                                        end2 = new Vector3(z2, y2, -x1);
                                    }
                                    else if (ev.Player.CurrentRoom.Transform.rotation.eulerAngles.y == 180f)
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
                                        Log.Info(end2.x < posroom.x);
                                        Log.Info(posroom.x < end.x);
                                        Log.Info(end2.y < posroom.y);
                                        Log.Info(posroom.y < end.y);
                                        Log.Info(end2.z < posroom.z);
                                        Log.Info(posroom.z < end.z);
                                    }
                                    if (end2.x < posroom.x && posroom.x < end.x && end2.y < posroom.y && posroom.y < end.y && end2.z < posroom.z && posroom.z < end.z)
                                    {

                                    }
                                    else
                                    {
                                        ev.Player.SendConsoleMessage("Tu doit étre dans ton confinement", "red");
                                        break;
                                    }
                                }
                                foreach (var door in Map.Doors)
                                {
                                    float dis = Vector3.Distance(door.transform.position, ev.Player.Position);
                                    if (door.name == "ContDoor" && dis < 25)
                                    {
                                        if (!door.NetworkTargetState && door.GetExactState() == 0f)
                                        {
                                            door.ActiveLocks = (ushort)DoorLockReason.SpecialDoorFeature;
                                            success = true;
                                        }
                                        else
                                        {
                                            ev.Player.SendConsoleMessage("Vous devez avoir la porte de votre confinement fermer", "red");
                                            return;
                                        }
                                    }
                                }
                                if (success)
                                {
                                    ev.Player.SetRole(RoleType.Spectator);
                                    RespawnEffectsController.PlayCassieAnnouncement("SCP 0 4 9 as been contained in there containment chamber", true, true);
                                    ev.Player.SendConsoleMessage("Le confinement a été effectué", "default");
                                }
                            }
                            break;
                        }
                    case RoleType.Scp93953:
                    case RoleType.Scp93989:
                        {
                            if (ev.Player.CurrentRoom.Name.Equals("HCZ_106"))
                            {
                                bool success = false;
                                {
                                    Vector3 end;
                                    Vector3 end2;
                                    var posroom = ev.Player.CurrentRoom.Transform.position - ev.Player.Position;
                                    var x1 = 9.6f;
                                    var x2 = -24.4f;
                                    var z1 = 30.8f;
                                    var z2 = -1.9f;
                                    var y1 = 20f;
                                    var y2 = 13f;
                                    if (ev.Player.CurrentRoom.Transform.rotation.eulerAngles.y == 0f)
                                    {
                                        end = new Vector3(x1, y1, z1);
                                        end2 = new Vector3(x2, y2, z2);
                                    }
                                    else if (ev.Player.CurrentRoom.Transform.rotation.eulerAngles.y == 90f)
                                    {
                                        end = new Vector3(z1, y1, -x2);
                                        end2 = new Vector3(z2, y2, -x1);
                                    }
                                    else if (ev.Player.CurrentRoom.Transform.rotation.eulerAngles.y == 180f)
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
                                        Log.Info(end2.x < posroom.x);
                                        Log.Info(posroom.x < end.x);
                                        Log.Info(end2.y < posroom.y);
                                        Log.Info(posroom.y < end.y);
                                        Log.Info(end2.z < posroom.z);
                                        Log.Info(posroom.z < end.z);
                                    }
                                    if (end2.x < posroom.x && posroom.x < end.x && end2.y < posroom.y && posroom.y < end.y && end2.z < posroom.z && posroom.z < end.z)
                                    {

                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                foreach (var door in Map.Doors)
                                {
                                    float dis = Vector3.Distance(door.transform.position, ev.Player.Position);
                                    if (door.name.Equals("106_BOTTOM"))
                                    {
                                        if (!door.NetworkTargetState && door.GetExactState() == 0f)
                                        {
                                            door.ActiveLocks = (ushort)DoorLockReason.SpecialDoorFeature;
                                            success = true;
                                        }
                                        else
                                        {
                                            ev.Player.SendConsoleMessage("La porte est ouverte", "red");
                                            return;
                                        }
                                    }
                                }
                                if (success)
                                {
                                    ev.Player.SetRole(RoleType.Spectator);
                                    RespawnEffectsController.PlayCassieAnnouncement("SCP 9 3 9 as been contained in the Containment Chamber of SCP 1 0 6", true, true);
                                    ev.Player.SendConsoleMessage("939 confiné", "default");
                                }
                            }
                            break;
                        }
                    case RoleType.Scp0492:
                        {
                            if (ev.Player.CurrentRoom.Name.Equals("HCZ_106"))
                            {
                                bool success = false;
                                {
                                    Vector3 end;
                                    Vector3 end2;
                                    var posroom = ev.Player.CurrentRoom.Transform.position - ev.Player.Position;
                                    var x1 = 9.6f;
                                    var x2 = -24.4f;
                                    var z1 = 30.8f;
                                    var z2 = -1.9f;
                                    var y1 = 20f;
                                    var y2 = 13f;
                                    if (ev.Player.CurrentRoom.Transform.rotation.eulerAngles.y == 0f)
                                    {
                                        end = new Vector3(x1, y1, z1);
                                        end2 = new Vector3(x2, y2, z2);
                                    }
                                    else if (ev.Player.CurrentRoom.Transform.rotation.eulerAngles.y == 90f)
                                    {
                                        end = new Vector3(z1, y1, -x2);
                                        end2 = new Vector3(z2, y2, -x1);
                                    }
                                    else if (ev.Player.CurrentRoom.Transform.rotation.eulerAngles.y == 180f)
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
                                        Log.Info(end2.x < posroom.x);
                                        Log.Info(posroom.x < end.x);
                                        Log.Info(end2.y < posroom.y);
                                        Log.Info(posroom.y < end.y);
                                        Log.Info(end2.z < posroom.z);
                                        Log.Info(posroom.z < end.z);
                                    }
                                    if (end2.x < posroom.x && posroom.x < end.x && end2.y < posroom.y && posroom.y < end.y && end2.z < posroom.z && posroom.z < end.z)
                                    {

                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                foreach (var door in Map.Doors)
                                {
                                    float dis = Vector3.Distance(door.transform.position, ev.Player.Position);
                                    if (door.name.Equals("106_BOTTOM"))
                                    {
                                        if (!door.NetworkTargetState && door.GetExactState() == 0f)
                                        {
                                            door.ActiveLocks = (ushort)DoorLockReason.SpecialDoorFeature;
                                            success = true;
                                        }
                                        else
                                        {
                                            ev.Player.SendConsoleMessage("La porte est ouverte", "red");
                                            return;
                                        }
                                    }
                                }
                                if (success)
                                {
                                    ev.Player.SetRole(RoleType.Spectator);
                                    ev.Player.SendConsoleMessage("049-2 confiné", "default");
                                }
                            }
                            break;
                        }
                }
            }
        }
    }
}