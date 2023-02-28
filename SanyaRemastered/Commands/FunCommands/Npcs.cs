using System;
using System.Collections.Generic;
using System.Linq;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using Mirror;
using RemoteAdmin;
using UnityEngine;


namespace SanyaRemastered.Commands.FunCommands
{

    public class Npcs : ICommand
    {
        public string Command => "Npcs";

        public string[] Aliases => new string[] { };

        public string Description => "Change the cap of the warhead";
        int Id = int.MaxValue;
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission($"sanya.{Command}"))
            {
                response = $"You don't have permission to execute this command. Required permission: sanya.{Command}";
                return false;
            }

            if (!int.TryParse(arguments.ElementAtOrDefault(0), out int number))
                number = 1;
            int count = 0;

            do
            {
                var newPlayer = UnityEngine.Object.Instantiate(NetworkManager.singleton.playerPrefab);
                var fakeConnection = new FakeConnection(Id--);
                var hubPlayer = newPlayer.GetComponent<ReferenceHub>();
                NetworkServer.AddPlayerForConnection(fakeConnection, newPlayer);
                hubPlayer.characterClassManager.InstanceMode = ClientInstanceMode.ReadyClient;
                count++;
                Log.Info($"TEST{count} <= {number}");
            }
            while (count <= number);

            response = $"Success";
            return true;
        }
    }
}