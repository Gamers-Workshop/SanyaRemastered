using Assets._Scripts.Dissonance;
using HarmonyLib;
using PlayableScps;
using SanyaRemastered;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace SanyaRemastered.Patches
{
	//[HarmonyPatch(typeof(Scp096), nameof(Scp096.ParseVisionInformation))]
	/*public static class Scp096ParseVisionInformation
	{
		public static void Postfix(Scp096 __instance, VisionInformation info)
		{
			if (info.IsLooking)
			{
				ReferenceHub hub = ReferenceHub.GetHub(info.Source);
				//__instance
				if (Scp096Helper.singleton.targets.ContainsKey(__instance.Hub))
				{
					List<ReferenceHub> list = Scp096Helper.singleton.targets[__instance.Hub];
					if (!list.Contains(hub))
						list.Add(hub);
				}
			}
		}
	}*/
}