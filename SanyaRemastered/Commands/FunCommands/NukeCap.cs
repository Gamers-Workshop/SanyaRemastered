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

	public class NukeCap : ICommand
	{
		public string Command => "NukeCap";

		public string[] Aliases => new string[] { };

		public string Description => "Change the cap of the warhead";

		public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
		{
			if (!sender.CheckPermission($"sanya.{Command}"))
			{
				response = $"You don't have permission to execute this command. Required permission: sanya.{Command}";
				return false;
			}
			var outsite = UnityEngine.Object.FindObjectOfType<AlphaWarheadOutsitePanel>();
			response = $"ok.[{outsite.keycardEntered}] -> ";
			outsite.NetworkkeycardEntered = !outsite.keycardEntered;
			response += $"[{outsite.keycardEntered}]";
			return true;
		}
	}
}