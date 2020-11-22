using Exiled.API.Features;
using HarmonyLib;

[HarmonyPatch(typeof(PlayerMovementSync), nameof(PlayerMovementSync.AntiCheatKillPlayer))]
public static class AntiCheatNotifyPatch
{
	public static bool Prefix(PlayerMovementSync __instance, string message, string code)
	{
		var player = Player.Get(__instance._hub);
		Log.Warn($"[SanyaPlugin] AntiCheatKill Detect:{player.Nickname} [{message}({code})]");
		return false;
	}
}