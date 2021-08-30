﻿using Exiled.API.Features;
using HarmonyLib;
using SanyaRemastered.Data;
using SanyaRemastered.Functions;
using UnityEngine;

namespace SanyaRemastered.Patches
{
    [HarmonyPatch(typeof(Recontainer079), nameof(Recontainer079.Recontain))]
    public static class BeginOverchargePatches
    {
        public static void Postfix()
        {
            Methods.SendSubtitle(Subtitles.OverchargeStart,5);
        }
    }
    [HarmonyPatch(typeof(Recontainer079), nameof(Recontainer079.EndOvercharge))]
    public static class EndOverchargePatches
    {
        public static void Postfix()
        {
            Methods.SendSubtitle(Subtitles.OverchargeFinish,5);
        }
    }
}