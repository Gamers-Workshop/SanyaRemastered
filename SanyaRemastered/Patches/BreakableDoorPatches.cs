﻿using HarmonyLib;
using Interactables.Interobjects.DoorUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanyaRemastered.Patches
{
    [HarmonyPatch(typeof(Interactables.Interobjects.BreakableDoor), nameof(Interactables.Interobjects.BreakableDoor.ServerDamage))]
    public static class DoorNotBreakPatches
    {
        public static bool Prefix(Interactables.Interobjects.BreakableDoor __instance)
        {
            if (SanyaRemastered.Instance.Config.Scp096Real && __instance.TryGetComponent<DoorNametagExtension>(out var doorNametagExtension) && doorNametagExtension._nametag == "096")
            {
                __instance.TargetState = true;
                return false;
            }
            return true;
        }
    }
}