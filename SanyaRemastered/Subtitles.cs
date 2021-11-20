namespace SanyaRemastered.Data
{
	internal static class Subtitles
	{

		internal static readonly string NoProfileKickMessage = string.Concat(
			"Votre SteamID n'a pas de profil créé. Ce serveur n'autorise pas les utilisateurs sans profil. \n",
			"Venez sur le serveur discord pour avoir plus de détails."
			);
		internal static readonly string LimitedKickMessage = string.Concat(
			"Vous avez été Kick, venez sur le serveur Discord pour être whiteList : https://discord.gg/ZsyeBED"
			);

		internal static readonly string ExtendEnabled = string.Concat(
			"<color=#bbee00><size=25>",
			"《Mode d'extension activé.》\n",
			"</size></color>"
			);

		internal static readonly string ExtendDisabled = string.Concat(
			"<size=25>",
			"《Mode d'extension désactivé.》\n",
			"</size>"
			);

		internal static readonly string Extend079First = string.Concat(
			"<color=#bbee00><size=25>",
			"《Vous pouvez utiliser vos raccourcis pour vous téléporter à un SCP ou activer un mode étendue de SCP-079.》\n",
			"</size></color>"
			);

		internal static readonly string Extend079Lv2 = string.Concat(
			"<color=#bbee00><size=25>",
			"《Vous êtes arrivé au niveau 2. En mode d'extension.》\n",
			"</size></color>"
			);

		internal static readonly string Extend079Lv3 = string.Concat(
			"<color=#bbee00><size=25>",
			"《Vous êtes arrivé au niveau 3. En mode d'extension.》\n",
			"</size></color>"
			);

		internal static readonly string Extend079Lv4 = string.Concat(
			"<color=#bbee00><size=25>",
			"《Vous êtes arrivé au niveau 4. En mode d'extension.》\n",
			"</size></color>"
			);

		internal static readonly string Extend079Lv5 = string.Concat(
			"<color=#bbee00><size=25>",
			"《Vous êtes arrivé au niveau 5.》\n",
			"</size></color>"
			);
		internal static readonly string Extend079NoEnergy = string.Concat(
			"<color=#bbee00><size=25>",
			"《Vous avez pas assez d'energie.》\n",
			"</size></color>"
			);
		internal static readonly string Extend079NoLevel = string.Concat(
			"<color=#bbee00><size=25>",
			"《Vous avez pas le niveau requis.》\n",
			"</size></color>"
			);
		internal static readonly string Extend079SuccessBlackoutIntercom = string.Concat(
			"<color=#bbee00><size=25>",
			"《Succée du blackout.》\n",
			"</size></color>"
			);

		internal static readonly string Extend079GazCooldown = string.Concat(
			"<color=#bbee00><size=25>",
			"《Vous devez attendre {0} seconde{s}》\n",
			"</size></color>"
			);
		internal static readonly string Extend079GazFail = string.Concat(
			"<color=#bbee00><size=25>",
			"《Le gaz n'est pas disponible pour cette salle.》\n",
			"</size></color>"
			);
		internal static readonly string Extend079SuccessGaz = string.Concat(
			"<color=#bbee00><size=25>",
			"《Gazage en cours.》\n",
			"</size></color>"
			);
		internal static readonly string ExtendGazWarn = string.Concat(
			"<color=#bbee00><size=25>",
			"《Gazage de la salle dans {1} second{s}》\n",
			"</size></color>"
		);
		internal static readonly string ExtendGazActive = string.Concat(
			"<color=#ff0000><size=25>",
			"《Gazage en cours.》\n",
			"</size></color>"
		);
		internal static readonly string MTFRespawnSCPs = string.Concat(
			"<color=#6c80ff><size=25>",
			"《L'unité mobile d'intervention, Epsilon-11, désignée,「{0}」, est entrée dans la fondation.\n Tout le personnel restant est conseillé de suivre les protocoles d'évacuation standards jusqu'à ce qu'une équipe MTF atteigne sa destination dans le site : Il reste sur site {1} SCP.》\n",
			"</size></color>"
		);

		internal static readonly string MTFRespawnNOSCPs = string.Concat(
			"<color=#6c80ff><size=25>",
			"《L'unité mobile d'intervention, Epsilon-11, désignée,「{0}」, est entrée dans la fondation.\n Tout le personnel restant est conseillé de suivre les protocoles d'évacuation standards jusqu'à ce qu'une équipe MTF atteigne sa destination dans le site.》\n",
			"</size></color>"
		);

		internal static readonly string SCPDeathTesla = string.Concat(
			"<color=#ff0000><size=25>",
			"《{0} a été éliminé avec succès par le système de sécurité automatique. {-1}》\n",
			"</size></color>"
		);

		internal static readonly string SCPDeathDecont = string.Concat(
			"<color=#ff0000><size=25>",
			"《{0} a été décontaminé. {-1}》\n",
			"</size></color>"
		);

		internal static readonly string SCPDeathWarhead = string.Concat(
			"<color=#ff0000><size=25>",
			"《{0} a été anéantit par une ogive alpha. {-1}》\n",
			"</size></color>"
		);

		internal static readonly string SCPDeathTerminated = string.Concat(
			"<color=#ff0000><size=25>",
			"《{0} a été éliminé par {1} {-1}》\n",
			"</size></color>"
		);

		internal static readonly string SCPDeathContainedMTF = string.Concat(
			"<color=#ff0000><size=25>",
			"《{0} vient d'être re-confiné avec succès par l'unité de confinement:{1}.{-1}》\n",
			"</size></color>"
		);

		internal static readonly string SCPDeathUnknown = string.Concat(
			"<color=#ff0000><size=25>",
			"《{0} a été éliminé avec succès. Cause du re-confinement inconnu.{-1}》\n",
			"</size></color>"
		);

		internal static readonly string StartNightMode = string.Concat(
			"<color=#ff0000><size=25>",
			"《Attention, le système électrique de l'installation a été attaqué. \nLa lumière de la plupart des zones de confinement n'est plus disponible tant que le générateur ne sera pas ré-activé.》\n",
			"</size></color>"
		);

		internal static readonly string DecontaminationInit = string.Concat(
			"<color=#bbee00><size=25>",
			"《Votre attention, à tout le personnel. Le processus de décontamination de la Light Containment Zone se déroulera dans moins de 15 minutes. \nToutes les substances biologiques doivent évacuées afin d'éviter la décontamination.》\n",
			"</size></color>"
		);

		internal static readonly string DecontaminationMinutesCount = string.Concat(
			"<color=#bbee00><size=25>",
			"《Danger ! Décontamination de la Light Containment Zone dans moins de {0} minutes.》\n",
			"</size></color>"
		);
		internal static readonly string Decontamination30s = string.Concat(
			"<color=#ff0000><size=25>",
			"《Danger ! Décontamination de la Light Containment Zone dans moins de 30 secondes. Toutes les portes et points de contrôle ont été ouverts. Veuillez évacuer immédiatement.》\n",
			"</size></color>"
		);
		internal static readonly string DecontaminationLockdown = string.Concat(
			"<color=#bbee00><size=25>",
			"《La Light Containment Zone est verrouillée et prête pour la décontamination. L'élimination des substances organiques a maintenant débuté.》\n",
			"</size></color>"
		);

		internal static readonly string GeneratorFinish = string.Concat(
			"<color=#bbee00><size=25>",
			"《{0} générateur{s} sur 3 activé{s}.》\n",
			"</size></color>"
		);

		internal static readonly string GeneratorComplete = string.Concat(
			"<color=#bbee00><size=25>",
			"《3 sur 3 générateurs sont activés. Tous les générateurs ont été engagés avec succès.》\n",
			"</size></color>"
		);
		internal static readonly string OverchargeStart = string.Concat(
			"<color=#bbee00><size=25>",
			"《Surcharge du site dans 3 2 1.》\n",
			"</size></color>"
		); 
		internal static readonly string OverchargeFinish = string.Concat(
			"<color=#bbee00><size=25>",
			"《L'installation est de nouveau opérationnelle.》\n",
			"</size></color>"
		);

		internal static readonly string AlphaWarheadStart = string.Concat(
			"<color=#ff0000><size=25>",
			"《L'ogive Alpha vient d'être activée, tout le personnel doit évacuer par les sorties les plus proches.\nLa section souterraine du site sera détruite dans t-moins {0} secondes.》\n",
			"</size></color>"
		);

		internal static readonly string AlphaWarheadResume = string.Concat(
			"<color=#ff0000><size=25>",
			"《La séquence de détonation de l'ogive Alpha vient de reprendre dû à sa réactivation. Temps restant avant la détonation : t-moins {0} secondes.》\n",
			"</size></color>"
		);

		internal static readonly string AlphaWarheadCancel = string.Concat(
			"<color=#ff0000><size=25>",
			"《La détonation a été annulé. Le redémarrage des systèmes a été accomplit.》\n",
			"</size></color>"
		);

		internal static readonly string AirbombStarting = string.Concat(
			"<color=#ff0000><size=25>",
			"《Danger ! La sécurité du site est compromise, lancement d'un bombardement sur l'ensemble du site, \n le bombardement est inévitable !》\n",
			"</size></color>"
		);
		internal static readonly string AirbombStartingWait30s = string.Concat(
			"<color=#ff0000><size=25>",
			"《Danger ! La surface du site va recevoir un bombardement aérien dans t-moins 30 seconds !》\n",
			"</size></color>"
		);
		internal static readonly string AirbombStartingWaitMinutes = string.Concat(
			"<color=#ff0000><size=25>",
			"《Danger ! La surface du site va recevoir un bombardement aérien dans t-moins {0} minutes !》\n",
			"</size></color>"
		);
		internal static readonly string AirbombStop = string.Concat(
			"<color=#ff0000><size=25>",
			"《Le bombardement aérien du site a été annulé !》\n",
			"</size></color>"
		);
		internal static readonly string AirbombEnded = string.Concat(
			"<color=#ff0000><size=25>",
			"《Le bombardement de l'ensemble du site à été effectué et s'est terminé avec succès !》\n",
			"</size></color>"
		);
		internal static readonly string DecontEvent = string.Concat(
			"<color=#bbee00><size=25>",
			"《Le protocole de décontamination de la Light Containment Zone a commencé. Les substances organiques dans la zone seront décontaminées.》\n",
			"</size></color>"
		);
	}											
}