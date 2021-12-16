using System;
using System;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using Mirror;
using RemoteAdmin;
using UnityEngine;


namespace SanyaRemastered.Commands.DevCommands
{

	public class Config : ICommand
	{
		public string Command => "Config";

		public string[] Aliases => new string[] { };

		public string Description => "Show the Config of sanya";

		public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
		{
			if (!sender.CheckPermission("sanya.dev"))
			{
				response = "Permission denied.";
				return false;
			}
			response = SanyaRemastered.Instance.Config.GetConfigs();
			return true;
		}
	}
}