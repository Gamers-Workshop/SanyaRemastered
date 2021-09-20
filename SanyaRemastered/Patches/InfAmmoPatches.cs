using Exiled.API.Features;
using HarmonyLib;
using InventorySystem.Items.MicroHID;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanyaRemastered.Patches
{
    public static class InfAmmoPatches
    {
		/*[HarmonyPatch(typeof(MicroHIDItem), nameof(MicroHIDItem.ExecuteServerside))]
		public static class FireArmsAmmoHid
		{
			public static void Postfix(MicroHIDItem __instance)
			{
				if (Player.Get(__instance.gameObject).SessionVariables.ContainsKey("InfAmmo"))
				{
					__instance.RemainingEnergy = 1;
					__instance.ServerSendStatus(HidStatusMessageType.EnergySync, __instance.EnergyToByte);
				}
			}
		}
		//[HarmonyPatch(typeof(MicroHIDItem), nameof(MicroHIDItem.ExecuteServerside))]
		public static class InefAmmoHid
		{
			public static void Postfix(MicroHIDItem __instance)
			{
				if (Player.Get(__instance.gameObject).SessionVariables.ContainsKey("InfAmmo"))
				{
					__instance.RemainingEnergy = 1;
					__instance.ServerSendStatus(HidStatusMessageType.EnergySync, __instance.EnergyToByte);
				}
			}
		}
		[HarmonyPatch(typeof(MicroHIDItem), nameof(MicroHIDItem.ExecuteServerside))]
		public static class InfAmmoHid
		{
			public static void Postfix(MicroHIDItem __instance)
			{
				if (Player.Get(__instance.gameObject).SessionVariables.ContainsKey("InfAmmo"))
				{
					__instance.RemainingEnergy = 1;
					__instance.ServerSendStatus(HidStatusMessageType.EnergySync, __instance.EnergyToByte);
				}
			}
		}*/
	}
}