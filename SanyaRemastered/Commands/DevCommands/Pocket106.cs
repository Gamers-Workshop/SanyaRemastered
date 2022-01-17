using System;
using System.Linq;
using AdminToys;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using Mirror;
using UnityEngine;


namespace SanyaRemastered.Commands.DevCommands
{

    public class Pocket106 : ICommand
    {
        public string Command => "Pocket106";

        public string[] Aliases => new string[] { };

        public string Description => "Make All exit safe";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission($"sanya.dev"))
            {
                response = $"You don't have permission to execute this command. Required permission: sanya.dev";
                return false;
            }

            foreach (var pocketteleport in UnityEngine.Object.FindObjectsOfType<PocketDimensionTeleport>())
            {
                pocketteleport.SetType(PocketDimensionTeleport.PDTeleportType.Exit);
            }

            response = "ok.";
            return true;
        }
    }
}