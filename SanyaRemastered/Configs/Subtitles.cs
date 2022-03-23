using System;

namespace SanyaRemastered.Configs
{
	[Serializable]
	public class HintList
	{
		public string ExtendEnabled { get; set; } = "<color=#bbee00><size=25>《Mode d'extension activé.》</size></color>";
		public string ExtendDisabled { get; set; } = "<size=25>《Mode d'extension désactivé.》</size>";
		public string Extend079First { get; set; } = 	"<color=#bbee00><size=25>《Vous pouvez utiliser vos raccourcis pour vous téléporter à un SCP ou activer un mode étendue de SCP-079.》</size></color>";
		public string Extend079Lv2 { get; set; } = "<color=#bbee00><size=25>《Vous êtes arrivé au niveau 2. En mode d'extension.》</size></color>";
		public string Extend079Lv3 { get; set; } = "<color=#bbee00><size=25>《Vous êtes arrivé au niveau 3. En mode d'extension.》</size></color>";
		public string Extend079Lv4 { get; set; } = "<color=#bbee00><size=25>《Vous êtes arrivé au niveau 4.》</size></color>";
		public string Extend079Lv5 { get; set; } = "<color=#bbee00><size=25>《Vous êtes arrivé au niveau 5.》</size></color>";
		public string Extend079NoEnergy { get; set; } = "<color=#bbee00><size=25>《Vous avez pas assez d'energie.》</size></color>";
		public string Extend079NoLevel { get; set; } = "<color=#bbee00><size=25>《Vous avez pas le niveau requis.》</size></color>";
		public string Extend079SuccessBlackoutIntercom { get; set; } = "<color=#bbee00><size=25>《Succée du blackout.》</size></color>";
		public string Extend079GazCooldown { get; set; } = "<color=#bbee00><size=25>《Vous devez attendre {0} seconde{s}》</size></color>";
		public string Extend079GazFail { get; set; } = "<color=#bbee00><size=25>《Le gaz n'est pas disponible pour cette salle.》</size></color>";
		public string Extend079SuccessGaz { get; set; } = "<color=#bbee00><size=25>《Gazage éffectué avec succée.》</size></color>";
		public string DeadBy079Gas { get; set; } = "Vous étes mort par le gazage de SCP-079";
	}
	[Serializable]
	public class CustomSubtitles
	{
		public string AirbombStarting { get; set; } = "Danger ! La sécurité du site est compromise, lancement d'un bombardement sur l'ensemble du site, \nle bombardement est inévitable !";
		public string AirbombStartingWait30s { get; set; } = "Danger ! La surface du site va recevoir un bombardement aérien dans t-moins 30 seconds !";
		public string AirbombStartingWaitMinutes { get; set; } = "Danger ! La surface du site va recevoir un bombardement aérien dans t-moins {0} minutes !";
		public string AirbombStop { get; set; } = "Le bombardement aérien du site a été annulé !";
		public string AirbombEnded { get; set; } = "Le bombardement de l'ensemble du site à été effectué et s'est terminé avec succès !";
	}
}