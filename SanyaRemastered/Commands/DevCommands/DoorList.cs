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

	public class DoorList : ICommand
	{
		public string Command => "DoorList";

		public string[] Aliases => new string[] { };

		public string Description => "Show all the door and there position in the map";

		public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
		{
			if (!sender.CheckPermission("sanya.dev"))
			{
				response = "Permission denied.";
				return false;
			}
			response = $"DoorList {Door.List.Count()}\n";
			foreach (var door in Door.List)
			{
				response += $"{door.Type} : {door.Position} : {door.Nametag} : {door.GameObject.name}\n";
				if (door.Type == Exiled.API.Enums.DoorType.UnknownDoor)
					Player.Get(sender).Position = door.Position;
			}
			return true;
		}
	}
}