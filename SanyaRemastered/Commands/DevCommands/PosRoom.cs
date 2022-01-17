using System;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using Mirror;
using RemoteAdmin;
using UnityEngine;


namespace SanyaRemastered.Commands.DevCommands
{

	public class PosRoom : ICommand
	{
		public string Command => "PosRoom";

		public string[] Aliases => new string[] { };

		public string Description => "Show your position in the room";

		public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
		{
			if (!sender.CheckPermission("sanya.dev"))
			{
				response = "Permission denied.";
				return false;
			}
			Player player;
			if (sender is PlayerCommandSender playerCommandSender) player = Player.Get(playerCommandSender.SenderId);
			else
			{
				response = $"You need to be an player to used this command";
				return false;
			}
			var roompos = player.CurrentRoom.Transform.position - player.Position;
			response = $"Verification\n{player.CurrentRoom.Transform.rotation.eulerAngles}";
			response += $"position en fonction de la salle : {roompos}";
			return true;
		}
	}
}