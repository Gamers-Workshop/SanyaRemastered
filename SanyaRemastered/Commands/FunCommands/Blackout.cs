using System;
using System.Collections.Generic;
using System.Linq;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using MapGeneration;
using Mirror;
using RemoteAdmin;
using UnityEngine;

namespace SanyaRemastered.Commands.FunCommands
{

	public class Blackout : ICommand
	{
		public string Command => "Blackout";

		public string[] Aliases => new string[] { };

		public string Description => "Extinction of the light";

		public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
		{
			if (!sender.CheckPermission($"sanya.{Command}"))
			{
				response = $"You don't have permission to execute this command. Required permission: sanya.{Command}";
				return false;
			}

			if (arguments.Count > 0 && arguments.At(0).ToLower() == "hcz")
			{
				if (float.TryParse(arguments.At(1), out float duration))
					foreach (FlickerableLightController flickerableLightController in FlickerableLightController.Instances)
					{
						if (RoomIdentifier.RoomsByCoordinatess.TryGetValue(RoomIdUtils.PositionToCoords(flickerableLightController.transform.position), out RoomIdentifier roomIdentifier2) && roomIdentifier2.Zone == MapGeneration.FacilityZone.HeavyContainment && flickerableLightController.TryGetComponent(out Scp079Interactable scp079Interactable) && scp079Interactable.type == Scp079Interactable.InteractableType.LightController)
						{
							flickerableLightController.ServerFlickerLights(duration);
						}
					}

				response = "HCZ blackout!";
				return true;
			}
			if (arguments.Count > 0 && arguments.At(0).ToLower() == "all")
			{
				if (arguments.Count > 1 && float.TryParse(arguments.At(1), out float duration))
				{
					foreach (FlickerableLightController flickerableLightController in FlickerableLightController.Instances)
					{
						if (RoomIdentifier.RoomsByCoordinatess.TryGetValue(RoomIdUtils.PositionToCoords(flickerableLightController.transform.position), out RoomIdentifier roomIdentifier2) && flickerableLightController.TryGetComponent(out Scp079Interactable scp079Interactable) && scp079Interactable.type == Scp079Interactable.InteractableType.LightController)
						{
							flickerableLightController.ServerFlickerLights(duration);
						}
					}
					response = "ALL blackout!";
					return true;
				}
				response = "need an duration";
			}
			else
				response = "all ou hcz";
			return false;
		}
	}
}