using HarmonyLib;
using SanyaRemastered.Data;
using Respawning.NamingRules;
using SanyaRemastered.Functions;
using Respawning;
using Exiled.API.Features;
using MapGeneration;

namespace SanyaRemastered.Patches
{
	[HarmonyPatch(typeof(RespawnEffectsController), nameof(RespawnEffectsController.ServerExecuteEffects))]
	public static class RespawnEffectPatch
	{
		public static bool Prefix(RespawnEffectsController.EffectType type, SpawnableTeamType team)
		{
			Log.Debug($"[RespawnEffectPatch] {type}:{team}");
			if (SanyaRemastered.Instance.Config.StopRespawnAfterDetonated && AlphaWarheadController.Detonated && type is RespawnEffectsController.EffectType.Selection) return false;
			else return true;
		}
	}
}
