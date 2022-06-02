using System;
using System.Collections.Generic;
using System.Linq;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using Mirror;
using RemoteAdmin;
using UnityEngine;
using Extensions = SanyaRemastered.Functions.Extensions;


namespace SanyaRemastered.Commands.StaffCommands
{

	public class Redirect : ICommand
	{
		public string Command => "Redirect";

		public string[] Aliases => new string[] { };

		public string Description => "Redirige les joueurs vers un autre serveur";

		public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
		{
			if (!sender.CheckPermission($"sanya.{Command}"))
			{
				response = $"You don't have permission to execute this command. Required permission: sanya.{Command}";
				return false;
			}

			Player player;
			if (sender is PlayerCommandSender playerCommandSender) player = Player.Get(playerCommandSender.SenderId);
			else
			{
				response = $"You need to be an player to used this command";
				return false;
			}

			if (arguments.At(0).ToLower() == "all")
			{
				if (player is not null && !player.CheckPermission($"sanya.{Command}all"))
				{
					response = $"You don't have permission to execute this command. Required permission: sanya.{Command}all";
					return false;
				}
				else if (arguments.Count > 1 && ushort.TryParse(arguments.At(1), out ushort port))
				{
					foreach (Player ply in Player.List)
					{
						ply.Reconnect(port, 3, true, RoundRestarting.RoundRestartType.RedirectRestart);
					}
					response = $"Reconnecting all the player on the port {port}";
					return true;
				}
				else
				{
					response = $"Sanya Redirect <player/all> <port> \n Ex: Redirect all 7777";
					return false;
				}
			}
			else if (arguments.Count > 1 && ushort.TryParse(arguments.At(1), out ushort port))
			{
				{
					string[] Users = arguments.At(0).Split('.');
					List<Player> PlyList = new();
					foreach (string s in Users)
					{
						if (int.TryParse(s, out int id) && Player.Get(id) is not null)
							PlyList.Add(Player.Get(id));
						else if (Player.Get(s) is not null)
							PlyList.Add(Player.Get(s));
					}
					if (PlyList.Count != 0)
					{
						foreach (Player ply in PlyList)
                        {
							ply.Reconnect(port, 3, true, RoundRestarting.RoundRestartType.RedirectRestart);
						}
						response = $"Reconnecting {PlyList.Count} the player on the port {port}";
						return true;
					}
					response = $"Sanya Redirect <player/all> <port> \n Ex: Redirect all 7777";
					return false;
				}
			}
			else
			{
				response = $"Sanya Redirect <player/all> <port> \n Ex: Redirect all 7777";
				return false;
			}
		}
	}
}