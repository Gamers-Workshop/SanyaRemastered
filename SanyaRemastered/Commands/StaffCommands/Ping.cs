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

	public class Ping : ICommand
	{
		public string Command => "Ping";

		public string[] Aliases => new string[] { };

		public string Description => "Show the ping of all the player";

		public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
		{
			if (!sender.CheckPermission($"sanya.{Command}"))
			{
				response = $"You don't have permission to execute this command. Required permission: sanya.{Command}";
				return false;
			}

			response = "Pings:\n";
			foreach (var ply in Player.List)
			{
				response += $"{ply.Nickname} : {LiteNetLib4MirrorServer.Peers[ply.Connection.connectionId].Ping * 2}ms\n";
			}
			return true;
		}
	}
}