using CustomPlayerEffects;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.DamageHandlers;
using Exiled.API.Features.Roles;
using Exiled.Events.EventArgs.Item;
using Exiled.Events.EventArgs.Player;
using InventorySystem.Items.Armor;
using InventorySystem.Items.Radio;
using MapGeneration.Distributors;
using MEC;
using Mirror;
using PlayerRoles;
using RelativePositioning;
using SanyaRemastered.Functions;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace SanyaRemastered.EventHandlers
{
    public class PlayerHandlers
    {
        public PlayerHandlers(SanyaRemastered plugin) => this.plugin = plugin;
        internal readonly SanyaRemastered plugin;
        public Dictionary<string, RelativePosition> Scp0492UserID = new();
        public void OnPlayerVerified(VerifiedEventArgs ev)
        {
            Log.Info($"[OnPlayerJoin] {ev.Player.Nickname} ({ev.Player.ReferenceHub.queryProcessor._ipAddress}:{ev.Player.UserId})");
            if (Player.List.Any(x => x.Nickname == ev.Player.Nickname && x != ev.Player))
            {
                if (!ReservedSlot.Users.Contains(ev.Player.UserId) )
                {
                    ev.Player.Kick("AutoKick par SanyaRemasteredPlugin : Ce pseudo est déjà utilisé sur ce serveur");
                    return;
                }
                Player.List.First(x => x.Nickname == ev.Player.Nickname && ev.Player != x).Ban(System.TimeSpan.FromDays(10), "AutoBan par SanyaRemasteredPlugin: Usurpation du pseudo d'un staff");
            }
            if (plugin.Config.AllStar && !ev.Player.HasCustomName)
            {
                ev.Player.CustomName = ev.Player.Nickname;
            }
            if (plugin.Config.StaffMaxPlayer && ReservedSlot.Users.Contains(ev.Player.UserId))
            {
                Server.MaxPlayerCount++;
            }
            //Component
            if (!ev.Player.GameObject.TryGetComponent<SanyaRemasteredComponent>(out _))
                ev.Player.GameObject.AddComponent<SanyaRemasteredComponent>();
            try
            {
                Methods.AddPlayerAudio(ev.Player);
            }
            catch { }
        }
        public void OnChangingRole(ChangingRoleEventArgs ev)
        {
            if (ev.Reason is SpawnReason.LateJoin)
            {

                if (Scp0492UserID.ContainsKey(ev.Player.UserId))
                {
                    ev.NewRole = RoleTypeId.Scp0492;
                }
            }
        }
        public void OnSpawned(SpawnedEventArgs ev)
        {
            if (ev.Player.Role.Type is RoleTypeId.Scp0492 && Scp0492UserID.TryGetValue(ev.Player.UserId, out RelativePosition relativePosition))
            {
                ev.Player.Position = relativePosition.Position;
                Scp0492UserID.Remove(ev.Player.UserId);
                ev.Player.Broadcast(10, "Vous avez été réscucité par Scp049 avant votre déconnexion\n Attention si vous étes repris a faire ça il y auras des sanction");
            }
        }

        public void OnPlayerDestroying(DestroyingEventArgs ev)
        {
            Log.Debug($"[OnPlayerLeave] {ev.Player.Nickname} ({ev.Player.ReferenceHub.queryProcessor._ipAddress}:{ev.Player.UserId})");
            if (plugin.Config.StaffMaxPlayer && ev.Player.RemoteAdminAccess)
            {
                Server.MaxPlayerCount--;
            }
            foreach (Player player in Player.List)
            {
                if (player.Role is Scp049Role scp049 && scp049.IsRecalling)
                {
                    if (Ragdoll.Get(scp049.ResurrectAbility.CurRagdoll)?.Owner.UserId != ev.Player.UserId) 
                        break;
                    Scp0492UserID.Add(ev.Player.UserId, scp049.RelativePosition);
                }
            }
            if (ev.Player.Role.Type is RoleTypeId.Scp0492)
                Scp0492UserID.Add(ev.Player.UserId, ev.Player.RelativePosition);

            if ((Round.IsLocked || Round.IsLobbyLocked) && ev.Player.RemoteAdminAccess && !Player.List.Any(x => x != ev.Player && x.RemoteAdminAccess))
            {
                Round.IsLocked = false;
                Round.IsLobbyLocked = false;
            }
            try
            {
                Methods.RemovePlayerAudio(ev.Player);
            }
            catch { }
        }
        public void OnPlayerHurting(HurtingEventArgs ev)
        {
            if (!ev.IsAllowed) return;
            if (ev.Player is null || ev.Player.Role.Type is RoleTypeId.Spectator || ev.Player.IsGodModeEnabled || ev.Player.IsSpawnProtected || !ev.IsAllowed) return;

            if (ev.DamageHandler.Type is not DamageType.Warhead or DamageType.Decontamination or DamageType.Crushed or DamageType.Tesla or DamageType.Scp207)
            {
                //GrenadeHitmark
                if (ev.Attacker is not null)
                    if (SanyaRemastered.Instance.Config.HitmarkGrenade
                    && ev.DamageHandler.Type is DamageType.Explosion
                    && ev.Player.UserId != ev.Attacker.UserId)
                    {
                        ev.Attacker.SendHitmarker();
                    }
            }
        }

        public void OnDied(DiedEventArgs ev)
        {
            if (ev.Player.Role.Type is RoleTypeId.Spectator || ev.Player.ReferenceHub.characterClassManager.GodMode || ev.Player.IsSpawnProtected) return;
            Log.Debug($"[OnAttackerDeath] {ev.Attacker?.Nickname}[{ev.Attacker?.Role}] -{ev.DamageHandler.Type}-> {ev.Player?.Nickname}[{ev.Player?.Role}]");

            if (ev.Attacker is null) return;


            if (SanyaRemastered.Instance.Config.HitmarkKilled
                && ev.Attacker.Role.Team is not Team.SCPs
                && !string.IsNullOrEmpty(ev.Attacker.UserId)
                && ev.Attacker.UserId != ev.Player.UserId)
            {
                ev.Attacker.SendHitmarker(3);
            }

            if (ev.DamageHandler.Type is DamageType.Decontamination or DamageType.Warhead)
            {
                ev.Player.Inventory.UserInventory.Items.Clear();
                ev.Player.Inventory.UserInventory.ReserveAmmo.Clear();
            }
            if (ev.DamageHandler.ServerLogsText.Remove(0, 30) is "Disconnect" && ev.Player.Role.Type is RoleTypeId.Scp0492)
            {
                Scp0492UserID.Add(ev.Player.UserId, ev.Player.RelativePosition);
            }
        }
        public void OnChangingAmmo(ChangingAmmoEventArgs ev)
        {
            if (ev.Player.SessionVariables.ContainsKey("InfAmmo") && ev.OldAmmo > ev.NewAmmo)
            {
                ev.IsAllowed = false;
            }
        }
        public void OnUsingMicroHIDEnergy(UsingMicroHIDEnergyEventArgs ev)
        {
            if (ev.Player.SessionVariables.ContainsKey("InfAmmo"))
            {
                ev.Drain = 0;
            }
        }
        public void OnActivatingWarheadPanel(ActivatingWarheadPanelEventArgs ev)
        {
            Log.Debug($"[OnActivatingWarheadPanel] Nickname : {ev.Player.Nickname}  Allowed : {ev.IsAllowed}");

            var outsite = Object.FindObjectOfType<AlphaWarheadOutsitePanel>();
            if (SanyaRemastered.Instance.Config.Nukecapclose && outsite.keycardEntered)
            {
                Timing.CallDelayed(0.1f,() => 
                {
                    outsite.NetworkkeycardEntered = false;
                });
            }
            else if (outsite.keycardEntered)
            {
                ev.IsAllowed = false;
            }
        }

        public void OnGeneratorUnlock(UnlockingGeneratorEventArgs ev)
        {
            if (ev.IsAllowed && SanyaRemastered.Instance.Config.GeneratorUnlockOpen) ev.Generator.Base.ServerSetFlag(Scp079Generator.GeneratorFlags.Open, true);
        }
        public void OnActivatingGenerator(ActivatingGeneratorEventArgs ev)
        {
            Log.Debug($"[OnActivatingGenerator] {ev.Player.Nickname} -> {ev.Generator.Room.Type}");
            if (SanyaRemastered.Instance.Config.GeneratorActivatingClose) ev.Generator.Base.ServerSetFlag(Scp079Generator.GeneratorFlags.Open, false);
        }
        public void OnHandcuffing(HandcuffingEventArgs ev)
        {
            Log.Debug($"[OnHandcuffing] {ev.Player.Nickname} -> {ev.Target.Nickname}");
            if (ev.Target.IsGodModeEnabled) ev.IsAllowed = false;
        }
    }
}
