using HarmonyLib;
using SanyaRemastered.Data;
using SanyaPlugin;
using Respawning.NamingRules;
using SanyaPlugin.Functions;
using Respawning;
using Exiled.API.Features;

namespace SanyaRemastered.Patches
{
	[HarmonyPatch(typeof(UnitNamingRule), nameof(UnitNamingRule.AddCombination))]
	public static class NTFUnitPatch
	{
		public static void Postfix(ref string regular)
		{
			if (PlayerManager.localPlayer == null || PlayerManager.localPlayer?.GetComponent<RandomSeedSync>().seed == 0) return;
			Log.Debug($"[NTFUnitPatch] unit:{regular}");

			if (SanyaPlugin.SanyaPlugin.Instance.Config.CassieSubtitle)
			{
				int SCPCount = 0;
				foreach (Player i in Player.List)
				{
					if (i.Team == Team.SCP && i.Role != RoleType.Scp0492)
					{
						SCPCount++;
					}
				}

				if (SCPCount > 0)
				{
					Methods.SendSubtitle(Subtitles.MTFRespawnSCPs.Replace("{0}", regular).Replace("{1}", SCPCount.ToString()), 30);
				}
				else
				{
					Methods.SendSubtitle(Subtitles.MTFRespawnNOSCPs.Replace("{0}", regular), 30);
				}
			}
		}
	}
	[HarmonyPatch(typeof(RespawnEffectsController), nameof(RespawnEffectsController.ServerExecuteEffects))]
	public static class RespawnEffectPatch
	{
		public static bool Prefix(RespawnEffectsController.EffectType type, SpawnableTeamType team)
		{
			Log.Debug($"[RespawnEffectPatch] {type}:{team}");
			if (SanyaPlugin.SanyaPlugin.Instance.Config.StopRespawnAfterDetonated && AlphaWarheadController.Host.detonated && type == RespawnEffectsController.EffectType.Selection) return false;
			else return true;
		}
	}
}
