using System;
using System.Linq;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using Mirror;
using RemoteAdmin;
using UnityEngine;


namespace SanyaRemastered.Commands.DevCommands
{

	public class WindowList : ICommand
	{
		public string Command => "WindowList";

		public string[] Aliases => new string[] { };

		public string Description => "Show all the Window and there position in the map";

		public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
		{
			if (!sender.CheckPermission("sanya.dev"))
			{
				response = "Permission denied.";
				return false;
			}
			response = $"WindowList\n";
			/*foreach (Window rooms in Window.List)
			{
				response += $"{rooms}\n";
			}*/
			return true;
		}
	}
}