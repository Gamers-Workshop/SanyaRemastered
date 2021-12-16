using System;
using System.Collections.Generic;
using System.Linq;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using Mirror;
using RemoteAdmin;
using UnityEngine;

namespace SanyaRemastered.Commands.FunCommands
{

	public class NukeLock : ICommand
	{
		public string Command => "NukeLock";

		public string[] Aliases => new string[] { };

		public string Description => "Lock the warhead";

		public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
		{
			if (!sender.CheckPermission($"sanya.{Command}"))
			{
				response = $"You don't have permission to execute this command. Required permission: sanya.{Command}";
				return false;
			}

			response = $"ok.[{AlphaWarheadController.Host._isLocked}] -> ";
			AlphaWarheadController.Host._isLocked = !AlphaWarheadController.Host._isLocked;
			response += $"[{AlphaWarheadController.Host._isLocked}]";
			return true;
		}
	}
}