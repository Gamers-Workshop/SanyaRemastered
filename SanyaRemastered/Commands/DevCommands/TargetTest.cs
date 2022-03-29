using System;
using System.Linq;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using Mirror;
using UnityEngine;

namespace SanyaRemastered.Commands.DevCommands
{
    public class TargetTest : ICommand
    {
        public string Command => "TargetTest";

        public string[] Aliases => new string[] { };

        public string Description => "Check what you look :)";
        private GameObject targetTarget = null;
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission($"sanya.dev"))
            {
                response = $"You don't have permission to execute this command. Required permission: sanya.dev";
                return false;
            }
            if (targetTarget is null)
            {
                var gameObject = UnityEngine.Object.Instantiate(CustomNetworkManager.singleton.spawnPrefabs.First(x => x.name.Contains("dboyTarget")),
                    new UnityEngine.Vector3(float.Parse(arguments.At(0)), float.Parse(arguments.At(1)), float.Parse(arguments.At(2))),
                    Quaternion.Euler(Vector3.up * float.Parse(arguments.At(3))));
                targetTarget = gameObject;
                NetworkServer.Spawn(gameObject);
            }
            else
            {
                NetworkServer.Destroy(targetTarget);
                targetTarget = null;
            }
            response = "ok.";
            return true;
        }
    }
}