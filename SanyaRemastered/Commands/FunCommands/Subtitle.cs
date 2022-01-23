using System;
using System.Collections.Generic;
using System.Linq;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using Mirror;
using RemoteAdmin;
using Respawning;
using Subtitles;
using UnityEngine;
using Utils;
using Utils.Networking;

namespace SanyaRemastered.Commands.FunCommands
{

	public class Subtitle : ICommand
	{
		public string Command => "subtitle";

		public string[] Aliases => new string[] { "sub" };

		public string Description => "Subtitles commands";

		public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
		{
			if (!sender.CheckPermission($"sanya.{Command}"))
			{
				response = $"You don't have permission to execute this command. Required permission: sanya.{Command}";
				return false;
			}

			if (arguments.Count < 1)
			{
				response = "To execute this command provide at least 1 argument!\nUsage: " + arguments.Array[0];
				return false;
			}
			string subtitles = string.Empty;
			foreach (string test in arguments)
				subtitles += "<voffset=0em>" + test + " ";
			RespawnEffectsController.PlayCassieAnnouncement(subtitles, false, false, true);
			response = "Silent announcement sent!";
			return true;
		}
	}
}