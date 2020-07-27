using Exiled.API.Features;
using Exiled.API.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;

namespace SanyaRemastered
{
	public sealed class Configs : IConfig
	{
		public Configs()
		{
			DataDirectory = Path.Combine(Paths.Plugins, "SanyaPlugin");
		}
		[Description("Activation du SanyaPlugin")]
		public bool IsEnabled { get; set; } = true;

		[Description("WIP")]
		public bool Scp106PortalExtensionEnabled { get; set; } = false;
		public bool Scp079_spot { get; set; } = false;
		public float PainEffectStart { get; set; } = 20f;
		public bool Scp939CanSeeVoiceChatting { get; set; } = false;

		[Description("Activation des données des joueurs")]
		public bool DataEnabled { get; set; } = false;

		[Description("Localisation des données des joueurs")]
		public string DataDirectory { get; private set; } = string.Empty;

		[Description("Informations sur le serveur Adresse IP de destination")]
		public string InfosenderIp { get; set; } = "none";

		[Description("Port UDP vers lequel les informations du serveur sont envoyées")]
		public int InfosenderPort { get; set; } = -1;

		[Description("Liens Webhook discord")]
		public string ReportWebhook { get; set; } = String.Empty;

		[Description("Config Player level")]
		public bool LevelEnabled { get; set; } = false;
		public int LevelExpKill { get; set; } = 3;
		public int LevelExpDeath { get; set; } = 1;
		public int LevelExpWin { get; set; } = 10;
		public int LevelExpLose { get; set; } = 1;
		public int Level_exp_other { get; set; } = 0;

		[Description("Event Du Sanya")]
		public List<int> EventModeWeight { get; set; } = new List<int>() { 0, 0, 0 };
		public List<ItemType> ClassdInsurgentInventoryClassd { get; set; } = new List<ItemType>();
		public List<ItemType> ClassdInsurgentInventoryScientist { get; set; } = new List<ItemType>();

		[Description("Tesla Config")]
		public float TeslaRange { get; set; } = 5.5f;
		public List<Team> TeslaTriggerableTeams { get; set; } = new List<Team>();
		public bool TeslaTriggerableDisarmed { get; set; } = false;

		[Description("ItemCleanup Config")]
		public int ItemCleanup { get; set; } = -1;
		public List<ItemType> ItemCleanupIgnore { get; set; } = new List<ItemType>();

		[Description("Kick Player")]
		public bool KickSteamLimited { get; set; } = false;
		public bool KickVpn { get; set; } = false;

		[Description("Message de Bienvenue")]
		public string MotdMessage { get; set; } = string.Empty;
		
		[Description("Generator Config")]
		public bool GeneratorUnlockOpen { get; set; } = false;
		public bool GeneratorFinishLock { get; set; } = true;
		public bool GeneratorActivatingClose { get; set; } = true;
		
		[Description("Game Config")]
		public bool Item_shoot_move { get; set; } = true;
		public bool Grenade_shoot_fuse { get; set; } = true;
		public int OutsidezoneTerminationTimeAfterNuke { get; set; } = -1;
		public bool StopRespawnAfterDetonated { get; set; } = true;
		public bool GodmodeAfterEndround { get; set; } = true;
		public bool InventoryKeycardActivation { get; set; } = true;
		public bool CassieSubtitle { get; set; } = true;
		public bool IntercomInformation { get; set; } = true;
		public bool CloseDoorsOnNukecancel { get; set; } = true;
		public bool Scp049_add_time_res_success { get; set; } = true;
		public bool Scp049_2DontOpenDoorAnd106 { get; set; } = true;
		public bool Scp939And096DontOpenlockerAndGenerator { get; set; } = true;
		public bool GrenadeEffect { get; set; } = true;

		[Description("Corosion sur les tache de SCP-106 WIP")]
		public bool Coroding106 { get; set; } = true;
		[Description("SCP-914 = Effect")]
		public bool Scp914Effect { get; set; } = true;

		[Description("ClassD Contain")]
		public bool ClassD_container_locked { get; set; } = true;
		public float ClassD_container_Unlocked { get; set; } = 10f;

		[Description("Disable Chat")]
		public bool DisableAllChat { get; set; } = false;
		public bool DisableSpectatorChat { get; set; } = false;
		public bool DisableChatBypassWhitelist { get; set; } = false;

		[Description("Stamina Add")]
		public bool StaminaEffect { get; set; } = true;
		public float StaminaLostJump { get; set; } = 0.05f;
		public float StaminaLostLogicer { get; set; } = 0.001f;

		[Description("Hitmark Add")]
		public bool HitmarkGrenade { get; set; } = true;
		public bool HitmarkKilled { get; set; } = true;

		[Description("Dégats Usp")]
		public int TraitorLimit { get; set; } = -1;
		public int TraitorChancePercent { get; set; } = 50;

		/*
				[Description("各ロールの初期装備")]
				public Dictionary<RoleType, List<ItemType>> Defaultitems { get; set; } = new Dictionary<RoleType, List<ItemType>>()
				{
					{ RoleType.ClassD, new List<ItemType>() },
					{ RoleType.Scientist, new List<ItemType>() },
					{ RoleType.FacilityGuard, new List<ItemType>() },
					{ RoleType.NtfCadet, new List<ItemType>() },
					{ RoleType.NtfLieutenant, new List<ItemType>() },
					{ RoleType.NtfCommander, new List<ItemType>() },
					{ RoleType.NtfScientist,new List<ItemType>() },
					{ RoleType.ChaosInsurgency, new List<ItemType>() },
					{ RoleType.Tutorial, new List<ItemType>() }
				};*/
		[Description("Dégats Usp")]
		public float UspDamageMultiplierHuman { get; set; } = 1f;
		public float UspDamageMultiplierScp { get; set; } = 1f;

