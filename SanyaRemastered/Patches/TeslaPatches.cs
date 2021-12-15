using Exiled.API.Features;
using HarmonyLib;
using InventorySystem.Items.Pickups;
using InventorySystem.Items.Radio;
using InventorySystem.Items.ThrowableProjectiles;
using MEC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SanyaRemastered.Patches
{
    [HarmonyPatch(typeof(TeslaGate), nameof(TeslaGate.PlayerInIdleRange))]
    public static class TeslaNotActiveWhenGodModOrBlackout
    {
        public static bool Prefix(TeslaGate __instance, ref bool __result, ReferenceHub player)
        {
            try
            {
                if (player != null && __instance != null)
                if ((SanyaRemastered.Instance.Config.NoIdlingTeslaGodmodAndBlackout && (Map.FindParentRoom(__instance.gameObject).LightsOff || player.characterClassManager.GodMode))
                    ||
                    (SanyaRemastered.Instance.Config.TeslaNoTriggerRadioPlayer && player.characterClassManager.IsHuman() && player.inventory.UserInventory.Items.Any(x => x.Value.ItemTypeId == ItemType.Radio && x.Value.GetComponent<RadioItem>().IsUsable)))
                {
                    __result = false;
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log.Error("[TeslaNotActiveWhenGodModOrBlackout] " + ex);
            }
            return true;
        }
    }
}