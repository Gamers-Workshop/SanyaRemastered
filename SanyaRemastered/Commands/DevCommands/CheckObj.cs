using System;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using Mirror;
using RemoteAdmin;
using UnityEngine;


namespace SanyaRemastered.Commands.DevCommands
{

	public class CheckObj : ICommand
    {
        public string Command => "CheckObj";

        public string[] Aliases => new string[] { };

        public string Description => "CheckTheObject";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission($"sanya.dev"))
            {
                response = $"You don't have permission to execute this command. Required permission: sanya.dev";
                return false;
            }
			Player player;
			if (sender is PlayerCommandSender playerCommandSender) player = Player.Get(playerCommandSender.SenderId);
			else
			{
				response = $"You need to be an player to used this command";
				return false;
			}
			response = string.Empty;
			if (Physics.Raycast(player.Position + player.CameraTransform.forward, player.CameraTransform.forward, out var casy, 25f))
			{
				Log.Warn($"{casy.transform.name} (layer{casy.transform.gameObject.layer})");
				Log.Warn($"HasComponents:");
				foreach (var i in casy.transform.gameObject.GetComponents<Component>())
				{
					Log.Warn($"    {i.name}:{i.GetType()}");
				}
				Log.Warn($"HasComponentsInChildren:");
				foreach (var i in casy.transform.gameObject.GetComponentsInChildren<Component>())
				{
					Log.Warn($"    {i.name}:{i.GetType()}");
				}
				Log.Warn($"HasComponentsInParent:");
				foreach (var i in casy.transform.gameObject.GetComponentsInParent<Component>())
				{
					Log.Warn($"    {i.name}:{i.GetType()}");
				}
			}
			response = "ok.";
            return true;
        }
    }
}