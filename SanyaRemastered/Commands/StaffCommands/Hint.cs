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

	public class Hint : ICommand
	{
		public string Command => "Hint";

		public string[] Aliases => new string[] { };

		public string Description => "Spawn an text on the Bottom of the screen";

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

			if (arguments.At(0).ToLower() == "all")
			{
				if (player != null && !player.CheckPermission($"sanya.{Command}all"))
				{
					response = $"You don't have permission to execute this command. Required permission: sanya.{Command}all";
					return false;
				}
				else if (arguments.Count > 1 && ulong.TryParse(arguments.At(1), out ulong duration))
				{
					foreach (Player ply in Player.List)
					{
						if (ply.ReferenceHub.TryGetComponent<SanyaRemasteredComponent>(out var Component))
							Component.AddHudCenterDownText(Extensions.FormatArguments(arguments, 2), duration);
						else
							Log.Debug($"{ply.Nickname} don't have SanyaRemasteredComponent");
					}
					response = $"Le Hint {Extensions.FormatArguments(arguments, 1)} a bien été envoyé a tout le monde ";
					return true;
				}
				else
				{
					response = $"Sanya hint <player/all> <durée> <message> // Sanya hint <id.id.id> <durée> <message>";
					return false;
				}
			}
			else if (arguments.Count > 1 && ulong.TryParse(arguments.At(1), out ulong duration))
			{
				{
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
							if (ply.ReferenceHub.TryGetComponent<SanyaRemasteredComponent>(out var Component))
							{
								Component.AddHudCenterDownText(Extensions.FormatArguments(arguments, 2), duration);
								response += $" - {ply.Nickname}\n";
							}
							else
								Log.Debug($"{ply.Nickname} don't have SanyaRemasteredComponent");
						}
						return true;
					}
					else
					{
						response = $"Sanya hint <player/all> <durée> <message> // Sanya hint <id.id.id> <durée> <message>";
						return false;
					}
				}
			}
			else
			{
				response = "Sanya hint <player> <durée> <message>";
				return false;
			}
		}
	}
}