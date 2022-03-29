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

	public class InfiniteAmmo : ICommand
	{
		public string Command => "InfiniteAmmo";

		public string[] Aliases => new string[] { "InfAmmo"};

		public string Description => "Unlimited ammo and power for MicroHid";

		public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
		{
			if (!sender.CheckPermission($"sanya.{Command}"))
			{
				response = $"You don't have permission to execute this command. Required permission: sanya.{Command}";
				return false;
			}
			if (arguments.Count > 0)
            {
				if (arguments.At(0).ToLower() == "all")
				{
					foreach (Player p in Player.List.Where(x => !x.SessionVariables.ContainsKey("InfAmmo")))
						p.SessionVariables.Add("InfAmmo", null);
					response = "Tous les joueurs on l'infinite ammo";
					return true;
				}
				else if (arguments.At(0).ToLower() == "clear")
				{
					foreach (Player p in Player.List.Where(x => x.SessionVariables.ContainsKey("InfAmmo")))
						p.SessionVariables.Remove("InfAmmo");
					response = "Plus aucun joueurs n'as le infinite ammo";
					return true;
				}
				else if (arguments.At(0).ToLower() == "list")
				{
					response = "Liste des joueurs avec infinite ammo";
					foreach (Player p in Player.List.Where(x => x.SessionVariables.ContainsKey("InfAmmo")))
						response += "\n  - " + p.Nickname;
					return true;
				}
				else
				{
					Player target = Player.Get(arguments.At(0));
					if (target is not null)
					{
						if (target.SessionVariables.ContainsKey("InfAmmo"))
							target.SessionVariables.Remove("InfAmmo");
						else
							target.SessionVariables.Add("InfAmmo", null);
						response = $"Inf Ammo: {target.SessionVariables.ContainsKey("InfAmmo")}.";
						return true;
					}
					response = $"Inf Ammo: Can't Find the player";
					return false;
				}
			}
			else if (sender is PlayerCommandSender playerCommandSender)
			{
				Player player = Player.Get(playerCommandSender.SenderId);

				if (player.SessionVariables.ContainsKey("InfAmmo"))
					player.SessionVariables.Remove("InfAmmo");
				else
					player.SessionVariables.Add("InfAmmo", null);
				response = $"Inf Ammo: {player.SessionVariables.ContainsKey("InfAmmo")}.";
				return true;
			}
			else
			{
				response = $"You need to add argument";
				return false;
			}
		}
	}
}