using HarmonyLib;
using LightContainmentZoneDecontamination;
using System;
using UnityEngine;
using Respawning;

namespace SanyaRemastered.Patches
{
	[HarmonyPatch(typeof(Intercom), nameof(Intercom.UpdateText))]

	class IntercomUpdateTextPatches
	{
		public static float time = 67.50499f;
		public static bool draw = true;
		public static bool Prefix(Intercom __instance)
		{	
		if (!SanyaPlugin.SanyaPlugin.Instance.Config.IntercomInformation) return true;
			{
				RespawnManager respawn = RespawnManager.Singleton;
				int leftdecont = (int)Math.Truncate(15f * 60 - DecontaminationController.GetServerTime);
			//	int nextRespawn = (int)Math.Truncate(RespawnManager.CurrentSequence() == RespawnManager.RespawnSequencePhase.RespawnCooldown ? RespawnManager.Singleton._timeForNextSequence : 0);
				int TimeWarhead = (int)Math.Truncate(AlphaWarheadOutsitePanel._host.timeToDetonation);
				bool isContain = PlayerManager.localPlayer.GetComponent<CharacterClassManager>()._lureSpj.NetworkallowContain;
				bool isAlreadyUsed = UnityEngine.Object.FindObjectOfType<OneOhSixContainer>().Networkused;
			//	bool SpawnCI = respawn.NextKnownTeam == SpawnableTeamType.ChaosInsurgency;
				float totalvoltagefloat = 0f;
				float TimeContained = 0f;

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
								$"SCP restants : {RoundSummary.singleton.CountTeam(Team.SCP):00}/{RoundSummary.singleton.classlistStart.scps_except_zombies:00}\n",
								$"Classe-D restants : {RoundSummary.singleton.CountTeam(Team.CDP):00}/{RoundSummary.singleton.classlistStart.class_ds:00}\n",
								$"Scientifique restants : {RoundSummary.singleton.CountTeam(Team.RSC):00}/{RoundSummary.singleton.classlistStart.scientists:00}\n",
								$"Nine-Tailed Fox restants : {RoundSummary.singleton.CountTeam(Team.MTF):00}\n"
								);
				//SCP-106 Femur
				if (isContain)
				{
					if (isAlreadyUsed)
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
				if (TimeWarhead < 10)
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
						contentfix += string.Concat($"Statut de l'Alpha Warhead : <color=#ff0000>DÉSACTIVÉE</color>\n");
					}
					else 
					{
						contentfix += string.Concat($"Statut de l'Alpha Warhead : <color=#228B22>PRÊTE</color>\n");
					}
				}

				//Générateur 

				if (TimeContained == 0 && IntercomUpdateTextPatches.draw == false)
				{ 
					string minutes = Mathf.Floor(time / 60).ToString("00");
					string seconds = (time % 60).ToString("00");
					if (minutes.StartsWith("00") && seconds.StartsWith("00"))
					{
						draw = true;
					}
					else
					{ 
					IntercomUpdateTextPatches.time -= Time.deltaTime;
					contentfix += string.Concat($"<color=#ff0000>Surcharge du site : "+minutes+":"+seconds+"</color>\n");
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

				/*if (!SpawnCI)
					contentfix += string.Concat($"Prochains renforts MTF : {nextRespawn / 60:00}:{nextRespawn % 60:00}\n");

				else if (RespawnTickets.Singleton.GetAvailableTickets(SpawnableTeamType.NineTailedFox) <= 0)
					contentfix += string.Concat($"Aucun renforts prévus pour le site\n");

				else
					contentfix += string.Concat($"Les renforts se préparent\n");*/
			
				//Voice intercom
				if (__instance.Muted)
				{
					__instance._content = contentfix + "<color=#ff0000>Accréditation insuffisante.</color>";
				}
				else if (Intercom.AdminSpeaking)
				{
					__instance._content = contentfix + "<color=#ff0000>L'administrateur a la priorité sur les équipements de diffusion.</color>";
				}
				else if (__instance.remainingCooldown > 0f)
				{
					__instance._content = contentfix + "Temps avent redémarrage : " + Mathf.CeilToInt(__instance.remainingCooldown) + " secondes ";
				}
				else if (__instance.Networkspeaker != null)
				{
					if (__instance.speechRemainingTime == -77f)
					{
						__instance._content = contentfix + $"{ReferenceHub.GetHub(__instance.Networkspeaker).nicknameSync._myNickSync} a une diffusion prioritaire";
					}
					else
					{
						__instance._content = contentfix + $"{ReferenceHub.GetHub(__instance.Networkspeaker).nicknameSync._myNickSync} Diffuse : " + Mathf.CeilToInt(__instance.speechRemainingTime) + " secondes ";
					}
				}
				else
				{
					__instance._content = contentfix + "Intercom prêt à l'emploi.";
				}
				if (__instance._contentDirty)
				{
					__instance.NetworkintercomText = __instance._content;
					__instance._contentDirty = false;
				}
				if (Intercom.AdminSpeaking != Intercom.LastState)
				{
					Intercom.LastState = Intercom.AdminSpeaking;
					__instance.RpcUpdateAdminStatus(Intercom.AdminSpeaking);
				}
				return false;
			}
		}
	}
}
