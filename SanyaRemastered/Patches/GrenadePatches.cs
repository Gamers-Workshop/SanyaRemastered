using System;
using Grenades;
using HarmonyLib;
using SanyaPlugin;

namespace SanyaRemastered.Patches
{

	//override - 10.0.0 checked
	[HarmonyPatch(typeof(Grenade), nameof(Grenade.ServersideExplosion))]
	public static class GrenadeLogPatch
	{
		public static bool Prefix(Grenade __instance, ref bool __result)
		{
			try
			{
				if (__instance.thrower?.name != "Host")
				{
					string text = (__instance.thrower != null) ? (__instance.thrower.hub.characterClassManager.UserId + " (" + __instance.thrower.hub.nicknameSync.MyNick + ")") : "(UNKNOWN)";
					ServerLogs.AddLog(ServerLogs.Modules.Logger, "Player " + text + "'s " + __instance.logName + " grenade exploded.", ServerLogs.ServerLogType.GameEvent);
				}
				__result = true;
				return false;
			}
			catch (Exception)
			{
				return true;
			}
		}
	}
}
