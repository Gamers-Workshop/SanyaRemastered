using HarmonyLib;
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

	[HarmonyPatch(typeof(Intercom), nameof(Intercom.CheckPlayer))]
	class IntercomUpdateSpeaker
	{
		public static void Postfix(Intercom __instance, ref bool __result)
        {
            if (__result is true && SanyaRemastered.Instance.Config.IntercomBrokenOnBlackout && (Room.Get(RoomType.EzIntercom)?.AreLightsOff ?? false))
                __result = false;
        }
	}

	[HarmonyPatch(typeof(IntercomDisplay), nameof(IntercomDisplay.Update))]

	class IntercomUpdateTextPatches
	{
		public static void Prefix(IntercomDisplay __instance)
		{
            try
            {
                if (!string.IsNullOrEmpty(SanyaRemastered.Instance.SpecialTextIntercom))
                {
                    __instance.Network_overrideText = SanyaRemastered.Instance.SpecialTextIntercom;
                    return;
                }
                if (!SanyaRemastered.Instance.Config.IntercomInformation || Warhead.Controller is null)
                    return;
                if (Room.Get(RoomType.EzIntercom)?.AreLightsOff ?? false && SanyaRemastered.Instance.Config.IntercomBrokenOnBlackout)
                {
                    __instance.Network_overrideText = " ";
                    return;
                }

                int leftdecont = (int)Math.Truncate(DecontaminationController.Singleton.DecontaminationPhases[DecontaminationController.Singleton.DecontaminationPhases.Length - 1].TimeTrigger - Math.Truncate(DecontaminationController.GetServerTime));
                int respawntime = (int)Math.Truncate(RespawnManager.CurrentSequence() is RespawnManager.RespawnSequencePhase.RespawnCooldown ? RespawnManager.Singleton._timeForNextSequence - RespawnManager.Singleton._stopwatch.Elapsed.TotalSeconds : 0);
                int TimeWarhead = (int)Math.Truncate(Warhead.DetonationTimer);

                leftdecont = Mathf.Clamp(leftdecont, 0, leftdecont);

                float totalvoltagefloat = 0f;
                foreach (var i in Generator.Get(GeneratorState.Activating | GeneratorState.Engaged))
                {
                    totalvoltagefloat += i.CurrentTime;
                }

                totalvoltagefloat = Mathf.CeilToInt(totalvoltagefloat);

                StringBuilder stringBuilder = new($"<color=#fffffff>────── Centre d'information FIM Epsilon-11 ──────</color>\n" +
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

                if (RespawnManager.Singleton.NextKnownTeam is SpawnableTeamType.ChaosInsurgency)
                    stringBuilder.Append($"Les renforts se préparent\n");
                else if (RespawnManager.SpawnableTeams.TryGetValue(SpawnableTeamType.NineTailedFox, out SpawnableTeamHandlerBase spawnableTeamHandlerBase) 
                    && spawnableTeamHandlerBase.StartTokens <= 0)
                    stringBuilder.Append($"Aucun renforts prévus pour le site\n");
                else
                    stringBuilder.Append($"Prochains renforts : {respawntime / 60:00}:{respawntime % 60:00}\n");

                //Speak Intercom
                stringBuilder.Append(SpeakIntercom(__instance));

                __instance.Network_overrideText = stringBuilder.ToString();
                stringBuilder.Clear();
                return;
            }
            catch (Exception ex)
            {
                Log.Error($"Intercom::UpdateText Prefix : {ex}\n {ex.StackTrace}");
            }
        }

        public static string SpeakIntercom(IntercomDisplay __instance) => Intercom.State switch
        {
            IntercomState.Ready => VoiceChatMuteIndicator.ReceivedFlags > VcMuteFlags.None ? "<color=#ff0000>Accréditation insuffisante.</color>" : "Intercom prêt à l'emploi.",
            IntercomState.Starting => "Attendez que l'intercom soit en ligne",
            IntercomState.InUse => __instance._icom.BypassMode ? 
                                      $"{ExiledIntercom.Speaker?.DisplayNickname ?? "(null)"} à une diffusion prioritaire" 
                                    : $"{ExiledIntercom.Speaker?.DisplayNickname ?? "(null)"} Diffuse : {Mathf.Round(__instance._icom.RemainingTime)} seconde{(__instance._icom.RemainingTime <= 1 ? "" : "s")}",
            IntercomState.Cooldown => $"Temps avant redémarrage : {Mathf.Round(__instance._icom._cooldownTime)} seconde{(__instance._icom._cooldownTime <= 1 ? "" : "s")}",
            IntercomState.NotFound => $"Unknown",
            _ => "ERROR",
        };
    }
}

