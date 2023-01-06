using System;
using System.Collections.Generic;
using System.Linq;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using MapGeneration.Distributors;
using Mirror;
using PlayerRoles.PlayableScps.Scp079;
using RemoteAdmin;
using UnityEngine;

namespace SanyaRemastered.Commands.FunCommands
{

	public class GeneratorControl : ICommand
	{
		public string Command => "gen";

		public string[] Aliases => new string[] { };

		public string Description => "Command to activate the Generator";

		public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
		{
			if (!sender.CheckPermission($"sanya.{Command}"))
			{
				response = $"You don't have permission to execute this command. Required permission: sanya.{Command}";
				return false;
			}

			if (arguments.Count > 0)
			{
				if (arguments.At(0).ToLower() == "unlock")
				{
					foreach (var generator in Scp079Recontainer.AllGenerators)
					{
						generator.ServerSetFlag(Scp079Generator.GeneratorFlags.Unlocked, true);
					}
					response = "gen unlocked.";
					return true;
				}
				else if (arguments.At(0).ToLower() == "door")
				{
					foreach (var generator in Scp079Recontainer.AllGenerators)
					{
						generator.ServerSetFlag(Scp079Generator.GeneratorFlags.Open, !generator.HasFlag(generator._flags, Scp079Generator.GeneratorFlags.Open));
						generator._targetCooldown = generator._doorToggleCooldownTime;
					}
					response = $"gen doors interacted.";
					return true;
				}
				else if (arguments.At(0).ToLower() == "set")
				{
					foreach (var generator in Scp079Recontainer.AllGenerators)
					{
						if (generator is not null && !generator.Engaged)
						{
							generator.Engaged = true;
							generator._currentTime = 1000;
							generator.Network_flags = (byte)Scp079Generator.GeneratorFlags.Engaged;
							response = "set once.";
						}
					}
					response = "gen set.";
					return true;
				}
				else if (arguments.At(0).ToLower() == "once")
				{
					var gen = Scp079Recontainer.AllGenerators.FirstOrDefault(x => !x.Engaged);

					if (gen is not null)
					{
						gen.Engaged = true;
						gen._currentTime = 1000;
						gen.Network_flags = (byte)Scp079Generator.GeneratorFlags.Engaged;
						response = "set once.";
						return true;
					}
					response = "All generator ";
					return false;
				}
				else if (arguments.At(0).ToLower() == "eject")
				{
					foreach (var generator in Generator.Get(Exiled.API.Enums.GeneratorState.Activating))
						generator.IsActivating = false;
					response = "gen ejected.";
					return true;
				}
				else
				{
					response = "[gen] Wrong Parameters.";
					return false;
				}
			}
			else
			{
				response = "[gen] Parameters : gen <unlock/door/set/once/eject>";
				return false;
			}
		}
	}
}