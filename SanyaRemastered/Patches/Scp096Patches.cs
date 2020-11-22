using HarmonyLib;
using PlayableScps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace SanyaRemastered.Patches
{
	//Enrange Scp096 au contact
	//[HarmonyPatch(typeof(Scp096), nameof(Scp096.GetVisionInformation))]
	public static class Scp096GetVisionPatch
	{
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
		{
			var newInst = instructions.ToList();
			var index = newInst.FindLastIndex(x => x.opcode == OpCodes.Neg) + 8;
			var lastTargetindex = newInst.FindLastIndex(x => x.opcode == OpCodes.Ldfld && x.operand is FieldInfo info && info == AccessTools.Field(typeof(VisionInformation), nameof(VisionInformation.Target)));
			var passlabel = newInst[lastTargetindex - 5].labels[0];

			newInst.InsertRange(index, new[] {
				new CodeInstruction(OpCodes.Ldc_R4, 1.5f),
				new CodeInstruction(OpCodes.Ldloc_0),
				new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(VisionInformation), nameof(VisionInformation.Distance))),
				new CodeInstruction(OpCodes.Bge_Un_S, passlabel),
			});

			for (int i = 0; i < newInst.Count; i++)
				yield return newInst[i];
		}
	}

	[HarmonyPatch(typeof(Scp096), nameof(Scp096.ParseVisionInformation))]
	public static class Scp096PareseVIsionInformation
    {
		public static void Postfix(Scp096 __instance, VisionInformation info)
        {
			if (info.Looking)
			{
				ReferenceHub hub = ReferenceHub.GetHub(info.Source);
				//__instance
				if(Scp096Helper.singleton.targets.ContainsKey(__instance.Hub))
                {
					List<ReferenceHub> list = Scp096Helper.singleton.targets[__instance.Hub];
					if(!list.Contains(hub))
						list.Add(hub);
                }
            }
        }

	}

	[HarmonyPatch(typeof(Scp096), nameof(Scp096.EndEnrage))]
	public static class Scp096EndEnrage
	{
		public static bool Prefix(Scp096 __instance)
		{
			if (Scp096Helper.singleton.targets.ContainsKey(__instance.Hub))
			{
				List<ReferenceHub> ReferenceHubs = Scp096Helper.singleton.targets[__instance.Hub];
				foreach(ReferenceHub hub in ReferenceHubs)
                {
					if(hub.characterClassManager.CurClass == RoleType.Spectator)
                    {
						ReferenceHubs.Remove(hub);
                    }
                }
				Scp096Helper.singleton.targets[__instance.Hub] = ReferenceHubs;
				if (ReferenceHubs.Count <= 0)
                {
					return true;
                }
			}
			return false;
		}

	}
}
