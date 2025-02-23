﻿using HarmonyLib;
using LightContainmentZoneDecontamination;
using System;
using UnityEngine;
using Respawning;
using Exiled.API.Features;
using System.Linq;
using Exiled.API.Enums;
using System.Text;
using PlayerRoles.Voice;
using PlayerRoles;
using Intercom = PlayerRoles.Voice.Intercom;
using ExiledIntercom = Exiled.API.Features.Intercom;
using VoiceChat;

namespace SanyaRemastered.Patches
{
	[HarmonyPatch(typeof(Intercom), nameof(Intercom.Update))]

	class IntercomUpdateTextPatches
	{
		public static void Prefix(Intercom __instance)
		{
            try
            {
                if (!string.IsNullOrEmpty(SanyaRemastered.Instance.SpecialTextIntercom))
                {
                    IntercomDisplay._singleton.Network_overrideText = SanyaRemastered.Instance.SpecialTextIntercom;
                    return;
                }
                if (!SanyaRemastered.Instance.Config.IntercomInformation || Warhead.Controller is null)
                    return;
                if (Room.Get(RoomType.EzIntercom)?.AreLightsOff ?? false && SanyaRemastered.Instance.Config.IntercomBrokenOnBlackout)
                {
                    IntercomDisplay._singleton.Network_overrideText = " ";

                    if (ExiledIntercom.InUse)
                        ExiledIntercom.Timeout();

                    return;
                }

                int leftdecont = (int)Math.Truncate(DecontaminationController.Singleton.DecontaminationPhases[DecontaminationController.Singleton.DecontaminationPhases.Length - 1].TimeTrigger - Math.Truncate(DecontaminationController.GetServerTime));
                int respawntime = 0;// (int)Math.Truncate(RespawnManager.CurrentSequence() is RespawnManager.RespawnSequencePhase.RespawnCooldown ? RespawnManager.Singleton._timeForNextSequence - RespawnManager.Singleton._stopwatch.Elapsed.TotalSeconds : 0);
                int TimeWarhead = (int)Math.Truncate(Warhead.DetonationTimer);

                leftdecont = Mathf.Clamp(leftdecont, 0, leftdecont);

                float totalvoltagefloat = 0f;
                foreach (var i in Generator.Get(GeneratorState.Activating | GeneratorState.Engaged))
                {
                    totalvoltagefloat += i.CurrentTime;
                }
                totalvoltagefloat = Mathf.CeilToInt(totalvoltagefloat);

                StringBuilder stringBuilder = new($"<nobr><color=#FFFFFF>───── Centre d'information FIM Epsilon-11 ─────</color></nobr>\n" +
                            $"Durée de la brèche : {RoundSummary.roundTime / 60:00}:{RoundSummary.roundTime % 60:00}\n" +
                            $"SCP restants : {RoundSummary.singleton.CountTeam(Team.SCPs) - RoundSummary.singleton.CountRole(RoleTypeId.Scp0492):00}/{RoundSummary.singleton.classlistStart.scps_except_zombies:00}\n" +
                            $"Classe-D restants : {RoundSummary.singleton.CountTeam(Team.ClassD):00}/{RoundSummary.singleton.classlistStart.class_ds:00}\n" +
                            $"Scientifique restants : {RoundSummary.singleton.CountTeam(Team.Scientists):00}/{RoundSummary.singleton.classlistStart.scientists:00}\n" +
                            $"Nine-Tailed Fox restants : {RoundSummary.singleton.CountTeam(Team.FoundationForces):00}\n"
                            );
                //warhead
                if (Warhead.IsInProgress)
                {
                    stringBuilder.Append($"<color=#ff0000>{(TimeWarhead > 10 ? "Explosion de l'Alpha Warhead" : "Détonation inévitable")} : {TimeWarhead / 60}:{TimeWarhead % 60:00}</color>\n");
                }
                else
                {
                    stringBuilder.Append($"Statut de l'Alpha Warhead : {(AlphaWarheadOutsitePanel.nukeside.Networkenabled ? "PRÊTE" : "DÉSACTIVÉE")}\n");
                }

                //Générateur 079
                if (Generator.Get(GeneratorState.Engaged).Count() is 3)
                {
                    stringBuilder.Append("Tous les générateurs du site sont activés\n");
                }
                else
                {
                    stringBuilder.Append($"Puissance des générateurs : {totalvoltagefloat:0000}KVA\n");
                }

                //décontamination
                if (!DecontaminationController.Singleton._decontaminationBegun)
                {
                    if (leftdecont > 30)
                    {
                        stringBuilder.Append($"Temps restant avant la décontamination de la LCZ :  {leftdecont / 60:00}:{leftdecont % 60:00}\n");
                    }
                    else
                    {
                        stringBuilder.Append($"<color=#ff0000>Temps restant avant la décontamination de la LCZ : {leftdecont / 60:00}:{leftdecont % 60:00}</color>\n");
                    }
                }
                else if (DecontaminationController.Singleton._decontaminationBegun)
                {
                    stringBuilder.Append($"La décontamination de la LCZ a été effectué\n");
                }

                //Prochain spawn + durée MTF
                /*
                if (Respawn.NextKnownTeam is SpawnableTeamType.ChaosInsurgency or SpawnableTeamType.NineTailedFox)
                    stringBuilder.Append($"Les renforts se préparent\n");
                else
                    stringBuilder.Append($"Prochains renforts : {respawntime / 60:00}:{respawntime % 60:00}\n");
                */
                //Speak Intercom
                stringBuilder.Append(SpeakIntercom());

                IntercomDisplay._singleton.Network_overrideText = stringBuilder.ToString();
                stringBuilder.Clear();
                return;
            }
            catch (Exception ex)
            {
                Log.Error($"Intercom::UpdateText Prefix : {ex}\n {ex.StackTrace}");
            }
        }

        public static string SpeakIntercom() => Intercom.State switch
        {
            IntercomState.Ready => "Intercom prêt à l'emploi.",
            IntercomState.Starting => "Attendez que l'intercom soit en ligne",
            IntercomState.InUse => IntercomDisplay._singleton._icom.BypassMode ? 
                                      $"{ExiledIntercom.Speaker?.DisplayNickname ?? "(null)"} à une diffusion prioritaire" 
                                    : $"{ExiledIntercom.Speaker?.DisplayNickname ?? "(null)"} Diffuse : {Mathf.Round(IntercomDisplay._singleton._icom.RemainingTime)} seconde{(IntercomDisplay._singleton._icom.RemainingTime <= 1 ? "" : "s")}",
            IntercomState.Cooldown => $"Temps avant redémarrage : {(int)ExiledIntercom.RemainingCooldown} seconde{(ExiledIntercom.RemainingCooldown <= 1 ? "" : "s")}",
            IntercomState.NotFound => $"Unknown",
            _ => "ERROR",
        };
    }
}

