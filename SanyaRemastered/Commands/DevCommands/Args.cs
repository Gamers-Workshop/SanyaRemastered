using System;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using Mirror;
using RemoteAdmin;
using UnityEngine;


namespace SanyaRemastered.Commands.DevCommands
{

	public class Args : ICommand
	{
		public string Command => "Args";

		public string[] Aliases => new string[] { };

		public string Description => "Give the number of Argument";

		public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
		{
			response = string.Empty;
			for (int i = 0; i < arguments.Count; i++)
			{
				response += $"[{i}]{arguments.At(i)}\n";
			}
			response.TrimEnd('\n');
			return true;
		}
	}
}