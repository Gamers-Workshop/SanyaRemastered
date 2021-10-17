using Exiled.API.Features;
using HarmonyLib;
using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using MapGeneration.Distributors;
using System.Collections.Generic;

namespace SanyaRemastered.Patches
{
	public static class DoorInteractPatch
	{
		[HarmonyPatch(typeof(BreakableDoor), nameof(BreakableDoor.AllowInteracting))]
		public static bool Prefix(BreakableDoor __instance, ReferenceHub ply)
		{
			try
			{
				if (SanyaRemastered.Instance.Config.ScpCantInteract)
				{
					Door door = Door.Get(__instance);
					Log.Debug($"[OnDoorAction] is ___instance = null {__instance == null} is DoorVariant null {door == null} player is {ply.nicknameSync._firstNickname}");
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
			catch (System.Exception ex)
			{
				Log.Error(ex);
				return true;
			}
		}
		[HarmonyPatch(typeof(BasicDoor), nameof(BasicDoor.AllowInteracting))]
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
			catch (System.Exception ex)
			{
				Log.Error(ex);
				return true;
			}
		}
		[HarmonyPatch(typeof(CheckpointDoor), nameof(CheckpointDoor.AllowInteracting))]
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
			catch (System.Exception ex)
			{
				Log.Error(ex);
				return true;
			}
		}
		[HarmonyPatch(typeof(PryableDoor), nameof(PryableDoor.AllowInteracting))]
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
			catch (System.Exception ex)
			{
				Log.Error(ex);
				return true;
			}
		}
	}
}