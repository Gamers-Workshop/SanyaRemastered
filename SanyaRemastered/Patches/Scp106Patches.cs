using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SanyaRemastered.Patches
{
	[HarmonyPatch(typeof(PlayerInteract), nameof(PlayerInteract.UserCode_CmdContain106))]
	public static class Scp106Patches
	{
		public static bool Prefix(PlayerInteract __instance)
		{
			if (SanyaRemastered.Instance.Config.FemurBreakerCanBeUsedWithNo106)
			{
				if (!__instance.CanInteract)
				{
					return false;
				}
				if (!UnityEngine.Object.FindObjectOfType<LureSubjectContainer>().allowContain || (__instance._ccm.CurRole.team == Team.SCP && __instance._ccm.CurClass != RoleType.Scp106) || !__instance.ChckDis(GameObject.FindGameObjectWithTag("FemurBreaker").transform.position) || OneOhSixContainer.used || __instance._ccm.CurRole.team == Team.RIP)
				{
					return false;
				}
				foreach (KeyValuePair<GameObject, ReferenceHub> keyValuePair in ReferenceHub.GetAllHubs())
				{
					if (keyValuePair.Value.characterClassManager.CurClass == RoleType.Scp106 && !keyValuePair.Value.characterClassManager.GodMode)
					{
						keyValuePair.Value.scp106PlayerScript.Contain(new Footprinting.Footprint(__instance._hub));
					}
				}
				{
					__instance.RpcContain106(__instance.gameObject);
					OneOhSixContainer.used = true;
				}
				__instance.OnInteract();
				return false;
			}
			return true;
		}
	}
}
