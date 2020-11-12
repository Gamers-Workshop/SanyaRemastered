﻿using Exiled.API.Features;
using Exiled.API.Interfaces;
using SanyaPlugin.Functions;
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
			DataDirectory = Path.Combine(Paths.Plugins, "SanyaPlugin");
		}
		[Description("Activation du SanyaPlugin")]
		public bool IsEnabled { get; set; }

		[Description("WIP")]
		public bool IsDebugged { get; set; } = false;
		public bool Scp106PortalExtensionEnabled { get; set; } = false;
		public float PainEffectStart { get; set; } = 20;
		public bool TeslaExplodeGrenade { get; set; } = false;
		public bool Coroding106 { get; set; } = false;

		[Description("RandomRespawnPosPercent")]
		public int RandomRespawnPosPercent { get; set; } = -1;
		public bool FacilityGuardChangeSpawnPos { get; set; } = false;
		[Description("\n  # Serveur Config\n  # Localisation des données des joueurs")]
		public string DataDirectory { get; private set; } = string.Empty;

		[Description("Informations sur le serveur Adresse IP de destination")]
		public string InfosenderIp { get; set; } = "none";

		[Description("Port UDP vers lequel les informations du serveur sont envoyées")]
		public int InfosenderPort { get; set; } = -1;

		[Description("Kick Player")]
		public bool KickSteamLimited { get; set; } = false;

		[Description("Hud Activé")]
		public bool ExHudEnabled { get; set; } = false;

		[Description("Hud Scp-079 auras plus d'info")]
		public bool ExHudScp079Moreinfo { get; set; } = false;
		[Description("Hud Scp-079 auras plus d'info")]
		public bool ExHudScpList { get; set; } = false;
		[Description("Message de Bienvenue")]
		public string MotdMessage { get; set; } = string.Empty;
		[Description("Disable Player lists")]
		public bool DisablePlayerLists { get; set; } = false;
		[Description("NukeCap peut étre refermer")]
		public bool nukecapclose { get; set; } = false;
		[Description("Tesla Config")]
		public float TeslaRange { get; set; } = 5.5f;
		public List<string> TeslaTriggerableTeams { get; set; } = new List<string>();
		public List<Team> TeslaTriggerableTeamsParsed = new List<Team>();
		public bool TeslaNoTriggerableDisarmed { get; set; } = false;

		[Description("ItemCleanup Config")]
		public int ItemCleanup { get; set; } = -1;
		public List<string> ItemCleanupIgnore { get; set; } = new List<string>();
		public List<ItemType> ItemCleanupIgnoreParsed = new List<ItemType>();
		public bool CassieSubtitle { get; set; } = false;
		public bool IntercomInformation { get; set; } = false;
		public bool CloseDoorsOnNukecancel { get; set; } = false;
		public bool Scp049_add_time_res_success { get; set; } = false;
		public int OutsidezoneTerminationTimeAfterNuke { get; set; } = -1;
		
		[Description("Disable Chat")]
		public bool DisableAllChat { get; set; } = false;
		public bool DisableSpectatorChat { get; set; } = false;
		public bool DisableChatBypassWhitelist { get; set; } = false;

		[Description("Generator Config")]
		public bool GeneratorUnlockOpen { get; set; } = false;
		public bool GeneratorFinishLock { get; set; } = false;
		public bool GeneratorActivatingClose { get; set; } = false;

		[Description("\n  # Human Balanced")]
		public bool StopRespawnAfterDetonated { get; set; } = false;
		public bool Item_shoot_move { get; set; } = false;
		public bool Grenade_shoot_fuse { get; set; } = false;
		public bool GrenadeChainSametiming { get; set; } = false;
		public bool GodmodeAfterEndround { get; set; } = false;
		public bool InventoryKeycardActivation { get; set; } = false;
		
		[Description("Donne un effect d'assourdissement quand on est proche de l'explosion d'une grenade")]
		public bool GrenadeEffect { get; set; } = false;
		
		[Description("ClassD Contain")]
		public bool ClassD_container_locked { get; set; } = false;
		public float ClassD_container_Unlocked { get; set; } = 10f;
		
		[Description("Stamina Add \n  # Stamina effect ajoute un leger ralentissement quand la personne n'as pas de stamina")]
		public bool StaminaEffect { get; set; } = false;
		public float StaminaLostJump { get; set; } = 0.05f;
		public float StaminaLostLogicer { get; set; } = 0.001f;
		
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
		public bool Scp939RemoveRagdoll { get; set; } = false;
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

		[Description("% de chance de blink quand on lui tire dessus")]
		public int Scp173ForceBlinkPercent { get; set; } = -1;

		[Description("SCP-939 Patches")]
		public bool Scp939AttackBleeding { get; set; } = false;
		public float Scp939AttackBleedingTime { get; set; } = 60f;
		public int Scp939SeeingAhpAmount { get; set; } = -1;

		public float Scp939Size { get; set; } = 1f;
		[Description("Dégats Usp")]
		public float UspDamageMultiplierHuman { get; set; } = 1f;
		public float UspDamageMultiplierScp { get; set; } = 1f;

		[Description("Recovery Amount")]
		public int Scp096RecoveryAmount { get; set; } = 0;
		public int Scp106RecoveryAmount { get; set; } = 0;
		public int Scp049RecoveryAmount { get; set; } = 0;
		public int Scp0492RecoveryAmount { get; set; } = 0;
		public int Scp173RecoveryAmount { get; set; } = 0;
		public int Scp939RecoveryAmount { get; set; } = 0;

		[Description("Multiplicateur de dégats")]
		public float Scp096DamageMultiplicator { get; set; } = 1f;
		public float Scp173DamageMultiplicator { get; set; } = 1f;
		public float Scp049DamageMultiplicator { get; set; } = 1f;
		public float Scp0492DamageMultiplicator { get; set; } = 1f;
		public float Scp939DamageMultiplicator { get; set; } = 1f;

		[Description("Ne comprends pas la MicroHid Ni la Tesla")]
		public float Scp106DamageMultiplicator { get; set; } = 1f;
		public float Scp106GrenadeMultiplicator { get; set; } = 1f;
		[Description("SCP-106Capacité de traverser les murs cooldown")]
		public int Scp106WalkthroughCooldown { get; set; } = -1;
		[Description("Vキーチャットが可能なSCP（SCP-939以外）")]
		public List<string> AltvoicechatScps { get; set; } = new List<string>();
		public List<RoleType> AltvoicechatScpsParsed = new List<RoleType>();

		[Description("SCP-079 Activé le mode Etendue de 079")]
		public bool Scp079ExtendEnabled { get; set; } = false;
		
		[Description("SCP-079 Config Plugin \n  # Pour désactivé une capacité Scp079ExtendLevel = 6")]
		public int Scp079ExtendLevelFindscp { get; set; } = 1;
		public float Scp079ExtendCostFindscp { get; set; } = 10f;
		public int Scp079ExtendLevelDoorbeep { get; set; } = 1;
		public float Scp079ExtendCostDoorbeep { get; set; } = 5f;

		[Description("SCP-079 Radar Humain")]
		public bool Scp079spot;

		[Description("SCP-079 Niveau requis pour le spot")]
		public int Scp079ExtendLevelSpot { get; set; } = 1;

		[Description("SCP-079 GAS Config")]
		public List<string> GazBlacklistRooms { get; set; } = new List<string>();
		public int GasDuration  { get; set; } = 60;
		public int TimerWaitGas { get; set; } = 60;
		public int GasExpGain { get; set; } = 2;
		public float Scp079ExCostGaz { get; set; } = 150;
		public int Scp079ExLevelGaz { get; set; } = 4;

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

		[Description("Ticket Gain Config add")]
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
		public void ParseConfig()
		{
			try
			{
				ItemCleanupIgnore.Clear();
				TeslaTriggerableTeams.Clear();
				AltvoicechatScpsParsed.Clear();

				foreach (var item in ItemCleanupIgnore)
					if (Enum.TryParse(item, out ItemType type))
						ItemCleanupIgnoreParsed.Add(type);

				foreach (var item in TeslaTriggerableTeams)
					if (Enum.TryParse(item, out Team team))
						TeslaTriggerableTeamsParsed.Add(team);

				foreach (var item in AltvoicechatScps)
					if (Enum.TryParse(item, out RoleType role))
						AltvoicechatScpsParsed.Add(role);
			}
			catch (Exception ex)
			{
				Log.Error($"[ParseConfig] Error : {ex}");
			}
		}
	}
}