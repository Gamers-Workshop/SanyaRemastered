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

	public class Box : ICommand
	{
		public string Command => "Box";

		public string[] Aliases => new string[] { };

		public string Description => "Spawn an ReportBox on the player";

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
				if (player != null && !player.CheckPermission($"sanya.{Command}all"))
				{
					response = $"You don't have permission to execute this command. Required permission: sanya.{Command}all";
					return false;
				}
				foreach (Player ply in Player.List)
				{
					ply.OpenReportWindow(Extensions.FormatArguments(arguments, 1));
				}
				response = $"La box avec : {Extensions.FormatArguments(arguments, 1)} a bien été envoyé a tout le monde ";
				return true;
			}

			string[] Users = arguments.At(0).Split('.');
			List<Player> PlyList = new List<Player>();
			foreach (string s in Users)
			{
				if (int.TryParse(s, out int id) && Player.Get(id) != null)
					PlyList.Add(Player.Get(id));
				else if (Player.Get(s) != null)
					PlyList.Add(Player.Get(s));
			}
			if (PlyList.Count != 0)
			{
				response = $"Votre message a bien été envoyé à :\n";
				foreach (Player ply in PlyList)
				{
					ply.OpenReportWindow(Extensions.FormatArguments(arguments, 1));
					response += $" - {ply.Nickname}\n";
				}
				return true;
			}
			else
			{
				response = $"Sanya box <player/all> <message> // Sanya box <id.id.id> <message>";
				return false;
			}
		}
	}
}