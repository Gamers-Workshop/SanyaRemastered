using Exiled.API.Features;
using HarmonyLib;
using InventorySystem;
using InventorySystem.Items.Pickups;
using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SanyaRemastered.Patches
{
	[HarmonyPatch(typeof(InventoryExtensions), nameof(InventoryExtensions.ServerDropEverything))]
	public static class ServerDropEverythingPatches
	{
		public static bool Prefix(this Inventory inv, ItemPickupBase __result, InventorySystem.Items.ItemBase item, PickupSyncInfo psi, bool spawn = true)
		{
			try
			{
				if (!NetworkServer.active)
				{
					throw new InvalidOperationException("Method ServerCreatePickup can only be executed on the server.");
				}
				InventorySystem.Items.Pickups.ItemPickupBase itemPickupBase = UnityEngine.Object.Instantiate(item.PickupDropModel, inv.transform.position, global::ReferenceHub.GetHub(inv.gameObject).PlayerCameraReference.rotation * item.PickupDropModel.transform.rotation);
				itemPickupBase.NetworkInfo = psi;
				if (spawn)
				{
					NetworkServer.Spawn(itemPickupBase.gameObject);
				}
				itemPickupBase.InfoReceived(default(InventorySystem.Items.Pickups.PickupSyncInfo), psi);
				__result = itemPickupBase;
				itemPickupBase.TryGetComponent(out Rigidbody rigidbody);
				rigidbody.velocity = inv._hub.playerMovementSync.PlayerVelocity;
				return false;
			}
			catch (System.Exception ex)
			{
				Log.Error("ServerDropEverything" + ex);
			}
			return true;
		}
	}
}
