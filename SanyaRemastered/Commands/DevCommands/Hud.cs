using System;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using Mirror;
using RemoteAdmin;
using UnityEngine;


namespace SanyaRemastered.Commands.DevCommands
{

	public class Hud : ICommand
	{
		public string Command => "Hud";

		public string[] Aliases => new string[] { };

		public string Description => "Show or not the Hud";

		public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
		{
			if (!sender.CheckPermission("sanya.dev"))
			{
				response = "Permission denied.";
				return false;
			}
			if (bool.TryParse(arguments.At(0).ToLower(),out bool EnableHud))
			{
				foreach (Player p in Player.List)
					p.GameObject.GetComponent<SanyaRemasteredComponent>().DisableHud = !EnableHud;
				response = $"all hud is = {EnableHud}";
				return true;
			}
			response = $"this is not an bool (true/false){arguments.At(0)}";
			return true;
		}
	}
}