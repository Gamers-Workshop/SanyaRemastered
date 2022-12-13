using CustomPlayerEffects;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.DamageHandlers;
using Exiled.API.Features.Items;
using Exiled.API.Features.Roles;
using Exiled.Events.EventArgs;
using Exiled.Events.EventArgs.Player;
using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items.Armor;
using InventorySystem.Items.Radio;
using MapGeneration.Distributors;
using MEC;
using Mirror;
using PlayerStatsSystem;
using SanyaRemastered.Data;
using SanyaRemastered.Functions;
using Scp914;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Camera = Exiled.API.Features.Camera;


namespace SanyaRemastered.EventHandlers
{
    public class PlayerHandlers
    {
        public PlayerHandlers(SanyaRemastered plugin) => this.plugin = plugin;
        internal readonly SanyaRemastered plugin;

        public void OnPreAuth(PreAuthenticatingEventArgs ev)
        {
            Log.Debug($"[OnPreAuth] {ev.Request.RemoteEndPoint.Address}:{ev.UserId}", SanyaRemastered.Instance.Config.IsDebugged);
        }

        public void OnPlayerVerified(VerifiedEventArgs ev)
        {
            if (ev.Player.ReferenceHub.characterClassManager.IsHost) return;
            Log.Info($"[OnPlayerJoin] {ev.Player.Nickname} ({ev.Player.ReferenceHub.queryProcessor._ipAddress}:{ev.Player.UserId})");

            if (plugin.Config.DisablePlayerLists && SanyaRemastered.Instance.ServerHandlers.playerlistnetid > 0)
            {
                ObjectDestroyMessage objectDestroyMessage = new()
                {
                    netId = SanyaRemastered.Instance.ServerHandlers.playerlistnetid
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

        public void OnChangingRole(ChangingRoleEventArgs ev)
        {
            if (ev.Player.IsHost) return;
            Log.Debug($"[OnPlayerSetClass] {ev.Player.Nickname} -{ev.Reason}> {ev.NewRole}", SanyaRemastered.Instance.Config.IsDebugged);
            if (ev.Player.GameObject.TryGetComponent<ContainScpComponent>(out var comp1))
                UnityEngine.Object.Destroy(comp1);

            if (SanyaRemastered.Instance.Config.Scp079ExtendEnabled)
            {
                if (ev.NewRole is RoleType.Scp079)
                {
                    ev.Items.AddRange(new List<ItemType> { ItemType.KeycardJanitor, ItemType.KeycardScientist, ItemType.GunCOM15, ItemType.GunShotgun, ItemType.Medkit, ItemType.GrenadeFlash });
                    SanyaRemastered.Instance.ServerHandlers.roundCoroutines.Add(Timing.CallDelayed(5f, () =>
                    {
                        ev.Player.ReferenceHub.GetComponent<SanyaRemasteredComponent>().AddHudCenterDownText(SanyaRemastered.Instance.Translation.HintList.Extend079First, 20);
                    }));
                }
                else if (ev.Player.Role.Type is RoleType.Scp079)
                    ev.Player.ClearInventory(true);
            }
            if (plugin.Config.Scp096Real && ev.Reason is SpawnReason.Escaped)
            {
                bool IsAnTarget = false;
                foreach (Player scp096 in Player.Get(RoleType.Scp096))
                    if (scp096.CurrentScp is PlayableScps.Scp096 Scp096 && Scp096.HasTarget(ev.Player.ReferenceHub))
                        IsAnTarget = true;
                if (IsAnTarget)
                {
                    ev.NewRole = RoleType.Spectator;
                    ev.Player.ReferenceHub.GetComponent<SanyaRemasteredComponent>().AddHudCenterDownText("Vous avez été abatue a la sortie car vous avez vue le visage de SCP-096", 10);
                }
            }
        }


        public void OnPlayerSpawning(SpawningEventArgs ev)
        {
            if (ev.Player.IsHost) return;
            Log.Debug($"[OnPlayerSpawn] {ev.Player.Nickname} -{ev.RoleType}-> {ev.Position}", SanyaRemastered.Instance.Config.IsDebugged);

            if (ev.Player.Role.Type is RoleType.Scp93953 or RoleType.Scp93989)
                ev.Player.Scale = new Vector3(SanyaRemastered.Instance.Config.Scp939Size, SanyaRemastered.Instance.Config.Scp939Size, SanyaRemastered.Instance.Config.Scp939Size);

            if (SanyaRemastered.Instance.Config.Scp106slow && ev.Player.Role.Type is RoleType.Scp106
                || SanyaRemastered.Instance.Config.Scp939slow && ev.Player.Role.Type is RoleType.Scp93953 or RoleType.Scp93989)
            {
                ev.Player.EnableEffect(EffectType.Disabled);
            }
        }
        public void OnPlayerHurting(HurtingEventArgs ev)
        {
            if (!ev.IsAllowed) return;
            if (ev.Target is null || ev.Target.IsHost || ev.Target.Role.Type is RoleType.Spectator || ev.Target.IsGodModeEnabled || ev.Target.IsSpawnProtected || !ev.IsAllowed) return;
            Log.Debug($"[OnPlayerHurt:Before] {ev.Player?.Nickname}[{ev.Player?.Role}] -{ev.DamageHandler.Type}({ev.Amount})-> {ev.Target?.Nickname}[{ev.Target?.Role}]", SanyaRemastered.Instance.Config.IsDebugged);

            if (SanyaRemastered.Instance.Config.Scp939EffectiveArmor > 0 && ev.DamageHandler.Type is DamageType.Scp939 && BodyArmorUtils.TryGetBodyArmor(ev.Target.Inventory, out BodyArmor bodyArmor))
                ev.Amount = BodyArmorUtils.ProcessDamage(bodyArmor.VestEfficacy, ev.Amount, SanyaRemastered.Instance.Config.Scp939EffectiveArmor);
            if (ev.DamageHandler.Type is not DamageType.Warhead or DamageType.Decontamination or DamageType.Crushed or DamageType.Tesla or DamageType.Scp207)
            {
                //GrenadeHitmark
                if (ev.Player is not null)
                    if (SanyaRemastered.Instance.Config.HitmarkGrenade
                    && ev.DamageHandler.Type is DamageType.Explosion
                    && ev.Target.UserId != ev.Player.UserId)
                    {
                        ev.Player.SendHitmarker();
                    }

                //USPMultiplier
                if (ev.DamageHandler.Type is DamageType.Com18)
                {
                    if (ev.Target.IsScp)
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
                if (ev.Target.IsScp && ev.DamageHandler.Type is not DamageType.Recontainment or DamageType.Poison)
                {
                    if (ev.Target.ArtificialHealth < ev.Amount && SanyaRemastered.Instance.Config.ScpDamageMultiplicator.TryGetValue(ev.Target.Role, out float AmmountDamage) && AmmountDamage != 1)
                    {
                        ev.Amount *= AmmountDamage;
                    }
                    if (ev.Target.Role == RoleType.Scp106)
                    {
                        if (ev.DamageHandler.Type is not DamageType.Explosion) ev.Amount *= SanyaRemastered.Instance.Config.Scp106GrenadeMultiplicator;
                        if (ev.DamageHandler.Type is not (DamageType.MicroHid or DamageType.Tesla))
                            ev.Amount *= SanyaRemastered.Instance.Config.Scp106DamageMultiplicator;
                    }
                }
            }
            Log.Debug($"[OnPlayerHurt:After] {ev.Player?.Nickname}[{ev.Player?.Role}] -{ev.DamageHandler.Base}({ev.Amount})-> {ev.Target?.Nickname}[{ev.Target?.Role}]", SanyaRemastered.Instance.Config.IsDebugged);
        }

        public void OnDied(DiedEventArgs ev)
        {
            if (ev.Target.IsHost || ev.Target.Role.Type is RoleType.Spectator || ev.Target.ReferenceHub.characterClassManager.GodMode || ev.Target.ReferenceHub.characterClassManager.SpawnProtected) return;
            Log.Debug($"[OnPlayerDeath] {ev.Player?.Nickname}[{ev.Player?.Role}] -{ev.DamageHandler.Type}-> {ev.Target?.Nickname}[{ev.Target?.Role}]", SanyaRemastered.Instance.Config.IsDebugged);

            if (SanyaRemastered.Instance.Config.Scp939Size is not 1)
            {
                ev.Target.Scale = Vector3.one;
            }
            if (ev.Player is null) return;

            if (SanyaRemastered.Instance.Config.ScpRecoveryAmount.TryGetValue(ev.DamageHandler.Type.ToString(), out int Heal) && Heal > 0)
            {
                ev.Player.Heal(Heal);
            }

            if (SanyaRemastered.Instance.Config.HitmarkKilled
                && ev.Player.Role.Team is not Team.SCP
                && !string.IsNullOrEmpty(ev.Player.UserId)
                && ev.Player.UserId != ev.Target.UserId)
            {
                ev.Player.SendHitmarker(3);
            }

            if (ev.DamageHandler.Type is DamageType.Decontamination or DamageType.Warhead or DamageType.FemurBreaker)
            {
                ev.Target.Inventory.UserInventory.Items.Clear();
                ev.Target.Inventory.UserInventory.ReserveAmmo.Clear();
            }
        }
        public void OnSpawningRagdoll(SpawningRagdollEventArgs ev)
        {
            ev.IsAllowed = false;
            DamageHandler damage = new CustomDamageHandler(ev.Player, ev.DamageHandlerBase);
            double time = NetworkTime.time;
            //Disable Ragdoll
            if (SanyaRemastered.Instance.Config.Scp106RemoveRagdoll && damage.Type is DamageType.Scp106
                || SanyaRemastered.Instance.Config.Scp096RemoveRagdoll && damage.Type is DamageType.Scp096)
                //Disable Recall By 079
                if (SanyaRemastered.Instance.Config.Scp049Real && damage.Type is not DamageType.Scp049) time = double.MinValue;
            //TeslaDestroyTheNameOfThePlayer
            if (SanyaRemastered.Instance.Config.TeslaDestroyName && damage.Type is DamageType.Tesla) ev.Nickname = "inconue";


            ev.Info = new RagdollInfo(ev.Player.ReferenceHub, ev.DamageHandlerBase, ev.Role, ev.Position, ev.Rotation, ev.Nickname, time);
            Exiled.API.Features.Ragdoll ragdoll = new(ev.Info);

            ragdoll.Scale = new Vector3(ev.Player.Scale.x * ragdoll.Scale.x,
                                              ev.Player.Scale.y * ragdoll.Scale.y,
                                              ev.Player.Scale.z * ragdoll.Scale.z);
        }
        public void OnPocketDimDeath(FailingEscapePocketDimensionEventArgs ev)
        {
            Log.Debug($"[OnPocketDimDeath] {ev.Player.Nickname}", SanyaRemastered.Instance.Config.IsDebugged);
            var Scp106 = Player.Get(RoleType.Scp106);
            foreach (Player player in Scp106)
            {
                player.SendHitmarker();
            }
            if (SanyaRemastered.Instance.Config.ScpRecoveryAmount.TryGetValue("Scp106", out int heal) && heal > 0)
            {
                foreach (Player player in Scp106)
                {
                    player.Heal(heal);
                }
            }
        }
        public void OnThrowingRequest(ThrowingItemEventArgs ev)
        {
            if (plugin.Config.Scp079ExtendEnabled && ev.Player.Role.Type is RoleType.Scp079)
                ev.IsAllowed = false;
        }
        public void OnPlayerUsingItem(UsingItemEventArgs ev)
        {
            if (plugin.Config.Scp079ExtendEnabled && ev.Player.Role.Type is RoleType.Scp079)
                ev.IsAllowed = false;
        }
        public void OnPlayerUsedItem(UsedItemEventArgs ev)
        {
            Log.Debug($"[OnPlayerUsedMedicalItem] {ev.Player.Nickname} -> {ev.Item.Type}", SanyaRemastered.Instance.Config.IsDebugged);
            switch (ev.Item.Type)
            {
                case ItemType.Medkit:
                    {
                        ev.Player.ReferenceHub.playerEffectsController.DisableEffect<Hemorrhage>();
                        ev.Player.ReferenceHub.playerEffectsController.DisableEffect<Bleeding>();
                        ev.Player.ReferenceHub.playerEffectsController.DisableEffect<Burned>();
                        break;
                    }
                case ItemType.Adrenaline:
                    {
                        ev.Player.ReferenceHub.playerEffectsController.DisableEffect<Disabled>();
                        break;
                    }
                case ItemType.SCP500:
                    {
                        ev.Player.DisableAllEffects();
                        if (ev.Player.IsInPocketDimension)
                            ev.Player.EnableEffect(EffectType.Corroding);
                        break;
                    }
            }
        }

        public void OnPlayerTriggerTesla(TriggeringTeslaEventArgs ev)
        {
            if (SanyaRemastered.Instance.Config.TeslaNoTriggerRadioPlayer
                && ev.Tesla.Room?.Players.Any(p => p.Items.Any(i => i.Base is RadioItem radio && radio.IsUsable)) is true)
            {
                ev.IsAllowed = false;
                ev.IsInIdleRange = false;
                ev.Tesla.InactiveTime = 1;
            }
        }

        public void OnPlayerDoorInteract(InteractingDoorEventArgs ev)
        {
            Log.Debug($"[OnPlayerDoorInteract] {ev.Player.Nickname}:{ev.Door?.Type}", SanyaRemastered.Instance.Config.IsDebugged);
            if (ev.Door.DoorLockType == DoorLockType.Isolation)
            {
                ev.IsAllowed = false;
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
            if (ev.Door.Type is (DoorType.GateA | DoorType.GateB) && ev.Door.Base.TryGetComponent(out GateTimerClose Gate))
            {
                Gate._timeBeforeClosing = -1;
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

            if (!Room.Get(RoomType.EzIntercom).LightsOn)
            {
                ev.IsAllowed = false;
            }
        }
        public void OnShooting(ShootingEventArgs ev)
        {
            Log.Debug($"[OnShooting] {ev.Player.Nickname} -{ev.ShotPosition}-> {Player.Get(ev.TargetNetId)?.Nickname}", SanyaRemastered.Instance.Config.IsDebugged);
            if (SanyaRemastered.Instance.Config.Scp079ExtendEnabled && ev.Player.Role.Type is RoleType.Scp079)
                ev.IsAllowed = false;
            if (SanyaRemastered.Instance.Config.Scp096Real)
            {
                Player target = Player.Get(ev.TargetNetId);
                if (target is not null && target.Role.Type is RoleType.Scp096)
                {
                    ev.IsAllowed = false;
                }
            }
            if (ev.Player.SessionVariables.ContainsKey("InfAmmo") && ev.Player.CurrentItem is Firearm firearm)
            {
                firearm.Ammo++;
            }
        }
        public void OnUsingMicroHIDEnergy(UsingMicroHIDEnergyEventArgs ev)
        {
            if (ev.Player.SessionVariables.ContainsKey("InfAmmo"))
            {
                ev.Drain = 0;
            }
        }
        public void OnJumping(JumpingEventArgs ev)
        {
            if (!ReferenceHub.LocalHub.characterClassManager.RoundStarted) return;

            if (SanyaRemastered.Instance.Config.StaminaLostJump > 0
                && ev.Player.IsHuman
                && !ev.Player.Stamina._invigorated.IsEnabled
                && !ev.Player.Stamina._scp207.IsEnabled)
            {
                ev.Player.Stamina.RemainingStamina -= SanyaRemastered.Instance.Config.StaminaLostJump;
                ev.Player.Stamina._regenerationTimer = 0f;
            }
        }
        public void OnActivatingWarheadPanel(ActivatingWarheadPanelEventArgs ev)
        {
            Log.Debug($"[OnActivatingWarheadPanel] Nickname : {ev.Player.Nickname}  Allowed : {ev.IsAllowed}", SanyaRemastered.Instance.Config.IsDebugged);

            var outsite = UnityEngine.Object.FindObjectOfType<AlphaWarheadOutsitePanel>();
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
        public void OnStoppingGenerator(StoppingGeneratorEventArgs ev)
        {
            Log.Debug($"[OnStoppingGenerator] {ev.Player.Nickname} -> {ev.Generator.Room.Type}", SanyaRemastered.Instance.Config.IsDebugged);
        }
        public void OnGeneratorOpen(OpeningGeneratorEventArgs ev)
        {
            Log.Debug($"[OnGeneratorOpen] {ev.Player.Nickname} -> {ev.Generator.Room.Type}", SanyaRemastered.Instance.Config.IsDebugged);
            if (ev.Generator.IsEngaged && SanyaRemastered.Instance.Config.GeneratorFinishLock)
            {
                ev.IsAllowed = false;
            }
        }

        public void OnGeneratorClose(ClosingGeneratorEventArgs ev)
        {
            Log.Debug($"[OnGeneratorClose] {ev.Player.Nickname} -> {ev.Generator.Room.Type}", SanyaRemastered.Instance.Config.IsDebugged);
        }

        public void OnActivatingGenerator(ActivatingGeneratorEventArgs ev)
        {
            Log.Debug($"[OnActivatingGenerator] {ev.Player.Nickname} -> {ev.Generator.Room.Type}", SanyaRemastered.Instance.Config.IsDebugged);
            if (SanyaRemastered.Instance.Config.GeneratorActivatingClose) ev.Generator.Base.ServerSetFlag(Scp079Generator.GeneratorFlags.Open, false);
        }

        public void OnHandcuffing(HandcuffingEventArgs ev)
        {
            Log.Debug($"[OnHandcuffing] {ev.Player.Nickname} -> {ev.Target.Nickname}", SanyaRemastered.Instance.Config.IsDebugged);
            if (ev.Target.IsGodModeEnabled) ev.IsAllowed = false;
        }
        public void OnProcessingHotkey(ProcessingHotkeyEventArgs ev)
        {
            try
            {
                Log.Debug($"[OnProcessingHotkey] {ev.Player.Nickname} -> {ev.Hotkey}", SanyaRemastered.Instance.Config.IsDebugged);
            }
            catch (Exception ex)
            {
                Log.Error($"[OnProcessingHotkey] event null {ev is null} Player null { ev?.Player is null} -> HotKey null {ev?.Hotkey is null} \n" + ex);
                return;
            }
            if (!plugin.Config.Scp079ExtendEnabled) 
                return;
            if (!ev.Player.Role.Is(out Scp079Role scp079)) 
                return;
            
            ev.IsAllowed = false;
            switch (ev.Hotkey)
            {
                case HotkeyButton.Keycard:
                    {
                        if (scp079.Level + 1 < SanyaRemastered.Instance.Config.Scp079ExtendLevelFindscp)
                        {
                            ev.Player.ReferenceHub.GetComponent<SanyaRemasteredComponent>().AddHudCenterDownText(SanyaRemastered.Instance.Translation.HintList.Extend079NoLevel, 5);
                            break;
                        }

                        List<Camera> cams = new();
                        foreach (var ply in Player.Get(Team.SCP))
                        {
                            if (ply.Role != RoleType.Scp079)
                            {
                                cams.AddRange(Map.GetNearCameras(ply.Position));
                            }
                        }

                        if (cams.Count <= 0)
                            break;
                        Camera target = cams.GetRandomOne();

                        if (target is null)
                            break;
                        if (SanyaRemastered.Instance.Config.Scp079ExtendCostFindscp > scp079.Energy)
                        {
                            scp079.Script.RpcNotEnoughMana(SanyaRemastered.Instance.Config.Scp079ExtendCostFindscp, scp079.Energy);
                            ev.Player.ReferenceHub.GetComponent<SanyaRemasteredComponent>().AddHudCenterDownText(SanyaRemastered.Instance.Translation.HintList.Extend079NoEnergy, 5);
                            break;
                        }

                        scp079.SetCamera(target);
                        scp079.Energy -= SanyaRemastered.Instance.Config.Scp079ExtendCostFindscp;
                        break;
                    }
                case HotkeyButton.PrimaryFirearm:
                    {

                        if (!ev.Player.SessionVariables.ContainsKey("scp079_advanced_mode"))
                            ev.Player.SessionVariables.Add("scp079_advanced_mode", null);
                        ev.Player.ReferenceHub.GetComponent<SanyaRemasteredComponent>().AddHudCenterDownText(SanyaRemastered.Instance.Translation.HintList.ExtendEnabled, 5);
                        ev.Player.Inventory.ServerSelectItem(4);
                        break;
                    }
                case HotkeyButton.SecondaryFirearm:
                    {
                        ev.Player.ReferenceHub.GetComponent<SanyaRemasteredComponent>().AddHudCenterDownText(SanyaRemastered.Instance.Translation.HintList.ExtendDisabled, 5);
                        ev.Player.SessionVariables.Remove("scp079_advanced_mode");
                        ev.Player.Inventory.ServerSelectItem(0);
                        break;
                    }
                case HotkeyButton.Medical:
                    {
                        if (scp079.Level + 1 < SanyaRemastered.Instance.Config.Scp079ExtendLevelFindGeneratorActive)
                        {
                            ev.Player.ReferenceHub.GetComponent<SanyaRemasteredComponent>().AddHudCenterDownText(SanyaRemastered.Instance.Translation.HintList.Extend079NoLevel, 5);
                            break;
                        }

                        List<Camera> cams = new();
                        foreach (var gen in Generator.Get(GeneratorState.Engaged))
                        {
                            cams.AddRange(Map.GetNearCameras(gen.Base.transform.position));
                        }

                        if (cams.Count <= 0)
                            break;

                        Camera target = cams.GetRandomOne();

                        if (target is null) 
                            break;

                        if (SanyaRemastered.Instance.Config.Scp079ExtendCostFindscp > scp079.Energy)
                        {
                            scp079.Script.RpcNotEnoughMana(SanyaRemastered.Instance.Config.Scp079ExtendCostFindscp, scp079.Energy);
                            ev.Player.ReferenceHub.GetComponent<SanyaRemasteredComponent>().AddHudCenterDownText(SanyaRemastered.Instance.Translation.HintList.Extend079NoEnergy, 5);
                            break;
                        }

                        scp079.SetCamera(target);
                        scp079.Energy -= SanyaRemastered.Instance.Config.Scp079ExtendCostFindGeneratorActive;
                        break;
                    }
            }

        }
    }
}
