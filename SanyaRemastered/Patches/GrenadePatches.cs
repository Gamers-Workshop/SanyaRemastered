using System;
using Exiled.API.Enums;
using Exiled.API.Features;
using HarmonyLib;
using InventorySystem;
using InventorySystem.Items;
using InventorySystem.Items.ThrowableProjectiles;
using Mirror;
using SanyaRemastered.Functions;
using UnityEngine;

namespace SanyaRemastered.Patches
{
	[HarmonyPatch(typeof(TimedGrenadePickup), nameof(TimedGrenadePickup.OnExplosionDetected))]
	public static class FragGrenadeChainPatch
	{
		public static bool Prefix(TimedGrenadePickup __instance,Footprinting.Footprint attacker, Vector3 source, float range)
		{
			if (!SanyaRemastered.Instance.Config.GrenadeChainSametiming || __instance.Info.ItemId == ItemType.SCP018) return true;

			if (Vector3.Distance(__instance.transform.position, source) / range > 0.4f)
			{
				return false;
			}
            if (!InventoryItemLoader.AvailableItems.TryGetValue(__instance.Info.ItemId, out ItemBase itemBase))
            {
                return false;
            }

            if ((itemBase as ThrowableItem) is null)
			{
				return false;
			}
			__instance.Info.Locked = true;
			__instance._attacker = attacker;
			if (__instance.Info.ItemId != ItemType.SCP018)
    			Methods.SpawnGrenade(__instance.Rb.position, __instance.Info.ItemId, 0.1f , attacker.Hub);
			__instance.DestroySelf();
			return false;
		}
	}
}
