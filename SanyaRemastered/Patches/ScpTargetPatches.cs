using Exiled.API.Features;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SanyaRemastered.Patches
{
	[HarmonyPatch(typeof(CharacterClassManager), nameof(CharacterClassManager.IsTargetForSCPs))]
	public static class ScpTargetPatch
	{
		public static bool Prefix(CharacterClassManager __instance,ref bool __result)
		{
			__result = __instance.CurRole.team != Team.SCP && __instance.CurRole.team != Team.RIP && __instance.CurRole.team != Team.CHI && __instance.CurRole.team != Team.TUT;
			return false;
		}
	}
}