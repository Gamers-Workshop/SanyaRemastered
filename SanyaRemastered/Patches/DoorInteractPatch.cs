using Exiled.API.Features;
using HarmonyLib;
using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using MapGeneration.Distributors;
using System.Collections.Generic;

namespace SanyaRemastered.Patches
{
	[HarmonyPatch(typeof(BreakableDoor), nameof(BreakableDoor.AllowInteracting))]
	public static class BreakableDoorAllowInteracting
    {
		public static bool Prefix(BreakableDoor __instance, ReferenceHub ply)
		{
			try
			{
				if (SanyaRemastered.Instance.Config.ScpCantInteract)
				{
					Door door = Door.Get(__instance);
					if (!door.IsOpen && SanyaRemastered.Instance.Config.ScpCantInteractList.TryGetValue("DoorInteractOpen", out var role) && role.Contains(ply.characterClassManager.CurRole.roleId))
					{
						return false;
					}
					if (door.IsOpen && SanyaRemastered.Instance.Config.ScpCantInteractList.TryGetValue("DoorInteractClose", out var role2) && role2.Contains(ply.characterClassManager.CurRole.roleId))
					{
						return false;
					}
				}
				return true;
			}
			catch
			{
				return true;
			}
		}
	}

	[HarmonyPatch(typeof(BasicDoor), nameof(BasicDoor.AllowInteracting))]
	public static class BasicDoorAllowInteracting
    {
		public static bool Prefix(BasicDoor __instance, ReferenceHub ply)
		{
			try
			{
				if (SanyaRemastered.Instance.Config.ScpCantInteract)
				{
					Door door = Door.Get(__instance);
					if (!door.IsOpen && SanyaRemastered.Instance.Config.ScpCantInteractList.TryGetValue("DoorInteractOpen", out var role) && role.Contains(ply.characterClassManager.CurRole.roleId))
					{
						return false;
					}
					if (door.IsOpen && SanyaRemastered.Instance.Config.ScpCantInteractList.TryGetValue("DoorInteractClose", out var role2) && role2.Contains(ply.characterClassManager.CurRole.roleId))
					{
						return false;
					}
				}
				return true;
			}
			catch 
			{
				return true;
			}
		}
	}

	[HarmonyPatch(typeof(CheckpointDoor), nameof(CheckpointDoor.AllowInteracting))]
	public static class CheckpointDoorAllowInteracting
    {
		public static bool Prefix(CheckpointDoor __instance, ReferenceHub ply)
		{
			try
			{
				if (SanyaRemastered.Instance.Config.ScpCantInteract)
				{
					Door door = Door.Get(__instance);
					if (!door.IsOpen && SanyaRemastered.Instance.Config.ScpCantInteractList.TryGetValue("DoorInteractOpen", out var role) && role.Contains(ply.characterClassManager.CurRole.roleId))
					{
						return false;
					}
					if (door.IsOpen && SanyaRemastered.Instance.Config.ScpCantInteractList.TryGetValue("DoorInteractClose", out var role2) && role2.Contains(ply.characterClassManager.CurRole.roleId))
					{
						return false;
					}
				}
				return true;
			}
			catch
			{
				return true;
			}
		}
	}

	[HarmonyPatch(typeof(PryableDoor), nameof(PryableDoor.AllowInteracting))]
	public static class PryableDoorAllowInteracting
    {
		public static bool Prefix(PryableDoor __instance, ReferenceHub ply)
		{
			try
			{
				if (SanyaRemastered.Instance.Config.ScpCantInteract)
				{
					Door door = Door.Get(__instance);
					if (!door.IsOpen && SanyaRemastered.Instance.Config.ScpCantInteractList.TryGetValue("DoorInteractOpen", out var role) && role.Contains(ply.characterClassManager.CurRole.roleId))
					{
						return false;
					}
					if (door.IsOpen && SanyaRemastered.Instance.Config.ScpCantInteractList.TryGetValue("DoorInteractClose", out var role2) && role2.Contains(ply.characterClassManager.CurRole.roleId))
					{
						return false;
					}
				}
				return true;
			}
			catch
			{
				return true;
			}
		}
	}
	[HarmonyPatch(typeof(AirlockController), nameof(AirlockController.OnDoorAction))]
	public static class AirlockControllerOnDoorAction
	{
		public static bool Prefix(AirlockController __instance, DoorVariant door, DoorAction action, ReferenceHub ply)
		{
			if (SanyaRemastered.Instance.Config.ScpCantInteract)
				try
				{
					{
						if (action == DoorAction.Opened && SanyaRemastered.Instance.Config.ScpCantInteractList.TryGetValue("DoorInteractOpen", out var role) && role.Contains(ply.characterClassManager.CurRole.roleId))
						{
							return false;
						}
						if (action == DoorAction.Closed && SanyaRemastered.Instance.Config.ScpCantInteractList.TryGetValue("DoorInteractClose", out var role2) && role2.Contains(ply.characterClassManager.CurRole.roleId))
						{
							return false;
						}
					}
					if (door.ActiveLocks > 0 || (door != __instance._doorA && door != __instance._doorB) || __instance.AirlockDisabled || __instance._warheadInProgress || !__instance._readyToUse)
					{
						return false;
					}
					if (action == DoorAction.Destroyed)
					{
						__instance.AirlockDisabled = true;
						return false;
					}
					if (__instance._frameCooldownTimer > 0)
					{
						return false;
					}
					if (action == DoorAction.Opened || action == DoorAction.Closed)
					{
						__instance.ToggleAirlock();
					}
					return false;
				}
				catch
				{
					return true;
				}
			return true;
		}
	}
}
