using CustomPlayerEffects;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.Events.EventArgs;
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
                ObjectDestroyMessage objectDestroyMessage = new ObjectDestroyMessage
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
                SanyaRemastered.Instance.ServerHandlers.roundCoroutines.Add(Timing.CallDelayed(5f, () =>
                {
                    ev.Player.ReferenceHub.GetComponent<SanyaRemasteredComponent>().AddHudCenterDownText(SubtitlesList.Extend079First, 20);
                    ev.Player.ResetInventory(new List<ItemType> { ItemType.KeycardJanitor, ItemType.KeycardScientist, ItemType.GunCOM15, ItemType.GunShotgun, ItemType.Medkit, ItemType.GrenadeFlash });
                }));
            }
            if (SanyaRemastered.Instance.Config.Scp079ExtendEnabled && ev.Player.Role == RoleType.Scp079)
            {
                ev.Player.ClearInventory();
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

            if (SanyaRemastered.Instance.Config.Scp106slow && ev.Player.Role == RoleType.Scp106
                || SanyaRemastered.Instance.Config.Scp939slow && ev.Player.Role == (RoleType.Scp93953 | RoleType.Scp93989))
            {
                ev.Player.EnableEffect(EffectType.Disabled);
            }
        }
        public void OnPlayerHurt(HurtingEventArgs ev)
        {
            if (ev.Target == null || ev.Target.IsHost || ev.Target.Role == RoleType.Spectator || ev.Target.ReferenceHub.characterClassManager.GodMode || ev.Target.ReferenceHub.characterClassManager.SpawnProtected || !ev.IsAllowed) return;
            Log.Debug($"[OnPlayerHurt:Before] {ev.Attacker?.Nickname}[{ev.Attacker?.Role}] -{ev.Handler.Type}({ev.Amount})-> {ev.Target?.Nickname}[{ev.Target?.Role}]", SanyaRemastered.Instance.Config.IsDebugged);

            if (ev.Handler.Type == DamageType.Scp && SanyaRemastered.Instance.Config.Scp939EffectiveArmor > 0 && BodyArmorUtils.TryGetBodyArmor(ev.Target.Inventory, out BodyArmor bodyArmor))
            {
                ev.Amount = BodyArmorUtils.ProcessDamage(bodyArmor.VestEfficacy, ev.Amount, SanyaRemastered.Instance.Config.Scp939EffectiveArmor);
            }
            {
                if (ev.Handler.Type != DamageType.Warhead
                    && ev.Handler.Type != DamageType.Decontamination
                    && ev.Handler.Type != DamageType.Crushed
                    && ev.Handler.Type != DamageType.Tesla
                    && ev.Handler.Type != DamageType.Scp207)
                {
                    //GrenadeHitmark
                    if (ev.Attacker != null)
                        if (SanyaRemastered.Instance.Config.HitmarkGrenade
                        && ev.Handler.Type != DamageType.Explosion
                        && ev.Target.UserId != ev.Attacker.UserId)
                        {
                            ev.Attacker.SendHitmarker();
                        }

                    //USPMultiplier
                    if (ev.Handler.Type == DamageType.Com18)
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
                        && ev.Handler.Type != DamageType.Recontainment
                        && ev.Handler.Type != DamageType.Poison)
                    {
                        if (ev.Target.ArtificialHealth < ev.Amount && SanyaRemastered.Instance.Config.ScpDamageMultiplicator.TryGetValue(ev.Target.Role, out float AmmountDamage) && AmmountDamage != 1)
                        {
                            ev.Amount *= AmmountDamage;
                        }
                        if (ev.Target.Role == RoleType.Scp106)
                        {
                            if (ev.Handler.Type != DamageType.Explosion) ev.Amount *= SanyaRemastered.Instance.Config.Scp106GrenadeMultiplicator;
                            if (ev.Handler.Type != DamageType.MicroHid && ev.Handler.Type != DamageType.Tesla)
                                ev.Amount *= SanyaRemastered.Instance.Config.Scp106DamageMultiplicator;
                        }
                    }
                }
            }
            Log.Debug($"[OnPlayerHurt:After] {ev.Attacker?.Nickname}[{ev.Attacker?.Role}] -{ev.Handler.Base}({ev.Amount})-> {ev.Target?.Nickname}[{ev.Target?.Role}]", SanyaRemastered.Instance.Config.IsDebugged);
        }

        public void OnDied(DiedEventArgs ev)
        {
            if (ev.Target.IsHost || ev.Target.Role == RoleType.Spectator || ev.Target.ReferenceHub.characterClassManager.GodMode || ev.Target.ReferenceHub.characterClassManager.SpawnProtected) return;
            Log.Debug($"[OnPlayerDeath] {ev.Killer?.Nickname}[{ev.Killer?.Role}] -{ev.Handler.Type}-> {ev.Target?.Nickname}[{ev.Target?.Role}]", SanyaRemastered.Instance.Config.IsDebugged);

            if (SanyaRemastered.Instance.Config.Scp939Size != 1)
            {
                ev.Target.Scale = Vector3.one;
            }
            if (ev.Killer == null) return;

            if (SanyaRemastered.Instance.Config.ScpRecoveryAmount.TryGetValue(ev.Handler.Type.ToString(), out int Heal) && Heal > 0)
            {
                ev.Killer.Heal(Heal);
            }

            if (SanyaRemastered.Instance.Config.HitmarkKilled
                && ev.Killer.Team != Team.SCP
                && !string.IsNullOrEmpty(ev.Killer.UserId)
                && ev.Killer.UserId != ev.Target.UserId)
            {
                ev.Killer.SendHitmarker(3);
            }

            if (SanyaRemastered.Instance.Config.CassieSubtitle
                && ev.Target.Team == Team.SCP)
            {
                string fullname = CharacterClassManager._staticClasses.Get(ev.Target.Role).fullName;
                string str;
                if (ev.Target.Role != RoleType.Scp0492)
                {
                    if (ev.Handler.Type == DamageType.Warhead)
                    {
                        str = SubtitlesList.SCPDeathWarhead.Replace("{0}", fullname);
                    }
                    else if (ev.Handler.Type == DamageType.Tesla)
                    {
                        str = SubtitlesList.SCPDeathTesla.Replace("{0}", fullname);
                    }
                    else if (ev.Handler.Type == DamageType.Decontamination)
                    {
                        str = SubtitlesList.SCPDeathDecont.Replace("{0}", fullname);
                    }
                    else
                    {
                        Log.Debug($"[CheckTeam] ply:{ev.Target.Id} kill:{ev.Killer.Id} killteam:{ev.Killer.Team}", SanyaRemastered.Instance.Config.IsDebugged);
                        switch (ev.Killer.Team)
                        {
                            case Team.CDP:
                                {
                                    str = SubtitlesList.SCPDeathTerminated.Replace("{0}", fullname).Replace("{1}", "un classe-D");
                                    break;
                                }
                            case Team.CHI:
                                {
                                    str = SubtitlesList.SCPDeathTerminated.Replace("{0}", fullname).Replace("{1}", "l'insurection du chaos");
                                    break;
                                }
                            case Team.RSC:
                                {
                                    str = SubtitlesList.SCPDeathTerminated.Replace("{0}", fullname).Replace("{1}", "un scientifique");
                                    break;
                                }
                            case Team.MTF:
                                {
                                    string unit = ev.Killer.ReferenceHub.characterClassManager.CurUnitName;
                                    str = SubtitlesList.SCPDeathContainedMTF.Replace("{0}", fullname).Replace("{1}", unit);
                                    break;
                                }
                            default:
                                {
                                    str = SubtitlesList.SCPDeathUnknown.Replace("{0}", fullname);
                                    break;
                                }
                        }
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
                    && ev.Handler.Type == DamageType.Warhead)
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

            if (ev.Handler.Type == DamageType.Decontamination || ev.Handler.Type == DamageType.Warhead || ev.Handler.Type == DamageType.FemurBreaker)
            {
                ev.Target.Inventory.UserInventory.Items.Clear();
                ev.Target.Inventory.UserInventory.ReserveAmmo.Clear();
            }
        }

        public void OnPocketDimDeath(FailingEscapePocketDimensionEventArgs ev)
        {
            Log.Debug($"[OnPocketDimDeath] {ev.Player.Nickname}", SanyaRemastered.Instance.Config.IsDebugged);
            List<Player> Scp106 = Player.List.Where(x => x.Role == RoleType.Scp106).ToList();
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
                ev.Player.ReferenceHub.playerEffectsController.DisableEffect<Disabled>();
            }
            if (ev.Item.Base.ItemTypeId == ItemType.SCP500)
            {
                ev.Player.DisableAllEffects();
                if (ev.Player.GameObject.TryGetComponent<Scp914Effect>(out var comp))
                    UnityEngine.Object.Destroy(comp);
                if (ev.Player.IsInPocketDimension)
                    ev.Player.EnableEffect(EffectType.Corroding);
            }
        }

        public void OnPlayerTriggerTesla(TriggeringTeslaEventArgs ev)
        {
            if (ev.Tesla.NetworkInactiveTime > 0)
            {
                ev.IsTriggerable = false;
                ev.IsInIdleRange = false;
            }
            else if (SanyaRemastered.Instance.Config.TeslaNoTriggerRadioPlayer 
                && Map.FindParentRoom(ev.Tesla.gameObject)?.Players.Any(p => p.Items.Any(i => i.Base is RadioItem radio && radio.IsUsable)) == true)
            {
                ev.IsTriggerable = false;
                ev.IsInIdleRange = false;
                ev.Tesla.NetworkInactiveTime = 1;
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
            if (ev.Door.Type == (DoorType.GateA | DoorType.GateB) && ev.Door.Base.TryGetComponent<GateTimerClose>(out var Gate))
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
            if (ev.Shooter.SessionVariables.ContainsKey("InfAmmo") && ev.Shooter?.CurrentItem is Firearm firearm)
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
        public void OnSyncingData(SyncingDataEventArgs ev)
        {
            if (ev.Player == null || ev.Player.IsHost || !ev.Player.ReferenceHub.Ready) return;

            if (SanyaRemastered.Instance.Config.Scp079ExtendEnabled && ev.Player.Role == RoleType.Scp079)
            {
                if (ev.CurrentAnimation == 1)
                    ev.Player.ReferenceHub.GetComponent<SanyaRemasteredComponent>().AddHudCenterDownText(SubtitlesList.ExtendEnabled, 3);
                else
                    ev.Player.ReferenceHub.GetComponent<SanyaRemasteredComponent>().AddHudCenterDownText(SubtitlesList.ExtendDisabled, 3);
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
                                        ev.Player.ReferenceHub.GetComponent<SanyaRemasteredComponent>().AddHudCenterDownText(SubtitlesList.Extend079NoEnergy, 5);
                                        break;
                                    }

                                    scp079.RpcSwitchCamera(target.cameraId, false);
                                    scp079.Mana -= SanyaRemastered.Instance.Config.Scp079ExtendCostFindscp;
                                    scp079.currentCamera = target;
                                    break;
                                }
                            }
                            ev.Player.ReferenceHub.GetComponent<SanyaRemasteredComponent>().AddHudCenterDownText(SubtitlesList.Extend079NoLevel, 5);
                            break;
                        }
                    case HotkeyButton.PrimaryFirearm:
                        {

                            if (!ev.Player.SessionVariables.ContainsKey("scp079_advanced_mode"))
                                ev.Player.SessionVariables.Add("scp079_advanced_mode", null);
                            ev.Player.ReferenceHub.GetComponent<SanyaRemasteredComponent>().AddHudCenterDownText(SubtitlesList.ExtendEnabled, 5);
                            ev.Player.Inventory.ServerSelectItem(4);
                            break;
                        }
                    case HotkeyButton.SecondaryFirearm:
                        {
                            ev.Player.ReferenceHub.GetComponent<SanyaRemasteredComponent>().AddHudCenterDownText(SubtitlesList.ExtendDisabled, 5);
                            ev.Player.SessionVariables.Remove("scp079_advanced_mode");
                            ev.Player.Inventory.ServerSelectItem(0);
                            break;
                        }
                    case HotkeyButton.Medical:
                        {
                            if (scp079.Network_curLvl + 1 >= SanyaRemastered.Instance.Config.Scp079ExtendLevelFindGeneratorActive)
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
                                        ev.Player.ReferenceHub.GetComponent<SanyaRemasteredComponent>().AddHudCenterDownText(SubtitlesList.Extend079NoEnergy, 5);
                                        break;
                                    }

                                    scp079.RpcSwitchCamera(target.cameraId, false);
                                    scp079.Mana -= SanyaRemastered.Instance.Config.Scp079ExtendCostFindGeneratorActive;
                                    scp079.currentCamera = target;
                                    break;
                                }
                            }
                            ev.Player.ReferenceHub.GetComponent<SanyaRemasteredComponent>().AddHudCenterDownText(SubtitlesList.Extend079NoLevel, 5);
                            break;
                        }
                }
            }
        }
    }
}
