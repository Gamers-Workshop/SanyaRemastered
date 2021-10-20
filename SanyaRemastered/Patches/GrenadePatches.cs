using System;
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
			ItemBase itemBase;
			if (!InventoryItemLoader.AvailableItems.TryGetValue(__instance.Info.ItemId, out itemBase))
			{
				return false;
			}
			ThrowableItem throwableItem;
			if ((throwableItem = (itemBase as ThrowableItem)) == null)
			{
				return false;
			}
			__instance.Info.Locked = true;
			__instance._attacker = attacker;
			Methods.SpawnGrenade(__instance.Rb.position, throwableItem.ItemTypeId,0.1f ,Player.Get(attacker.NetId));
			__instance.DestroySelf();
			return false;
		}
	}
}
