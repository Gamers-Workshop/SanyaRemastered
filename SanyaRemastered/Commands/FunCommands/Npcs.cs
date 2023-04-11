using System;
using System.Collections.Generic;
using System.Linq;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using Mirror;
using RemoteAdmin;
using SanyaRemastered.Functions;
using UnityEngine;


namespace SanyaRemastered.Commands.FunCommands
{

    public class Npcs : ICommand
    {
        public string Command => "Npcs";

        public string[] Aliases => new string[] { };

        public string Description => "Spawn an npc";
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission($"sanya.{Command}"))
            {
                response = $"You don't have permission to execute this command. Required permission: sanya.{Command}";
                return false;
            }

            if (!int.TryParse(arguments.ElementAtOrDefault(0), out int number))
                number = 0;
            int count = 1;

            do
            {
                Player player = Player.Get(sender) ?? Server.Host;
                Methods.SpawnDummyModel(player.Position, player.Role.Type, player.CustomName, player.Rotation, player.Scale);
                count++;
                Log.Info($"TEST{count} <= {number}");
            }
            while (count <= number);

            response = $"Success";
            return true;
        }
    }
}