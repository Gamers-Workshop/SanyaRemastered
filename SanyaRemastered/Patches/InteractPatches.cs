using Exiled.API.Features;
using HarmonyLib;
using MapGeneration.Distributors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SanyaRemastered.Patches
{
	public class PlayerInteracting
	{
		[HarmonyPatch(typeof(Scp914.Scp914Controller), nameof(Scp914.Scp914Controller.ServerInteract))]
		public static class PlayerInteract914
		{
			public static bool Prefix(Scp914.Scp914Controller __instance, ReferenceHub ply, byte colliderId)
			{
				try
				{
					if (SanyaRemastered.Instance.Config.ScpCantInteract && SanyaRemastered.Instance.Config.ScpCantInteractList.TryGetValue("Use914", out List<RoleType> roles))
					{
						if (roles.Contains(ply.characterClassManager.CurRole.roleId))
							return false;
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
		[HarmonyPatch(typeof(PlayerInteract), nameof(PlayerInteract.UserCode_RpcContain106))]
		public static class PlayerInteractContain106
		{
			public static bool Prefix(PlayerInteract __instance)
			{
				try
				{
					if (SanyaRemastered.Instance.Config.ScpCantInteract && SanyaRemastered.Instance.Config.ScpCantInteractList.TryGetValue("Contain106", out List<RoleType> roles))
					{
						if (roles.Contains(__instance._ccm.CurRole.roleId))
							return false;
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
		[HarmonyPatch(typeof(PlayerInteract), nameof(PlayerInteract.UserCode_CmdDetonateWarhead))]
		public static class PlayerInteractDetonateWarhead
		{
			public static bool Prefix(PlayerInteract __instance)
			{
				try
				{
					if (SanyaRemastered.Instance.Config.ScpCantInteract && SanyaRemastered.Instance.Config.ScpCantInteractList.TryGetValue("DetonateWarhead", out List<RoleType> roles))
					{
						if (roles.Contains(__instance._ccm.CurRole.roleId))
							return false;
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
		[HarmonyPatch(typeof(PlayerInteract), nameof(PlayerInteract.UserCode_CmdSwitchAWButton))]
		public static class PlayerInteractAWButton
		{
			public static bool Prefix(PlayerInteract __instance)
			{
				try
				{
					if (SanyaRemastered.Instance.Config.ScpCantInteract && SanyaRemastered.Instance.Config.ScpCantInteractList.TryGetValue("AlphaWarheadButton", out List<RoleType> roles))
					{
						if (roles.Contains(__instance._ccm.CurRole.roleId))
							return false;
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
		[HarmonyPatch(typeof(Scp079Generator), nameof(Scp079Generator.ServerInteract))]
		public static class PlayerInteractScp079Generator
		{
			public static bool Prefix(Scp079Generator __instance, ReferenceHub ply, byte colliderId)
			{
				try
				{
					if (SanyaRemastered.Instance.Config.ScpCantInteract && SanyaRemastered.Instance.Config.ScpCantInteractList.TryGetValue("UseGenerator", out List<RoleType> roles))
					{
						if (roles.Contains(ply.characterClassManager.CurRole.roleId))
							return false;
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
		[HarmonyPatch(typeof(Locker), nameof(Locker.ServerInteract))]
		public static class PlayerInteractUseLocker
		{
			public static bool Prefix(Locker __instance,ReferenceHub ply,byte colliderId)
			{
				try
				{
					if (SanyaRemastered.Instance.Config.ScpCantInteract && SanyaRemastered.Instance.Config.ScpCantInteractList.TryGetValue("UseLocker", out List<RoleType> roles))
					{
						if (roles.Contains(ply.characterClassManager.CurRole.roleId))
							return false;
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
		[HarmonyPatch(typeof(PlayerInteract), nameof(PlayerInteract.UserCode_CmdUsePanel))]
		public static class PlayerInteractUsePanel
		{
			public static bool Prefix(PlayerInteract __instance)
			{
				try
				{
					if (SanyaRemastered.Instance.Config.ScpCantInteract && SanyaRemastered.Instance.Config.ScpCantInteractList.TryGetValue("UseAlphaWarheadPanel", out List<RoleType> roles))
					{
						if (roles.Contains(__instance._ccm.CurRole.roleId))
							return false;
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
}
