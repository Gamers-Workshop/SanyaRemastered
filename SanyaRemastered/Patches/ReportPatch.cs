using Exiled.API.Features;
using HarmonyLib;
using SanyaPlugin.Functions;

namespace SanyaRemastered.Patches
{
	[HarmonyPatch(typeof(CheaterReport), nameof(CheaterReport.CallCmdReport))]
	public static class ReportPatch
	{
		public static void Prefix(CheaterReport __instance, int playerId, string reason, bool notifyGm)
		{
			Player reported = Player.Dictionary[ReferenceHub.GetHub(playerId).gameObject];
			Player reporter = Player.Dictionary[__instance.gameObject];
			Log.Debug($"[ReportPatch] Reported:{reported.Nickname} Reason:{reason} Reporter:{reporter.Nickname}");

			if (!string.IsNullOrEmpty(SanyaPlugin.SanyaPlugin.instance.Config.ReportWebhook)
				&& !string.IsNullOrEmpty(reporter.UserId)
				&& !string.IsNullOrEmpty(reported.UserId)
				&& !notifyGm)
			{
				Log.Warn($"[Report] {reporter.Nickname} -> {reported.Nickname} Reason:{reason}");
				Methods.SendReport(reported.ReferenceHub, reason, reporter.ReferenceHub);
			}
		}
	}
}
