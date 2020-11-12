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
	/*[HarmonyPatch(typeof(Scp096), nameof(Scp096.GetVisionInformation))]
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
	}*/
}
