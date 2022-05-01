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

	public class RoomList : ICommand
	{
		public string Command => "RoomList";

		public string[] Aliases => new string[] { };

		public string Description => "Show all the room and there position in the map";

		public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
		{
			if (!sender.CheckPermission("sanya.dev"))
			{
				response = "Permission denied.";
				return false;
			}
			response = $"RoomList\n";
			foreach (var rooms in Room.List)
			{
				response += $"{rooms.Zone} : {rooms.Type} : {rooms.Doors.Count()} : {rooms.Cameras.Count()} : {rooms.TeslaGate is null}\n";
			}
			return true;
		}
	}
}