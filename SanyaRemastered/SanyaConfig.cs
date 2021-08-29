using Exiled.API.Features;
using Exiled.API.Interfaces;
using SanyaRemastered.Functions;
using System;
using System.Collections;
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
			DataDirectory = Path.Combine(Paths.Plugins, "SanyaRemastered");
		}
		[Description("Activation du SanyaRemastered")]
		public bool IsEnabled { get; set; }
		public string IsBeta { get; set; } = string.Empty;


		[Description("WIP")]
		public bool IsDebugged { get; set; } = false;
		public bool Coroding106 { get; set; } = false;
		public bool ExplodingGrenadeTesla { get; set; } = false;
		public bool TeslaDestroyName { get; set; } = false;
		public bool ScpTakeFallDamage { get; set; } = false;
		public string BoxMessageOnJoin { get; set; } = string.Empty;
		[Description("Jump attaque")]
		public bool JumpingKickAttack { get; set; } = false;


		[Description("RandomRespawnPosPercent")]
		public int RandomRespawnPosPercent { get; set; } = -1;
		[Description("\n  # Serveur Config\n  # Localisation des données des joueurs")]
		public string DataDirectory { get; private set; } = string.Empty;

		[Description("Hud Activé")]
		public bool ExHudEnabled { get; set; } = false;

		[Description("Hud Scp-079 auras plus d'info")]
		public bool ExHudScp079Moreinfo { get; set; } = false;
		[Description("Hud Scp-096 auras plus d'info")]
		public bool ExHudScp096 { get; set; } = false;

		[Description("Disable Player lists")]
		public bool DisablePlayerLists { get; set; } = false;
		[Description("NukeCap peut étre refermer")]
		public bool Nukecapclose { get; set; } = false;
		[Description("Tesla Config")]
		public float TeslaRange { get; set; } = 5.5f;
		public bool TeslaNoTriggerableDisarmed { get; set; } = false;

		[Description("Ajout de porte sur la map")]
		public bool AddDoorsOnSurface { get; set; } = false;
		[Description("Game config")]

		public bool CassieSubtitle { get; set; } = false;
		public bool IntercomInformation { get; set; } = false;
		public bool IntercomBrokenOnBlackout { get; set; } = false;
		public bool CloseDoorsOnNukecancel { get; set; } = false;
		public bool Scp049_add_time_res_success { get; set; } = false;
		public int OutsidezoneTerminationTimeAfterNuke { get; set; } = -1;
		
		[Description("Generator Config")]
		public bool GeneratorUnlockOpen { get; set; } = false;
		public bool GeneratorFinishLock { get; set; } = false;
		public bool GeneratorActivatingClose { get; set; } = false;

		[Description("\n  # Human Balanced")]
		public bool StopRespawnAfterDetonated { get; set; } = false;
		public bool Item_shoot_move { get; set; } = false;
		public bool Grenade_shoot_fuse { get; set; } = false;
		public bool OpenDoorOnShoot { get; set; } = false;
		public bool GrenadeChainSametiming { get; set; } = false;
		public bool GodmodeAfterEndround { get; set; } = false;
		public bool InventoryKeycardActivation { get; set; } = false;
		public float PainEffectStart { get; set; } = -1;

		[Description("Donne un effect d'assourdissement quand on est proche de l'explosion d'une grenade")]
		public bool GrenadeEffect { get; set; } = false;
				
		[Description("Stamina Add \n  # Stamina effect ajoute un leger ralentissement quand la personne n'as pas de stamina")]
		public bool StaminaEffect { get; set; } = false;
		public float StaminaLostJump { get; set; } = 0.05f;
		
		[Description("Traitre")]
		public int TraitorLimit { get; set; } = -1;
		public int TraitorChancePercent { get; set; } = 50;

		[Description("\n  # SCP Balanced")]
		public float Scp939CanSeeVoiceChatting { get; set; } = 0f;

		[Description("Interdiction d'ouvrir ou de fermer")]
		public bool Scp049_2DontOpenDoorAnd106 { get; set; } = false;
		public bool Scp939And096DontOpenlockerAndGenerator { get; set; } = false;

		[Description("Permet au SCP de .contain")]
		public bool ContainCommand { get; set; } = false;

		[Description("Le cadavre n'apparait pas quand on se fait tuer par")]
		public bool Scp106RemoveRagdoll { get; set; } = false;
		public bool Scp096RemoveRagdoll { get; set; } = false;
		[Description("Scp106 est ralentie")]
		public bool Scp106slow { get; set; } = false;
		[Description("Scp939 est ralentie")]
		public bool Scp939slow { get; set; } = false;
		[Description("Effect sur SCP-049-2")]
		public bool Scp0492effect { get; set; } = false;

		[Description("SCP-914 = Effect")]
		public bool Scp914Effect { get; set; } = false;

		[Description("Hitmark Add")]
		public bool HitmarkGrenade { get; set; } = false;
		public bool HitmarkKilled { get; set; } = false;

		[Description("Config de SCP-018")]
		public float Scp018DamageMultiplier { get; set; } = 1f;
		public bool Scp018FriendlyFire { get; set; } = false;
		public bool Scp018CantDestroyObject { get; set; } = false;

		[Description("SCP-939 Patches")]
		public int Scp939SeeingAhpAmount { get; set; } = -1;

		public float Scp939Size { get; set; } = 1f;
		[Description("RP")]
		public bool Scp096Real { get; set; } = false;
		public bool Scp049Real { get; set; } = false;
		public List<string> ScpFallDamage { get; set; } = new List<string> 
		{
			"Scp049",
			"Scp0492",
			"Scp93989",
			"Scp93953"
		};

		[Description("Dégats Usp")]
		public float UspDamageMultiplierHuman { get; set; } = 1f;
		public float UspDamageMultiplierScp { get; set; } = 1f;

		[Description("Recovery Amount")]
		public Dictionary<string, int> ScpRecoveryAmount { get; set; } = new Dictionary<string, int>()
		{
			{"Scp049", 0},
			{"Scp0492", 0},
			{"Scp096", 0},
			{"Scp106", 0},
			{"Scp173", 0},
			{"Scp939", 0}
		};
		[Description("Multiplicateur de dégats")]
		public Dictionary<RoleType, float> ScpDamageMultiplicator { get; set; } = new Dictionary<RoleType, float>()
		{
			{RoleType.Scp049, 1f},
			{RoleType.Scp0492, 1f},
			{RoleType.Scp096, 1f},
			{RoleType.Scp173, 1f},          
			{RoleType.Scp93953, 1f},
			{RoleType.Scp93989, 1f},
		};
		[Description("Ne comprends pas la MicroHid Ni la Tesla")]
		public float Scp106DamageMultiplicator { get; set; } = 1f;
		public float Scp106GrenadeMultiplicator { get; set; } = 1f;

		[Description("SCP Activé les mode étendue")]
		public bool Scp079ExtendEnabled { get; set; } = false;
		[Description("SCP-106 Config \n  # Quand vous marcher sur le portail de 106 vous tomber dans la dimmenssion de poche")]
		public bool Scp106PortalEffect { get; set; } = false;

		[Description("SCP-079 Config Plugin \n  # Pour désactivé une capacité Scp079ExtendLevel = 6")]
		public int Scp079ExtendLevelFindscp { get; set; } = 1;
		public float Scp079ExtendCostFindscp { get; set; } = 10f;
		public int Scp079ExtendLevelDoorbeep { get; set; } = 1;
		public float Scp079ExtendCostDoorbeep { get; set; } = 5f;
		public int Scp079ExtendBlackoutIntercom { get; set; } = 1;
		public float Scp079ExtendCostBlackoutIntercom { get; set; } = 5f;

		[Description("SCP-079 Radar Humain")]
		public bool Scp079spot { get; set; } = false;

		[Description("SCP-079 Niveau requis pour le spot")]
		public int Scp079ExtendLevelSpot { get; set; } = 1;

		[Description("SCP-079 GAS Config")]
		public List<string> GazBlacklistRooms { get; set; } = new List<string>();
		public int GasDuration  { get; set; } = 60;
		public int TimerWaitGas { get; set; } = 60;
		public int GasExpGain { get; set; } = 10;
		public float Scp079ExCostGaz { get; set; } = 150;
		public int Scp079ExLevelGaz { get; set; } = 4;

		[Description("SCP-079 Config")]
		public Dictionary<string, float> Scp079ManaCost { get; set; } = new Dictionary<string, float>()
		{
			{"Camera Switch",                   1f },
			{"Door Lock",                       4f },
			{"Door Lock Start",                 5f },
			{"Door Lock Minimum",              10f },
			{"Door Interaction DEFAULT",        5f },
			{"Door Interaction CONT_LVL_1",    50f },
			{"Door Interaction CONT_LVL_2",    40f },
			{"Door Interaction CONT_LVL_3",   110f },
			{"Door Interaction ARMORY_LVL_1",  50f },
			{"Door Interaction ARMORY_LVL_2",  60f },
			{"Door Interaction ARMORY_LVL_3",  70f },
			{"Door Interaction EXIT_ACC",      60f },
			{"Door Interaction INCOM_ACC",     30f },
			{"Door Interaction CHCKPOINT_ACC", 10f },
			{"Room Lockdown",                  60f },
			{"Tesla Gate Burst",               50f },
			{"Elevator Teleport",              30f },
			{"Elevator Use",                   10f },
			{"Speaker Start",                  10f },
			{"Speaker Update",                0.8f }
		};
		[Description("SCP Can't interact Now")]
		public bool ScpCantInteract { get; set; } = false;
		public Dictionary<string, List<RoleType>> ScpCantInteractList { get; set; } = new Dictionary<string, List<RoleType>>()
		{
			{"Change914Knob",            new List<RoleType>{RoleType.Scp173,RoleType.Scp106,RoleType.Scp096,RoleType.Scp049,RoleType.Scp0492, RoleType.Scp93953, RoleType.Scp93989} },
			{"Use914",                   new List<RoleType>{RoleType.Scp173,RoleType.Scp106,RoleType.Scp096,RoleType.Scp049,RoleType.Scp0492, RoleType.Scp93953, RoleType.Scp93989} },
			{"Contain106",               new List<RoleType>{RoleType.Scp173,RoleType.Scp106,RoleType.Scp096,RoleType.Scp049,RoleType.Scp0492, RoleType.Scp93953, RoleType.Scp93989} },
			{"DetonateWarhead",          new List<RoleType>{RoleType.Scp173,RoleType.Scp106,RoleType.Scp096,RoleType.Scp049,RoleType.Scp0492, RoleType.Scp93953, RoleType.Scp93989} },
			{"AlphaWarheadButton",       new List<RoleType>{RoleType.Scp173,RoleType.Scp106,RoleType.Scp096,RoleType.Scp049,RoleType.Scp0492, RoleType.Scp93953, RoleType.Scp93989} },
			{"UseElevator",              new List<RoleType>{RoleType.Scp173,RoleType.Scp106,RoleType.Scp096,RoleType.Scp049,RoleType.Scp0492, RoleType.Scp93953, RoleType.Scp93989} },
			{"UseGenerator",             new List<RoleType>{RoleType.Scp173,RoleType.Scp106,RoleType.Scp096,RoleType.Scp049,RoleType.Scp0492, RoleType.Scp93953, RoleType.Scp93989} },
			{"UseLocker",                new List<RoleType>{RoleType.Scp173,RoleType.Scp106,RoleType.Scp096,RoleType.Scp049,RoleType.Scp0492, RoleType.Scp93953, RoleType.Scp93989} },
			{"UseAlphaWarheadPanel",     new List<RoleType>{RoleType.Scp173,RoleType.Scp106,RoleType.Scp096,RoleType.Scp049,RoleType.Scp0492, RoleType.Scp93953, RoleType.Scp93989} },
			{"DoorInteractOpen",		 new List<RoleType>{RoleType.Scp173,RoleType.Scp106,RoleType.Scp096,RoleType.Scp049,RoleType.Scp0492, RoleType.Scp93953, RoleType.Scp93989} },
			{"DoorInteractClose",		 new List<RoleType>{RoleType.Scp173,RoleType.Scp106,RoleType.Scp096,RoleType.Scp049,RoleType.Scp0492, RoleType.Scp93953, RoleType.Scp93989} },

		};

		public string GetConfigs()
		{
			string returned = "\n";

			PropertyInfo[] infoArray = typeof(Configs).GetProperties(BindingFlags.Public | BindingFlags.Instance);

			foreach (PropertyInfo info in infoArray)
			{
				if (info.PropertyType.IsList())
				{
					var list = info.GetValue(this) as IEnumerable;
					returned += $"{info.Name}:\n";
					if (list != null)
						foreach (var i in list) returned += $"{i}\n";
				}
				else if (info.PropertyType.IsDictionary())
				{
					returned += $"{info.Name}: ";

					var obj = info.GetValue(this);

					IDictionary dict = (IDictionary)obj;

					var key = obj.GetType().GetProperty("Keys");
					var value = obj.GetType().GetProperty("Values");
					var keyObj = key.GetValue(obj, null);
					var valueObj = value.GetValue(obj, null);
					var keyEnum = keyObj as IEnumerable;

					foreach (var i in dict.Keys)
					{
						returned += $"[{i}:{dict[i]}]";
					}

					returned += "\n";
				}
				else
				{
					returned += $"{info.Name}: {info.GetValue(this)}\n";
				}
			}

			FieldInfo[] fieldInfos = typeof(Configs).GetFields(BindingFlags.Public | BindingFlags.Instance);

			foreach (var info in fieldInfos)
			{
				if (info.FieldType.IsList())
				{
					var list = info.GetValue(this) as IEnumerable;
					returned += $"{info.Name}:\n";
					if (list != null)
						foreach (var i in list) returned += $"{i}\n";
				}
				else if (info.FieldType.IsDictionary())
				{
					returned += $"{info.Name}: ";

					var obj = info.GetValue(this);

					IDictionary dict = (IDictionary)obj;

					var key = obj.GetType().GetProperty("Keys");
					var value = obj.GetType().GetProperty("Values");
					var keyObj = key.GetValue(obj, null);
					var valueObj = value.GetValue(obj, null);
					var keyEnum = keyObj as IEnumerable;

					foreach (var i in dict.Keys)
					{
						if (dict[i].GetType().IsList())
						{
							var list = dict[i] as IEnumerable;
							returned += $"[{i}:";
							if (list != null)
								foreach (var x in list) returned += $"{x},";
							returned += "]";
						}
						else
						{
							returned += $"[{i}:{dict[i]}]";
						}
					}

					returned += "\n";
				}
				else
				{
					returned += $"{info.Name}: {info.GetValue(this)}\n";
				}
			}

			return returned;
		}

	}
}