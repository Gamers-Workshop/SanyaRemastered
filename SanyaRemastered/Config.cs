using System;
using System.Collections.Generic;
using System.Reflection;
using Exiled;
using Exiled.API.Features;

namespace SanyaPlugin
{
	/*internal static class Configs
	{
		//info and report
		internal static string infosender_ip;
		internal static int infosender_port;
		internal static string report_webhook;

		//Smod Emulation
		internal static Dictionary<RoleType, List<ItemType>> defaultitems;
		internal static List<int> tesla_triggerable_teams;
		internal static int item_cleanup;
		internal static List<ItemType> item_cleanup_ignore;

		//SanyaPlugin
		internal static bool kick_steam_limited;
		internal static bool kick_vpn;
		internal static string kick_vpn_apikey;
		internal static string motd_message = "";
		internal static List<int> event_mode_weight;
		internal static bool cassie_subtitle;
		internal static bool tesla_triggerable_disarmed;
		internal static float tesla_range;
		internal static bool close_doors_on_nukecancel;
		internal static bool generator_unlock_to_open;
		internal static bool generator_finish_to_lock;
		internal static bool generator_activating_opened;
		internal static bool intercom_information;
		internal static int outsidezone_termination_time_after_nuke;
		internal static bool godmode_after_endround;
		internal static bool disable_all_chat;
		internal static bool disable_spectator_chat;
		internal static bool disable_chat_bypass_whitelist;
		internal static bool beta_anticheat_disable;

		//SanyaPlugin:Event
		internal static List<ItemType> classd_insurgency_classd_inventory;
		internal static List<ItemType> classd_insurgency_scientist_inventory;

		//SanyaPlugin:Data
		internal static bool data_enabled;
		internal static bool level_enabled;
		internal static int level_exp_kill;
		internal static int level_exp_death;
		internal static int level_exp_win;
		internal static int level_exp_other;

		//Human:Balanced
		internal static bool stop_respawn_after_detonated;
		internal static bool inventory_keycard_act;
		internal static bool item_shoot_move;
		internal static bool grenade_shoot_fuse;
		internal static bool grenade_hitmark;
		internal static bool kill_hitmark;
		internal static int traitor_limitter;
		internal static int traitor_chance_percent;
		internal static float stamina_jump_used;
		internal static bool classd_container_locked;
		//	internal static int stamina_logicer_used;
		//SCP:Balanced
		internal static bool scp018_friendly_fire;
		internal static float scp018_damage_multiplier;
		internal static bool scp018_cant_destroy_object;
		internal static bool scp049_add_time_res_success;
		internal static bool scp0492_hurt_effect;
		internal static bool scp106_ex_enabled;
		internal static bool scp079_spot;
		internal static int scp173_hurt_blink_percent;
		internal static bool scp939_attack_bleeding;
		internal static bool scp914_intake_death;

		//Ticket Extend
		internal static int tickets_mtf_killed_by_scp_count;
		internal static int tickets_mtf_classd_killed_count;
		internal static int tickets_mtf_scientist_died_count;
		internal static int tickets_ci_killed_by_scp_count;
		internal static int tickets_ci_scientist_killed_count;
		internal static int tickets_ci_classd_died_count;


		//Damage and Recovery
		internal static float damage_usp_multiplier_human;
		internal static float damage_usp_multiplier_scp;
		internal static float damage_divisor_cuffed;
		internal static float damage_divisor_scp049;
		internal static float damage_divisor_scp0492;
		internal static float damage_divisor_scp096;
		internal static float damage_divisor_scp106;
		internal static float damage_divisor_scp106_grenade;
		internal static float damage_divisor_scp173;
		internal static float damage_divisor_scp939;
		internal static int recovery_amount_scp049;
		internal static int recovery_amount_scp0492;
		internal static int recovery_amount_scp096;
		internal static int recovery_amount_scp106;
		internal static int recovery_amount_scp173;
		internal static int recovery_amount_scp939;

		//SCP-079
		internal static float scp079_cost_camera;
		internal static float scp079_cost_lock;
		internal static float scp079_cost_lock_start;
		internal static float scp079_cost_lock_minimum;
		internal static float scp079_cost_door_default;
		internal static float scp079_cost_door_contlv1;
		internal static float scp079_cost_door_contlv2;
		internal static float scp079_cost_door_contlv3;
		internal static float scp079_cost_door_armlv1;
		internal static float scp079_cost_door_armlv2;
		internal static float scp079_cost_door_armlv3;
		internal static float scp079_cost_door_exit;
		internal static float scp079_cost_door_intercom;
		internal static float scp079_cost_door_checkpoint;
		internal static float scp079_cost_lockdown;
		internal static float scp079_cost_tesla;
		internal static float scp079_cost_elevator_use;
		internal static float scp079_cost_elevator_teleport;
		internal static float scp079_cost_speaker_start;
		internal static float scp079_cost_speaker_update;

		//SCP-079-Extend
		internal static bool scp079_ex_enabled;
		internal static float scp079_ex_level_findscp;
		internal static float scp079_ex_cost_findscp;
		internal static float scp079_ex_level_doorbeep;
		internal static float scp079_ex_cost_doorbeep;
		internal static float scp079_ex_level_nuke;
		internal static float scp079_ex_cost_nuke;
		internal static float scp079_ex_level_airbomb;
		internal static float scp079_ex_cost_airbomb;
		internal static float scp079_ex_level_gaz;
		internal static float scp079_ex_cost_gaz;

		//GAZ 
		public static int A2Timer;
		public static int A2TimerGas;
		public static float A2Exp;
		public static List<string> A2BlacklistRooms;
		public static string A2WarnMsg;
		public static string A2ActiveMsg;
		public static string RunA2Msg;
		public static string HelpMsgA2;
		public static string FailA2Msg;

		internal static void Reload()
		{
			infosender_ip = SanyaPlugin.instance.Config.GetString("sanya_infosender_ip", "none");
			infosender_port = SanyaPlugin.instance.Config.GetInt("sanya_infosender_port", 37813);
			report_webhook = SanyaPlugin.instance.Config.GetString("sanya_report_webhook", string.Empty);
			tesla_triggerable_teams = SanyaPlugin.instance.Config.GetIntList("sanya_tesla_triggerable_teams");
			defaultitems = new Dictionary<RoleType, List<ItemType>>
			{
				{ RoleType.ClassD, new List<ItemType>(SanyaPlugin.instance.Config.GetStringList("sanya_defaultitem_classd").ConvertAll((string x) => { return (ItemType)Enum.Parse(typeof(ItemType), x); })) },
				{ RoleType.Scientist, new List<ItemType>(SanyaPlugin.instance.Config.GetStringList("sanya_defaultitem_scientist").ConvertAll((string x) => { return (ItemType)Enum.Parse(typeof(ItemType), x); })) },
				{ RoleType.FacilityGuard, new List<ItemType>(SanyaPlugin.instance.Config.GetStringList("sanya_defaultitem_guard").ConvertAll((string x) => { return (ItemType)Enum.Parse(typeof(ItemType), x); })) },
				{ RoleType.NtfCadet, new List<ItemType>(SanyaPlugin.instance.Config.GetStringList("sanya_defaultitem_cadet").ConvertAll((string x) => { return (ItemType)Enum.Parse(typeof(ItemType), x); })) },
				{ RoleType.NtfLieutenant, new List<ItemType>(SanyaPlugin.instance.Config.GetStringList("sanya_defaultitem_lieutenant").ConvertAll((string x) => { return (ItemType)Enum.Parse(typeof(ItemType), x); })) },
				{ RoleType.NtfCommander, new List<ItemType>(SanyaPlugin.instance.Config.GetStringList("sanya_defaultitem_commander").ConvertAll((string x) => { return (ItemType)Enum.Parse(typeof(ItemType), x); })) },
				{ RoleType.NtfScientist, new List<ItemType>(SanyaPlugin.instance.Config.GetStringList("sanya_defaultitem_ntfscientist").ConvertAll((string x) => { return (ItemType)Enum.Parse(typeof(ItemType), x); })) },
				{ RoleType.ChaosInsurgency, new List<ItemType>(SanyaPlugin.instance.Config.GetStringList("sanya_defaultitem_ci").ConvertAll((string x) => { return (ItemType)Enum.Parse(typeof(ItemType), x); })) }
			};
			item_cleanup = SanyaPlugin.instance.Config.GetInt("sanya_item_cleanup", -1);
			item_cleanup_ignore = SanyaPlugin.instance.Config.GetStringList("sanya_item_cleanup_ignore").ConvertAll((string x) => { return (ItemType)Enum.Parse(typeof(ItemType), x); });

			kick_steam_limited = SanyaPlugin.instance.Config.GetBool("sanya_kick_steam_limited", false);
			kick_vpn = SanyaPlugin.instance.Config.GetBool("sanya_kick_vpn", false);
			kick_vpn_apikey = SanyaPlugin.instance.Config.GetString("sanya_kick_vpn_apikey", string.Empty);
			motd_message = SanyaPlugin.instance.Config.GetString("sanya_motd_message", string.Empty);
			event_mode_weight = SanyaPlugin.instance.Config.GetIntList("sanya_event_mode_weight");
			cassie_subtitle = SanyaPlugin.instance.Config.GetBool("sanya_cassie_subtitle", true);
			tesla_triggerable_disarmed = SanyaPlugin.instance.Config.GetBool("sanya_tesla_triggerable_disarmed", false);
			tesla_range = SanyaPlugin.instance.Config.GetFloat("sanya_tesla_range", 5.5f);
			close_doors_on_nukecancel = SanyaPlugin.instance.Config.GetBool("sanya_close_doors_on_nukecancel", true);
			generator_unlock_to_open = SanyaPlugin.instance.Config.GetBool("sanya_generator_unlock_to_open", false);
			generator_finish_to_lock = SanyaPlugin.instance.Config.GetBool("sanya_generator_finish_to_lock", true);
			generator_activating_opened = SanyaPlugin.instance.Config.GetBool("sanya_generator_activating_opened", false);
			intercom_information = SanyaPlugin.instance.Config.GetBool("sanya_intercom_information", true);
			outsidezone_termination_time_after_nuke = SanyaPlugin.instance.Config.GetInt("sanya_outsidezone_termination_time_after_nuke", -1);
			godmode_after_endround = SanyaPlugin.instance.Config.GetBool("sanya_godmode_after_endround", false);
			disable_spectator_chat = SanyaPlugin.instance.Config.GetBool("sanya_disable_spectator_chat", false);
			disable_all_chat = SanyaPlugin.instance.Config.GetBool("sanya_disable_all_chat", false);
			disable_chat_bypass_whitelist = SanyaPlugin.instance.Config.GetBool("sanya_disable_chat_bypass_whitelist", false);
			beta_anticheat_disable = SanyaPlugin.instance.Config.GetBool("sanya_beta_anticheat_disable", true);

			classd_insurgency_classd_inventory = SanyaPlugin.instance.Config.GetStringList("sanya_classd_insurgency_classd_inventory").ConvertAll((string x) => { return (ItemType)Enum.Parse(typeof(ItemType), x); });
			classd_insurgency_scientist_inventory = SanyaPlugin.instance.Config.GetStringList("sanya_classd_insurgency_scientist_inventory").ConvertAll((string x) => { return (ItemType)Enum.Parse(typeof(ItemType), x); });

			data_enabled = SanyaPlugin.instance.Config.GetBool("sanya_data_enabled", false);
			level_enabled = SanyaPlugin.instance.Config.GetBool("sanya_level_enabled", false);
			level_exp_kill = SanyaPlugin.instance.Config.GetInt("sanya_level_exp_kill", 3);
			level_exp_death = SanyaPlugin.instance.Config.GetInt("sanya_level_exp_death", 1);
			level_exp_win = SanyaPlugin.instance.Config.GetInt("sanya_level_exp_win", 10);
			level_exp_other = SanyaPlugin.instance.Config.GetInt("sanya_level_exp_other", 1);

			stop_respawn_after_detonated = SanyaPlugin.instance.Config.GetBool("sanya_stop_respawn_after_detonated", true);
			inventory_keycard_act = SanyaPlugin.instance.Config.GetBool("sanya_inventory_keycard_act", true);
			item_shoot_move = SanyaPlugin.instance.Config.GetBool("sanya_item_shoot_move", true);
			grenade_shoot_fuse = SanyaPlugin.instance.Config.GetBool("sanya_grenade_shoot_fuse", true);
			grenade_hitmark = SanyaPlugin.instance.Config.GetBool("sanya_grenade_hitmark", true);
			kill_hitmark = SanyaPlugin.instance.Config.GetBool("sanya_kill_hitmark", true);
			traitor_limitter = SanyaPlugin.instance.Config.GetInt("sanya_traitor_limitter", -1);
			traitor_chance_percent = SanyaPlugin.instance.Config.GetInt("sanya_traitor_chance_percent", 50);
			stamina_jump_used = SanyaPlugin.instance.Config.GetFloat("sanya_stamina_jump_used", -1f);
			//	stamina_logicer_used = SanyaPlugin.instance.Config.GetInt("stamina_logicer_used", 1);
			classd_container_locked = SanyaPlugin.instance.Config.GetBool("sanya_classd_container_locked", false);

			scp018_friendly_fire = SanyaPlugin.instance.Config.GetBool("sanya_grenade_friendly_fire", true);
			scp018_damage_multiplier = SanyaPlugin.instance.Config.GetFloat("sanya_scp018_damage_multiplier", 1f);
			scp018_cant_destroy_object = SanyaPlugin.instance.Config.GetBool("sanya_scp018_cant_destroy_object", false);
			scp079_spot = SanyaPlugin.instance.Config.GetBool("sanya_scp079_spot", false);
			scp049_add_time_res_success = SanyaPlugin.instance.Config.GetBool("sanya_scp049_add_time_res_success", false);
			scp0492_hurt_effect = SanyaPlugin.instance.Config.GetBool("sanya_scp0492_hurt_effect", false);
			scp106_ex_enabled = SanyaPlugin.instance.Config.GetBool("sanya_scp106_ex_enabled", false);
			scp173_hurt_blink_percent = SanyaPlugin.instance.Config.GetInt("sanya_scp173_hurt_blink_percent", -1);
			scp939_attack_bleeding = SanyaPlugin.instance.Config.GetBool("sanya_scp939_attack_bleeding", false);
			scp914_intake_death = SanyaPlugin.instance.Config.GetBool("sanya_scp914_intake_death", true);

			tickets_mtf_killed_by_scp_count = SanyaPlugin.instance.Config.GetInt("sanya_tickets_mtf_killed_by_scp_count", 0);
			tickets_mtf_classd_killed_count = SanyaPlugin.instance.Config.GetInt("sanya_tickets_mtf_classd_killed_count", 0);
			tickets_mtf_scientist_died_count = SanyaPlugin.instance.Config.GetInt("sanya_tickets_mtf_scientist_died_count", 0);
			tickets_ci_killed_by_scp_count = SanyaPlugin.instance.Config.GetInt("sanya_tickets_ci_killed_by_scp_count", 0);
			tickets_ci_scientist_killed_count = SanyaPlugin.instance.Config.GetInt("sanya_tickets_ci_scientist_killed_count", 0);
			tickets_ci_classd_died_count = SanyaPlugin.instance.Config.GetInt("sanya_tickets_ci_classd_died_count", 0);

			damage_usp_multiplier_human = SanyaPlugin.instance.Config.GetFloat("sanya_damage_usp_multiplier_human", 1.0f);
			damage_usp_multiplier_scp = SanyaPlugin.instance.Config.GetFloat("sanya_damage_usp_multiplier_scp", 1.0f);
			damage_divisor_cuffed = SanyaPlugin.instance.Config.GetFloat("sanya_damage_divisor_cuffed", 1.0f);
			damage_divisor_scp049 = SanyaPlugin.instance.Config.GetFloat("sanya_damage_divisor_scp049", 1.0f);
			damage_divisor_scp0492 = SanyaPlugin.instance.Config.GetFloat("sanya_damage_divisor_scp0492", 1.0f);
			damage_divisor_scp096 = SanyaPlugin.instance.Config.GetFloat("sanya_damage_divisor_scp096", 1.0f);
			damage_divisor_scp106 = SanyaPlugin.instance.Config.GetFloat("sanya_damage_divisor_scp106", 1.0f);
			damage_divisor_scp106_grenade = SanyaPlugin.instance.Config.GetFloat("sanya_damage_divisor_scp106_grenade", 1.0f);
			damage_divisor_scp173 = SanyaPlugin.instance.Config.GetFloat("sanya_damage_divisor_scp173", 1.0f);
			damage_divisor_scp939 = SanyaPlugin.instance.Config.GetFloat("sanya_damage_divisor_scp939", 1.0f);
			recovery_amount_scp049 = SanyaPlugin.instance.Config.GetInt("sanya_recovery_amount_scp049", -1);
			recovery_amount_scp0492 = SanyaPlugin.instance.Config.GetInt("sanya_recovery_amount_scp0492", -1);
			recovery_amount_scp096 = SanyaPlugin.instance.Config.GetInt("sanya_recovery_amount_scp096", -1);
			recovery_amount_scp106 = SanyaPlugin.instance.Config.GetInt("sanya_recovery_amount_scp106", -1);
			recovery_amount_scp173 = SanyaPlugin.instance.Config.GetInt("sanya_recovery_amount_scp173", -1);
			recovery_amount_scp939 = SanyaPlugin.instance.Config.GetInt("sanya_recovery_amount_scp939", -1);

			scp079_cost_camera = SanyaPlugin.instance.Config.GetFloat("sanya_scp079_cost_camera", 1);
			scp079_cost_lock = SanyaPlugin.instance.Config.GetFloat("sanya_scp079_cost_lock", 4);
			scp079_cost_lock_start = SanyaPlugin.instance.Config.GetFloat("sanya_scp079_cost_lock_start", 5);
			scp079_cost_lock_minimum = SanyaPlugin.instance.Config.GetFloat("sanya_scp079_cost_lock_minimum", 10);
			scp079_cost_door_default = SanyaPlugin.instance.Config.GetFloat("sanya_scp079_cost_door_default", 5);
			scp079_cost_door_contlv1 = SanyaPlugin.instance.Config.GetFloat("sanya_scp079_cost_door_contlv1", 50);
			scp079_cost_door_contlv2 = SanyaPlugin.instance.Config.GetFloat("sanya_scp079_cost_door_contlv2", 40);
			scp079_cost_door_contlv3 = SanyaPlugin.instance.Config.GetFloat("sanya_scp079_cost_door_contlv3", 110);
			scp079_cost_door_armlv1 = SanyaPlugin.instance.Config.GetFloat("sanya_scp079_cost_door_armlv1", 50);
			scp079_cost_door_armlv2 = SanyaPlugin.instance.Config.GetFloat("sanya_scp079_cost_door_armlv2", 60);
			scp079_cost_door_armlv3 = SanyaPlugin.instance.Config.GetFloat("sanya_scp079_cost_door_armlv3", 70);
			scp079_cost_door_exit = SanyaPlugin.instance.Config.GetFloat("sanya_scp079_cost_door_exit", 60);
			scp079_cost_door_intercom = SanyaPlugin.instance.Config.GetFloat("sanya_scp079_cost_door_intercom", 30);
			scp079_cost_door_checkpoint = SanyaPlugin.instance.Config.GetFloat("sanya_scp079_cost_door_checkpoint", 10);
			scp079_cost_lockdown = SanyaPlugin.instance.Config.GetFloat("sanya_scp079_cost_lockdown", 60);
			scp079_cost_tesla = SanyaPlugin.instance.Config.GetFloat("sanya_scp079_cost_tesla", 50);
			scp079_cost_elevator_teleport = SanyaPlugin.instance.Config.GetFloat("sanya_scp079_cost_elevator_teleport", 30);
			scp079_cost_elevator_use = SanyaPlugin.instance.Config.GetFloat("sanya_scp079_cost_elevator_use", 10);
			scp079_cost_speaker_start = SanyaPlugin.instance.Config.GetFloat("sanya_scp079_cost_speaker_start", 10);
			scp079_cost_speaker_update = SanyaPlugin.instance.Config.GetFloat("sanya_scp079_cost_speaker_update", 0.8f);

			scp079_ex_enabled = SanyaPlugin.instance.Config.GetBool("sanya_scp079_ex_enabled", true);
			scp079_ex_level_findscp = SanyaPlugin.instance.Config.GetInt("sanya_scp079_ex_level_findscp", 1);
			scp079_ex_cost_findscp = SanyaPlugin.instance.Config.GetFloat("sanya_scp079_ex_cost_findscp", 10);
			scp079_ex_level_doorbeep = SanyaPlugin.instance.Config.GetInt("sanya_scp079_ex_level_doorbeep", 2);
			scp079_ex_cost_doorbeep = SanyaPlugin.instance.Config.GetFloat("sanya_scp079_ex_cost_doorbeep", 5);
			scp079_ex_level_nuke = SanyaPlugin.instance.Config.GetInt("sanya_scp079_ex_level_nuke", 3);
			scp079_ex_cost_nuke = SanyaPlugin.instance.Config.GetFloat("sanya_scp079_ex_cost_nuke", 50);
			scp079_ex_level_airbomb = SanyaPlugin.instance.Config.GetInt("sanya_scp079_ex_level_airbomb", 4);
			scp079_ex_cost_airbomb = SanyaPlugin.instance.Config.GetFloat("sanya_scp079_ex_cost_airbomb", 100);
			scp079_ex_level_gaz = SanyaPlugin.instance.Config.GetInt("sanya_scp079_ex_level_gaz", 4);
			scp079_ex_cost_gaz = SanyaPlugin.instance.Config.GetFloat("sanya_scp079_ex_cost_gaz", 90);
			//GAZ
			A2Timer = SanyaPlugin.instance.Config.GetInt("b079_a2_timer", 5);
			A2TimerGas = SanyaPlugin.instance.Config.GetInt("b079_a2_gas_timer", 10);
			A2Exp = SanyaPlugin.instance.Config.GetFloat("b079_a2_exp", 35f);
			A2BlacklistRooms = SanyaPlugin.instance.Config.GetStringList("b079_a2_blacklisted_rooms");
			HelpMsgA2 = SanyaPlugin.instance.Config.GetString("b079_help_a2", "Activate a memetic in the current room (only on humans)");
			FailA2Msg = SanyaPlugin.instance.Config.GetString("b079_msg_a2_fail", "Memetics don't work here!");
			RunA2Msg = SanyaPlugin.instance.Config.GetString("b079_msg_a2_run", "Activating...");
			A2WarnMsg = SanyaPlugin.instance.Config.GetString("b079_msg_a2_warn", "<color=#ff0000>MEMETIC KILL AGENT will activate in this room in $seconds seconds.</color>");
			A2ActiveMsg = SanyaPlugin.instance.Config.GetString("b079_msg_a2_active", "<color=#ff0000>MEMETIC KILL AGENT ACTIVATED.</color>");

			if (A2BlacklistRooms == null)
			{
				A2BlacklistRooms = new List<string>();
			}

			Log.Info("[SanyaPluginConfig] Reloaded!");
		}

		internal static string GetConfigs()
		{
			string returned = "\n";

			FieldInfo[] infoArray = typeof(Configs).GetFields(BindingFlags.Static | BindingFlags.NonPublic);

			foreach (FieldInfo info in infoArray)
			{
				returned += $"{info.Name}: {info.GetValue(null)}\n";
			}

			return returned;
		}
	}
	*/
}