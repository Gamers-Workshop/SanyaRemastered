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

	/*public class Speed : ICommand
	{
		public string Command => "Speed";

		public string[] Aliases => new string[] { };

		public string Description => "Change the Speed of the player";

		public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
		{
			if (!sender.CheckPermission($"sanya.{Command}"))
			{
				response = $"You don't have permission to execute this command. Required permission: sanya.{Command}";
				return false;
			}
			if (arguments.Count > 2)
			{
				Player target = Player.Get(arguments.At(0));
				if (target is not null)
				{
					if (arguments.At(1).ToLower() == "walk")
					{
						if (arguments.Count() > 2 && float.TryParse(arguments.At(2), out float speed))
						{
							target.WalkingSpeed = speed;
							response = $"Change the walk speed to {speed}";
							return true;
						}
					}
					else if (arguments.At(1).ToLower() == "sprint")
					{
						if (arguments.Count() > 2 && float.TryParse(arguments.At(2), out float speed))
						{
							target.RunningSpeed = speed;
							response = $"Change the sprint speed to {speed}";
							return true;
						}
					}
					else if (arguments.At(1).ToLower() == "all")
					{
						if (arguments.Count() > 2 && float.TryParse(arguments.At(2), out float speed))
						{
							target.WalkingSpeed = speed;
							target.RunningSpeed = speed;
							response = $"Change the speed to {speed}";
							return true;
						}
					}
					else if (arguments.At(1).ToLower() == "reset")
					{
						target.WalkingSpeed = ServerConfigSynchronizer.Singleton.NetworkHumanWalkSpeedMultiplier;
						target.RunningSpeed = ServerConfigSynchronizer.Singleton.NetworkHumanSprintSpeedMultiplier;
					}
					response = "[speed] missing args <all/Player> <walk/sprint/all> <speed>";
					return false;
				}
				else if (arguments.At(0).ToLower() == "all")
				{
					if (!sender.CheckPermission($"sanya.{Command}all"))
					{
						response = $"You don't have permission to execute this command. Required permission: sanya.{Command}";
						return false;
					}
					if (arguments.At(1).ToLower() == "walk")
					{
						if (arguments.Count() > 2 && float.TryParse(arguments.At(2), out float speed))
						{
							foreach (var ply in Player.List)
							{
								ply.WalkingSpeed = speed;
							}
							response = $"Change the walk speed to {speed}";
							return true;
						}
					}
					else if (arguments.At(1).ToLower() == "sprint")
					{
						if (arguments.Count() > 2 && float.TryParse(arguments.At(2), out float speed))
						{
							foreach (var ply in Player.List)
							{
								ply.RunningSpeed = speed;
							}
							response = $"Change the sprint speed to {speed}";
							return true;
						}
					}
					else if (arguments.At(1).ToLower() == "all")
					{
						if (arguments.Count() > 2 && float.TryParse(arguments.At(2), out float speed))
						{
							foreach (var ply in Player.List)
							{
								ply.WalkingSpeed = speed;
								ply.RunningSpeed = speed;
							}
							response = $"Change the speed to {speed}";
							return true;
						}
						else if (arguments.At(2).ToLower() is "reset")
						{
							foreach (var ply in Player.List)
							{
								ply.WalkingSpeed = ServerConfigSynchronizer.Singleton.NetworkHumanWalkSpeedMultiplier;
								ply.RunningSpeed = ServerConfigSynchronizer.Singleton.NetworkHumanSprintSpeedMultiplier;
							}
							response = $"Change the speed to normal";
							return true;
						}
					}
					response = "fail to change the speed <all/Player> <walk/sprint/all> <speed> or reset";
					return false;
				}
				else
				{
					response = "[speed] missing target.";
					return false;
				}
			}
			else
			{
				Player player;
				if (sender is PlayerCommandSender playerCommandSender) player = Player.Get(playerCommandSender.SenderId);
				else
				{
					response = $"You need to be an player to used this command";
					return false;
				}
				if (player != null)
				{
					if (arguments.At(0).ToLower() == "walk")
					{
						if (arguments.Count() > 1 && float.TryParse(arguments.At(1), out float speed))
						{
							player.WalkingSpeed = speed;
							response = $"Change the walk speed to {speed}";
							return true;
						}
					}
					else if (arguments.At(0).ToLower() == "sprint")
					{
						if (arguments.Count() > 1 && float.TryParse(arguments.At(1), out float speed))
						{
							player.RunningSpeed = speed;
							response = $"Change the sprint speed to {speed}";
							return true;
						}
					}
					else if (arguments.At(0).ToLower() == "all")
					{
						if (arguments.Count() > 1 && float.TryParse(arguments.At(1), out float speed))
						{
							player.WalkingSpeed = speed;
							player.RunningSpeed = speed;
							response = $"Change the speed to {speed}";
							return true;
						}
					}
					else if (arguments.At(0).ToLower() == "reset")
					{
						player.WalkingSpeed = ServerConfigSynchronizer.Singleton.NetworkHumanWalkSpeedMultiplier;
						player.RunningSpeed = ServerConfigSynchronizer.Singleton.NetworkHumanSprintSpeedMultiplier;
					}
					response = $"please take <walk/sprint/all> <speed>";
					return false;
				}
				else
				{
					response = "[speed] missing target.";
					return false;
				}
			}
		}
	}*/
}