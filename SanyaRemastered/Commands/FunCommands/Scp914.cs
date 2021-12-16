using System;
using System.Collections.Generic;
using System.Linq;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using Mirror;
using RemoteAdmin;
using Scp914;
using UnityEngine;

namespace SanyaRemastered.Commands.FunCommands
{

	public class Scp914Prefix : ICommand
	{
		public string Command => "914";

		public string[] Aliases => new string[] { };

		public string Description => "Command to use Scp914";

		public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
		{
			if (!sender.CheckPermission($"sanya.{Command}"))
			{
				response = $"You don't have permission to execute this command. Required permission: sanya.{Command}";
				return false;
			}

			if (arguments.Count > 0)
			{
				Scp914Controller Scp914 = UnityEngine.Object.FindObjectOfType<Scp914Controller>();
				if (arguments.At(0).ToLower() == "use")
				{
					if (!Scp914._isUpgrading)
					{
						Scp914._remainingCooldown = Scp914._totalSequenceTime;
						Scp914._isUpgrading = true;
						Scp914._itemsAlreadyUpgraded = false;
						Scp914.RpcPlaySound(1);
						response = "ok.";
						return true;
					}
					else
					{
						response = "Scp914 now working.";
						return false;
					}

				}
				else if (arguments.At(0).ToLower() == "knob")
				{
					response = $"ok. [{Scp914._knobSetting}] -> ";
					Scp914._remainingCooldown = Scp914._knobChangeCooldown;
					Type typeFromHandle = typeof(Scp914KnobSetting);
					Scp914KnobSetting scp914KnobSetting = Scp914._knobSetting + 1;
					Scp914.Network_knobSetting = scp914KnobSetting;
					if (!Enum.IsDefined(typeFromHandle, scp914KnobSetting))
					{
						Scp914.Network_knobSetting = Scp914KnobSetting.Rough;
					}
					Scp914.RpcPlaySound(0);

					response += $"[{Scp914._knobSetting}]";
					return true;
				}
				else if (Enum.TryParse(arguments.At(0), out Scp914KnobSetting knob))
				{
					response = $"ok. [{Scp914.Network_knobSetting}] -> ";
					Scp914.Network_knobSetting = knob;
					response += $"[{Scp914.Network_knobSetting}]";
					return true;
				}
				else
				{
					response = "invalid parameters. (use/knob) or (Rough/Coarse/OneToOne/Fine/VeryFine)";
					return false;
				}
			}
			else
			{
				response = "invalid parameters. (need params)";
				return false;
			}
		}
	}
}