using System;
using System.Linq;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using Interactables.Interobjects.DoorUtils;
using MapGeneration;
using Mirror;
using UnityEngine;


namespace SanyaRemastered.Commands.DevCommands
{

    public class DoorTest : ICommand
    {
        public string Command => "DoorTest";

        public string[] Aliases => new string[] { };

        public string Description => "Spawn an Door Uwu";

        private DoorVariant targetdoor = null;

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission($"sanya.dev"))
            {
                response = $"You don't have permission to execute this command. Required permission: sanya.dev";
                return false;
            }
            if (targetdoor is null)
            {
                var prefab = UnityEngine.Object.FindObjectsOfType<DoorSpawnpoint>().First(x => x.TargetPrefab.name.Contains("HCZ"));
                var door = UnityEngine.Object.Instantiate(prefab.TargetPrefab, new UnityEngine.Vector3(float.Parse(arguments.At(0)), float.Parse(arguments.At(1)), float.Parse(arguments.At(2))), Quaternion.Euler(Vector3.up * 180f));
                door.transform.localScale = new UnityEngine.Vector3(float.Parse(arguments.At(3)), float.Parse(arguments.At(4)), float.Parse(arguments.At(5)));
                targetdoor = door;
                NetworkServer.Spawn(door.gameObject);
            }
            else
            {
                NetworkServer.Destroy(targetdoor.gameObject);
                targetdoor = null;
            }
            response = "ok.";
            return true;
        }
    }
}