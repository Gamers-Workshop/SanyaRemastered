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

	public class PlayAmbiant : ICommand
	{
		public string Command => "PlayAmbiant";

		public string[] Aliases => new string[] { };

		public string Description => "Play ambiant sound";

		public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
		{
			if (!sender.CheckPermission($"sanya.{Command}"))
			{
				response = $"You don't have permission to execute this command. Required permission: sanya.{Command}";
				return false;
			}

			if (arguments.Count > 1 && int.TryParse(arguments.At(1), out int sound))
			{
				Methods.PlayAmbientSound(sound);
			}
			response = $"Ambient sound \n";
			return true;
		}
	}
}