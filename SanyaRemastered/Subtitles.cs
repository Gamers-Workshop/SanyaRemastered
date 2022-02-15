namespace SanyaRemastered.Data
{

	internal static class HintList
	{
		internal static readonly string NoProfileKickMessage = string.Concat(
			"Votre SteamID n'a pas de profil créé. Ce serveur n'autorise pas les utilisateurs sans profil.",
			"Venez sur le serveur discord pour avoir plus de détails."
			);
		internal static readonly string LimitedKickMessage = string.Concat(
			"Vous avez été Kick, venez sur le serveur Discord pour être whiteList : https://discord.gg/ZsyeBED"
			);

		internal static readonly string ExtendEnabled = string.Concat(
			"<color=#bbee00><size=25>",
			"《Mode d'extension activé.》",
			"</size></color>"
			);

		internal static readonly string ExtendDisabled = string.Concat(
			"<size=25>",
			"《Mode d'extension désactivé.》\n",
			"</size>"
			);

		internal static readonly string Extend079First = string.Concat(
			"<color=#bbee00><size=25>",
			"《Vous pouvez utiliser vos raccourcis pour vous téléporter à un SCP ou activer un mode étendue de SCP-079.》",
			"</size></color>"
			);

		internal static readonly string Extend079Lv2 = string.Concat(
			"<color=#bbee00><size=25>",
			"《Vous êtes arrivé au niveau 2. En mode d'extension.》",
			"</size></color>"
			);

		internal static readonly string Extend079Lv3 = string.Concat(
			"<color=#bbee00><size=25>",
			"《Vous êtes arrivé au niveau 3. En mode d'extension.》",
			"</size></color>"
			);

		internal static readonly string Extend079Lv4 = string.Concat(
			"<color=#bbee00><size=25>",
			"《Vous êtes arrivé au niveau 4. En mode d'extension.》",
			"</size></color>"
			);

		internal static readonly string Extend079Lv5 = string.Concat(
			"<color=#bbee00><size=25>",
			"《Vous êtes arrivé au niveau 5.》",
			"</size></color>"
			);
		internal static readonly string Extend079NoEnergy = string.Concat(
			"<color=#bbee00><size=25>",
			"《Vous avez pas assez d'energie.》",
			"</size></color>"
			);
		internal static readonly string Extend079NoLevel = string.Concat(
			"<color=#bbee00><size=25>",
			"《Vous avez pas le niveau requis.》",
			"</size></color>"
			);
		internal static readonly string Extend079SuccessBlackoutIntercom = string.Concat(
			"<color=#bbee00><size=25>",
			"《Succée du blackout.》",
			"</size></color>"
			);

		internal static readonly string Extend079GazCooldown = string.Concat(
			"<color=#bbee00><size=25>",
			"《Vous devez attendre {0} seconde{s}》",
			"</size></color>"
			);
		internal static readonly string Extend079GazFail = string.Concat(
			"<color=#bbee00><size=25>",
			"《Le gaz n'est pas disponible pour cette salle.》",
			"</size></color>"
			);
		internal static readonly string Extend079SuccessGaz = string.Concat(
			"<color=#bbee00><size=25>",
			"《Gazage en cours.》\n",
			"</size></color>"
			);
		internal static readonly string ExtendGazWarn = string.Concat(
			"<color=#bbee00><size=25>",
			"《Gazage de la salle dans {1} second{s}》",
			"</size></color>"
			);
		internal static readonly string ExtendGazActive = string.Concat(
			"<color=#ff0000><size=25>",
			"《Gazage en cours.》\n",
			"</size></color>"
			);
	}
	internal static class CustomSubtitles
	{
		internal static readonly string AirbombStarting = "Danger ! La sécurité du site est compromise, lancement d'un bombardement sur l'ensemble du site, \nle bombardement est inévitable !";
		internal static readonly string AirbombStartingWait30s = "Danger ! La surface du site va recevoir un bombardement aérien dans t-moins 30 seconds !";
		internal static readonly string AirbombStartingWaitMinutes = "Danger ! La surface du site va recevoir un bombardement aérien dans t-moins {0} minutes !";
		internal static readonly string AirbombStop = "Le bombardement aérien du site a été annulé !";
		internal static readonly string AirbombEnded = "Le bombardement de l'ensemble du site à été effectué et s'est terminé avec succès !";
	}
}