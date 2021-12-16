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

	public class Pocket : ICommand
	{
		public string Command => "Pocket";

		public string[] Aliases => new string[] { };

		public string Description => "Tp to the pocket";

		public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
		{
			if (!sender.CheckPermission($"sanya.{Command}"))
			{
				response = $"You don't have permission to execute this command. Required permission: sanya.{Command}";
				return false;
			}
			Player player = null;
			if (sender is PlayerCommandSender playerCommandSender) player = Player.Get(playerCommandSender.SenderId);
			else
			{
				response = $"You need to be an player to used this command";
				return false;
			}

			player.Position = new Vector3(0f, -1998f, 0f);
			response = "ok.";
			return true;
		}
	}
}