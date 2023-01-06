using System;
using System.Collections.Generic;
using System.Linq;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using Mirror;
using PlayerRoles;
using RemoteAdmin;
using UnityEngine;

namespace SanyaRemastered.Commands.FunCommands
{

	public class Scale : ICommand
	{
		public string Command => "Scale";

		public string[] Aliases => new string[] { };

		public string Description => "Change the Scale of the player";

		public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
		{
			if (!sender.CheckPermission($"sanya.{Command}"))
			{
				response = $"You don't have permission to execute this command. Required permission: sanya.{Command}";
				return false;
			}
			if (arguments.At(0).ToLower() == "all")
			{
				if (!sender.CheckPermission($"sanya.{Command}all"))
				{
					response = $"You don't have permission to execute this command. Required permission: sanya.{Command}all";
					return false;
				}
				if (arguments.Count > 3
				&& float.TryParse(arguments.At(1), out float x)
				&& float.TryParse(arguments.At(2), out float y)
				&& float.TryParse(arguments.At(3), out float z))
                {
					Vector3 pos;
					pos = new Vector3(x, y, z);

					foreach (Player ply in Player.List)
						if (ply.Role.Team != Team.Dead)
							ply.Scale = pos;
					response = $"All the player position has been change to {pos}";
					return true;
				}
				response = $"All the player position has been change one of this valeur is not an float x:{arguments.At(1)} |y:{arguments.At(2)}|z:{arguments.At(3)}";
				return true;
			}

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
				if (float.TryParse(arguments.At(1), out float x)
				&& float.TryParse(arguments.At(2), out float y)
				&& float.TryParse(arguments.At(3), out float z))
                {
					Vector3 pos = new(x, y, z);

					response = $"Votre message a bien été envoyé à :\n";
					foreach (Player ply in PlyList)
					{
						if (ply.Role.Team is not Team.Dead)
							ply.Scale = pos;
						response += $" - {ply.Nickname}\n";
					}
					return true;
				}
				response = $"Error on this valeur is not an float x:{arguments.At(1)} |y:{arguments.At(2)}|z:{arguments.At(3)}";
				return false;
			}
			else
			{
				response = $"Sanya box <player/all> <message> // Sanya box <id.id.id> <message>";
				return false;
			}
		}
	}
}