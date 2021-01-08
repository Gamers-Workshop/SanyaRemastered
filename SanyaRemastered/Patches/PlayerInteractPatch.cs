using Exiled.API.Features;
using HarmonyLib;
namespace SanyaRemastered.Patches
{
	public static class OpenDoor
	{
		//[HarmonyPatch(typeof(PlayerInteract), nameof(PlayerInteract.CmdOpenDoor))]
		public static bool Prefix(PlayerInteract __instance)
		{
			if (SanyaRemastered.Instance.Config.Scp049_2DontOpenDoorAnd106
				&& (__instance._ccm.CurRole.roleId == RoleType.Scp106
				|| __instance._ccm.CurRole.roleId == RoleType.Scp0492))
			{
				return false;
			}
			return true;
		}
	}
}