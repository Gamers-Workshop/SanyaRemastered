using Exiled.API.Features;
using HarmonyLib;

namespace SanyaRemastered.Patches
{ 	
	[HarmonyPatch(typeof(Radio), nameof(Radio.CallCmdSyncVoiceChatStatus))]
	public static class VCPreventsPatch
	{
		public static bool Prefix(Radio __instance, ref bool b)
		{
			if (SanyaPlugin.SanyaPlugin.Instance.Config.DisableChatBypassWhitelist&& WhiteList.Users != null && !string.IsNullOrEmpty(__instance.ccm.UserId) && WhiteList.IsOnWhitelist(__instance.ccm.UserId)) return true;
			if (SanyaPlugin.SanyaPlugin.Instance.Config.DisableAllChat) return false;
			if (!SanyaPlugin.SanyaPlugin.Instance.Config.DisableSpectatorChat || (SanyaPlugin.SanyaPlugin.Instance.Config.DisableChatBypassWhitelist && WhiteList.IsOnWhitelist(__instance.ccm.UserId))) return true;
			var team = __instance.ccm.Classes.SafeGet(__instance.ccm.CurClass).team;
			Log.Debug($"[VCPreventsPatch] team:{team} value:{b} current:{__instance.isVoiceChatting} RoundEnded:{RoundSummary.singleton._roundEnded}");
			if (SanyaPlugin.SanyaPlugin.Instance.Config.DisableSpectatorChat&& team == Team.RIP && !RoundSummary.singleton._roundEnded) b = false;
			return true;
		}
	}

//override - 10.0.0 checked
	[HarmonyPatch(typeof(Radio), nameof(Radio.CallCmdUpdateClass))]
	public static class VCTeamPatch
	{
		public static bool Prefix(Radio __instance)
		{
			if (SanyaPlugin.SanyaPlugin.Instance.Config.DisableChatBypassWhitelist && !string.IsNullOrEmpty(__instance.ccm.UserId) && WhiteList.Users != null && WhiteList.IsOnWhitelist(__instance.ccm.UserId)) return true;
			if (!SanyaPlugin.SanyaPlugin.Instance.Config.DisableAllChat) return true;
			Log.Debug($"[VCTeamPatch] {Player.Dictionary[__instance.ccm.gameObject].Nickname} [{__instance.ccm.CurClass}]");
			__instance._dissonanceSetup.TargetUpdateForTeam(Team.RIP);
			return false;
		}
	}
}
