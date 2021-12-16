using System;
using System.Collections.Generic;
using System.Linq;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using MEC;
using Mirror;
using RemoteAdmin;
using SanyaRemastered.Functions;
using UnityEngine;

namespace SanyaRemastered.Commands.FunCommands
{

	public class AirBomb : ICommand
	{
		public string Command => "AirBomb";

		public string[] Aliases => new string[] { "Air" };

		public string Description => "AirBomB command";

		public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
		{
			if (!sender.CheckPermission($"sanya.{Command}"))
			{
				response = $"You don't have permission to execute this command. Required permission: sanya.{Command}";
				return false;
			}
			if (arguments.At(0).ToLower() == "start")
			{
				if (arguments.Count() > 1 && int.TryParse(arguments.At(1), out int duration))
				{
					if (arguments.Count() > 2 && float.TryParse(arguments.At(2), out float duration2))
					{
						SanyaRemastered.Instance.ServerHandlers.RoundCoroutines.Add(Timing.RunCoroutine(Coroutines.AirSupportBomb(false, duration, duration2), Segment.FixedUpdate));
						response = $"The AirBombing start in {duration / 60}:{duration % 60:00} and stop in {(int)duration2 / 60}:{(int)duration2 % 60:00}";
						return true;
					}
					else
					{
						SanyaRemastered.Instance.ServerHandlers.RoundCoroutines.Add(Timing.RunCoroutine(Coroutines.AirSupportBomb(false, duration), Segment.FixedUpdate));
						response = $"The AirBombing start in {duration / 60}:{duration % 60:00}!";
						return true;
					}
				}
				else
				{
					SanyaRemastered.Instance.ServerHandlers.RoundCoroutines.Add(Timing.RunCoroutine(Coroutines.AirSupportBomb(false), Segment.FixedUpdate));
					response = "Started!";
					return true;
				}
			}
			else if (arguments.At(1).ToLower() == "stop")
			{
				Coroutines.AirSupportBomb(true);
				Coroutines.isAirBombGoing = false;
				response = $"Stop ok.";
				return true;
			}
			else
			{
				response = $"sanya air start/stop";
				return false;
			}
		}
	}
}