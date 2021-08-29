using Exiled.API.Features;
using HarmonyLib;
using Hints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SanyaRemastered.Patches
{
	/*[HarmonyPatch(typeof(Handcuffs), nameof(Handcuffs.CallCmdFreeTeammate))]
	public static class HandcuffsPatch
	{
		public static bool Prefix(Handcuffs __instance, GameObject target)
		{
			if (!SanyaRemastered.Instance.Config.Scp049Real) return true;

			if (!__instance._interactRateLimit.CanExecute(true))
			{
				return false;
			}
			if (target == null || Vector3.Distance(target.transform.position, __instance.transform.position) > __instance.raycastDistance * 1.1f)
			{
				return false;
			}
			ReferenceHub.GetHub(target).hand.NetworkCufferId = -1;

			return false;
		}
	}*/
}
