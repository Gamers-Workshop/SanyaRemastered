using HarmonyLib;
using LightContainmentZoneDecontamination;
using System;
using UnityEngine;
using Respawning;
using Exiled.API.Features;
using System.Linq;
using Exiled.API.Enums;

namespace SanyaRemastered.Patches
{
	[HarmonyPatch(typeof(Intercom), nameof(Intercom.UpdateText))]

	class IntercomUpdateTextPatches
	{
		public static void Prefix(Intercom __instance)
		{	
		if (!SanyaRemastered.Instance.Config.IntercomInformation) return;
			{
				Room Hcz106 = Map.Rooms.Where(x => x.Type == RoomType.Hcz106).Single();
				Room RoomIntercom = Map.Rooms.Where(x => x.Type == RoomType.EzIntercom).Single();
				if (!RoomIntercom.LightsOff && SanyaRemastered.Instance.Config.IntercomBrokenOnBlackout)
				{
					int leftdecont = (int)Math.Truncate((DecontaminationController.Singleton.DecontaminationPhases[DecontaminationController.Singleton.DecontaminationPhases.Length - 1].TimeTrigger) - Math.Truncate(DecontaminationController.GetServerTime));
					int respawntime = (int)Math.Truncate(RespawnManager.CurrentSequence() == RespawnManager.RespawnSequencePhase.RespawnCooldown ? RespawnManager.Singleton._timeForNextSequence - RespawnManager.Singleton._stopwatch.Elapsed.TotalSeconds : 0);
					int TimeWarhead = (int)Math.Truncate(AlphaWarheadOutsitePanel._host.timeToDetonation);
					bool isContain = PlayerManager.localPlayer.GetComponent<CharacterClassManager>()._lureSpj.NetworkallowContain;

					leftdecont = Mathf.Clamp(leftdecont, 0, leftdecont);

					float totalvoltagefloat = 0f;
					foreach (var i in Recontainer079.AllGenerators.ToList())
					{
						totalvoltagefloat += i._currentTime;
					}

					totalvoltagefloat = (int)(totalvoltagefloat * 26.666f);

					string contentfix = string.Concat(
								$"<color=#fffffff>──── Centre d'information FIM Epsilon-11 ────</color>\n",
								$"Durée de la brèche : {RoundSummary.roundTime / 60:00}:{RoundSummary.roundTime % 60:00}\n",
								$"SCP restants : {RoundSummary.singleton.CountTeam(Team.SCP) - RoundSummary.singleton.CountRole(RoleType.Scp0492):00}/{RoundSummary.singleton.classlistStart.scps_except_zombies:00}\n",
								$"Classe-D restants : {RoundSummary.singleton.CountTeam(Team.CDP):00}/{RoundSummary.singleton.classlistStart.class_ds:00}\n",
								$"Scientifique restants : {RoundSummary.singleton.CountTeam(Team.RSC):00}/{RoundSummary.singleton.classlistStart.scientists:00}\n",
								$"Nine-Tailed Fox restants : {RoundSummary.singleton.CountTeam(Team.MTF):00}\n"
								);
					//SCP-106 Femur
					if (!Hcz106.LightsOff)
						if (isContain)
						{
							if (OneOhSixContainer.used)
							{
								contentfix += $"Statut du briseur de fémur : <color=#228B22>Utilisé</color>\n";
							}
							else
							{
								contentfix += $"<color=#ff0000>Statut du briseur de fémur : Prêt</color>\n";
							}
						}
						else
						{
							contentfix += $"Statut du briseur de fémur : Vide\n";
						}
					else
					{
						contentfix += $"Statut du briseur de fémur : <color=#228B22>No Data</color>\n";
					}

					//warhead

					if (TimeWarhead <= 10)
					{
						contentfix += $"<color=#ff0000>Détonation inévitable : {TimeWarhead / 60}:{TimeWarhead % 60:00}</color>\n";
					}
					else if (AlphaWarheadOutsitePanel._host.inProgress)
					{
						contentfix += $"<color=#ff0000>Explosion de l'Alpha Warhead : {TimeWarhead / 60}:{TimeWarhead % 60:00}</color>\n";
					}
					else
					{
						if (!AlphaWarheadOutsitePanel.nukeside.Networkenabled)
						{
							contentfix += $"Statut de l'Alpha Warhead : DÉSACTIVÉE\n";
						}
						else
						{
							contentfix += $"Statut de l'Alpha Warhead : PRÊTE\n";
						}
					}

					//Générateur 079
					if (Map.ActivatedGenerators == 3)
					{
						contentfix += "Tous les générateurs du site sont activés\n";
					}
					else
					{
						contentfix += $"Puissance des générateurs : {totalvoltagefloat:0000}KVA\n";
					}

					//décontamination
					if (!DecontaminationController.Singleton._decontaminationBegun)
					{
						if (leftdecont > 30)
						{
							contentfix += $"Temps restant avant la décontamination de la LCZ :  {leftdecont / 60:00}:{leftdecont % 60:00}\n";
						}
						else if (leftdecont <= 30)
						{
							contentfix += $"<color=#ff0000>Temps restant avant la décontamination de la LCZ : {leftdecont / 60:00}:{leftdecont % 60:00}</color>\n";
						}
					}
					else if (DecontaminationController.Singleton._decontaminationBegun)
					{
						contentfix += $"La décontamination de la LCZ a été effectué\n";
					}

					//Prochain spawn + durée MTF

					if (RespawnManager.Singleton.NextKnownTeam == SpawnableTeamType.ChaosInsurgency)
						contentfix += $"Les renforts se préparent\n";
					else if (RespawnTickets.Singleton.GetAvailableTickets(SpawnableTeamType.NineTailedFox) <= 0)
						contentfix += $"Aucun renforts prévus pour le site\n";
					else
						contentfix += $"Prochains renforts : {respawntime / 60:00}:{respawntime % 60:00}\n";

					//Speak Intercom
					if (__instance.Muted)
					{
						contentfix += "<color=#ff0000>Accréditation insuffisante.</color>";
					}
					else if (Intercom.AdminSpeaking)
					{
						contentfix += "<color=#ff0000>L'administrateur a la priorité sur les équipements de diffusion.</color>";
					}
					else if (__instance.remainingCooldown > 0f)
					{
						contentfix += "Temps avent redémarrage : " + Mathf.CeilToInt(__instance.remainingCooldown) + $" seconde{(__instance.remainingCooldown <= 1 ? "" : "s")} ";
					}
					else if (__instance.speechRemainingTime == -77f)
					{
						contentfix += $"{ReferenceHub.GetHub(__instance.Networkspeaker).nicknameSync._myNickSync} à une diffusion prioritaire";
					}

					else if (__instance.speaker != null)
					{
						contentfix += $"{ReferenceHub.GetHub(__instance.Networkspeaker).nicknameSync._myNickSync} Diffuse : " + Mathf.CeilToInt(__instance.speechRemainingTime) + " secondes ";
					}
					else
					{
						contentfix += "Intercom prêt à l'emploi.";
					}
					__instance.CustomContent = contentfix;

					return;
				}
				__instance.CustomContent = " ";
			}
		}
	}
}

