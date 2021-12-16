using System;
using System;
using System.Linq;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using Mirror;
using RemoteAdmin;
using UnityEngine;


namespace SanyaRemastered.Commands.DevCommands
{

	public class AvlCol : ICommand
	{
		public string Command => "AvlCol";

		public string[] Aliases => new string[] { };

		public string Description => "Give All authorized color";

		public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
		{
			if (!sender.CheckPermission($"sanya.dev"))
			{
				response = $"You don't have permission to execute this command. Required permission: sanya.dev";
				return false;
			}

			response = "Available colors:\n";
			foreach (var i in ReferenceHub.HostHub.serverRoles.NamedColors.OrderBy(x => x.Restricted))
				response += $"[#{i.ColorHex}] {i.Name,-13} {(i.Restricted ? "Restricted" : "Not Restricted")}\n";
			return true;
		}
	}
}