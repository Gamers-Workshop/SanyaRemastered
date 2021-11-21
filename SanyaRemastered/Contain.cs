using CommandSystem;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using PlayableScps;
using RemoteAdmin;
using Respawning;
using System;
using System.Linq;

namespace SanyaRemastered
{
    [CommandHandler(typeof(ClientCommandHandler))]
	class Contain : ICommand
	{
		public string Command { get; } = "contain";

		public string[] Aliases { get; } = new string[] { "cont" };

		public string Description { get; } = "Contains commands";
		public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Log.Debug($"[Commands] Sender:{sender.LogName} args:{arguments.Count}", SanyaRemastered.Instance.Config.IsDebugged);

            Player player = null;
            if (sender is PlayerCommandSender playerCommandSender) player = Player.Get(playerCommandSender.SenderId);
            {
                if (SanyaRemastered.Instance.Config.ContainCommand && player.Team == Team.SCP)
                {
                    switch (player.Role)
                    {
                        case RoleType.Scp173:
                            {
                                if (Player.List.Any(x => x.Role == RoleType.Scp079))
                                {
                                    foreach (var ply in Player.List.Where(x => x.Role == RoleType.Scp079))
                                        ply.ReferenceHub.BroadcastMessage($"SCP-173 a fait la commande .contain dans la salle {player.CurrentRoom.Name}");

                                    response = "SCP-079 est toujours présent";
                                    return false;
                                }

                                switch (player.CurrentRoom.Type)
                                {
                                    case RoomType.Lcz914:
                                        {
                                            if (!Functions.Extensions.IsInTheBox(player .CurrentRoom.Transform.position - player .Position, 2.9f, -10.2f, 10.1f, -10.2f, 0, -5f, player .CurrentRoom.Transform.rotation.eulerAngles.y))
                                            {
                                                response = "Tu n'est pas confiné";
                                                return false;
                                            }
                                            var door = player.CurrentRoom.Doors.First(x => x.Nametag == "914");
                                            if (door.Base.GetExactState() == 0f)
                                            {
                                                response = "173 room 049";
                                                if (!player.GameObject.TryGetComponent<ContainScpComponent>(out _))
                                                {
                                                    var containScpComponent = player.GameObject.AddComponent<ContainScpComponent>();
                                                    containScpComponent.doors.Add(door);
                                                    containScpComponent.CassieAnnounceContain = "SCP 1 7 3 as been contained in the Containment chamber of SCP 9 1 4";
                                                }
                                                return true;
                                            }
                                            response = "La gate n'est pas fermer";
                                            return false;
                                            
                                        }
                                    case RoomType.Lcz012:
                                        {
                                            if (!Functions.Extensions.IsInTheBox(player.CurrentRoom.Transform.position - player.Position, 10.2f, -9.6f, 8.2f, 2.7f, 8f, -3f, player.CurrentRoom.Transform.rotation.eulerAngles.y)
                                                && !Functions.Extensions.IsInTheBox(player.CurrentRoom.Transform.position - player.Position, 9.8f, -8.9f, 7.8f, -10f, 8f, 2.5f, player.CurrentRoom.Transform.rotation.eulerAngles.y))
                                            {
                                                response = "Tu n'est pas confiné";
                                                return false;
                                            }
                                            var door = player.CurrentRoom.Doors.First(x => x.Nametag == "012");
                                            {
                                                if (door.Base.GetExactState() == 0f)
                                                {
                                                    if (!player.GameObject.TryGetComponent<ContainScpComponent>(out _))
                                                    {
                                                        var containScpComponent = player.GameObject.AddComponent<ContainScpComponent>();
                                                        containScpComponent.doors.Add(door);
                                                        containScpComponent.CassieAnnounceContain = "SCP 1 7 3 as been contained in the Containment chamber of SCP 0 1 2";
                                                    }
                                                    response = "Scp 173 a bien été reconfiné dans la salle de Scp012";
                                                    return true;
                                                }
                                            }
                                            response = "La porte n'est pas fermer";
                                            return false;
                                        }
                                    case RoomType.HczArmory:
                                        {
                                            if(!Functions.Extensions.IsInTheBox(player.CurrentRoom.Transform.position - player.Position, 0.1f, -5.6f, 2.9f, -2.8f, 0f, -5f, player.CurrentRoom.Transform.rotation.eulerAngles.y))
                                            {
                                                response = "Tu doit étre dans ton confinement";
                                                return false; 
                                            }
                                            var door = player.CurrentRoom.Doors.First(x => x.Nametag == "HCZ_ARMORY");
                                            {
                                                if (door.Base.GetExactState() == 0f)
                                                {
                                                    if (!player.GameObject.TryGetComponent<ContainScpComponent>(out _))
                                                    {
                                                        var containScpComponent = player.GameObject.AddComponent<ContainScpComponent>();
                                                        containScpComponent.doors.Add(door);
                                                        containScpComponent.CassieAnnounceContain = "SCP 1 7 3 as been contained in the Armory of Heavy containment Zone";
                                                    }
                                                    response = "Scp 173 a bien été reconfiné dans la HczArmory";
                                                    return true;
                                                }
                                            }
                                            response = "La porte n'est pas fermer";
                                            return false;
                                        }
                                    case RoomType.LczArmory:
                                        {
                                            if (!Functions.Extensions.IsInTheBox(player.CurrentRoom.Transform.position - player.Position, 1.2f, -9.5f, 6f, -7f, -1f, -10f, player.CurrentRoom.Transform.rotation.eulerAngles.y))
                                            {
                                                response = "Tu doit étre confiné";
                                                return false;
                                            }
                                            var door = player.CurrentRoom.Doors.First(x => x.Nametag == "LCZ_ARMORY");
                                            if (door.Base.GetExactState() == 0f)
                                            {
                                                if (!player.GameObject.TryGetComponent<ContainScpComponent>(out _))
                                                {
                                                    var containScpComponent = player.GameObject.AddComponent<ContainScpComponent>();
                                                    containScpComponent.doors.Add(door);
                                                    containScpComponent.CassieAnnounceContain = "SCP 1 7 3 as been contained in the Armory of Light Containment Zone";
                                                }
                                                response = "Scp 173 a bien été reconfiné dans la LczArmory";
                                                return true;
                                            }
                                            response = "La porte n'est pas fermer";
                                            return false;
                                        }
                                    case RoomType.HczHid:
                                        {
                                            if (!Functions.Extensions.IsInTheBox(player.CurrentRoom.Transform.position - player.Position, 3.7f, -4.0f, 9.8f, 7.4f, 0f, -5f, player.CurrentRoom.Transform.rotation.eulerAngles.y))
                                            {
                                                response = "Tu doit étre confiné";
                                                return false;
                                            }
                                            var door = player.CurrentRoom.Doors.First(x => x.Nametag == "HID");
                                            if (door.Base.GetExactState() == 0f)
                                            {
                                                if (!player.GameObject.TryGetComponent<ContainScpComponent>(out _))
                                                {
                                                    var containScpComponent = player.GameObject.AddComponent<ContainScpComponent>();
                                                    containScpComponent.doors.Add(door);
                                                    containScpComponent.CassieAnnounceContain = "SCP 1 7 3 as been contained in the Storage of Micro H I D";
                                                }
                                                response = "Scp 173 a bien été reconfiné dans la Hid";
                                            }
                                            response = "La porte n'est pas fermer";
                                            return false;
                                        }
                                    case RoomType.Hcz049:
                                        {
                                            if (!Functions.Extensions.IsInTheBox(player.CurrentRoom.Transform.position - player.Position, -3f, -8.6f, -4.6f, -10.1f, -260f, -270f, player.CurrentRoom.Transform.rotation.eulerAngles.y))
                                            {
                                                response = "Tu doit étre confiné";
                                                return false;
                                            }
                                            var door = player.CurrentRoom.Doors.First(x => x.Nametag == "049_ARMORY");
                                            if (door.Base.GetExactState() == 0f)
                                            {
                                                if (!player.GameObject.TryGetComponent<ContainScpComponent>(out _))
                                                {
                                                    var containScpComponent = player.GameObject.AddComponent<ContainScpComponent>();
                                                    containScpComponent.doors.Add(door);
                                                    containScpComponent.CassieAnnounceContain = "SCP 1 7 3 as been contained in the Armory of SCP 0 4 9";
                                                }
                                                response = "Scp 173 a bien été reconfiné dans l'armurerie de Scp 049";
                                                return true;
                                            }
                                            response = "La porte n'est pas fermer";
                                            return false;
                                        }
                                    case RoomType.Hcz106:
                                        {
                                            if (Functions.Extensions.IsInTheBox(player.CurrentRoom.Transform.position - player.Position, 9.6f, -24.5f , 30.8f , -1.9f, 20f, 10f, player.CurrentRoom.Transform.rotation.eulerAngles.y))
                                            {
                                                var door = player.CurrentRoom.Doors.First(x => x.Nametag == "106_BOTTOM");
                                                {
                                                    if (door.Base.GetExactState() == 0f)
                                                    {
                                                        if (!player.GameObject.TryGetComponent<ContainScpComponent>(out _))
                                                        {
                                                            var containScpComponent = player.GameObject.AddComponent<ContainScpComponent>();
                                                            containScpComponent.doors.Add(door);
                                                            containScpComponent.CassieAnnounceContain = "SCP 1 7 3 as been contained in the Containment chamber of SCP 1 0 6";
                                                        }
                                                        response = "Scp 173 a bien été reconfiné dans la salle de Scp 106";
                                                        return true;
                                                    }
                                                    else
                                                    {
                                                        response = "les porte sont pas fermer";
                                                        return false;
                                                    }
                                                }
                                            }
                                            if (Functions.Extensions.IsInTheBox(player.CurrentRoom.Transform.position - player.Position, -25.6f, -33.7f, 32f, -4.6f, 20f, -10f, player.CurrentRoom.Transform.rotation.eulerAngles.y))
                                            {
                                                if (Player.List.Any((p) => p.Role == RoleType.Scp106))
                                                {
                                                    response = "Tu ne peux pas te faire reconfiner ici car SCP-106 n'est pas confiné";
                                                    return false;
                                                }
                                                var door1 = player.CurrentRoom.Doors.First(x => x.Nametag == "106_PRIMARY");
                                                var door2 = player.CurrentRoom.Doors.First(x => x.Nametag == "106_SECOND");
                                                if (door1.Base.GetExactState() == 0f && door2.Base.GetExactState() == 0f)
                                                {
                                                    if (!player.GameObject.TryGetComponent<ContainScpComponent>(out _))
                                                    {
                                                        var containScpComponent = player.GameObject.AddComponent<ContainScpComponent>();
                                                        containScpComponent.doors.Add(door1);
                                                        containScpComponent.doors.Add(door2);
                                                        containScpComponent.CassieAnnounceContain = "SCP 1 7 3 as been contained in the Containment chamber of SCP 1 0 6";
                                                    }
                                                    response = "Scp 173 a bien été reconfiné dans le confinement de Scp 106";
                                                    return true;
                                                }
                                                else
                                                {
                                                    response = "les porte sont pas fermer";
                                                    return false;
                                                }
                                            }
                                            response = "Tu doit étre confiné";
                                            return false;
                                        }
                                    case RoomType.Hcz079:
                                        {
                                            byte TEST = 0;
                                            if (Functions.Extensions.IsInTheBox(player.CurrentRoom.Transform.position - player.Position, 10.3f, -8.2f, 22.5f, 5.2f, 10f, 0f, player.CurrentRoom.Transform.rotation.eulerAngles.y))
                                            {
                                                TEST = 1;
                                            }
                                            if (TEST != 1)
                                            {
                                                if (Functions.Extensions.IsInTheBox(player.CurrentRoom.Transform.position - player.Position, -12.3f, -20.8f, 18.7f, -2.5f, 7f, 0f, player.CurrentRoom.Transform.rotation.eulerAngles.y))
                                                {
                                                    TEST = 2;
                                                }
                                            }
                                            if (TEST == 1)
                                            {
                                                var door = player.CurrentRoom.Doors.First(x => x.Nametag == "079_SECOND");
                                                if (door.Base.GetExactState() == 0f)
                                                {
                                                    if (!player.GameObject.TryGetComponent<ContainScpComponent>(out _))
                                                    {
                                                        var containScpComponent = player.GameObject.AddComponent<ContainScpComponent>();
                                                        containScpComponent.doors.Add(door);
                                                        containScpComponent.CassieAnnounceContain = "SCP 1 7 3 as been contained in the Containment chamber of SCP 0 7 9";
                                                    }
                                                    response = "Scp 173 a bien été reconfiné dans le confinement de Scp 079";
                                                    return true;
                                                }
                                                response = "la gate n'est pas fermer";
                                                return false;
                                            }
                                            if (TEST == 2)
                                            {
                                                var door = player.CurrentRoom.Doors.First(x => x.Nametag == "079_FIRST");
                                                {
                                                    if (door.Base.GetExactState() == 0f)
                                                    {
                                                        if (!player.GameObject.TryGetComponent<ContainScpComponent>(out _))
                                                        {
                                                            var containScpComponent = player.GameObject.AddComponent<ContainScpComponent>();
                                                            containScpComponent.doors.Add(door);
                                                            containScpComponent.CassieAnnounceContain = "SCP 1 7 3 as been contained in the Containment chamber of SCP 0 7 9";
                                                        }
                                                        response = "Scp 173 a bien été reconfiné dans le confinement de Scp 079";
                                                        return true;
                                                    }
                                                    response = "la gate n'est pas fermer";
                                                    return false;
                                                }
                                            }
                                            response = "Tu doit étre confiné";
                                            return false;
                                        }
                                    default:
                                        {
                                            response = "Cette commande doit étre utilisé quand tu est bloqué";
                                            return false;
                                        }
                                }
                            }
                        case RoleType.Scp096:
                            {
                                Log.Debug($"096 state : {(player .ReferenceHub.scpsController.CurrentScp as PlayableScps.Scp096).PlayerState}", SanyaRemastered.Instance.Config.IsDebugged);
                                if (Scp096PlayerState.Docile != (player .ReferenceHub.scpsController.CurrentScp as PlayableScps.Scp096).PlayerState
                                    && Scp096PlayerState.TryNotToCry != (player .ReferenceHub.scpsController.CurrentScp as PlayableScps.Scp096).PlayerState)
                                {
                                    response = "NON MEC VAS TUER LES GENS IL Doivent pas te reconf si t'es trigger";
                                    return false;
                                }
                                if (player.CurrentRoom.Type == RoomType.Hcz096)
                                {
                                    if (!Functions.Extensions.IsInTheBox(player.CurrentRoom.Transform.position - player.Position, 4.4f, 0.5f, 1.9f, -1.9f, 0f, -5f, player.CurrentRoom.Transform.rotation.eulerAngles.y))
                                    {
                                        response = "Tu doit étre confiné";
                                        return false;
                                    }
                                    var door = player.CurrentRoom.Doors.First(x => x.Nametag == "096");
                                    if (door.Base.GetExactState() == 0f)
                                    {
                                        if (!player.GameObject.TryGetComponent<ContainScpComponent>(out _))
                                        {
                                            var containScpComponent = player.GameObject.AddComponent<ContainScpComponent>();
                                            containScpComponent.doors.Add(door);
                                            containScpComponent.CassieAnnounceContain = "SCP 0 9 6 as been contained in there containment chamber";
                                        }
                                        response = "096 room 096";
                                        return true;
                                    }
                                    response = "La gate n'est pas fermer";
                                    return false;
                                }
                                else
                                {
                                    response = "Tu n'est pas confiné";
                                    return false;
                                }
                            }
                        case RoleType.Scp049:
                            {
                                if (player.CurrentRoom.Type == RoomType.Hcz049)
                                {
                                    if (Functions.Extensions.IsInTheBox(player.CurrentRoom.Transform.position - player.Position, -3f, -8.6f, -4.6f, -10.1f, -260f, -270f, player.CurrentRoom.Transform.rotation.eulerAngles.y))
                                    {
                                        var door = player.CurrentRoom.Doors.First(x => x.Nametag == "049_ARMORY");
                                        if (door.Base.GetExactState() == 0f)
                                        {
                                            if (!player.GameObject.TryGetComponent<ContainScpComponent>(out _))
                                            {
                                                var containScpComponent = player.GameObject.AddComponent<ContainScpComponent>();
                                                containScpComponent.doors.Add(door);
                                                containScpComponent.CassieAnnounceContain = "SCP 0 4 9 as been contained in there containment chamber";
                                            }
                                            response = "Le confinement a été effectué";
                                            return true;
                                        }
                                        else
                                        {
                                            response = "Vous devez avoir la porte de votre confinement fermer";
                                            return false;
                                        }
                                    }
                                    else if (Functions.Extensions.IsInTheBox(player.CurrentRoom.Transform.position - player.Position, 9.3f, -9.6f, -11, -16.8f, -260f, -270f, player.CurrentRoom.Transform.rotation.eulerAngles.y))
                                    {
                                        var door = player.CurrentRoom.Doors.First(x => x.Nametag == "049_ARMORY");
                                        if (door.Base.GetExactState() == 0f)
                                        {
                                            if (!player.GameObject.TryGetComponent<ContainScpComponent>(out _))
                                            {
                                                var containScpComponent = player.GameObject.AddComponent<ContainScpComponent>();
                                                containScpComponent.doors.Add(door);
                                                containScpComponent.CassieAnnounceContain = "SCP 0 4 9 as been contained in there containment chamber";
                                            }
                                            response = "Le confinement a été effectué";
                                            return true;
                                        }
                                        else
                                        {
                                            response = "Vous devez avoir la porte de votre confinement fermer";
                                            return false;
                                        }
                                    }
                                    response = "Tu doit étre dans ton confinement";
                                    return false;
                                }
                                else
                                {
                                    response = "Tu n'est pas confiné";
                                    return false;
                                }
                            }
                        case RoleType.Scp93953:
                        case RoleType.Scp93989:
                            {
                                if (player.CurrentRoom.Type == RoomType.Hcz106)
                                {
                                    if (!Functions.Extensions.IsInTheBox(player.CurrentRoom.Transform.position - player.Position, 9.6f, -24.4f, 30.8f, -1.9f, 20f, 13f, player.CurrentRoom.Transform.rotation.eulerAngles.y))
                                    {
                                        response = "Tu doit étre dans ton confinement";
                                        return false;
                                    }
                                    var door = player .CurrentRoom.Doors.First(x => x.Nametag == "106_BOTTOM");
                                    {
                                        if (door.Base.GetExactState() == 0f)
                                        {
                                            if (!player.GameObject.TryGetComponent<ContainScpComponent>(out _))
                                            {
                                                var containScpComponent = player.GameObject.AddComponent<ContainScpComponent>();
                                                containScpComponent.doors.Add(door);
                                                containScpComponent.CassieAnnounceContain = "SCP 9 3 9 as been contained in the Containment Chamber of SCP 1 0 6";
                                            }
                                            response = "939 confiné";
                                            return true;

                                        }
                                        else
                                        {
                                            response = "La porte est ouverte";
                                            return false;
                                        }
                                    }
                                }
                                response = "Tu n'est pas confiné";
                                return false;
                            }
                    }
                    response = "Tu n'est pas un SCP confinable";
                    return false;
                }
                response = "La commande contain est seulement pour les SCP en RP";
                return false;
            }
        }
    }
}