﻿using System;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using SanyaRemastered.Commands.DevCommands;
using SanyaRemastered.Commands.FunCommands;
using SanyaRemastered.Commands.StaffCommands;
using Hint = SanyaRemastered.Commands.StaffCommands.Hint;

namespace SanyaRemastered.Commands
{
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class SanyaPrefixCommand : ParentCommand
    {
        public SanyaPrefixCommand() => LoadGeneratedCommands();

        public override string Command => "sanya";

        public override string[] Aliases => new string[] { "sn" };

        public override string Description => "Sanya Command";

        public override void LoadGeneratedCommands()
        {
            //Developement
            RegisterCommand(new Args());
            RegisterCommand(new AvlCol());
            RegisterCommand(new CameraList());
            RegisterCommand(new CheckObj());
            RegisterCommand(new CheckObjDel());
            RegisterCommand(new Config());
            RegisterCommand(new DoorList());
            RegisterCommand(new DoorTest());
            RegisterCommand(new Hud());
            RegisterCommand(new IdentityPos());
            RegisterCommand(new IdentityTree());
            RegisterCommand(new ItemTest());
            RegisterCommand(new LightTest());
            RegisterCommand(new Now());
            RegisterCommand(new Pocket106());
            RegisterCommand(new PosRoom());
            RegisterCommand(new RoomList());
            RegisterCommand(new SpawnObject());
            RegisterCommand(new TargetTest());
            RegisterCommand(new Test());
            RegisterCommand(new WallTest());
            RegisterCommand(new WindowList());
            RegisterCommand(new WorkTest());

            //Fun Commands
            RegisterCommand(new AirBomb());
            RegisterCommand(new ColorRoom());
            RegisterCommand(new Explode());
            RegisterCommand(new GeneratorControl());
            RegisterCommand(new Heli());
            RegisterCommand(new InfiniteAmmo());
            RegisterCommand(new Npcs());
            RegisterCommand(new NukeCap());
            RegisterCommand(new PlayAmbiant());
            RegisterCommand(new Scale());
            RegisterCommand(new Scp914Prefix());
            //RegisterCommand(new Speed());
            RegisterCommand(new Van());

            //StaffCommand
            RegisterCommand(new Box());
            RegisterCommand(new ForceEnd());
            RegisterCommand(new Hint());
            RegisterCommand(new List());
            RegisterCommand(new Ping());
            RegisterCommand(new Redirect());
        }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            response = "\nPlease enter a valid subcommand:\n\n";

            foreach (var command in AllCommands)
            {
                if (sender.CheckPermission($"sanya.{command.Command}"))
                {
                    response += $"<color=yellow><b>- {command.Command} ({string.Join(", ", command.Aliases)})</b></color>\n<color=white>{command.Description}</color>\n";
                }
            }

            return false;
        }
    }
}
