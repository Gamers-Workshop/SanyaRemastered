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
using UnityEngine;

namespace SanyaRemastered
{
    public class Contain
    {
        public static void IsCanBeContain(Player player)
        {
            try
            {
                var PlayeRoom = player.CurrentRoom;
                if (player.Team == Team.SCP)
                {
                    switch (player.Role)
                    {
                        case RoleType.Scp173:
                            {
                                if (Player.Get(RoleType.Scp079).Count() > 0) return;

                                switch (PlayeRoom.Type)
                                {
                                    case RoomType.Lcz914:
                                        {
                                            if (Functions.Extensions.IsInTheBox(PlayeRoom.Transform.position - player.Position, new Vector3(2.9f, 0, 10.1f), new Vector3(-10.2f, -5f, -10.2f), PlayeRoom.Transform.rotation.eulerAngles.y))
                                            {
                                                var door = PlayeRoom.Doors.First(x => x.Nametag == "914");
                                                if (door.Base.GetExactState() == 0f)
                                                {
                                                    foreach (Player player1 in PlayeRoom.Players)
                                                    {
                                                        if (player1.Team == Team.SCP) continue;
                                                        if (Functions.Extensions.IsInTheBox(PlayeRoom.Transform.position - player1.Position, new Vector3(2.9f, 0, 10.1f), new Vector3(-10.2f, -5f, -10.2f), PlayeRoom.Transform.rotation.eulerAngles.y))
                                                        {
                                                            return;
                                                        }
                                                    }
                                                    if (!player.GameObject.TryGetComponent<ContainScpComponent>(out _))
                                                    {
                                                        var containScpComponent = player.GameObject.AddComponent<ContainScpComponent>();
                                                        containScpComponent.doors.Add(door);
                                                        containScpComponent.CassieAnnounceContain = "SCP 1 7 3 as been contained in the Containment chamber of SCP 9 1 4";
                                                    }
                                                }
                                            }
                                            return;
                                        }
                                    case RoomType.Lcz173:
                                        {
                                            if (Functions.Extensions.IsInTheBox(PlayeRoom.Transform.position - player.Position, new Vector3(-16.4f, -16.8f, -5.2f), new Vector3(-30.2f, -22.3f, -16.7f), PlayeRoom.Transform.rotation.eulerAngles.y))
                                            {
                                                var door = PlayeRoom.Doors.First(x => x.Nametag == "173_GATE");
                                                if (door.Base.GetExactState() == 0f && !door.Base.GetComponent<Timed173PryableDoor>()._stopwatch.IsRunning)
                                                {
                                                    foreach (Player player1 in PlayeRoom.Players)
                                                    {
                                                        if (player1.Team == Team.SCP) continue;
                                                        if (Functions.Extensions.IsInTheBox(PlayeRoom.Transform.position - player1.Position, new Vector3(-16.4f, -16.8f, -5.2f), new Vector3(-30.2f, -22.3f, -16.7f), PlayeRoom.Transform.rotation.eulerAngles.y))
                                                        {
                                                            return;
                                                        }
                                                    }
                                                    if (!player.GameObject.TryGetComponent<ContainScpComponent>(out _))
                                                    {
                                                        var containScpComponent = player.GameObject.AddComponent<ContainScpComponent>();
                                                        containScpComponent.doors.Add(door);
                                                        containScpComponent.CassieAnnounceContain = "SCP 1 7 3 as been contained in there containment chamber";
                                                    }
                                                }
                                            }
                                            return;
                                        }
                                    case RoomType.HczArmory:
                                        {
                                            if (Functions.Extensions.IsInTheBox(PlayeRoom.Transform.position - player.Position, new Vector3(0.1f, 0f, 2.9f), new Vector3(-5.6f, -5f, -2.8f), PlayeRoom.Transform.rotation.eulerAngles.y))
                                            {
                                                var door = PlayeRoom.Doors.First(x => x.Nametag == "HCZ_ARMORY");
                                                {
                                                    if (door.Base.GetExactState() == 0f)
                                                    {
                                                        foreach (Player player1 in PlayeRoom.Players)
                                                        {
                                                            if (player1.Team == Team.SCP) continue;
                                                            if (Functions.Extensions.IsInTheBox(PlayeRoom.Transform.position - player1.Position, new Vector3(0.1f, 0f, 2.9f), new Vector3(-5.6f, -5f, -2.8f), PlayeRoom.Transform.rotation.eulerAngles.y))
                                                            {
                                                                return;
                                                            }
                                                        }
                                                        if (!player.GameObject.TryGetComponent<ContainScpComponent>(out _))
                                                        {
                                                            var containScpComponent = player.GameObject.AddComponent<ContainScpComponent>();
                                                            containScpComponent.doors.Add(door);
                                                            containScpComponent.CassieAnnounceContain = "SCP 1 7 3 as been contained in the Armory of Heavy containment Zone";
                                                        }
                                                    }
                                                }
                                            }
                                            return;
                                        }
                                    case RoomType.LczArmory:
                                        {
                                            if (Functions.Extensions.IsInTheBox(PlayeRoom.Transform.position - player.Position, new Vector3(1.2f, -1f, 6f), new Vector3(-9.5f, -10f, -7f), PlayeRoom.Transform.rotation.eulerAngles.y))
                                            {
                                                var door = PlayeRoom.Doors.First(x => x.Nametag == "LCZ_ARMORY");
                                                if (door.Base.GetExactState() == 0f)
                                                {
                                                    foreach (Player player1 in PlayeRoom.Players)
                                                    {
                                                        if (player1.Team == Team.SCP) continue;
                                                        if (Functions.Extensions.IsInTheBox(PlayeRoom.Transform.position - player1.Position, new Vector3(1.2f, -1f, 6f), new Vector3(-9.5f, -10f, -7f), PlayeRoom.Transform.rotation.eulerAngles.y))
                                                        {
                                                            return;
                                                        }
                                                    }
                                                    if (!player.GameObject.TryGetComponent<ContainScpComponent>(out _))
                                                    {
                                                        var containScpComponent = player.GameObject.AddComponent<ContainScpComponent>();
                                                        containScpComponent.doors.Add(door);
                                                        containScpComponent.CassieAnnounceContain = "SCP 1 7 3 as been contained in the Armory of Light Containment Zone";
                                                    }
                                                }
                                            }
                                            return;
                                        }
                                    case RoomType.HczHid:
                                        {
                                            if (Functions.Extensions.IsInTheBox(PlayeRoom.Transform.position - player.Position, new Vector3(3.7f, 0f, 9.8f), new Vector3(-4.0f, -5f, 7.4f), PlayeRoom.Transform.rotation.eulerAngles.y))
                                            {
                                                var door = PlayeRoom.Doors.First(x => x.Nametag == "HID");
                                                if (door.Base.GetExactState() == 0f)
                                                {
                                                    foreach (Player player1 in PlayeRoom.Players)
                                                    {
                                                        if (player1.Team == Team.SCP) continue;
                                                        if (Functions.Extensions.IsInTheBox(PlayeRoom.Transform.position - player1.Position, new Vector3(3.7f, 0f, 9.8f), new Vector3(-4.0f, -5f, 7.4f), PlayeRoom.Transform.rotation.eulerAngles.y))
                                                        {
                                                            return;
                                                        }
                                                    }
                                                    if (!player.GameObject.TryGetComponent<ContainScpComponent>(out _))
                                                    {
                                                        var containScpComponent = player.GameObject.AddComponent<ContainScpComponent>();
                                                        containScpComponent.doors.Add(door);
                                                        containScpComponent.CassieAnnounceContain = "SCP 1 7 3 as been contained in the Storage of Micro H I D";
                                                    }
                                                }
                                            }
                                            return;
                                        }
                                    case RoomType.Hcz049:
                                        {
                                            if (!Functions.Extensions.IsInTheBox(PlayeRoom.Transform.position - player.Position, new Vector3(-3f, -260f, -4.6f), new Vector3(-8.6f, -270f, -10.1f), PlayeRoom.Transform.rotation.eulerAngles.y))
                                            {
                                                return;
                                            }
                                            var door = PlayeRoom.Doors.First(x => x.Nametag == "049_ARMORY");
                                            if (door.Base.GetExactState() == 0f)
                                            {
                                                foreach (Player player1 in PlayeRoom.Players)
                                                {
                                                    if (player1.Team == Team.SCP) continue;
                                                    if (Functions.Extensions.IsInTheBox(PlayeRoom.Transform.position - player1.Position, new Vector3(-3f, -260f, -4.6f), new Vector3(-8.6f, -270f, -10.1f), PlayeRoom.Transform.rotation.eulerAngles.y))
                                                    {
                                                        return;
                                                    }
                                                }
                                                if (!player.GameObject.TryGetComponent<ContainScpComponent>(out _))
                                                {
                                                    var containScpComponent = player.GameObject.AddComponent<ContainScpComponent>();
                                                    containScpComponent.doors.Add(door);
                                                    containScpComponent.CassieAnnounceContain = "SCP 1 7 3 as been contained in the Armory of SCP 0 4 9";
                                                }
                                            }
                                            return;
                                        }
                                    case RoomType.Hcz106:
                                        {
                                            if (!Functions.Extensions.IsInTheBox(PlayeRoom.Transform.position - player.Position, new Vector3(9.6f, 20f, 30.8f), new Vector3(-24.4f, 13f, -1.9f), PlayeRoom.Transform.rotation.eulerAngles.y))
                                            {
                                                var door = PlayeRoom.Doors.First(x => x.Nametag == "106_BOTTOM");
                                                {
                                                    if (door.Base.GetExactState() == 0f)
                                                    {
                                                        foreach (Player player1 in PlayeRoom.Players)
                                                        {
                                                            if (player1.Team == Team.SCP) continue;
                                                            if (Functions.Extensions.IsInTheBox(PlayeRoom.Transform.position - player1.Position, new Vector3(9.6f, 20f, 30.8f), new Vector3(-24.4f, 13f, -1.9f), PlayeRoom.Transform.rotation.eulerAngles.y))
                                                            {
                                                                return;
                                                            }
                                                        }
                                                        if (!player.GameObject.TryGetComponent<ContainScpComponent>(out _))
                                                        {
                                                            var containScpComponent = player.GameObject.AddComponent<ContainScpComponent>();
                                                            containScpComponent.doors.Add(door);
                                                            containScpComponent.CassieAnnounceContain = "SCP 1 7 3 as been contained in the Containment chamber of SCP 1 0 6";
                                                        }
                                                    }
                                                }
                                            }
                                            else if (Functions.Extensions.IsInTheBox(PlayeRoom.Transform.position - player.Position, new Vector3(-25.6f, 20f, 32f), new Vector3(-33.7f, -10f, -4.6f), PlayeRoom.Transform.rotation.eulerAngles.y))
                                            {
                                                if (Player.Get(RoleType.Scp106).Count() > 0)
                                                {
                                                    return;
                                                }
                                                var door1 = PlayeRoom.Doors.First(x => x.Nametag == "106_PRIMARY");
                                                var door2 = PlayeRoom.Doors.First(x => x.Nametag == "106_SECOND");
                                                if (door1.Base.GetExactState() == 0f && door2.Base.GetExactState() == 0f)
                                                {
                                                    foreach (Player player1 in PlayeRoom.Players)
                                                    {
                                                        if (player1.Team == Team.SCP) continue;
                                                        if (Functions.Extensions.IsInTheBox(PlayeRoom.Transform.position - player1.Position, new Vector3(-25.6f, 20f, 32f), new Vector3(-33.7f, -10f, -4.6f), PlayeRoom.Transform.rotation.eulerAngles.y))
                                                        {
                                                            return;
                                                        }
                                                    }
                                                    if (!player.GameObject.TryGetComponent<ContainScpComponent>(out _))
                                                    {
                                                        var containScpComponent = player.GameObject.AddComponent<ContainScpComponent>();
                                                        containScpComponent.doors.Add(door1);
                                                        containScpComponent.doors.Add(door2);
                                                        containScpComponent.CassieAnnounceContain = "SCP 1 7 3 as been contained in the Containment chamber of SCP 1 0 6";
                                                    }
                                                }
                                            }
                                            return;
                                        }
                                    case RoomType.Hcz079:
                                        {
                                            if (Functions.Extensions.IsInTheBox(PlayeRoom.Transform.position - player.Position, new Vector3(10.3f, 10f, 22.5f), new Vector3(-8.2f, 0f, 5.2f), PlayeRoom.Transform.rotation.eulerAngles.y))
                                            {
                                                var door = PlayeRoom.Doors.First(x => x.Nametag == "079_SECOND");
                                                if (door.Base.GetExactState() == 0f)
                                                {
                                                    foreach (Player player1 in PlayeRoom.Players)
                                                    {
                                                        if (player1.Team == Team.SCP) continue;
                                                        if (Functions.Extensions.IsInTheBox(PlayeRoom.Transform.position - player1.Position, new Vector3(10.3f, 10f, 22.5f), new Vector3(-8.2f, 0f, 5.2f), PlayeRoom.Transform.rotation.eulerAngles.y))
                                                        {
                                                            return;
                                                        }
                                                    }
                                                    if (!player.GameObject.TryGetComponent<ContainScpComponent>(out _))
                                                    {
                                                        var containScpComponent = player.GameObject.AddComponent<ContainScpComponent>();
                                                        containScpComponent.doors.Add(door);
                                                        containScpComponent.CassieAnnounceContain = "SCP 1 7 3 as been contained in the Containment chamber of SCP 0 7 9";
                                                    }
                                                }
                                            }
                                            else if (Functions.Extensions.IsInTheBox(PlayeRoom.Transform.position - player.Position, new Vector3(-12.3f, 7f, 18.7f), new Vector3(-20.8f, 0f, -2.5f), PlayeRoom.Transform.rotation.eulerAngles.y))
                                            {
                                                var door = PlayeRoom.Doors.First(x => x.Nametag == "079_FIRST");
                                                if (door.Base.GetExactState() == 0f)
                                                {
                                                    foreach (Player player1 in PlayeRoom.Players)
                                                    {
                                                        if (player1.Team == Team.SCP) continue;
                                                        if (Functions.Extensions.IsInTheBox(PlayeRoom.Transform.position - player1.Position, new Vector3(-12.3f, 7f, 18.7f), new Vector3(-20.8f, 0f, -2.5f), PlayeRoom.Transform.rotation.eulerAngles.y))
                                                        {
                                                            return;
                                                        }
                                                    }
                                                    if (!player.GameObject.TryGetComponent<ContainScpComponent>(out _))
                                                    {
                                                        var containScpComponent = player.GameObject.AddComponent<ContainScpComponent>();
                                                        containScpComponent.doors.Add(door);
                                                        containScpComponent.CassieAnnounceContain = "SCP 1 7 3 as been contained in the Containment chamber of SCP 0 7 9";
                                                    }
                                                }
                                            }
                                            return;
                                        }
                                    default:
                                        {
                                            return;
                                        }
                                }
                            }
                        case RoleType.Scp096:
                            {
                                PlayableScps.Scp096 scp096 = (player.ReferenceHub.scpsController.CurrentScp as PlayableScps.Scp096);
                                if (Scp096StateExtensions.IsOffensive((scp096).PlayerState) || scp096.Enraging)
                                {
                                    return;
                                }
                                if (PlayeRoom.Type == RoomType.Hcz096)
                                {
                                    if (Functions.Extensions.IsInTheBox(PlayeRoom.Transform.position - player.Position, new Vector3(4.4f, 0f, 1.9f), new Vector3(0.5f, -5f, -1.9f), PlayeRoom.Transform.rotation.eulerAngles.y))
                                    {
                                        var door = PlayeRoom.Doors.First(x => x.Nametag == "096");
                                        if (door.Base.GetExactState() == 0f)
                                        {
                                            foreach (Player player1 in PlayeRoom.Players)
                                            {
                                                if (player1.Team == Team.SCP) continue;
                                                if (Functions.Extensions.IsInTheBox(PlayeRoom.Transform.position - player1.Position, new Vector3(4.4f, 0f, 1.9f), new Vector3(0.5f, -5f, -1.9f), PlayeRoom.Transform.rotation.eulerAngles.y))
                                                {
                                                    return;
                                                }
                                            }
                                            if (!player.GameObject.TryGetComponent<ContainScpComponent>(out _))
                                            {
                                                var containScpComponent = player.GameObject.AddComponent<ContainScpComponent>();
                                                containScpComponent.doors.Add(door);
                                                containScpComponent.CassieAnnounceContain = "SCP 0 9 6 as been contained in there containment chamber";
                                            }
                                        }
                                    }
                                }
                                return;
                            }
                        case RoleType.Scp049:
                            {
                                if (PlayeRoom.Type == RoomType.Hcz049)
                                {
                                    if (Functions.Extensions.IsInTheBox(PlayeRoom.Transform.position - player.Position, new Vector3(-3f, -260f, -4.6f), new Vector3(-8.6f, -270f, -10.1f), PlayeRoom.Transform.rotation.eulerAngles.y))
                                    {
                                        var door = PlayeRoom.Doors.First(x => x.Nametag == "049_ARMORY");
                                        if (door.Base.GetExactState() == 0f)
                                        {
                                            foreach (Player player1 in PlayeRoom.Players)
                                            {
                                                if (player1.Team == Team.SCP) continue;
                                                if (Functions.Extensions.IsInTheBox(PlayeRoom.Transform.position - player1.Position, new Vector3(-3f, -260f, -4.6f), new Vector3(-8.6f, -270f, -10.1f), PlayeRoom.Transform.rotation.eulerAngles.y))
                                                {
                                                    return;
                                                }
                                            }
                                            if (!player.GameObject.TryGetComponent<ContainScpComponent>(out _))
                                            {
                                                var containScpComponent = player.GameObject.AddComponent<ContainScpComponent>();
                                                containScpComponent.doors.Add(door);
                                                containScpComponent.CassieAnnounceContain = "SCP 0 4 9 as been contained in there containment chamber";
                                            }
                                        }
                                    }
                                    else if (Functions.Extensions.IsInTheBox(PlayeRoom.Transform.position - player.Position, new Vector3(9.3f, -260f, -11f), new Vector3(-9.6f, -270f, -16.8f), PlayeRoom.Transform.rotation.eulerAngles.y))
                                    {
                                        var door = PlayeRoom.Doors.First(x => x.Base is PryableDoor);
                                        if (door.Base.GetExactState() == 0f)
                                        {
                                            foreach (Player player1 in PlayeRoom.Players)
                                            {
                                                if (player1.Team == Team.SCP) continue;
                                                if (Functions.Extensions.IsInTheBox(PlayeRoom.Transform.position - player1.Position, new Vector3(9.3f, -260f, -11f), new Vector3(-9.6f, -270f, -16.8f), PlayeRoom.Transform.rotation.eulerAngles.y))
                                                {
                                                    return;
                                                }
                                            }
                                            if (!player.GameObject.TryGetComponent<ContainScpComponent>(out _))
                                            {
                                                var containScpComponent = player.GameObject.AddComponent<ContainScpComponent>();
                                                containScpComponent.doors.Add(door);
                                                containScpComponent.CassieAnnounceContain = "SCP 0 4 9 as been contained in there containment chamber";
                                            }
                                        }
                                    }
                                }
                                return;
                            }
                        case RoleType.Scp93953:
                        case RoleType.Scp93989:
                            {
                                if (PlayeRoom.Type == RoomType.Hcz106)
                                {
                                    if (Functions.Extensions.IsInTheBox(PlayeRoom.Transform.position - player.Position, new Vector3(9.6f, 20f, 30.8f), new Vector3(-24.4f, 13f, -1.9f), PlayeRoom.Transform.rotation.eulerAngles.y))
                                    {
                                        var door = PlayeRoom.Doors.First(x => x.Nametag == "106_BOTTOM");
                                        {
                                            if (door.Base.GetExactState() == 0f)
                                            {
                                                foreach (Player player1 in PlayeRoom.Players)
                                                {
                                                    if (player1.Team == Team.SCP) continue;
                                                    if (Functions.Extensions.IsInTheBox(PlayeRoom.Transform.position - player1.Position, new Vector3(9.6f, 20f, 30.8f), new Vector3(-24.4f, 13f, -1.9f), PlayeRoom.Transform.rotation.eulerAngles.y))
                                                    {
                                                        return;
                                                    }
                                                }
                                                if (!player.GameObject.TryGetComponent<ContainScpComponent>(out _))
                                                {
                                                    var containScpComponent = player.GameObject.AddComponent<ContainScpComponent>();
                                                    containScpComponent.doors.Add(door);
                                                    containScpComponent.CassieAnnounceContain = "SCP 9 3 9 as been contained in the Containment Chamber of SCP 1 0 6";
                                                }
                                            }
                                        }
                                    }
                                }
                                return;
                            }
                    }
                }
                return;
            }
            catch (Exception ex)
            {
                Log.Error("Error in IsCanBeContain" + ex);
            }
        }
    }
}