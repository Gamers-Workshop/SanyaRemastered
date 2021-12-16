using System;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using Mirror;
using UnityEngine;

namespace SanyaRemastered.Commands.DevCommands
{
    public class IdentityTree : ICommand
    {
        public string Command => "Identitytree";

        public string[] Aliases => new string[] { };

        public string Description => "Check all NetworkIdentity";

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
                response += $"{identity.transform.name} (layer{identity.transform.gameObject.layer})";
                response += $"HasComponents:";
                foreach (var i in identity.transform.gameObject.GetComponents<Component>())
                {
                    response = $"    {i?.name}:{i?.GetType()}";
                }
                response += $"HasComponentsInChildren:";
                foreach (var i in identity.transform.gameObject.GetComponentsInChildren<Component>())
                {
                    response += $"    {i?.name}:{i?.GetType()}";
                }
                response += $"HasComponentsInParent:";
                foreach (var i in identity.transform.gameObject.GetComponentsInParent<Component>())
                {
                    response += $"    {i?.name}:{i?.GetType()}";
                }
            }
            response = "ok.";
            return true;
        }
    }
}