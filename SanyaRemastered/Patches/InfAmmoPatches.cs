using Exiled.API.Features;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanyaRemastered.Patches
{
    public static class InfAmmoPatches
    {
		/*[HarmonyPatch(typeof(WeaponManager), "CallCmdShoot")]
		public static class InfAmmoGun
		{
			public static void Postfix(WeaponManager __instance)
			{
				if (Player.Get(__instance._hub).SessionVariables.ContainsKey("InfAmmo"))
                {
					int itemIndex = __instance._hub.inventory.GetItemIndex();
					__instance._hub.inventory.items.ModifyDuration(itemIndex, __instance._hub.inventory.items[itemIndex].durability + 1f);
				}
			}
		}*/
		[HarmonyPatch(typeof(InventorySystem.Items.MicroHID.MicroHIDItem), nameof(InventorySystem.Items.MicroHID.MicroHIDItem.ExecuteServerside))]
		public static class InfAmmoHid
		{
			public static void Postfix(InventorySystem.Items.MicroHID.MicroHIDItem __instance)
			{
				if (Player.Get(__instance.gameObject).SessionVariables.ContainsKey("InfAmmo"))
				{
					__instance.RemainingEnergy = 1;
				}
			}
		}
	}
}