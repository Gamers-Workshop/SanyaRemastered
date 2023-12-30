using System;
using System.Linq;
using CommandSystem;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Doors;
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
				response += $"{door.Type} : {door.Zone} : {door.Nametag} : {door.GameObject.name}\n";
            }

            response += "--------------------------------------\n";

            foreach (var door in Door.List.Where(x => x.Type is DoorType.UnknownDoor))
			{
                response += $"{door.Nametag?.GetName} : {door.Room} : {door.Room?.gameObject.name}\n";
            }

            return true;
		}
	}
}