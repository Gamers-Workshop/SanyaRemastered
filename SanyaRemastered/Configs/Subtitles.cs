using System;

namespace SanyaRemastered.Configs
{
	[Serializable]
	public sealed class HintList
	{
		public string NoProfileKickMessage = "Votre SteamID n'a pas de profil créé. Ce serveur n'autorise pas les utilisateurs sans profil.\nVenez sur le serveur discord 'https://discord.gg/ZsyeBED'pour avoir plus de détails.";
		public string LimitedKickMessage = "Vous avez été Kick, venez sur le serveur Discord pour être whiteList : https://discord.gg/ZsyeBED";
		public string ExtendEnabled = "<color=#bbee00><size=25>《Mode d'extension activé.》</size></color>";
		public string ExtendDisabled = "<size=25>《Mode d'extension désactivé.》\n</size>";
		public string Extend079First = 	"<color=#bbee00><size=25>《Vous pouvez utiliser vos raccourcis pour vous téléporter à un SCP ou activer un mode étendue de SCP-079.》</size></color>";
		public string Extend079Lv2 = "<color=#bbee00><size=25>《Vous êtes arrivé au niveau 2. En mode d'extension.》</size></color>";
		public string Extend079Lv3 = "<color=#bbee00><size=25>《Vous êtes arrivé au niveau 3. En mode d'extension.》</size></color>";
		public string Extend079Lv4 = "<color=#bbee00><size=25>《Vous êtes arrivé au niveau 4.》</size></color>";
		public string Extend079Lv5 = "<color=#bbee00><size=25>《Vous êtes arrivé au niveau 5.》</size></color>";
		public string Extend079NoEnergy = "<color=#bbee00><size=25>《Vous avez pas assez d'energie.》</size></color>";
		public string Extend079NoLevel = "<color=#bbee00><size=25>《Vous avez pas le niveau requis.》</size></color>";
		public string Extend079SuccessBlackoutIntercom = "<color=#bbee00><size=25>《Succée du blackout.》</size></color>";
		public string Extend079GazCooldown = "<color=#bbee00><size=25>《Vous devez attendre {0} seconde{s}》</size></color>";
		public string Extend079GazFail = "<color=#bbee00><size=25>《Le gaz n'est pas disponible pour cette salle.》</size></color>";
		public string Extend079SuccessGaz = "<color=#bbee00><size=25>《Gazage éffectué avec succée.》\n</size></color>";
		public string ExtendGazWarn = "<color=#bbee00><size=25>《Gazage de la salle dans {1} second{s}》</size></color>";
		public string ExtendGazActive = "<color=#ff0000><size=25>《Gazage en cours.》</size></color>";
	}
	[Serializable]
	public sealed class CustomSubtitles
	{
		public string AirbombStarting = "Danger ! La sécurité du site est compromise, lancement d'un bombardement sur l'ensemble du site, \nle bombardement est inévitable !";
		public string AirbombStartingWait30s = "Danger ! La surface du site va recevoir un bombardement aérien dans t-moins 30 seconds !";
		public string AirbombStartingWaitMinutes = "Danger ! La surface du site va recevoir un bombardement aérien dans t-moins {0} minutes !";
		public string AirbombStop = "Le bombardement aérien du site a été annulé !";
		public string AirbombEnded = "Le bombardement de l'ensemble du site à été effectué et s'est terminé avec succès !";
	}
}