using Exiled.API.Features;
using HarmonyLib;
using Interactables;
using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using System.Collections.Generic;

namespace SanyaRemastered.Patches
{
	//[HarmonyPatch(typeof(DoorVariant), nameof(DoorVariant.ServerInteract))]
	public static class DoorInteractPatch
	{
		public static bool Prefix(DoorVariant __instance, ReferenceHub ply, byte colliderId)
		{
			try
			{
				if (SanyaRemastered.Instance.Config.ScpCantInteract && SanyaRemastered.Instance.Config.ScpCantInteractList.TryGetValue("UseElevator", out List<RoleType> roles) && SanyaRemastered.Instance.Config.ScpCantInteractList.TryGetValue("UseElevator", out List<RoleType> roles2))
				{
					if (__instance.NetworkTargetState)
					{
						if (roles.Contains(ply.characterClassManager.CurRole.roleId))
						{
							return false;
						}
					}
					else
					{
						if (roles2.Contains(ply.characterClassManager.CurRole.roleId))
						{
							return false;
						}
					}
				}
				return true;
			}
			catch (System.Exception ex)
			{
				Log.Error(ex);
				return true;
			}
		}
	}
	//[HarmonyPatch(typeof(BasicDoor), nameof(BasicDoor.AllowInteracting))]
	public static class DoorAllowInteractingPatch
	{
		public static bool Prefix(BasicDoor __instance,ReferenceHub ply, byte colliderId, ref bool __result)
		{
			try
			{
				if (ply == null)
                {
					if (SanyaRemastered.Instance.Config.ScpCantInteract && SanyaRemastered.Instance.Config.ScpCantInteractList.TryGetValue("UseElevator", out List<RoleType> roles) && SanyaRemastered.Instance.Config.ScpCantInteractList.TryGetValue("UseElevator", out List<RoleType> roles2))
					{
						if (__instance.NetworkTargetState)
						{
							if (roles.Contains(ply.characterClassManager.CurRole.roleId))
							{
								__result = false;
								return false;
							}
						}
						else
						{
							if (roles2.Contains(ply.characterClassManager.CurRole.roleId))
							{
								__result = false;
								return false;
							}
						}
					}
				}
				return true;
			}
			catch (System.Exception ex)
			{
				Log.Error(ex);
				return true;
			}
		}
	}
}