		[Description("Division des dommages quand la personne est désarmé")]
		public float CuffedDamageDivisor { get; set; } = 1f;

		[Description("Config de SCP-018")]
		public float Scp018DamageMultiplier { get; set; } = 1f;
		public bool Scp018FriendlyFire { get; set; } = true;
		public bool Scp018CantDestroyObject { get; set; } = false;

		[Description("% de chance de blink quand on lui tire dessus")]
		public int Scp173ForceBlinkPercent { get; set; } = -1;

		[Description("SCP-939 Patches")]
		public bool Scp939AttackBleeding { get; set; } = true;
		public float Scp939AttackBleedingTime { get; set; } = 60f;
		public int Scp939SeeingAhpAmount { get; set; } = -1;
		
		[Description("Recovery Amount")]
		public int Scp096RecoveryAmount { get; set; } = 0;
		public int Scp106RecoveryAmount { get; set; } = 0;
		public int Scp049RecoveryAmount { get; set; } = 0;
		public int Scp0492RecoveryAmount { get; set; } = 0;
		public int Scp173RecoveryAmount { get; set; } = 0;
		public int Scp939RecoveryAmount { get; set; } = 0;

		[Description("Multiplicateur de dégats")]
		public float Scp096DamageMultiplier { get; set; } = 1f;
		public float Scp173DamageMultiplier { get; set; } = 1f;
		public float Scp049DamageMultiplier { get; set; } = 1f;
		public float Scp0492DamageMultiplier { get; set; } = 1f;
		public float Scp939DamageMultiplier { get; set; } = 1f;

		[Description("Ne comprends pas la MicroHid Ni la Tesla")]
		public float Scp106DamageMultiplier { get; set; } = 1f;
		public float Scp106GrenadeMultiplier { get; set; } = 1f;
		

		[Description("SCP-079 Activé le mode Etendue de 079")]
		public bool Scp079ExtendEnabled { get; set; } = true;
		
		[Description("SCP-079 Config Plugin \n# Pour désactivé une capacité Scp079ExtendLevel = 6")]
		public int Scp079ExtendLevelFindscp { get; set; } = 1;
		public float Scp079ExtendCostFindscp { get; set; } = 10f;
		public int Scp079ExtendLevelDoorbeep { get; set; } = 1;
		public float Scp079ExtendCostDoorbeep { get; set; } = 5f;

		[Description("Gas Config")]
		public string[] GazBlacklistRooms { get; set; } = new string[0];
		public int GasDuration  { get; set; } = 60;
		public int TimerWaitGas { get; set; } = 60;
		public int GasExpGain { get; set; } = 2;
		public float Scp079_ex_cost_gaz { get; set; } = 150;
		public int Scp079_ex_level_gaz { get; set; } = 4;

		[Description("SCP-079 Config")]
		public float Scp079CostCamera { get; set; } = 1f;
		public float Scp079CostLock { get; set; } = 4f;
		public float Scp079CostLockStart { get; set; } = 5f;
		public float Scp079RequiredLockStart { get; set; } = 10f;
		public float Scp079CostDoorDefault { get; set; } = 5f;
		public float Scp079CostDoorContlv1 { get; set; } = 50f;
		public float Scp079CostDoorContlv2 { get; set; } = 40f;
		public float Scp079CostDoorContlv3 { get; set; } = 110f;
		public float Scp079CostDoorArmlv1 { get; set; } = 50f;
		public float Scp079CostDoorArmlv2 { get; set; } = 60f;
		public float Scp079CostDoorArmlv3 { get; set; } = 70f;
		public float Scp079CostDoorGate { get; set; } = 60f;
		public float Scp079CostDoorIntercom { get; set; } = 30f;
		public float Scp079CostDoorCheckpoint { get; set; } = 10f;
		public float Scp079CostLockDown { get; set; } = 60f;
		public float Scp079CostTesla { get; set; } = 50f;
		public float Scp079CostElevatorTeleport { get; set; } = 30f;
		public float Scp079CostElevatorUse { get; set; } = 10f;
		public float Scp079CostSpeakerStart { get; set; } = 10f;
		public float Scp079CostSpeakerUpdate { get; set; } = 0.8f;

		[Description("Ticket Gain Config")]
		public int Tickets_ci_classd_died_count { get; set; } = 0;
		public int Tickets_mtf_scientist_died_count { get; set; } = 0;
		public int Tickets_mtf_killed_by_scp_count { get; set; } = 0;
		public int Tickets_mtf_classd_killed_count { get; set; } = 0;
		public int Tickets_ci_killed_by_scp_count { get; set; } = 0;
		public int Tickets_ci_scientist_killed_count { get; set; } = 0;

		public string GetConfigs()
		{
			string returned = "\n";

			PropertyInfo[] infoArray = typeof(Configs).GetProperties(BindingFlags.Public | BindingFlags.Instance);

			foreach (PropertyInfo info in infoArray)
			{
				returned += $"{info.Name}: {info.GetValue(this)}\n";
			}

			return returned;
		}
	}
}

