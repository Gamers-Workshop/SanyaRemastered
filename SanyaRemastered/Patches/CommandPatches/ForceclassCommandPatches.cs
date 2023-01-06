using CommandSystem;
using CommandSystem.Commands.RemoteAdmin;
using CommandSystem.Commands.RemoteAdmin.MutingAndIntercom;
using HarmonyLib;
using Mirror;
using Mirror.LiteNetLib4Mirror;
using NorthwoodLib.Pools;
using PlayerRoles;
using PlayerStatsSystem;
using RemoteAdmin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace SanyaRemastered.Patches.CommandPatches
{
	[HarmonyPatch(typeof(ForceRoleCommand), nameof(ForceRoleCommand.Execute))]
	public static class ForceclassCommandPatches
	{
		public static bool Prefix(ForceRoleCommand __instance, ref bool __result, ArraySegment<string> arguments, ICommandSender sender, out string response)
		{
            if (!ReferenceHub.TryGetHostHub(out ReferenceHub referenceHub))
            {
                response = "You are not connected to a server.";
                __result = false;
                return false;
            }
            if (arguments.Count < 2)
            {
                response = "To execute this command provide at least 2 arguments!\nUsage: " + arguments.Array[0] + " " + __instance.DisplayCommandUsage();
                __result = false;
                return false;
            }
            CharacterClassManager characterClassManager = referenceHub.characterClassManager;
            if (characterClassManager == null || !characterClassManager.isLocalPlayer || !characterClassManager.isServer)
            {
                response = "Please start round before using this command.";
                __result = false;
                return false;
            }
            List<ReferenceHub> list = RAUtils.ProcessPlayerIdOrNamesList(arguments, 0, out string[] array, false);
            bool flag = list.Count == 1 && sender is PlayerCommandSender playerCommandSender && playerCommandSender.ReferenceHub == list[0];
            if (!__instance.TryParseRole(array[0], out PlayerRoleBase playerRoleBase))
            {
                response = "Invalid class ID / name.";
                __result = false;
                return false;
            }
            if (!__instance.HasPerms(playerRoleBase.RoleTypeId, flag, sender, out response))
            {
                __result = false;
                return false;
            }
            int num = 0;
            foreach (ReferenceHub referenceHub2 in list)
            {
                if (!(referenceHub2 == null) && referenceHub2.GetRoleId() != playerRoleBase.RoleTypeId)
                {
                    referenceHub2.roleManager.ServerSetRole(playerRoleBase.RoleTypeId, RoleChangeReason.RemoteAdmin);
                    ServerLogs.AddLog(ServerLogs.Modules.ClassChange, string.Concat(new string[]
                    {
                        sender.LogName,
                        " changed class of player ",
                        referenceHub2.LoggedNameFromRefHub(),
                        " to ",
                        playerRoleBase.RoleName,
                        "."
                    }), ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging, false);
                    num++;
                }
            }
            response = string.Format("Done! Changed role of {0} player{1} to {2}!", num, (num == 1) ? "" : "s", playerRoleBase.RoleName);
            __result = true;
			return false;
		}
	}
}