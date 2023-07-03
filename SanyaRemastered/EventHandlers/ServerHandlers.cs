using AdminToys;
using AudioPlayer;
using CustomPlayerEffects;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Pickups;
using Exiled.API.Features.Pickups.Projectiles;
using Exiled.Events.EventArgs;
using Exiled.Events.EventArgs.Map;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Server;
using Exiled.Events.EventArgs.Warhead;
using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items.Pickups;
using InventorySystem.Items.ThrowableProjectiles;
using InventorySystem.Items.Usables.Scp330;
using LightContainmentZoneDecontamination;
using MapGeneration;
using MapGeneration.Distributors;
using MEC;
using Mirror;
using PlayerRoles;
using SanyaRemastered.Data;
using SanyaRemastered.Functions;
using SCPSLAudioApi.AudioCore;
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
        internal IEnumerator<float> Every30minute()
        {
            yield return Timing.WaitForSeconds(30f);
            while (true)
            {
                try
                {
                    MemoryMetrics metrics = Ram.MemoryService.CurrentTotalMetrics;
                    if (SanyaRemastered.Instance.Config.RamInfo)
                        try
                        {
                            Methods.DiscordLogPlayer($"Total Ram Usage: {metrics.Used / 1024:0.##}/{metrics.Total / 1024:0.##} Go [{((metrics.Used / metrics.Total) * 100):0.##}%]\n");
                        }
                        catch (Exception ex){

                            Log.Error(ex);
                        }
                    double slRamUsage = Ram.MemoryService.CurrentProcessRamUsage;
                    if (SanyaRemastered.Instance.Config.RamInfo)
                        try
                        {
                            Methods.DiscordLogStaff($"SL Ram Usage: {slRamUsage / 1024:0.##}/{metrics.Total / 1024:0.##} Go [{((slRamUsage / metrics.Total) * 100):0.##}%]\n");
                        }
                        catch (Exception ex)
                        {

                            Log.Error(ex);
                        }

                    if (Player.List.IsEmpty())
                    {
                        if (plugin.Config.RamRestartNoPlayer < slRamUsage / 1024 && plugin.Config.RamRestartNoPlayer > 0)
                        {
                            try
                            {
                                Methods.DiscordLogStaff($"**The Ram exceed the limit NP**:\n");
                            }
                            catch (Exception ex)
                            {

                                Log.Error(ex);
                            }

                            RoundCoroutines.Add(Timing.RunCoroutine(Coroutines.RestartServer(), Segment.RealtimeUpdate));
                        }
                    }
                    else
                    {
                        if (plugin.Config.RamRestartWithPlayer < slRamUsage / 1024 && plugin.Config.RamRestartWithPlayer > 0)
                        {
                            try
                            {
                                Methods.DiscordLogStaff($"**The Ram exceed the limit WP**:\n");
                            }
                            catch (Exception ex)
                            {

                                Log.Error(ex);
                            }

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

        public List<CoroutineHandle> RoundCoroutines { get => roundCoroutines; set => roundCoroutines = value; }

        public void OnWaintingForPlayers()
        {
            if (plugin.Config.RamRestartNoPlayer > 0 || plugin.Config.RamRestartWithPlayer > 0 || plugin.Config.RamInfo)
                RoundCoroutines.Add(Timing.RunCoroutine(Every30minute(), Segment.RealtimeUpdate));
            loaded = true;

            Coroutines.IsAirBombGoing = false;
            Coroutines.IsActuallyBombGoing = false;
            Coroutines.AirBombWait = 0;
            Methods.SurfaceBombArea.Clear();
            SanyaRemastered.Instance.PlayerHandlers.Scp0492UserID.Clear();
            Log.Info($"[OnWaintingForPlayers] Waiting for Players...");
        }

        public void OnRoundEnd(RoundEndedEventArgs ev)
        {
            Log.Info($"[OnRoundEnd] Round Ended.{ev.TimeToRestart}");

            if (SanyaRemastered.Instance.Config.GodmodeAfterEndround)
            {
                foreach (Player player in Player.List)
                    player.ReferenceHub.characterClassManager.GodMode = true;
            }
            Coroutines.IsAirBombGoing = false;
        }

        public void OnRoundRestart()
        {
            Log.Info($"[OnRoundRestart] Restarting...");
            foreach (Player player in Player.List)
                if (player.GameObject.TryGetComponent(out SanyaRemasteredComponent comp))
                    UnityEngine.Object.Destroy(comp);

            foreach (CoroutineHandle cor in RoundCoroutines)
                Timing.KillCoroutines(cor);
            RoundCoroutines.Clear();

            RoundSummary.singleton._roundEnded = true;
        }
        public void OnTeamRespawn(RespawningTeamEventArgs ev)
        {
            Log.Debug($"[OnTeamRespawn] Queues:{ev.Players.Count()} NextKnowTeam:{ev.NextKnownTeam} MaxAmount:{ev.MaximumRespawnAmount}");

            if (SanyaRemastered.Instance.Config.StopRespawnAfterDetonated && Warhead.IsDetonated
                || Coroutines.IsAirBombGoing && Coroutines.AirBombWait < 60
                || StopRespawn)
                ev.IsAllowed = false;
        }

        public void OnWarheadCancel(StoppingEventArgs ev)
        {
            Log.Debug($"[OnWarheadCancel] {ev.Player?.Nickname}");

            if (SanyaRemastered.Instance.Config.CloseDoorsOnNukecancel)
            {
                foreach (Door door in Door.List)
                    if (door.DoorLockType is DoorLockType.Warhead)
                        door.IsOpen = false;
            }
        }

        public void OnDetonated()
        {
            Log.Debug($"[OnDetonated] Detonated:{RoundSummary.roundTime / 60:00}:{RoundSummary.roundTime % 60:00}");

            if (SanyaRemastered.Instance.Config.OutsidezoneTerminationTimeAfterNuke >= 0)
            {
                RoundCoroutines.Add(Timing.RunCoroutine(Coroutines.AirSupportBomb(false, SanyaRemastered.Instance.Config.OutsidezoneTerminationTimeAfterNuke), Segment.FixedUpdate));
            }
        }
        public void OnGeneratorFinish(GeneratorActivatedEventArgs ev)
        {
            Log.Debug($"[OnGeneratorFinish] {ev.Generator.Room.Type}");

            if (SanyaRemastered.Instance.Config.GeneratorFinishLock)
                ev.Generator.Base.ServerSetFlag(Scp079Generator.GeneratorFlags.Open, false);
        }
        public void OnPlacingBulletHole(PlacingBulletHole ev)
        {
            // Smoke Grenade Pickup ButtonDoor
            if (ev.Position == Vector3.zero || !Physics.Linecast(ev.Player.Position, ev.Position, out RaycastHit raycastHit, 0b11000000000001000001000))
                return;
            if (SanyaRemastered.Instance.Config.OpenDoorOnShoot)
            {
                BasicDoor basicDoor = raycastHit.transform.GetComponentInParent<BasicDoor>();
                if (basicDoor is not null)
                {
                    if ((basicDoor is IDamageableDoor damageableDoor) && damageableDoor.IsDestroyed
                        || basicDoor.GetExactState() is not (1f or 0f)
                        || basicDoor.NetworkActiveLocks is not 0)
                        return;

                    if (basicDoor.RequiredPermissions.RequiredPermissions is Interactables.Interobjects.DoorUtils.KeycardPermissions.None && basicDoor is not PryableDoor)
                        basicDoor.ServerInteract(ev.Player.ReferenceHub, 0);
                }
            }
            if (SanyaRemastered.Instance.Config.ItemShootMove && raycastHit.transform.TryGetComponent(out Rigidbody rigidbody))
            {
                if (raycastHit.transform.TryGetComponent(out ItemPickupBase pickupBase))
                {
                    pickupBase.PreviousOwner = ev.Player.Footprint;
                }
                rigidbody.AddExplosionForce(Mathf.Min(1 / rigidbody.mass * 4 + 1, 5), ev.Player.Position, 500f, 3f, ForceMode.Impulse);
            }
        }
    }
}
