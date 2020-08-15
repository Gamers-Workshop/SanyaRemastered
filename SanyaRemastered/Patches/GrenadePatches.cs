using System;
using Grenades;
using HarmonyLib;
using Mirror;
using UnityEngine;

namespace SanyaRemastered.Patches
{
	[HarmonyPatch(typeof(FragGrenade), nameof(FragGrenade.ChangeIntoGrenade))]
	public static class FragGrenadeChainPatch
	{
		public static bool Prefix(FragGrenade __instance, Pickup item, ref bool __result)
		{
			if (!SanyaPlugin.SanyaPlugin.Instance.Config.GrenadeChainSametiming) return true;

			GrenadeSettings grenadeSettings = null;
			int i = 0;
			while (i < __instance.thrower.availableGrenades.Length)
			{
				GrenadeSettings grenadeSettings2 = __instance.thrower.availableGrenades[i];
				if (grenadeSettings2.inventoryID == item.ItemId)
				{
					if (!__instance.chainSupportedGrenades.Contains(i))
					{
						__result = false;
						return false;
					}
					grenadeSettings = grenadeSettings2;
					break;
				}
				else
				{
					i++;
				}
			}
			if (grenadeSettings == null)
			{
				__result = false;
				return false;
			}
			Transform transform = item.transform;
			Grenade component = UnityEngine.Object.Instantiate(grenadeSettings.grenadeInstance, transform.position, transform.rotation).GetComponent<Grenade>();
			component.fuseDuration = 0.1f;
			component.InitData(__instance, item);
			NetworkServer.Spawn(component.gameObject);
			item.Delete();
			__result = true;
			return false;
		}
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
		}/*
	[HarmonyPatch(typeof(TeslaGate), nameof(TeslaGate.IsInvoking))]
	public static class TeslaExplodeGrenade
	{
		public static bool Prefix()
		{
			if (!SanyaPlugin.SanyaPlugin.Instance.Config.TeslaExplodeGrenade) return true;
			{
				return true;
			}
		}*/
	}
	
}
