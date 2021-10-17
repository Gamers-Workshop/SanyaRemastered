using HarmonyLib;
using NorthwoodLib.Pools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static HarmonyLib.AccessTools;

namespace SanyaRemastered.Patches
{
	/*[HarmonyPatch(typeof(Lift), nameof(Lift._LiftAnimation))]
	public static class LiftPatches
	{
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			var newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);
			var index = newInstructions.FindLastIndex(op => op.opcode == OpCodes.Blt);

			newInstructions.InsertRange(++index, new[]
			{
				new CodeInstruction(OpCodes.Ldarg_0),
				new CodeInstruction(OpCodes.Ldfld, Field(typeof(Lift), nameof(Lift))),

				new CodeInstruction(OpCodes.Call, Method(typeof(LiftPatches), nameof(LiftPatches.WaitingLift))),


			});

			for (int z = 0; z < newInstructions.Count; z++)
				yield return newInstructions[z];

			ListPool<CodeInstruction>.Shared.Return(newInstructions);
		}
		public static IEnumerable<float> WaitingLift(Lift __instance)
		{
			Transform target = null;
			foreach (Lift.Elevator elevator in __instance.elevators)
			{
				if (!elevator.door.GetBool(Lift.IsOpen))
				{
					target = elevator.target;
				}
			}
			Lift.Status previousStatus = __instance.status;
			__instance.SetStatus(2);
			int j;
			int i;
			for (i = 0; i < 35; i = j + 1)
			{
				yield return 0f;
				j = i;
			}
			__instance.RpcPlayMusic();
			for (i = 0; i < 100; i = j + 1)
			{
				yield return 0f;
				j = i;
			}
			__instance.MovePlayers(target);
			i = 0;
			while (i < (__instance.movingSpeed - 2f) * 50f)
			{
				yield return 0f;
				j = i;
				i = j + 1;
			}
			__instance.SetStatus((previousStatus == Lift.Status.Down) ? (byte)0 : (byte)1);
			for (i = 0; i < 100 * SanyaRemastered.Instance.Config.WaitForUseLift; i = j + 1)
			{
				yield return 0f;
				j = i;
			}

			__instance.operative = true;
			yield break;
		}
	}*/
}
