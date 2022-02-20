using CustomPlayerEffects;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using PlayerStatsSystem;
using SanyaRemastered.Data;
using Scp914;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SanyaRemastered.EventHandlers
{
    public class ScpHandlers
    {
        public ScpHandlers(SanyaRemastered plugin) => this.plugin = plugin;
        internal readonly SanyaRemastered plugin;
        public void On079LevelGain(GainingLevelEventArgs ev)
        {
            Log.Debug($"[On079LevelGain] {ev.Player.Nickname} : {ev.NewLevel}", SanyaRemastered.Instance.Config.IsDebugged);

            if (SanyaRemastered.Instance.Config.Scp079ExtendEnabled)
            {
                switch (ev.NewLevel)
                {
                    case 1:
                        ev.Player.ReferenceHub.GetComponent<SanyaRemasteredComponent>().AddHudCenterDownText(SanyaRemastered.Instance.Translation.HintList.Extend079Lv2, 10);
                        break;
                    case 2:
                        ev.Player.ReferenceHub.GetComponent<SanyaRemasteredComponent>().AddHudCenterDownText(SanyaRemastered.Instance.Translation.HintList.Extend079Lv3, 10);
                        break;
                    case 3:
                        ev.Player.ReferenceHub.GetComponent<SanyaRemasteredComponent>().AddHudCenterDownText(SanyaRemastered.Instance.Translation.HintList.Extend079Lv4, 10);
                        break;
                    case 4:
                        ev.Player.ReferenceHub.GetComponent<SanyaRemasteredComponent>().AddHudCenterDownText(SanyaRemastered.Instance.Translation.HintList.Extend079Lv5, 10);
                        break;
                }
            }
        }
        public void On914UpgradingPlayer(UpgradingPlayerEventArgs ev)
        {
            if (SanyaRemastered.Instance.Config.Scp914Effect)
            {
                switch (ev.KnobSetting)
                {
                    case Scp914KnobSetting.Rough:
                        {
                            ev.Player.ReferenceHub.playerStats.DealDamage(new CustomReasonDamageHandler("SCP-914"));
                            if (ev.Player.Role.Team != Team.SCP)
                                ev.Player.ReferenceHub.GetComponent<SanyaRemasteredComponent>().AddHudCenterDownText("Un cadavre gravement mutilé a été trouvé à l'intérieur de SCP-914. Le sujet a évidemment été affiné par le SCP-914 sur le réglage Rough.", 30);
                        }
                        break;
                    case Scp914KnobSetting.Coarse:
                        {
                            if (ev.Player.Role == RoleType.Scp93953 || ev.Player.Role == RoleType.Scp93989)
                            {
                                ev.Player.ReferenceHub.playerStats.DealDamage(new CustomReasonDamageHandler("SCP-914"));
                            }
                            if (ev.Player.Role.Team != Team.SCP)
                            {
                                ev.Player.ReferenceHub.playerStats.DealDamage(new CustomReasonDamageHandler("SCP-914")
                                {
                                    Damage = 70,
                                });
                                ev.Player.ReferenceHub.playerEffectsController.GetEffect<Hemorrhage>();
                                ev.Player.ReferenceHub.playerEffectsController.GetEffect<Bleeding>();
                                ev.Player.ReferenceHub.playerEffectsController.GetEffect<Disabled>();
                                if (ev.Player.IsAlive)
                                    ev.Player.ReferenceHub.GetComponent<SanyaRemasteredComponent>().AddHudCenterDownText("Vous remarquez d'innombrables petites incisions dans votre corps.", 10);
                            }
                        }
                        break;
                    case Scp914KnobSetting.OneToOne:
                        {
                            if (ev.Player.Role == RoleType.Scp93953)
                            {
                                ev.Player.SetRole(RoleType.Scp93989, lite: true);
                                break;
                            }
                            else if (ev.Player.Role == RoleType.Scp93989)
                            {
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
                            if (!ev.Player.ReferenceHub.playerEffectsController.GetEffect<Scp914>().IsEnabled)
                            {
                                ev.Player.EnableEffect<MovementBoost>();
                                ev.Player.ChangeEffectIntensity<MovementBoost>(40);
                                ev.Player.EnableEffect<Invigorated>();
                                ev.Player.EnableEffect<Scp914>();
                                ev.Player.ChangeEffectIntensity<Scp914>(1);
                            }
                            else
                            {
                                if (ev.Player.ReferenceHub.playerEffectsController.AllEffects.TryGetValue(typeof(MovementBoost), out PlayerEffect playerEffect))
                                {
                                    playerEffect.Intensity = (byte)Mathf.Clamp(1.5f * playerEffect.Intensity, 0, 255);
                                }
                                if (ev.Player.ReferenceHub.playerEffectsController.AllEffects.TryGetValue(typeof(Scp914), out PlayerEffect Death))
                                {
                                    Death.Intensity = (byte)Mathf.Clamp(2 * Death.Intensity, 0, 255);
                                }
                            }
                        }
                        break;
                    case Scp914KnobSetting.VeryFine:
                        {
                            if (!ev.Player.ReferenceHub.playerEffectsController.GetEffect<Scp914>().IsEnabled)
                            {
                                ev.Player.EnableEffect<MovementBoost>();
                                ev.Player.ChangeEffectIntensity<MovementBoost>(80);
                                ev.Player.EnableEffect<Invigorated>();
                                ev.Player.EnableEffect<Scp914>();
                                ev.Player.ChangeEffectIntensity<Scp914>(10);
                            }
                            else
                            {
                                if (ev.Player.ReferenceHub.playerEffectsController.AllEffects.TryGetValue(typeof(MovementBoost), out PlayerEffect playerEffect))
                                {
                                    playerEffect.Intensity = (byte)Mathf.Clamp((float)2 * playerEffect.Intensity, 0, 255);
                                }
                                else
                                {
                                    ev.Player.EnableEffect<MovementBoost>();
                                    ev.Player.ChangeEffectIntensity<MovementBoost>(80);
                                    ev.Player.EnableEffect<Invigorated>();
                                }
                                if (ev.Player.ReferenceHub.playerEffectsController.AllEffects.TryGetValue(typeof(Scp914), out PlayerEffect Death))
                                {
                                    Death.Intensity = (byte)Mathf.Clamp(6 * Death.Intensity, 0, 255);
                                }
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
                ev.EnrageTimeToAdd = 0f;
            }
        }
        public void On096Enraging(EnragingEventArgs ev)
        {
            Log.Debug($"[On096Enraging] {ev.Player.Nickname} : {ev.Scp096.EnrageTimeLeft}", SanyaRemastered.Instance.Config.IsDebugged);
            if (SanyaRemastered.Instance.Config.Scp096Real)
            {
                ev.Scp096.EnrageTimeLeft = -ev.Scp096.EnrageTimeLeft -12f ;
            }
        }
        public void On096CalmingDown(CalmingDownEventArgs ev)
        {
            if (SanyaRemastered.Instance.Config.Scp096Real && ev.Scp096._targets.ToList().Count != 0)
            {
                ev.IsAllowed = false;
                ev.Scp096.EnrageTimeLeft = 0.5f;
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
