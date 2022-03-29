using System;
using System.Linq;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using Mirror;
using UnityEngine;

namespace SanyaRemastered.Commands.DevCommands
{
    public class WorkTest : ICommand
    {
        public string Command => "WorkTest";

        public string[] Aliases => new string[] { };

        public string Description => "Spawn an pretty cool WorkStation";

        private GameObject targetstation = null;

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission($"sanya.dev"))
            {
                response = $"You don't have permission to execute this command. Required permission: sanya.dev";
                return false;
            }
            if (targetstation is null)
            {
                var prefab = CustomNetworkManager.singleton.spawnPrefabs.First(x => x.name.Contains("Station"));
                var station = UnityEngine.Object.Instantiate(prefab,
                    new UnityEngine.Vector3(float.Parse(arguments.At(0)), float.Parse(arguments.At(1)), float.Parse(arguments.At(2))),
                    Quaternion.Euler(Vector3.up * float.Parse(arguments.At(3))));
                station.transform.localScale = new UnityEngine.Vector3(float.Parse(arguments.At(4)), float.Parse(arguments.At(5)), float.Parse(arguments.At(6)));
                targetstation = station;
                NetworkServer.Spawn(station);
            }
            else
            {
                NetworkServer.Destroy(targetstation);
                targetstation = null;
            }
            response = $"worktest.";
            response = "ok.";
            return true;
        }
    }
}