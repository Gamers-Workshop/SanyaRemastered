using System;
using System.Collections.Generic;
using System.Linq;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using Mirror;
using Mirror.LiteNetLib4Mirror;
using RemoteAdmin;
using UnityEngine;


namespace SanyaRemastered.Commands.StaffCommands
{

	public class ForceEnd : ICommand
	{
		public string Command => "ForceEnd";

		public string[] Aliases => new string[] { };

		public string Description => "Force the end of the game";

		public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
		{
			if (!sender.CheckPermission($"sanya.{Command}"))
			{
				response = $"You don't have permission to execute this command. Required permission: sanya.{Command}";
				return false;
			}

			RoundSummary.singleton.ForceEnd();
			response = "Force Ended!";
			return true;
		}
	}
}