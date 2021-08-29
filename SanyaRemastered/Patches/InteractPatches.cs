using Exiled.API.Features;
using HarmonyLib;
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


		/*[HarmonyPatch(typeof(PlayerInteract), nameof(PlayerInteract.CallCmdChange914Knob))]
		public static class PlayerInteractChange914Knob
		{
			public static bool Prefix(PlayerInteract __instance)
			{
				try
				{
					if (SanyaRemastered.Instance.Config.ScpCantInteract && SanyaRemastered.Instance.Config.ScpCantInteractList.TryGetValue("Change914Knob", out List<RoleType> roles))
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
		[HarmonyPatch(typeof(PlayerInteract), nameof(PlayerInteract.CallCmdUse914))]
		public static class PlayerInteractUse914
		{
			public static bool Prefix(PlayerInteract __instance)
			{
				try
				{
					if (SanyaRemastered.Instance.Config.ScpCantInteract && SanyaRemastered.Instance.Config.ScpCantInteractList.TryGetValue("Use914", out List<RoleType> roles))
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
		[HarmonyPatch(typeof(PlayerInteract), nameof(PlayerInteract.CallCmdContain106))]
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
		[HarmonyPatch(typeof(PlayerInteract), nameof(PlayerInteract.CallCmdDetonateWarhead))]
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
		[HarmonyPatch(typeof(PlayerInteract), nameof(PlayerInteract.CallCmdSwitchAWButton))]
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
		[HarmonyPatch(typeof(PlayerInteract), nameof(PlayerInteract.InvokeCmdCmdUseElevator))]
		public static class PlayerInteractUseElevator
		{
			public static bool Prefix(PlayerInteract __instance,Mirror.NetworkBehaviour obj)
			{
				try
				{
					__instance = (PlayerInteract)obj;
					if (SanyaRemastered.Instance.Config.ScpCantInteract && SanyaRemastered.Instance.Config.ScpCantInteractList.TryGetValue("UseElevator", out List<RoleType> roles))
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
		[HarmonyPatch(typeof(PlayerInteract), nameof(PlayerInteract.CallCmdUseGenerator))]
		public static class PlayerInteractUseGenerator
		{
			public static bool Prefix(PlayerInteract __instance)
			{
				try
				{
					if (SanyaRemastered.Instance.Config.ScpCantInteract && SanyaRemastered.Instance.Config.ScpCantInteractList.TryGetValue("UseGenerator", out List<RoleType> roles))
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
		[HarmonyPatch(typeof(PlayerInteract), nameof(PlayerInteract.InvokeCmdCmdUseLocker))]
		public static class PlayerInteractUseLocker
		{
			public static bool Prefix(PlayerInteract __instance,Mirror.NetworkBehaviour obj)
			{
				try
				{
					__instance = (PlayerInteract)obj;
					if (SanyaRemastered.Instance.Config.ScpCantInteract && SanyaRemastered.Instance.Config.ScpCantInteractList.TryGetValue("UseLocker", out List<RoleType> roles))
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
		[HarmonyPatch(typeof(PlayerInteract), nameof(PlayerInteract.CallCmdUsePanel))]
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
		}*/
	}
}
