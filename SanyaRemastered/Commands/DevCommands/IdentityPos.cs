using System;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using Mirror;
using UnityEngine;


namespace SanyaRemastered.Commands.DevCommands
{

    public class IdentityPos : ICommand
    {
        public string Command => "Identitypos";

        public string[] Aliases => new string[] { };

        public string Description => "Check all NetworkIdentity Position";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission($"sanya.dev"))
            {
                response = $"You don't have permission to execute this command. Required permission: sanya.dev";
                return false;
            }
            response = string.Empty;
            foreach (var identity in UnityEngine.Object.FindObjectsOfType<NetworkIdentity>())
            {
                response += $"{identity.transform.name}{identity.transform.position}\n";
            }
            response += "ok.";
            return true;
        }
    }
}