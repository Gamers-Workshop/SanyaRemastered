using HarmonyLib;
using LightContainmentZoneDecontamination;
using System;
using UnityEngine;
using Respawning;
using Exiled.API.Features;

namespace SanyaRemastered.Patches
{
	[HarmonyPatch(typeof(Intercom), nameof(Intercom.UpdateText))]

	class IntercomUpdateTextPatches
	{
		public static float time = 67.50499f;
		public static bool draw = true;
		public static void Prefix(Intercom __instance)
		{	
		if (!SanyaRemastered.Instance.Config.IntercomInformation) return;
		{
			if (true)
			{

				int leftdecont = (int)Math.Truncate((DecontaminationController.Singleton.DecontaminationPhases[DecontaminationController.Singleton.DecontaminationPhases.Length - 1].TimeTrigger) - Math.Truncate(DecontaminationController.GetServerTime));
				int respawntime = (int)Math.Truncate(RespawnManager.CurrentSequence() == RespawnManager.RespawnSequencePhase.RespawnCooldown ? RespawnManager.Singleton._timeForNextSequence - RespawnManager.Singleton._stopwatch.Elapsed.TotalSeconds : 0);
				int TimeWarhead = (int)Math.Truncate(AlphaWarheadOutsitePanel._host.timeToDetonation);
				bool isContain = PlayerManager.localPlayer.GetComponent<CharacterClassManager>()._lureSpj.NetworkallowContain;
				float TimeContained = 0f;

				leftdecont = Mathf.Clamp(leftdecont, 0, leftdecont);

				float totalvoltagefloat = 0f;
				foreach (var i in Generator079.Generators)
				{
					totalvoltagefloat += i.localVoltage;
				}
				totalvoltagefloat *= 1000f;
				
				foreach (Generator079 gen in Generator079.Generators)
				{
					TimeContained += gen.NetworkremainingPowerup;
				}

				string contentfix = string.Concat(
								$"<color=#fffffff>──── Centre d'information FIM Epsilon-11 ────</color>\n",
								$"Durée de la brèche : {RoundSummary.roundTime / 60:00}:{RoundSummary.roundTime % 60:00}\n",
								$"SCP restants : {RoundSummary.singleton.CountTeam(Team.SCP) - RoundSummary.singleton.CountRole(RoleType.Scp0492):00}/{RoundSummary.singleton.classlistStart.scps_except_zombies:00}\n",
								$"Classe-D restants : {RoundSummary.singleton.CountTeam(Team.CDP):00}/{RoundSummary.singleton.classlistStart.class_ds:00}\n",
								$"Scientifique restants : {RoundSummary.singleton.CountTeam(Team.RSC):00}/{RoundSummary.singleton.classlistStart.scientists:00}\n",
								$"Nine-Tailed Fox restants : {RoundSummary.singleton.CountTeam(Team.MTF):00}\n"
								);
				//SCP-106 Femur
				if (isContain)
				{
					if (OneOhSixContainer.used)
					{
						contentfix += string.Concat($"Statut du briseur de fémur : <color=#228B22>Utilisé</color>\n");
					}
					else
					{
						contentfix += string.Concat($"<color=#ff0000>Statut du briseur de fémur : Prêt</color>\n");
					}
				}
				else
				{
					contentfix += string.Concat($"Statut du briseur de fémur : Vide\n");
				}

				//warhead

				if (TimeWarhead <= 10)
				{
					contentfix += string.Concat($"<color=#ff0000>Détonation inévitable : {TimeWarhead / 60}:{TimeWarhead % 60:00}</color>\n");
				}
				else if (AlphaWarheadOutsitePanel._host.inProgress)
				{
					contentfix += string.Concat($"<color=#ff0000>Explosion de l'Alpha Warhead : {TimeWarhead / 60}:{TimeWarhead % 60:00}</color>\n");
				}
				else
				{
					if (!AlphaWarheadOutsitePanel.nukeside.Networkenabled)
					{
						contentfix += string.Concat($"Statut de l'Alpha Warhead : DÉSACTIVÉE\n");
					}
					else 
					{
						contentfix += string.Concat($"Statut de l'Alpha Warhead : PRÊTE\n");
					}
				}

				//Générateur 079

				if (TimeContained == 0 && IntercomUpdateTextPatches.draw == false)
				{ 
					if (time == 0f)
					{
						draw = true;
					}
					else
					{ 
						IntercomUpdateTextPatches.time -= Time.deltaTime;
						contentfix += string.Concat($"<color=#ff0000>Surcharge du site : {time / 60}:{time % 60}</color>\n");
					}
				}
				else
				{
					IntercomUpdateTextPatches.time = 69.50499f;
				}

				if (totalvoltagefloat == 5000 && IntercomUpdateTextPatches.draw == true)
				{

					contentfix += string.Concat("Tous les générateurs du site sont activés\n");
					IntercomUpdateTextPatches.draw = false;
				}
				else if (totalvoltagefloat != 5000)
				{
					contentfix += string.Concat($"Puissance des générateurs : {totalvoltagefloat:0000}KVA\n");
				}

				//décontamination
				if (!DecontaminationController.Singleton._decontaminationBegun)
				{
					{
						contentfix += string.Concat($"La décontamination de la LCZ vas étre effectué\n");
					}
					if (leftdecont >= 30)
						{
							contentfix += string.Concat($"Temps restant avant la décontamination de la LCZ :  {leftdecont / 60:00}:{leftdecont % 60:00}\n");
						}
					else if (leftdecont >= 15)
						{
							contentfix += string.Concat($"<color=#ff0000>Temps restant avant la décontamination de la LCZ : {leftdecont / 60:00}:{leftdecont % 60:00}</color>\n");
						}
				}
				else if (DecontaminationController.Singleton._decontaminationBegun)
				{
					contentfix += string.Concat($"La décontamination de la LCZ a été effectué\n");
				}

				//Prochain spawn + durée MTF

				if (RespawnManager.Singleton.NextKnownTeam == SpawnableTeamType.ChaosInsurgency)
					contentfix += string.Concat($"Les renforts se préparent\n");
				else if (RespawnTickets.Singleton.GetAvailableTickets(SpawnableTeamType.NineTailedFox) <= 0)
					contentfix += string.Concat($"Aucun renforts prévus pour le site\n");
				else
					contentfix += string.Concat($"Prochains renforts : {respawntime / 60:00}:{respawntime % 60:00}\n");

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
						contentfix +=  $"{ReferenceHub.GetHub(__instance.Networkspeaker).nicknameSync._myNickSync} à une diffusion prioritaire";
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
			}
		}
	}
}

