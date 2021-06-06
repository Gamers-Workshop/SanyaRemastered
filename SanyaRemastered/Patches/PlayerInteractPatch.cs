using Exiled.API.Features;
using HarmonyLib;
namespace SanyaRemastered.Patches
{
	//[HarmonyPatch(typeof(Interactables.Interobjects.DoorUtils.DoorVariant), nameof(Interactables.Interobjects.DoorUtils.DoorVariant.AllowInteracting))]
	public class Change914Knob
	{
		public bool Prefix(Interactables.Interobjects.DoorUtils.DoorVariant __instance,
			ReferenceHub ply,
			byte colliderId)
			{
			if (SanyaRemastered.Instance.Config.Scp049_2DontOpenDoorAnd106
				&& (ply.characterClassManager.CurRole.roleId == RoleType.Scp106
				|| ply.characterClassManager.CurRole.roleId == RoleType.Scp0492))
			{
				ply.GetComponent<SanyaRemasteredComponent>().AddHudBottomText("Test yamato", 5);
				return false;
			}
			return true;
		}
	}
}