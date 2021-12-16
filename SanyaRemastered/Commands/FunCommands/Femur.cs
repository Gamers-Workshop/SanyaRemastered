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

	public class Femur : ICommand
	{
		public string Command => "Femur";

		public string[] Aliases => new string[] { };

		public string Description => "Active the sound of femur (do not kill 106)";

		public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
		{
			if (!sender.CheckPermission($"sanya.{Command}"))
			{
				response = $"You don't have permission to execute this command. Required permission: sanya.{Command}";
				return false;
			}

			ReferenceHub.HostHub.playerInteract.RpcContain106(null);
			response = "ok.";
			return true;
		}
	}
}