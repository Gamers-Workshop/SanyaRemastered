using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CommandSystem;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using Mirror;
using RemoteAdmin;
using SanyaRemastered.Commands.StaffCommands;

namespace SanyaRemastered.Commands.DevCommands
{

    public class CameraList : ICommand
    {
        public string Command => "CameraList";

        public string[] Aliases => new string[] { };

        public string Description => "Show all the camera and there position in the map";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("sanya.dev"))
            {
                response = "Permission denied.";
                return false;
            }
            response = $"CameraList : {Camera.List.Count()}";
            foreach (Camera camera in Camera.List)
            {
                response += $"{camera.Type} : {camera.Name} : {camera.Room}\n";
            }
            response += "--------------------------------------\n";

            foreach (Camera camera in Camera.List.Where(x => x.Type is CameraType.Unknown))
            {
                response += $"{camera.Type} : {camera.Name} : {camera.Room}\n";
            }
            response += "--------------------------------------\n";

            foreach (CameraType type in Enum.GetValues(typeof(CameraType)).Cast<CameraType>())
            {
                if (!response.Contains(type.ToString()))
                    response += type.ToString() + "\n";
            }
            return true;
        }
    }
}