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
    public class WallTest : ICommand
    {
        public string Command => "WallTest";

        public string[] Aliases => new string[] { };

        public string Description => "Spawn The GREAT WALL";

        private PrimitiveObjectToy targetPrimitive = null;

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission($"sanya.dev"))
            {
                response = $"You don't have permission to execute this command. Required permission: sanya.dev";
                return false;
            }
            if (targetPrimitive is null)
            {
                var prefab = CustomNetworkManager.singleton.spawnPrefabs.First(x => x.name.Contains("Primitive"));
                var pobject = UnityEngine.Object.Instantiate(prefab.GetComponent<PrimitiveObjectToy>());

                pobject.NetworkScale = Vector3.one;
                pobject.NetworkMaterialColor = Color.black;
                targetPrimitive = pobject;

                NetworkServer.Spawn(pobject.gameObject, ownerConnection: null);
            }

            targetPrimitive.NetworkPrimitiveType = PrimitiveType.Cube;
            targetPrimitive.transform.position = new UnityEngine.Vector3(float.Parse(arguments.At(0)), float.Parse(arguments.At(1)), float.Parse(arguments.At(2)));
            targetPrimitive.transform.localScale = new UnityEngine.Vector3(float.Parse(arguments.At(3)), float.Parse(arguments.At(4)), float.Parse(arguments.At(5)));

            response = "ok.";
            return true;
        }
    }
}