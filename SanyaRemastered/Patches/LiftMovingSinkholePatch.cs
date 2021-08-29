using HarmonyLib;
using SanyaRemastered.Functions;
using UnityEngine;

namespace SanyaRemastered.Patches
{
	/*[HarmonyPatch(typeof(Lift), nameof(Lift.MovePlayers))]
	public static class LiftMovingSinkholePatch
	{
		public static void Postfix(Lift __instance, Transform target)
		{
			if (__instance.InRange(SanyaRemastered.Instance.Handlers.Sinkhole.transform.position, out var gameObject, 1f, 2f, 1f)
				&& gameObject.transform != target)
				Methods.MoveNetworkIdentityObject(SanyaRemastered.Instance.Handlers.Sinkhole, target.TransformPoint(gameObject.transform.InverseTransformPoint(SanyaRemastered.Instance.Handlers.Sinkhole.transform.position)));
		}
	}*/
}
