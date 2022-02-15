using CommandSystem;
using CommandSystem.Commands.RemoteAdmin;
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
	[HarmonyPatch(typeof(ForceClassCommand), nameof(ForceClassCommand.Execute))]
	public static class ForceclassCommandPatches
	{
		public static bool Prefix(ForceClassCommand __instance, ref bool __result, ArraySegment<string> arguments, ICommandSender sender, out string response)
		{
			if (arguments.Count < 2)
			{
				response = "To execute this command provide at least 2 arguments!\nUsage: " + arguments.Array[0] + " " + __instance.DisplayCommandUsage();
				return false;
			}
			CharacterClassManager characterClassManager = ReferenceHub.HostHub.characterClassManager;

            List<ReferenceHub> list = Utils.RAUtils.ProcessPlayerIdOrNamesList(arguments, 0, out string[] newargs, false);
            RemoteAdmin.PlayerCommandSender playerCommandSender;
			bool flag = list.Count == 1 && (playerCommandSender = (sender as RemoteAdmin.PlayerCommandSender)) != null && playerCommandSender.ReferenceHub == list[0];
            if (!int.TryParse(newargs[0], out int roleId) || roleId < 0 || roleId >= RemoteAdmin.QueryProcessor.LocalCCM.Classes.Length)
            {
                Role role = RemoteAdmin.QueryProcessor.LocalCCM.Classes.SingleOrDefault((Role c) => c.fullName.Replace(" ", string.Empty).ToLower() == newargs[0].ToLower());
                if (role == null)
                {
                    response = "Invalid class ID / name.";
                    return false;
                }
                roleId = (int)role.roleId;
            }
            bool flag2 = roleId == 2;
			string fullName = RemoteAdmin.QueryProcessor.LocalCCM.Classes.SafeGet(roleId).fullName;
			if (flag && flag2 && !sender.CheckPermission(new PlayerPermissions[]
			{
				PlayerPermissions.ForceclassWithoutRestrictions,
				PlayerPermissions.ForceclassToSpectator,
				PlayerPermissions.ForceclassSelf
			}, out response))
			{
				return false;
			}
			if (flag && !flag2 && !sender.CheckPermission(new PlayerPermissions[]
			{
				PlayerPermissions.ForceclassWithoutRestrictions,
				PlayerPermissions.ForceclassSelf
			}, out response))
			{
				return false;
			}
			if (!flag && flag2 && !sender.CheckPermission(new PlayerPermissions[]
			{
				PlayerPermissions.ForceclassWithoutRestrictions,
				PlayerPermissions.ForceclassToSpectator
			}, out response))
			{
				return false;
			}
			if (!flag && !flag2 && !sender.CheckPermission(new PlayerPermissions[]
			{
				PlayerPermissions.ForceclassWithoutRestrictions
			}, out response))
			{
				return false;
			}
			int num = 0;
			foreach (ReferenceHub referenceHub in list)
			{
				if (referenceHub != null)
				{
					RemoteAdmin.QueryProcessor.LocalCCM.SetPlayersClass((RoleType)roleId, referenceHub.gameObject, CharacterClassManager.SpawnReason.ForceClass, false);
					ServerLogs.AddLog(ServerLogs.Modules.ClassChange, string.Concat(new string[]
					{
						sender.LogName,
						" changed class of player ",
						referenceHub.LoggedNameFromRefHub(),
						" to ",
						fullName,
						"."
					}), ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging, false);
					num++;
				}
			}
			response = string.Format("Done! The request affected {0} player{1}", num, (num == 1) ? "!" : "s!");
			__result = true;
			return false;
		}
	}
}