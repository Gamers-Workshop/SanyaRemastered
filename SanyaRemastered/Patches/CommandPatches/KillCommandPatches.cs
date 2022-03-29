using CommandSystem;
using CommandSystem.Commands.RemoteAdmin;
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
	[HarmonyPatch(typeof(KillCommand), nameof(KillCommand.Execute))]
	public static class KillCommandPatches
	{
		public static bool Prefix(KillCommand __instance,ref bool __result,ArraySegment<string> arguments, ICommandSender sender, out string response)
		{

			if (!sender.CheckPermission(PlayerPermissions.PlayersManagement, out response))
			{
				__result = false;
				return false;
			}
			if (arguments.Count < 1)
			{
				response = string.Format("To execute this command provide at least 1 argument!\nUsage: {0} {1}", arguments.Array[0], __instance.Usage);
				__result = false;
				return false;
			}
            List<ReferenceHub> list = Utils.RAUtils.ProcessPlayerIdOrNamesList(arguments, 0, out string[] _, false);
            StringBuilder stringBuilder = StringBuilderPool.Shared.Rent();
			int num = 0;
			if (list is not null)
			{
				foreach (ReferenceHub referenceHub in list)
				{
					if (referenceHub is not null && referenceHub.playerStats.DealDamage(new CustomReasonDamageHandler("Kill By Admin")))

					{
						if (num != 0)
						{
							stringBuilder.Append(", ");
						}
						stringBuilder.Append(referenceHub.LoggedNameFromRefHub());
						num++;
					}
				}
			}
			if (num > 0)
			{
				ServerLogs.AddLog(ServerLogs.Modules.Administrative, string.Format("{0} administratively killed player{1}{2}.", sender.LogName, (num == 1) ? " " : "s ", stringBuilder), ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging, false);
			}
			StringBuilderPool.Shared.Return(stringBuilder);
			response = string.Format("Done! The request affected {0} player{1}", num, (num == 1) ? "!" : "s!");
			__result = true;
			return false;
		}
	}
}