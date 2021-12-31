using CommandSystem;
using CommandSystem.Commands.RemoteAdmin.MutingAndIntercom;
using HarmonyLib;
using Mirror;
using Mirror.LiteNetLib4Mirror;
using NorthwoodLib.Pools;
using PlayerStatsSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanyaRemastered.Patches.CommandPatches
{
	[HarmonyPatch(typeof(IntercomTextCommand), nameof(IntercomTextCommand.Execute))]
	public static class IntercomTextCommandPatches
	{
		public static bool Prefix(ref bool __result, ArraySegment<string> arguments, ICommandSender sender, out string response)
		{
			if (!sender.CheckPermission(PlayerPermissions.Broadcasting, out response))
			{
				__result = false;
				return false;
			}

			string text = string.Join(" ", arguments);
			if (string.IsNullOrEmpty(text.Trim()))
			{
				ServerLogs.AddLog(ServerLogs.Modules.Administrative, sender.LogName + " cleared the intercom text.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging, false);
				response = "Reset intercom text.";
				SanyaRemastered.Instance.SpecialTextIntercom = null;
				__result = true;
				return false;
			}
			ServerLogs.AddLog(ServerLogs.Modules.Administrative, sender.LogName + " set the intercom text to \"" + text + "\".", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging, false);
			response = "Set intercom text to: " + text;
			SanyaRemastered.Instance.SpecialTextIntercom = text;
			__result = true;
			return false;
		}
	}
}