using Exiled.API.Features;
using HarmonyLib;
using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using MapGeneration.Distributors;
using System.Collections.Generic;

namespace SanyaRemastered.Patches
{
	[HarmonyPatch(typeof(AirlockController), nameof(AirlockController.OnDoorAction))]
	public static class DoorInteractPatch
	{
		public static bool Prefix(AirlockController __instance, DoorVariant door, DoorAction action, ReferenceHub ply)
		{
			try
			{
				if (SanyaRemastered.Instance.Config.ScpCantInteract)
                {
                    {
						if (SanyaRemastered.Instance.Config.ScpCantInteractList.TryGetValue("DoorInteractOpen", out List<RoleType> roles) && SanyaRemastered.Instance.Config.ScpCantInteractList.TryGetValue("UseElevator", out List<RoleType> roles2))
						{
							if (action == DoorAction.Opened || action == DoorAction.Closed && !door.TargetState)
							{
								if (roles.Contains(ply.characterClassManager.CurRole.roleId))
								{
									return false;
								}
							}
						}
					}
                    {
						if (SanyaRemastered.Instance.Config.ScpCantInteractList.TryGetValue("DoorInteractClose", out List<RoleType> roles) && SanyaRemastered.Instance.Config.ScpCantInteractList.TryGetValue("UseElevator", out List<RoleType> roles2))
						{
							if (action == DoorAction.Opened || action == DoorAction.Closed && door.TargetState)
							{
								if (roles.Contains(ply.characterClassManager.CurRole.roleId))
								{
									return false;
								}
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