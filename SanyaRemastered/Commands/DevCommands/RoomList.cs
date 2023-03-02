using System;
using System.Linq;
using CommandSystem;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using Mirror;
using PluginAPI.Core.Zones;
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
				response += $"{rooms.Zone} : {rooms.Type} : {rooms.Doors?.Count()} : {rooms.Cameras?.Count()} : {rooms.TeslaGate is null}\n";
			}

            response += "--------------------------------------\n";

            foreach (var rooms in Room.List.Where(x => x.Type is RoomType.Unknown || x.Zone is (ZoneType.Entrance | ZoneType.HeavyContainment)))
            {
                response += $"{rooms.Zone} : {rooms.Type} : {rooms.Name} : {rooms.gameObject.name.RemoveBracketsOnEndOfName()} : {rooms.gameObject.transform.parent.name} : {rooms.Position}\n";
            }


            return true;
		}
	}
}