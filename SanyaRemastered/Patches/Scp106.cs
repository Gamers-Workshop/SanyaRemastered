using HarmonyLib;
using MEC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SanyaRemastered.Patches
{
	//transpiler - fix
	[HarmonyPatch(typeof(Scp106PlayerScript), nameof(Scp106PlayerScript.CallCmdUsePortal))]
	public static class Scp106PortalAnimationPatch
	{
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			foreach (var code in instructions)
			{
				if (code.opcode == OpCodes.Call)
					if (code.operand != null
						&& code.operand is MethodBase methodBase
						&& methodBase.Name != nameof(Scp106PlayerScript._DoTeleportAnimation))
						yield return code;
					else
						yield return new CodeInstruction(OpCodes.Call, SymbolExtensions.GetMethodInfo(() => _DoTeleportAnimation(null)));
				else
					yield return code;
			}
		}

		private static IEnumerator<float> _DoTeleportAnimation(Scp106PlayerScript scp106PlayerScript)
		{
			if (scp106PlayerScript.portalPrefab != null && !scp106PlayerScript.goingViaThePortal)
			{
				scp106PlayerScript.RpcTeleportAnimation();
				scp106PlayerScript.goingViaThePortal = true;
				yield return Timing.WaitForSeconds(3.5f);
				scp106PlayerScript._hub.playerMovementSync.OverridePosition(scp106PlayerScript.portalPrefab.transform.position + Vector3.up * 1.5f, 0f, false);
				yield return Timing.WaitForSeconds(3.5f);
				if (AlphaWarheadController.Host.detonated && scp106PlayerScript.transform.position.y < 800f)
					scp106PlayerScript._hub.playerStats.HurtPlayer(new PlayerStats.HitInfo(9000f, "WORLD", DamageTypes.Nuke, 0), scp106PlayerScript.gameObject, true);
				scp106PlayerScript.goingViaThePortal = false;
			}
		}
	}
}
