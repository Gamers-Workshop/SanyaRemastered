using System;
using System.Collections.Generic;
using System.Linq;
using CommandSystem;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using Interactables.Interobjects.DoorUtils;
using MapEditorReborn.Commands.UtilityCommands;
using MapGeneration;
using Mirror;
using UnityEngine;


namespace SanyaRemastered.Commands.DevCommands
{

    public class DoorTest : ICommand
    {
        public string Command => "SpawnPrefab";

        public string[] Aliases => new string[] { };

        public string Description => "Spawn an Door Uwu";

        private List<GameObject> prefab = new();

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission($"sanya.dev"))
            {
                response = $"You don't have permission to execute this command. Required permission: sanya.dev";
                return false;
            }
            Player player = Player.Get(sender);
            if (Enum.TryParse(arguments.ElementAtOrDefault(0), out PrefabType prefabType))
                prefab.Add(PrefabHelper.Spawn(prefabType, player.Position, player.Rotation));

            if (arguments.ElementAtOrDefault(0) is "clear")
            {
                foreach (var prefab in prefab)
                {
                    NetworkServer.Destroy(prefab);
                }
                prefab.Clear();
            }

            response = "ok.";
            return true;
        }
    }
}