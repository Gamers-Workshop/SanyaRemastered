namespace SanyaRemastered.Data
{
	internal static class Subtitles
	{

		internal static readonly string NoProfileKickMessage = string.Concat(
			"Votre SteamID n'a pas de profil créé. Ce serveur n'autorise pas les utilisateurs sans profil. \n",
			"Venez sur le serveur discord pour avoir plus de détail"
			);
		internal static readonly string LimitedKickMessage = string.Concat(
			"Venez sur le serveur discord pour avoir plus de détail"
			);
		internal static readonly string VPNKickMessage = string.Concat(
			"Ce serveur n'accepte pas les VPN ou les Proxy\n",
			"Venez sur le serveur discord pour avoir plus de détail"
			);
		internal static readonly string VPNKickMessageShort = string.Concat(
			"Ce serveur n'accepte pas les VPN ou les Proxy.\n",
			"Venez sur le serveur discord pour avoir plus de détail"
			);

		internal static readonly string ExtendEnabled = string.Concat(
			"<color=#bbee00><size=25>",
			"《Mode d'extension activé. Vous pouvez basculer par les touches de sprint.》\n",
			"</size></color>"
			);

		internal static readonly string ExtendDisabled = string.Concat(
			"<size=25>",
			"《Mode d'extension désactivé. Vous pouvez basculer par les touches de sprint.》\n",
			"</size>"
			);

		internal static readonly string Extend079First = string.Concat(
			 "<color=#bbee00><size=25>",
			"《Vous pouvez utiliser le mode d'extension. Vous pouvez basculer par les touches de sprint.》\n",
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
		internal static readonly string Extend079GazCooldown = string.Concat(
			"<color=#bbee00><size=25>",
			"《Vous devez attendre {0}》\n",
			"</size></color>"
			);
		internal static readonly string Extend079GazFail = string.Concat(
			"<color=#bbee00><size=25>",
			"《Le gaz n'est pas disponible pour cette sale》\n",
			"</size></color>"
			);
		internal static readonly string Extend079SuccessGaz = string.Concat(
			"<color=#bbee00><size=25>",
			"《Gazage en cours》\n",
			"</size></color>"
			);
		internal static readonly string ExtendGazWarn = string.Concat(
			"<color=#bbee00><size=25>",
			"《Gazage de la salle dans {1} seconds》\n",
			"</size></color>"
		);
		internal static readonly string ExtendGazActive = string.Concat(
			"<color=#ff0000><size=25>",
			"《Gazage En Cours》\n",
			"</size></color>"
		);
		internal static readonly string Extend106First = string.Concat(
			"<color=#bbee00><size=25>",
			"WIP Extend mode available. You can toggle by sprint keys.\nIt can be created at random humans position by using [create sinkhole] in extend mode.",
			"WIP Le mode étendue de SCP-106. Vous permet de vous tp dans la salle devant vous en appuyant sur sprint et en utilisant votre tp",
			"</size></color>"
		);

		internal static readonly string Extend106RoomtNotFound = string.Concat(
			"<color=#bbee00><size=25>",
			"Aucun salle trouvé devant vous",
			"</size></color>"
		);

		internal static readonly string Extend106Success = string.Concat(
			"<color=#bbee00><size=25>",
			"Création d'un portail dans la {0} position.",
			"</size></color>"
		);

		internal static readonly string MTFRespawnSCPs = string.Concat(
			"<color=#6c80ff><size=25>",
			"《L'unité mobile d'intervention,Epsilon-11, désignée,「{0}」, est entrée dans la fondation.\n Tout le personnel restant est conseillé de suivre les protocoles d'évacuations standards jusqu'à qu'une équipe MTF atteigne sa destination dans le site : Il reste sur site {1} SCP.》\n",
			"</size></color>"
		);

		internal static readonly string MTFRespawnNOSCPs = string.Concat(
			"<color=#6c80ff><size=25>",
			"《L'unité mobile d'intervention, Epsilon-11, désignée, 「{0}」, est entrée dans la fondation.\n Tout le personnel restant est conseillé de suivre les protocoles d'évacuation standard, jusqu'à ce que l'équipe MTF ait atteint sa destination.》\n",
			"</size></color>"
		);

		internal static readonly string SCPDeathTesla = string.Concat(
			"<color=#ff0000><size=25>",
			"《{0} éliminé avec succès par le système de sécurité automatique. {-1}》\n",
			"</size></color>"
		);

		internal static readonly string SCPDeathDecont = string.Concat(
			"<color=#ff0000><size=25>",
			"《{0} à était décontaminé. {-1}》\n",
			"</size></color>"
		);

		internal static readonly string SCPDeathWarhead = string.Concat(
			"<color=#ff0000><size=25>",
			"《{0} anéanti par une ogive alpha. {-1}》\n",
			"</size></color>"
		);

		internal static readonly string SCPDeathTerminated = string.Concat(
			"<color=#ff0000><size=25>",
			"《{0} {-1} a été éliminé par {1} 》\n",
			"</size></color>"
		);

		internal static readonly string SCPDeathContainedMTF = string.Concat(
			"<color=#ff0000><size=25>",
			"《{0} vient d'être re-confiné avec succès par l'unité de confinement:{1}.{-1}》\n",
			"</size></color>"
		);

		internal static readonly string SCPDeathUnknown = string.Concat(
			"<color=#ff0000><size=25>",
			"《{0} fut éliminé avec succès. Cause du re-confinement inconnu.{-1}》\n",
			"</size></color>"
		);

		internal static readonly string StartNightMode = string.Concat(
			"<color=#ff0000><size=25>",
			"《Avertissement, le système électrique de l'installation a été attaqué. \nLa lumière de la plupart des zones de confinement n'est plus disponible tant que le générateur n'est pas activé.》\n",
			"</size></color>"
		);

		internal static readonly string DecontaminationInit = string.Concat(
			"<color=#bbee00><size=25>",
			"《Votre attention, à tout le personnel. Le processus de décontamination de la Light Containement Zone se déroulera dans moins de 15 minutes. \nToutes les substances biologiques doivent évacué afin d'éviter la décontamination.》\n",
			"</size></color>"
		);

		internal static readonly string DecontaminationMinutesCount = string.Concat(
			"<color=#bbee00><size=25>",
			"《Danger ! Décontamination de la Light Containement Zone dans moins de {0} minutes》\n",
			"</size></color>"
		);
		internal static readonly string Decontamination30s = string.Concat(
			"<color=#ff0000><size=25>",
			"《Danger, décontamination de la Light Containment Zone dans moins de 30 secondes. Toutes les portes et points de contrôle ont été ouvertes. Veuillez évacuer immédiatement.》\n",
			"</size></color>"
		);
		internal static readonly string DecontaminationLockdown = string.Concat(
			"<color=#bbee00><size=25>",
			"《La Light Containement Zone est verrouillée et prête pour la décontamination. L'élimination des substances organiques à maintenant débuté.》\n",
			"</size></color>"
		);

		internal static readonly string GeneratorFinish = string.Concat(
			"<color=#bbee00><size=25>",
			"《{0} générateurs sur 5 activés.》\n",
			"</size></color>"
		);

		internal static readonly string GeneratorComplete = string.Concat(
			"<color=#bbee00><size=25>",
			"《5 des 5 générateurs sont activés.\nTous les générateurs ont été engagés avec succès.Finalisation de la séquence de confinement. La Heavy Containement sera surchargée dans t-moins 1 minute.》\n",
			"</size></color>"
		);

		internal static readonly string AlphaWarheadStart = string.Concat(
			"<color=#ff0000><size=25>",
			"《L'Alpha Warhead vient d'être enclenchée, tout le personnel doit évacuer par les sorties les plus proches.\nLa section souterraine du site sera anéanti dans t-moins {0} secondes.》\n",
			"</size></color>"
		);

		internal static readonly string AlphaWarheadResume = string.Concat(
			"<color=#ff0000><size=25>",
			"《La séquence de détonation vient de reprendre dû à sa réactivation. Temps restant t-moins {0} secondes.》\n",
			"</size></color>"
		);

		internal static readonly string AlphaWarheadCancel = string.Concat(
			"<color=#ff0000><size=25>",
			"《La détonation à été annulée. Le redémarrage des systèmes à été accompli.》\n",
			"</size></color>"
		);

		internal static readonly string AirbombStarting = string.Concat(
			"<color=#ff0000><size=25>",
			"《Danger ! Le site est compromis, lancement d'un bombardement sur lancement du site, \n le bombardement est inévitable !》\n",
			"</size></color>"
		);
		internal static readonly string AirbombStartingWait30s = string.Concat(
			"<color=#ff0000><size=25>",
			"《Danger ! Le site vas recevoir un bombardement aérien dans t-moins 30 seconds !》\n",
			"</size></color>"
		);
		internal static readonly string AirbombStartingWaitMinutes = string.Concat(
			"<color=#ff0000><size=25>",
			"《Danger ! Le site vas recevoir un bombardement aérien dans t-moins {0} minutes !》\n",
			"</size></color>"
		);
		internal static readonly string AirbombStop = string.Concat(
			"<color=#ff0000><size=25>",
			"《Le bombardement a été annulé !》\n",
			"</size></color>"
		);
		internal static readonly string AirbombEnded = string.Concat(
			"<color=#ff0000><size=25>",
			"《Le bombardement de l'ensemble du site à été effectué et c'est terminé avec succès !》\n",
			"</size></color>"
		);

		internal static readonly string DecontEvent = string.Concat(
			"<color=#bbee00><size=25>",
			"《Le protocole de décontamination de la Light Containement Zone a commencé. Les objets SCP dans la zone seront décontaminés.》\n",
			"</size></color>"
		);

    /*    internal static readonly string SCPEscapeEvent = string.Concat(
            "<color=#bbee00><size=25>",
            "《Le {0} s'est échappé de la fondation.》\n",
            "</size></color>"
		);
	*/	
    }											
}