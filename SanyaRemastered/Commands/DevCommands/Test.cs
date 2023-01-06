using System;
using System.Linq;
using AdminToys;
using CommandSystem;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.Permissions.Extensions;
using Mirror;
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
            response = "NetworkClient";
            foreach (var i in NetworkClient.prefabs)
                Log.Debug($"{i.Key} , {i.Value}");

            response = "CustomNetworkManager";
            foreach (var i in CustomNetworkManager.singleton.spawnPrefabs)
                Log.Debug($"Name : {i.name} gameObject.name : {i.gameObject.name}");

            return true;
        }
    }
}