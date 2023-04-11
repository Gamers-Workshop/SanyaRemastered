using System;
using System.Collections.Generic;
using System.Linq;
using AdminToys;
using CommandSystem;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.Permissions.Extensions;
using Mirror;
using PlayerRoles;
using PlayerRoles.PlayableScps.Scp079;
using PlayerRoles.PlayableScps.Scp079.Pinging;
using RelativePositioning;
using RemoteAdmin;
using UnityEngine;

namespace SanyaRemastered.Commands.DevCommands
{
    public class Test : ICommand
    {
        public string Command => "Test";

        public string[] Aliases => new string[] { };

        public string Description => "The test command";
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission($"sanya.dev"))
            {
                response = $"You don't have permission to execute this command. Required permission: sanya.dev";
                return false;
            }

            response = $"ok.";
            return true;
        }
    }
}