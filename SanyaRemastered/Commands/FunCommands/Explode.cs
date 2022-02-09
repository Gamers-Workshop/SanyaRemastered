using System;
using System.Collections.Generic;
using System.Linq;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using Mirror;
using RemoteAdmin;
using SanyaRemastered.Functions;
using UnityEngine;

namespace SanyaRemastered.Commands.FunCommands
{

	public class Explode : ICommand
	{
		public string Command => "Explode";

		public string[] Aliases => new string[] { "expl" };

		public string Description => "Et ça fait bim, bam, boum";

		public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
		{
			if (!sender.CheckPermission($"sanya.{Command}"))
			{
				response = $"You don't have permission to execute this command. Required permission: sanya.{Command}";
				return false;
			}
			Player player = null;
			if (sender is PlayerCommandSender playerCommandSender) player = Player.Get(playerCommandSender.SenderId);

			if (arguments.Count > 0)
			{
				Player target = Player.Get(arguments.At(0));
				if (target != null && target.Role != RoleType.Spectator)
				{
					Methods.Explode(target.Position);
					response = $"success. target:{target.Nickname}";
					return true;
				}
				if (arguments.At(0).ToLower() == "all")
				{
					if (!sender.CheckPermission($"sanya.{Command}all"))
					{
						response = $"You don't have permission to execute this command. Required permission: sanya.{Command}all";
						return false;
					}
					foreach (var ply in Player.List.Where((p) => p.Role.Team != Team.RIP))
					{
						Methods.Explode(ply.Position);
					}
					response = "success spawn grenade on all player";
					return true;
				}
				else
				{
					response = "[explode] missing target.";
					return false;
				}
			}
			else
			{
				if (player != null)
				{
					Methods.Explode(player.Position);
					response = $"success. target:{Player.Get(player.ReferenceHub.gameObject).Nickname}";
					return true;
				}
				else
				{
					response = "[explode] missing target.";
					return false;
				}
			}
		}
	}
}