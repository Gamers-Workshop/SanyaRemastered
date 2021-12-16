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

	public class Heli : ICommand
	{
		public string Command => "Heli";

		public string[] Aliases => new string[] { };

		public string Description => "Summon the Helicoptere";

		public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
		{
			if (!sender.CheckPermission($"sanya.{Command}"))
			{
				response = $"You don't have permission to execute this command. Required permission: sanya.{Command}";
				return false;
			}

			Respawn.SummonNtfChopper();
			response = "Heli as comming";
			return true;
		}
	}
}