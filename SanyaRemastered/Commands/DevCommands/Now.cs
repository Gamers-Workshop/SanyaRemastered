using System;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using Mirror;
using RemoteAdmin;
using UnityEngine;


namespace SanyaRemastered.Commands.DevCommands
{

	public class Now : ICommand
	{
		public string Command => "Now";

		public string[] Aliases => new string[] { };

		public string Description => "Show the number of tick the server have passed";

		public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
		{
			if (!sender.CheckPermission("sanya.dev"))
			{
				response = "Permission denied.";
				return false;
			}
			response = $"now ticks:{TimeBehaviour.CurrentTimestamp()}";
			return true;
		}
	}
}