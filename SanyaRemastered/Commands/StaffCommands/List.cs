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

	public class List : ICommand
	{
		public string Command => "List";

		public string[] Aliases => new string[] { };

		public string Description => "Show the List of player on the server";

		public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
		{
			if (!sender.CheckPermission($"sanya.{Command}"))
			{
				response = $"You don't have permission to execute this command. Required permission: sanya.{Command}";
				return false;
			}

			response = $"Players List ({PlayerManager.players.Count}/{Server.MaxPlayerCount})\n";
			foreach (var i in Player.List)
			{
				response += $"[{i.Id}]{i.Nickname}({i.UserId})<{i.Role}/{i.Health}HP> {i?.CurrentRoom.Type}\n";
			}
			return true;
		}
	}
}