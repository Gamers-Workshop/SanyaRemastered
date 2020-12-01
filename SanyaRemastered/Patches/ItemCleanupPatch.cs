using Exiled.API.Features;
using HarmonyLib;
using Mirror;
using System.Collections.Generic;
using UnityEngine;

namespace SanyaRemastered.Patches
{
	[HarmonyPatch(typeof(Inventory), nameof(Inventory.SetPickup))]
	public static class ItemCleanupPatch
	{
		public static Dictionary<GameObject, float> items = new Dictionary<GameObject, float>();

		public static bool Prefix(Inventory __instance, ref Pickup __result, ItemType droppedItemId, float dur, Vector3 pos, Quaternion rot, int s, int b, int o)
		{
			if (SanyaRemastered.Instance.Config.ItemCleanup < 0 || __instance.name == "Host") return true;

			if (SanyaRemastered.Instance.Config.ItemCleanupIgnoreParsed.Contains(droppedItemId))
			{
				Log.Debug($"[ItemCleanupPatch] Ignored:{droppedItemId}");
				return true;
			}

			Log.Debug($"[ItemCleanupPatch] {droppedItemId}{pos} Time:{Time.time} Cleanuptimes:{SanyaRemastered.Instance.Config.ItemCleanup}");

			if (droppedItemId < ItemType.KeycardJanitor)
			{
				__result = null;
				return false;
			}
			GameObject gameObject = UnityEngine.Object.Instantiate(__instance.pickupPrefab);
			NetworkServer.Spawn(gameObject);
			items.Add(gameObject, Time.time);
			/*gameObject.GetComponent<Pickup>().SetupPickup(new Pickup.PickupInfo
			{
				itemId = droppedItemId,
				durability = dur,
				weaponMods = new int[]
				{
				s,
				b,
				o
				},
				ownerPlayer = __instance.gameObject
			}, pos, rot);
			__result = gameObject.GetComponent<Pickup>();*/
			return false;
		}
	}
}