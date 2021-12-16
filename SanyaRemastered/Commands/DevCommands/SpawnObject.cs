using System;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using Mirror;
using RemoteAdmin;
using UnityEngine;

namespace SanyaRemastered.Commands.DevCommands
{
    public class SpawnObject : ICommand
    {
        public string Command => "SpawnObject";

        public string[] Aliases => new string[] {  };

        public string Description => "Spawn an cube";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission($"sanya.dev"))
            {
                response = $"You don't have permission to execute this command. Required permission: sanya.dev";
                return false;
            }
            Player player = null;
            if (sender is PlayerCommandSender playerCommandSender) player = Player.Get(playerCommandSender.SenderId);
            else
            {
                response = $"You need to be an player to used this command";
                return false;
            }
            NetworkServer.Spawn(UnityEngine.Object.Instantiate(GameObject.CreatePrimitive(PrimitiveType.Cube), player.Position, player.GameObject.transform.rotation));
            response = "test ok.";
            return true;
        }
    }
}
