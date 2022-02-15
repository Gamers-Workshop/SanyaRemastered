using System;
using System.Collections.Generic;
using System.Linq;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using Mirror;
using RemoteAdmin;
using UnityEngine;

namespace SanyaRemastered.Commands.FunCommands
{

	public class ColorRoom : ICommand
	{
		public string Command => "ColorRoom";

		public string[] Aliases => new string[] { "color" };

		public string Description => "Change the color of the room";

		public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
		{
			if (!sender.CheckPermission($"sanya.{Command}"))
			{
				response = $"You don't have permission to execute this command. Required permission: sanya.{Command}";
				return false;
			}

			if (arguments.At(0).ToLower() == "all")
			{
				if (arguments.Count > 1 && arguments.At(1).ToLower() == "reset")
				{
					foreach (var i in FlickerableLightController.Instances)
					{
						i.WarheadLightColor = FlickerableLightController.DefaultWarheadColor;
						i.WarheadLightOverride = false;
					}
					response = "reset ok.";
					return true;
				}
				if (arguments.Count > 1 && arguments.At(1).ToLower() == "rand")
				{
					System.Random rng = new System.Random();
					foreach (var i in FlickerableLightController.Instances)
					{
						i.WarheadLightColor = new Color((float)rng.NextDouble(), (float)rng.NextDouble(), (float)rng.NextDouble());
						i.WarheadLightOverride = true;
					}
					response = "random color ok.";
					return true;
				}
				if (arguments.Count > 3
					&& float.TryParse(arguments.At(1), out var r)
					&& float.TryParse(arguments.At(2), out var g)
					&& float.TryParse(arguments.At(3), out var b))
				{
					foreach (var i in FlickerableLightController.Instances)
					{
						i.WarheadLightColor = new Color(r / 255f, g / 255f, b / 255f);
						i.WarheadLightOverride = true;
					}
					response = $"color set:{r},{g},{b}";
					return true;
				}
				response = $"lightcolor: invalid params.";
				return false;
			}
			else if (arguments.At(0).ToLower() == "set")
			{
				foreach (Room room in Room.List)
				{
					if (room.Type.ToString().Contains(arguments.At(2)))
					{
						if (arguments.Count > 3
						&& float.TryParse(arguments.At(1), out var r)
						&& float.TryParse(arguments.At(2), out var g)
						&& float.TryParse(arguments.At(3), out var b))
						{
							room.Color = new Color(r / 255f, g / 255f, b / 255f);
						}
					}
				}
			}
			else if (arguments.At(0) == "reset")
			{
				foreach (Room room in Room.List)
				{
					if (room.Type.ToString().ToLower().Contains(arguments.At(1).ToLower()))
					{
						room.ResetColor();
					}
				}
			}
			response = $"lightcolor: invalid params.";
			return false;
		}
	}
}