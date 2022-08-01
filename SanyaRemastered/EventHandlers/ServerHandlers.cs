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
        internal List<CoroutineHandle> roundCoroutines = new();
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
                            if (player.IsHuman && player.GetHealthAmountPercent() > SanyaRemastered.Instance.Config.PainEffectStart)
                                player.EnableEffect<Disabled>(1.2f);
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
        public bool StopRespawn = false;
        public List<Vector3> DecalList = new();

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
            if (SanyaRemastered.Instance.Config.TeslaRange != 5.5f)
            {
                foreach (TeslaGate tesla in UnityEngine.Object.FindObjectsOfType<TeslaGate>())
                {
                    tesla.sizeOfTrigger = SanyaRemastered.Instance.Config.TeslaRange;
                }
            }
            if (plugin.Config.DisablePlayerLists)
            {
                foreach (NetworkIdentity identity in UnityEngine.Object.FindObjectsOfType<NetworkIdentity>())
                {
                    if (identity.name is "PlayerList")
                    {
                        playerlistnetid = identity.netId;
                    }
                }
            }
            if (plugin.Config.Scp096Real)
            {
                foreach (CheckpointDoor cp in UnityEngine.Object.FindObjectsOfType<CheckpointDoor>())
                {
                    if (!cp.TryGetComponent(out DoorNametagExtension name))
                        continue;
                    foreach (DoorVariant door in cp._subDoors)
                    {
                        if (door is not BreakableDoor dr)
                            continue;
                        dr._remainingHealth = 750f;
                        dr._ignoredDamageSources &= ~DoorDamageType.Scp096;
                    }
                }
            }
            if (plugin.Config.Scp914Effect)
            {
                if (NetworkClient.prefabs.TryGetValue(Guid.Parse("43658aa2-f339-6044-eb2b-937db0c2c4bd"), out GameObject player))
                {
                    if (player.name.Equals("Player"))
                    {
                        Transform playerEffects = player.transform.Find("PlayerEffects");

                        GameObject effectObj = new("Scp914", typeof(Scp914));
                        effectObj.transform.parent = playerEffects;
                    }
                }
                PlayerEffectsController effectcontroller = UnityEngine.Object.FindObjectOfType<PlayerEffectsController>();

                effectcontroller.AllEffects.Clear();
                effectcontroller.syncEffectsIntensity.Clear();

                effectcontroller.Awake();
            }
            if (plugin.Config.GateClosingAuto)
            {
                DoorNametagExtension.NamedDoors.TryGetValue("GATE_A", out DoorNametagExtension GateA);
                GateA.TargetDoor.gameObject.GetComponent<PryableDoor>().gameObject.AddComponent<GateTimerClose>();
                DoorNametagExtension.NamedDoors.TryGetValue("GATE_B", out DoorNametagExtension GateB);
                GateB.TargetDoor.gameObject.GetComponent<PryableDoor>().gameObject.AddComponent<GateTimerClose>();
            }
            if (plugin.Config.AddDoorsOnSurface)
            {
                Vector3 DoorScale = new(1f, 1f, 1.8f);
                DoorSpawnpoint LCZprefab = UnityEngine.Object.FindObjectsOfType<DoorSpawnpoint>().First(x => x.TargetPrefab.name.Contains("LCZ"));
                DoorSpawnpoint EZprefab = UnityEngine.Object.FindObjectsOfType<DoorSpawnpoint>().First(x => x.TargetPrefab.name.Contains("EZ"));
                DoorSpawnpoint HCZprefab = UnityEngine.Object.FindObjectsOfType<DoorSpawnpoint>().First(x => x.TargetPrefab.name.Contains("HCZ"));

                // Couloir spawn Chaos
                DoorVariant door1 = UnityEngine.Object.Instantiate(LCZprefab.TargetPrefab, new Vector3(14.425f, 995.2f, -43.525f), Quaternion.Euler(Vector3.zero));
                DoorVariant door2 = UnityEngine.Object.Instantiate(LCZprefab.TargetPrefab, new Vector3(14.425f, 995.2f, -23.2f), Quaternion.Euler(Vector3.zero));
                // Exit
                DoorVariant door3 = UnityEngine.Object.Instantiate(EZprefab.TargetPrefab, new Vector3(176.2f, 983.24f, 35.23f), Quaternion.Euler(Vector3.up * 180f));
                DoorVariant door4 = UnityEngine.Object.Instantiate(EZprefab.TargetPrefab, new Vector3(174.4f, 983.24f, 29.1f), Quaternion.Euler(Vector3.up * 90f));
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
                GameObject primitivePrefab = CustomNetworkManager.singleton.spawnPrefabs.First(x => x.name.Contains("Primitive"));
                GameObject lightPrefab = CustomNetworkManager.singleton.spawnPrefabs.First(x => x.name.Contains("LightSource"));
                GameObject stationPrefab = CustomNetworkManager.singleton.spawnPrefabs.First(x => x.name.Contains("Station"));

                //MTF
                GameObject station1 = UnityEngine.Object.Instantiate(stationPrefab, new(147.9f, 992.77f, -46.2f), Quaternion.Euler(Vector3.up * 90f));
                //CI
                GameObject station2 = UnityEngine.Object.Instantiate(stationPrefab, new(10.37f, 987.5f, -47.5f), Quaternion.Euler(Vector3.up * 180f));

                //Light Nuke Red
                LightSourceToy light_nuke = UnityEngine.Object.Instantiate(lightPrefab.GetComponent<LightSourceToy>());
                light_nuke.transform.position = new(40.75f, 991f, -35.75f);
                light_nuke.NetworkLightRange = 4.5f;
                light_nuke.NetworkLightIntensity = 2f;
                light_nuke.NetworkLightColor = Color.red;

                //Lumiére dans le couloir de la GateA 
                LightSourceToy light_GateA1 = UnityEngine.Object.Instantiate(lightPrefab.GetComponent<LightSourceToy>());
                light_GateA1.transform.position = new(-1, 1005.2f, -37);
                light_GateA1.NetworkLightRange = 6f;
                light_GateA1.NetworkLightIntensity = 1f;
                light_GateA1.NetworkLightColor = Color.white;

                LightSourceToy light_GateA2 = UnityEngine.Object.Instantiate(lightPrefab.GetComponent<LightSourceToy>());
                light_GateA2.transform.position = new(-1, 1005.2f, -29.5f);
                light_GateA2.NetworkLightRange = 6f;
                light_GateA2.NetworkLightIntensity = 1f;
                light_GateA2.NetworkLightColor = Color.white;

                //SCP-106 Do not go on Container Of 106
                Room room106 = Room.List.First(x => x.Type == RoomType.Hcz106);

                PrimitiveObjectToy wall_106 = UnityEngine.Object.Instantiate(primitivePrefab.GetComponent<PrimitiveObjectToy>());
                wall_106.transform.SetParentAndOffset(room106.transform, new(7.2f, 2.6f, -14.3f));
                wall_106.transform.localScale = new(20, 5, 14);
                if (room106.transform.forward == Vector3.left || room106.transform.forward == Vector3.right)
                    wall_106.transform.rotation = Quaternion.Euler(Vector3.up * 90f);
                wall_106.UpdatePositionServer();
                wall_106.NetworkPrimitiveType = PrimitiveType.Cube;

                NetworkServer.Spawn(station1);
                NetworkServer.Spawn(station2);

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
                    player.ReferenceHub.characterClassManager.GodMode = true;
            }
            Coroutines.isAirBombGoing = false;
        }

        public void OnRoundRestart()
        {
            Log.Info($"[OnRoundRestart] Restarting...");
            foreach (Player player in Player.List)
                if (player.GameObject.TryGetComponent(out SanyaRemasteredComponent comp))
                    UnityEngine.Object.Destroy(comp);
            foreach (Player player in Player.List)
                if (player.GameObject.TryGetComponent(out ContainScpComponent comp))
                    UnityEngine.Object.Destroy(comp);
            SanyaRemasteredComponent._scplists.Clear();

            foreach (CoroutineHandle cor in RoundCoroutines)
                Timing.KillCoroutines(cor);
            RoundCoroutines.Clear();

            RoundSummary.singleton.RoundEnded = true;
        }
        public void OnTeamRespawn(RespawningTeamEventArgs ev)
        {
            Log.Debug($"[OnTeamRespawn] Queues:{ev.Players.Count()} NextKnowTeam:{ev.NextKnownTeam} MaxAmount:{ev.MaximumRespawnAmount}", SanyaRemastered.Instance.Config.IsDebugged);

            if (SanyaRemastered.Instance.Config.StopRespawnAfterDetonated && AlphaWarheadController.Host.detonated 
                || Coroutines.isAirBombGoing && Coroutines.AirBombWait < 60 
                || StopRespawn)
                ev.IsAllowed = false;
        }

        public void OnWarheadCancel(StoppingEventArgs ev)
        {
            Log.Debug($"[OnWarheadCancel] {ev.Player?.Nickname}");

            if (AlphaWarheadController.Host._isLocked) return;

            if (SanyaRemastered.Instance.Config.CloseDoorsOnNukecancel)
            {
                foreach (Door door in Door.List)
                    if (door.Base.NetworkActiveLocks is (ushort)DoorLockReason.Warhead)
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

        public void OnExplodingGrenade(ExplodingGrenadeEventArgs ev)
        {
            if (SanyaRemastered.Instance.Config.GrenadeEffect)
            {
                foreach (Player ply in Player.List)
                {
                    float dis = Vector3.Distance(ev.Grenade.Position, ply.Position);
                    if (dis >= 15) continue;
                    ply.ReferenceHub.playerEffectsController.EnableEffect<Deafened>(20f / dis, true);
                }
            }
        }
        public void OnGeneratorFinish(GeneratorActivatedEventArgs ev)
        {
            Log.Debug($"[OnGeneratorFinish] {ev.Generator.Room.Type}", SanyaRemastered.Instance.Config.IsDebugged);

            if (SanyaRemastered.Instance.Config.GeneratorFinishLock)
                ev.Generator.Base.ServerSetFlag(Scp079Generator.GeneratorFlags.Open, false);
        }
        public void OnPlacingBulletHole(PlacingBulletHole ev)
        {
            //1*2^22 + 1*2^21 + 1*2^9 = 6291968 // Smoke Grenade Pickup
            if (ev.Position == Vector3.zero || !Physics.Linecast(ev.Owner.Position, ev.Position, out RaycastHit raycastHit, 6291968))
                return;
            if (raycastHit.transform.TryGetComponent(out ItemPickupBase pickup))
            {
                if (SanyaRemastered.Instance.Config.GrenadeShootFuse)
                {
                    TimeGrenade timeGrenade = raycastHit.transform.GetComponentInParent<TimeGrenade>();
                    if (timeGrenade is not null && timeGrenade.Info.ItemId is not ItemType.SCP018)
                    {
                        if (timeGrenade.Info.ItemId == ItemType.GrenadeHE)
                            Methods.Explode(pickup.Info.Position, ev.Owner.ReferenceHub);
                        else
                            Methods.SpawnGrenade(pickup.Info.Position, timeGrenade.Info.ItemId, 0.1f, ev.Owner);
                        pickup.DestroySelf();

                        return;
                    }
                }
                if (SanyaRemastered.Instance.Config.ItemShootMove)
                {
                    pickup.Rb.AddExplosionForce((2.5f / (pickup.Info.Weight + 1)) + 4, ev.Owner.Position, 500f, 3f, ForceMode.Impulse);

                    pickup.PreviousOwner = ev.Owner.Footprint;
                    return;
                }
            }

            if (SanyaRemastered.Instance.Config.OpenDoorOnShoot)
            {
                BasicDoor basicDoor = raycastHit.transform.GetComponentInParent<BasicDoor>();
                if (basicDoor is not null)
                {
                    if ((basicDoor is IDamageableDoor damageableDoor) && damageableDoor.IsDestroyed 
                        || basicDoor.GetExactState() is not 1f or 0f 
                        || basicDoor.NetworkActiveLocks is not 0) 
                        return;

                    if (basicDoor.RequiredPermissions.RequiredPermissions is Interactables.Interobjects.DoorUtils.KeycardPermissions.None && basicDoor is not PryableDoor)
                        basicDoor.ServerInteract(ev.Owner.ReferenceHub, 0);
                }
            }
        }
        public void OnChangingIntoGrenade(ChangingIntoGrenadeEventArgs ev)
        {
            if (plugin.Config.GrenadeChainSametiming)
                ev.FuseTime = 0.1f;
        }
        public void OnPlayerDamageWindow(DamagingWindowEventArgs ev)
        {
            if (plugin.Config.ContainCommand && ev.Window.Type is GlassType.Scp049)
            {
                ev.Handler.DealtHealthDamage = 0;
            }
        }
    }
}
