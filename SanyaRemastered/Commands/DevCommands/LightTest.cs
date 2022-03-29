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

    public class LightTest : ICommand
    {
        public string Command => "Lighttest";

        public string[] Aliases => new string[] { };

        public string Description => "The Light Appear";

        private LightSourceToy targetLight = null;

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission($"sanya.dev"))
            {
                response = $"You don't have permission to execute this command. Required permission: sanya.dev";
                return false;
            }
            if (targetLight is null)
            {
                var prefab = CustomNetworkManager.singleton.spawnPrefabs.First(x => x.name.Contains("LightSource"));
                var pobject = UnityEngine.Object.Instantiate(prefab.GetComponent<LightSourceToy>());
                targetLight = pobject;

                NetworkServer.Spawn(pobject.gameObject, ownerConnection: null);
            }

            targetLight.transform.position = new UnityEngine.Vector3(float.Parse(arguments.At(0)), float.Parse(arguments.At(1)), float.Parse(arguments.At(2)));
            targetLight.NetworkLightIntensity = float.Parse(arguments.At(3));
            targetLight.NetworkLightRange = float.Parse(arguments.At(4));
            targetLight.NetworkLightShadows = bool.Parse(arguments.At(5));

            response = "ok.";
            return true;
        }
    }
}