using System;
using System.Collections.Generic;
using CommandSystem;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using MEC;
using Mirror;
using Mirror.LiteNetLib4Mirror;
using RemoteAdmin;
using Respawning;
using SanyaRemastered.Functions;
using UnityEngine;
using System.IO;
using MapGeneration;
using System.Linq;
using Interactables.Interobjects.DoorUtils;

namespace SanyaRemastered.Commands
{
	[CommandHandler(typeof(GameConsoleCommandHandler))]
	[CommandHandler(typeof(RemoteAdminCommandHandler))]
	class Commands : ICommand
	{
		public string Command { get; } = "sanya";

		public string[] Aliases { get; } = new string[] { "sn" };

		public string Description { get; } = "SanyaRemastered Commands";

		private bool isActwatchEnabled = false;
		private DoorVariant targetdoor = null;


		public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
		{
			Log.Debug($"[Commands] Sender:{sender.LogName} args:{arguments.Count}", SanyaRemastered.Instance.Config.IsDebugged);

			Player player = null;
			if(sender is PlayerCommandSender playerCommandSender) player = Player.Get(playerCommandSender.SenderId);

			if (arguments.Count == 0)
			{
				response = "sanya plugins command params: <hud/ping/override/actwatch/106/914/nukecap/nukelock/femur/blackout/addscps/ammo/forcend/now/config>";
				return true;
			}

			switch (arguments.FirstElement().ToLower())
			{
				case "test":
					{
						response = "test ok.";
						return true;
					}
				case "scale":
					{
						var target = Player.Get(int.Parse(arguments.At(1)));

						target.Scale = new UnityEngine.Vector3(
							float.Parse(arguments.At(2)),
							float.Parse(arguments.At(3)),
							float.Parse(arguments.At(4))
						);

						response = $"{target.Nickname} ok.";
						return true;
					}
				case "args":
					{
						response = "ok.\n";
						for (int i = 0; i < arguments.Count; i++)
						{
							response += $"[{i}]{arguments.At(i)}\n";
						}
						response.TrimEnd('\n');
						return true;
					}
				case "hint":
					{
						if (player != null && !player.CheckPermission("sanya.hint"))
						{
							response = "Permission denied.";
							return false;
						}
						if (ulong.TryParse(arguments.At(2), out ulong duration))
						{
							string[] Users = arguments.At(1).Split('.');
							List<Player> PlyList = new List<Player>();
							foreach (string s in Users)
							{
								if (int.TryParse(s, out int id) && Player.Get(id) != null)
									PlyList.Add(Player.Get(id));
								else if (Player.Get(s) != null)
									PlyList.Add(Player.Get(s));
							}
							if (PlyList.Count != 0)
							{
								foreach (Player ply in PlyList)
								{	
									ply.ReferenceHub.GetComponent<SanyaRemasteredComponent>().AddHudCenterDownText(Extensions.FormatArguments(arguments, 3), duration);
								}
								response = $"Votre message a bien été envoyé à {PlyList.Count}";
								return true;
							}
							else
							{
								response = $"Sanya hint <durée> <player> <message> \\\\ Sanya hint <Player.Otherplayer.AnotherPlayerAgain> <durée> <message>";
								return false;
							}
						}
						else
						{
							response = "Sanya hint <player> <durée> <message>";
							return false;
						}
					}
				case "hintall":
					{
						if (player != null && !player.CheckPermission("sanya.hintall"))
						{
							response = "Permission denied.";
							return false;
						}
						if (ulong.TryParse(arguments.At(1), out ulong duration))
						{
							foreach (Player ply in Player.List.Where((p) => p.Role != RoleType.None))
							{
								ply.ReferenceHub.GetComponent<SanyaRemasteredComponent>().AddHudCenterDownText(Extensions.FormatArguments(arguments, 2), duration);
							}
							response = $"Le Hint {Extensions.FormatArguments(arguments, 2)} a bien été envoyé a tout le monde ";
							return true;
						}
						else
						{
							response = "Sanya hintall <durée> <message>";
							return false;
						}
					}
				case "hud":
					{
						if (player != null && !player.CheckPermission("sanya.hud"))
						{
							response = "Permission denied.";
							return false;
						}
						var comp = player.GameObject.GetComponent<SanyaRemasteredComponent>();
						response = $"ok.{comp.DisableHud} -> ";
						comp.DisableHud = !comp.DisableHud;
						response += $"{comp.DisableHud}";
						return true;
					}
				case "ping":
					{
						if (player != null && !player.CheckPermission("sanya.ping"))
						{
							response = "Permission denied.";
							return false;
						}
						response = "Pings:\n";

						foreach(var ply in Player.List.Where((p) => p.Role != RoleType.None))
						{
							response += $"{ply.Nickname} : {LiteNetLib4MirrorServer.Peers[ply.Connection.connectionId].Ping}ms\n";
						}
						return true;
					}
				case "actwatch":
					{
						if (player == null)
						{
							response = "Only can use with RemoteAdmin.";
							return false;
						}

						if (!isActwatchEnabled)
						{
							player.SendCustomSyncObject(player.ReferenceHub.networkIdentity, typeof(PlayerEffectsController), (writer) =>
							{
								writer.WritePackedUInt64(1ul);
								writer.WritePackedUInt32((uint)1);
								writer.WriteByte((byte)SyncList<byte>.Operation.OP_SET);
								writer.WritePackedUInt32((uint)3);
								writer.WriteByte((byte)1);
							});
							isActwatchEnabled = true;
						}
						else
						{
							player.SendCustomSyncObject(player.ReferenceHub.networkIdentity, typeof(PlayerEffectsController), (writer) =>
							{
								writer.WritePackedUInt64(1ul);
								writer.WritePackedUInt32((uint)1);
								writer.WriteByte((byte)SyncList<byte>.Operation.OP_SET);
								writer.WritePackedUInt32((uint)3);
								writer.WriteByte((byte)0);
							});
							isActwatchEnabled = false;
						}

						response = $"ok. [{isActwatchEnabled}]";
						return true;
					}
				case "106":
					{
						if (player != null && !player.CheckPermission("sanya.106"))
						{
							response = "Permission denied.";
							return false;
						}
						foreach (var pocketteleport in UnityEngine.Object.FindObjectsOfType<PocketDimensionTeleport>())
						{
							pocketteleport.SetType(PocketDimensionTeleport.PDTeleportType.Exit);
						}
						response = "ok.";
						return true;
					}
				case "pocket":
					{
						if (player != null && !player.CheckPermission("sanya.pocket"))
						{
							response = "Permission denied.";
							return false;
						}
						player.Position = new Vector3(0f, -1998f, 0f);
						response = "ok.";
						return true;
					}
				case "914":
					{
						if (player != null && !player.CheckPermission("sanya.914"))
						{
							response = "Permission denied.";
							return false;
						}
						if (arguments.Count > 1)
						{
							if(arguments.At(1).ToLower() == "use")
							{
								if(!Scp914.Scp914Machine.singleton.working)
								{
									Scp914.Scp914Machine.singleton.RpcActivate(NetworkTime.time);
									response = "ok.";
									return true;
								}
								else
								{
									response = "Scp914 now working.";
									return false;
								}

							}
							else if(arguments.At(1).ToLower() == "knob")
							{
								response = $"ok. [{Scp914.Scp914Machine.singleton.knobState}] -> ";
								Scp914.Scp914Machine.singleton.ChangeKnobStatus();
								response += $"[{Scp914.Scp914Machine.singleton.knobState}]";
								return true;
							}
							else
							{
								response = "invalid parameters. (use/knob)";
								return false;
							}
						}
						else
						{
							response = "invalid parameters. (need params)";
							return false;
						}
					}
				case "nukecap":
					{
						if (player != null && !player.CheckPermission("sanya.nukecap"))
						{
							response = "Permission denied.";
							return false;
						}
						var outsite = UnityEngine.Object.FindObjectOfType<AlphaWarheadOutsitePanel>();
						response = $"ok.[{outsite.keycardEntered}] -> ";
						outsite.NetworkkeycardEntered = !outsite.keycardEntered;
						response += $"[{outsite.keycardEntered}]";
						return true;
					}
				case "nukelock":
					{
						if (player != null && !player.CheckPermission("sanya.nukelock"))
						{
							response = "Permission denied.";
							return false;
						}
						response = $"ok.[{AlphaWarheadController.Host._isLocked}] -> ";
						AlphaWarheadController.Host._isLocked = !AlphaWarheadController.Host._isLocked;
						response += $"[{AlphaWarheadController.Host._isLocked}]";
						return true;
					}
				case "femur":
					{
						if (player != null && !player.CheckPermission("sanya.femur"))
						{
							response = "Permission denied.";
							return false;
						}
						ReferenceHub.HostHub.playerInteract.RpcContain106(null);
						response = "ok.";
						return true;
					}
				case "blackout":
					{
						if (player != null && !player.CheckPermission("sanya.blackout"))
						{
							response = "Permission denied.";
							return false;
						}		
						if (arguments.Count > 1 && arguments.At(1) == "hcz")
						{
							if (float.TryParse(arguments.At(2), out float duration))
								Generator079.mainGenerator.ServerOvercharge(duration, true);
							response = "HCZ blackout!";
							return true;
						}
						if (arguments.Count > 1 && arguments.At(1) == "all")
						{
							if (float.TryParse(arguments.At(2), out float duration))
								Generator079.mainGenerator.ServerOvercharge(duration, false);
							response = "ALL blackout!";
							return true;
						}
						else
							response = "all ou hcz";
						return false;
					}
				case "addscps":
					{
						if (player != null && !player.CheckPermission("sanya.addscps"))
						{
							response = "Permission denied.";
							return false;
						}
						response = $"ok.{RoundSummary.singleton.classlistStart.scps_except_zombies} -> ";
						RoundSummary.singleton.classlistStart.scps_except_zombies++;
						response += $"[{RoundSummary.singleton.classlistStart.scps_except_zombies}]";
						return true;
					}
				case "ammo":
					{
						if (player == null)
						{
							response = "Only can use with RemoteAdmin.";
							return false;
						}
						if (arguments.Count > 1)
						{
							Player target = Player.Get(arguments.At(1));
							if (target != null && target.Role != RoleType.Spectator)
							{
								if (uint.TryParse(arguments.At(2), out uint Nato556) &&
									uint.TryParse(arguments.At(3), out uint Nato762) &&
									uint.TryParse(arguments.At(4), out uint Nato9))
								{
									target.Ammo[(int)AmmoType.Nato556] = Nato556;
									target.Ammo[(int)AmmoType.Nato762] = Nato762;
									target.Ammo[(int)AmmoType.Nato9] = Nato9;
									response = $"{target.Nickname}  {Nato556}:{Nato762}:{Nato9}";
									return true;
								}
								else
								{
									response = "sanya ammo {player} (5.56) (7.62) (9mm).";
									return false;
								}
							}
							if (arguments.At(1) == "all")
							{
								if (player != null && !player.CheckPermission("sanya.allammo"))
								{
									response = "Permission denied.";
									return false;
								}
								if (uint.TryParse(arguments.At(2), out uint Nato556) &&
									uint.TryParse(arguments.At(3), out uint Nato762) &&
									uint.TryParse(arguments.At(4), out uint Nato9))
								{
									foreach (var ply in Player.List.Where((p) => p.Role != RoleType.None))
									{
										ply.Ammo[(int)AmmoType.Nato556] = Nato556;
										ply.Ammo[(int)AmmoType.Nato762] = Nato762;
										ply.Ammo[(int)AmmoType.Nato9] = Nato9;
									}
									response = $"ammo set {Nato556}:{Nato762}:{Nato9}";
									return true;
								}
								else
								{
									response = "sanya ammo all (5.56) (7.62) (9mm)";
									return false;
								}
							}
							else
							{
								response = "sanya (player id ou all) ";
								return false;
							}
						}
						else
						{
							response = "Failed to set. (cant use from SERVER)";
							return false;
						}
					}
				case "forceend":
					{
						if (player != null && !player.CheckPermission("sanya.forceend"))
						{
							response = "Permission denied.";
							return false;
						}
						RoundSummary.singleton.ForceEnd();
						response = "Force Ended!";
						return true;
					}
				case "now":
					{
						if (player != null && !player.CheckPermission("sanya.now"))
						{
							response = "Permission denied.";
							return false;
						}
						response = $"now ticks:{TimeBehaviour.CurrentTimestamp()}";
						return true;
					}
				case "config":
					{
						if (player != null && !player.CheckPermission("sanya.config"))
						{
							response = "Permission denied.";
							return false;
						}
						response = SanyaRemastered.Instance.Config.GetConfigs();
						return true;
					}
				case "posroom":
					{
						if (player != null && !player.CheckPermission("sanya.dev"))
						{
							response = "Permission denied.";
							return false;
						}
						var roompos = player.CurrentRoom.Transform.position - player.Position;
						response = $"Verification\n{player.CurrentRoom.Transform.rotation.eulerAngles}";
						response += $"position en fonction de la salle : {roompos}";
						return true;
					}
				case "roomlist":
					{
						if (player != null && !player.CheckPermission("sanya.dev"))
						{
							response = "Permission denied.";
							return false;
						}
						response = $"RoomList\n";
						foreach (var rooms in Map.Rooms)
						{
							response += $"{rooms.Name} : {rooms.Position}\n";
						}
						return true;
					}
				case "playambiant":
					{
						if (player != null && !player.CheckPermission("sanya.dev"))
						{
							response = "Permission denied.";
							return false;
						}
						if (int.TryParse(arguments.At(1), out int sound))
						{
							Methods.PlayAmbientSound(sound);
						}
						response = $"Ambien sound \n";
						return true;
					}
				case "listdoor":
					{
						response = $"DoorList\n";
						foreach (var doors in Map.Doors)
						{
							response += $"{doors.name} : {doors.name} \n";
						}
						foreach (var doors2 in UnityEngine.Object.FindObjectsOfType<DoorSpawnpoint>())
						{
							response += doors2.TargetPrefab.name;
						}
						return true;
					}
				case "reload":
					{
						if (player != null && !player.CheckPermission("sanya.reload"))
						{
							response = "Permission denied.";
							return false;
						}
						SanyaRemastered.Instance.Config.GetConfigs();
						response = "reload ok";
						return true;
					}
				case "list":
					{
						if (player != null && !player.CheckPermission("sanya.list"))
						{
							response = "Permission denied.";
							return false;
						}
						response = $"Players List ({PlayerManager.players.Count})\n";
						foreach (var i in Player.List.Where((p) => p.Role != RoleType.None))
						{
							response += $"[{i.Id}]{i.Nickname}({i.UserId})<{i.Role}/{i.Health}HP> {i.Position}\n";
						}
						response.Trim();
						return true;
					}
				case "air":
					{
						if (player != null && !player.CheckPermission("sanya.airbomb"))
						{
							response = "Permission denied.";
							return false;
						}
						if (arguments.At(1).ToLower() == "start")
						{
							if (int.TryParse(arguments.At(2), out int duration))
							{
								if (float.TryParse(arguments.At(3), out float duration2))
								{
									SanyaRemastered.Instance.Handlers.RoundCoroutines.Add(Timing.RunCoroutine(Coroutines.AirSupportBomb(false, duration, duration2)));
									response = $"The AirBombing start in {duration / 60}:{duration % 60:00} and stop in {duration2 / 60}:{duration2 % 60:00}";
									return true;
								}
								else
								{
									SanyaRemastered.Instance.Handlers.RoundCoroutines.Add(Timing.RunCoroutine(Coroutines.AirSupportBomb(false, duration)));
									response = $"The AirBombing start in {duration / 60}:{duration % 60:00}!";
									return true;
								}
							}
							else
							{
								SanyaRemastered.Instance.Handlers.RoundCoroutines.Add(Timing.RunCoroutine(Coroutines.AirSupportBomb(false)));
								response = "Started!";
								return true;
							}
						}
						else if (arguments.At(1).ToLower() == "stop")
						{
							Coroutines.AirSupportBomb(true);
							Coroutines.isAirBombGoing = false;
							response = $"Stop ok.";
							return true;
						}
						else
						{
							response = $"sanya air start/stop";
							return false;
						}
					}
				case "dlock":
					{
						if (player != null && !player.CheckPermission("sanya.dlock"))
						{
							response = "Permission denied.";
							return false;
						}
						{
							if (int.TryParse(arguments.At(1), out int duration))
							{
								//Coroutines.StartContainClassD(false, duration);
								response = $"The classD are lock for {duration / 60}:{duration % 60}";
								return true;
							}
							else if (arguments.At(1).ToLower() == "false" || arguments.At(1).ToLower() == "stop")
							{
								//Coroutines.StartContainClassD(true);
								response = "Stop!";
								return true;
							}
							else if (arguments.At(1).ToLower() == "true" || arguments.At(1).ToLower() == "start")
							{
								//Coroutines.StartContainClassD(false);
								response = "Started!";
								return true;
							}
							else
							{
								response = "dlock {durée du lock} ou start/stop";
								return false;
							}
						}
					}
				case "expl":
				case "explode":
					{
						if (player != null && !player.CheckPermission("sanya.explode"))
						{
							response = "Permission denied.";
							return false;
						}
						if (arguments.Count > 1)
						{
							Player target = Player.Get(arguments.At(1));
							if (target != null && target.Role != RoleType.Spectator)
							{
								Methods.SpawnGrenade(target.Position, false, 0.1f, target.ReferenceHub);
								response = $"success. target:{target.Nickname}";
								return true;
							}
							if (arguments.At(1).ToLower() == "all")
							{
								if (player != null && !player.CheckPermission("sanya.allexplode"))
								{
									response = "Permission denied.";
									return false;
								}
								foreach (var ply in Player.List.Where((p) => p.Role != RoleType.None))
								{
									Methods.SpawnGrenade(ply.Position, false, 0.1f, ply.ReferenceHub);
								}
								response = "success spawn grenade on all player";
								return true;
							}
							else
							{
								response = "[explode] missing target.";
								return false;
							}
						}
						else
						{
							if (player != null)
							{
								Methods.SpawnGrenade(player.ReferenceHub.transform.position, false, 0.1f, player.ReferenceHub);
								response = $"success. target:{Player.Get(player.ReferenceHub.gameObject).Nickname}";
								return true;
							}
							else
							{
								response = "[explode] missing target.";
								return false;
							}
						}
					}
				case "ball":
					{
						if (player != null && !player.CheckPermission("sanya.ball"))
						{
							response = "Permission denied.";
							return false;
						}
						if (arguments.Count > 1)
						{
							Player target = Player.Get(arguments.At(1));
							if (target != null && target.Role != RoleType.Spectator)
							{
								Methods.Spawn018(target.ReferenceHub);
								response = $"success. target:{target.Nickname}";
								return true;
							}
							if (arguments.At(1).ToLower() == "all")
							{
								if (player != null && !player.CheckPermission("sanya.allball"))
								{
									response = "Permission denied.";
									return false;
								}
								foreach (var ply in Player.List.Where((p) => p.Role != RoleType.None))
								{
									Methods.Spawn018(ply.ReferenceHub);
								}
								response = "success spawn ball on all player";
								return true;
							}
							else
							{
								response = "[ball] missing target.";
								return false;
							}
						}
						else
						{
							if (player != null)
							{
								Methods.Spawn018(player.ReferenceHub);
								response = $"success. target:{Player.Get(player.ReferenceHub.gameObject).Nickname}";
								return true;
							}
							else
							{
								response = "[ball] missing target.";
								return false;
							}
						}
					}
				case "grenade":
					{
						if (player != null && !player.CheckPermission("sanya.grenade"))
						{
							response = "Permission denied.";
							return false;
						}
						if (arguments.Count > 1)
						{
							Player target = Player.Get(arguments.At(1));
							if (target != null && target.Role != RoleType.Spectator)
							{
								Methods.SpawnGrenade(target.Position, false, -1f, target.ReferenceHub);
								response = $"success. target:{target.Nickname}";
								return false;
							}
							if (arguments.At(1).ToLower() == "all")
							{
								if (player != null && !player.CheckPermission("sanya.allgrenade"))
								{
									response = "Permission denied.";
									return false;
								}
								foreach (var ply in Player.List.Where((p) => p.Role != RoleType.None))
								{
									Methods.SpawnGrenade(ply.Position, false, -1f, ply.ReferenceHub);
								}
								response = "success spawn grenade on all player";
								return true;
							}
							else
							{
								response = "[grenade] missing target.";
								return false;
							}
						}
						else
						{
							if (player != null)
							{
								Methods.SpawnGrenade(player.ReferenceHub.transform.position, false, -1f, player.ReferenceHub);
								response = $"success. target:{Player.Get(player.ReferenceHub.gameObject).Nickname}";
								return true;
							}
							else
							{
								response = "[ball] missing target.";
								return false;
							}
						}
					}
				case "clearinv":
					{
						if (player != null && !player.CheckPermission("sanya.clearinv"))
						{
							response = "Permission denied.";
							return false;
						}
						Player target = Player.UserIdsCache[arguments.At(1)];
						if (target != null && target.Role != RoleType.Spectator)
						{
							target.ClearInventory();
							response = $"Clear Inventory : {target.Nickname}";
							return true;
						}
						else if (arguments.At(1).ToLower() == "all")
						{
							if (player != null && !player.CheckPermission("sanya.allclearinv"))
							{
								response = "Permission denied.";
								return false;
							}
							foreach (var ply in Player.List.Where((p) => p.Role != RoleType.None))
							{
								ply.ClearInventory();
							}
							response = "INVENTORY OF ALL PLAYER AS BEEN CLEAR";
							return true;
						}
						else if (player.Sender != null)
						{
							player.ClearInventory();
							response = "Your Inventory as been clear";
							return true;
						}
						else
						{
							response = $"sanya clearinv <target/all>";
							return false;
						}
					}
				/*case "dummy":
					{
						if (!perm.CheckPermission("sanya.dummy"))
						{
							ev.Sender.RemoteAdminMessage("Permission denied.");
							return;
						}
						if (arguments.Count > 1)
						{
							Player target = Player.Get(arguments.At(1));
							var roletype = target.Role;
							if (target != null && target.Role != RoleType.Spectator)
							{
							Methods.SpawnDummy(target.Role , target.Position, target.ReferenceHub.transform.rotation);
							response = $"{target.Role}'s Dummy Created. pos:{target.Position} rot:{target.ReferenceHub.transform.rotation}";
							return;
							}
							if (arguments.At(1) == "all")
							{
								if (!perm.CheckPermission("sanya.alldummy"))
								{
									ev.Sender.RemoteAdminMessage("Permission denied.");
									return;
								}
								foreach (var ply in Player.List.Where((p) => p.Role != RoleType.None))
								{
									Methods.SpawnDummy(ply.Role,ply.Position,ply.ReferenceHub.transform.rotation);
								}
								response = "success spawn grenade on all player";
								return;
							}
							else
							{
								isSuccess = false;
								response = "[explode] missing target.";
								return;
							}
						}
						else
						{
							if (player != null)
							{
								Methods.SpawnDummy(RoleType.ClassD , perm.Position, player.transform.rotation);
								response = $"{perm.Role}'s Dummy Created. pos:{perm.Position} rot:{player.transform.rotation}";
								return;
							}
							else
							{
								isSuccess = false;
								response = "[explode] missing target.";
								return;
							}
						}
					}*/
				case "tppos":
					{
						if (player != null && !player.CheckPermission("sanya.tppos"))
						{
							response = "Permission denied.";
							return false;
						}
						ReferenceHub target = Player.UserIdsCache[arguments.At(1)].ReferenceHub;
						if (target != null)
						{
							if (float.TryParse(arguments.At(2), out float x)
								&& float.TryParse(arguments.At(3), out float y)
								&& float.TryParse(arguments.At(4), out float z))
							{
								Vector3 pos = new Vector3(x, y, z);
								target.playerMovementSync.OverridePosition(pos, 0f, true);
								response = $"TP to {pos}.";
								return true;
							}
							else
							{
								response = "[tppos] manque les coordonés <x> <y> <z>.";
								return false;
							}
						}
						else if (arguments.At(1).ToLower() == "all")
						{
							if (player != null && !player.CheckPermission("sanya.alltppos"))
							{
								response = "Permission denied.";
								return false;
							}
							if (float.TryParse(arguments.At(2), out float x)
								&& float.TryParse(arguments.At(3), out float y)
								&& float.TryParse(arguments.At(4), out float z))
							{
								Vector3 pos = new Vector3(x, y, z);
								foreach (var ply in Player.List.Where((p) => p.Role != RoleType.None))
								{
									ply.ReferenceHub.playerMovementSync.OverridePosition(pos, 0f, true);
								}
								response = $"TP to {pos}.";
								return true;
							}
							else
							{
								response = "[tppos] manque les coordonés <x> <y> <z>.";
								return false;
							}
						}
						else
						{
							response = "[tppos] manque la cible.";
							return false;
						}
					}
				case "gen":
					{
						if (player != null && !player.CheckPermission("sanya.gen"))
						{
							response = "Permission denied.";
							return false;
						}
						if (arguments.Count > 1)
						{
							if (arguments.At(1).ToLower() == "unlock")
							{
								foreach (var generator in Generator079.Generators)
								{
									generator.NetworkisDoorUnlocked = true;
									generator.NetworkisDoorOpen = true;
									generator._doorAnimationCooldown = 0.5f;
								}
								response = "gen unlocked.";
								return true;
							}
							else if (arguments.At(1).ToLower() == "door")
							{
								foreach (var generator in Generator079.Generators)
								{
									if (!generator.prevFinish)
									{
										bool now = !generator.isDoorOpen;
										generator.NetworkisDoorOpen = now;
										generator.CallRpcDoSound(now);
									}
								}
								response = $"gen doors interacted.";
								return true;
							}
							else if (arguments.At(1).ToLower() == "set")
							{
								float cur = 10f;
								foreach (var generator in Generator079.Generators)
								{
									if (!generator.prevFinish)
									{
										generator.NetworkisDoorOpen = true;
										generator.NetworkisTabletConnected = true;
										generator.NetworkremainingPowerup = cur;
										cur += 10f;
									}
								}
								response = "gen set.";
								return true;
							}
							else if (arguments.At(1).ToLower() == "once")
							{
								Generator079 gen = Generator079.Generators.FindAll(x => !x.prevFinish).GetRandomOne();

								if (gen != null)
								{
									gen.NetworkisDoorUnlocked = true;
									gen.NetworkisTabletConnected = true;
									gen.NetworkisDoorOpen = true;
								}
								response = "set once.";
								return true;
							}
							else if (arguments.At(1).ToLower() == "eject")
							{
								foreach (var generator in Generator079.Generators)
								{
									if (generator.isTabletConnected)
									{
										generator.EjectTablet();
									}
								}
								response = "gen ejected.";
								return true;
							}
							else
							{
								response = "[gen] Wrong Parameters.";
								return false;
							}
						}
						else
						{
							response = "[gen] Parameters : gen <unlock/door/set/once/eject>";
							return false;
						}
					}
				case "spawn":
					{
						if (player != null && !player.CheckPermission("sanya.spawn"))
						{
							response = "Permission denied.";
							return false;
						}
						var mtfRespawn = RespawnManager.Singleton;
						if (arguments.Count > 3)
						{
							if (arguments.At(1).ToLower() == "ci" || arguments.At(1).ToLower() == "ic")
							{
								mtfRespawn._timeForNextSequence = 0f;
								mtfRespawn.NextKnownTeam = SpawnableTeamType.ChaosInsurgency;
								response = $"force spawn ChaosInsurgency";
								return true;
							}
							else if (arguments.At(1).ToLower() == "mtf" || arguments.At(1).ToLower() == "ntf")
							{
								mtfRespawn._timeForNextSequence = 0f;
								mtfRespawn.NextKnownTeam = SpawnableTeamType.NineTailedFox;
								response = $"force spawn NineTailedFox";
								return true;
							}
							else if (arguments.At(1).ToLower() == "stop")
							{
								response = $"ok.[{SanyaRemastered.Instance.Handlers.StopRespawn}] -> ";
								SanyaRemastered.Instance.Handlers.StopRespawn = !SanyaRemastered.Instance.Handlers.StopRespawn;
								response += $"[{SanyaRemastered.Instance.Handlers.StopRespawn}]";
								return true;
							}
							else
							{
								response = $"ntf/mtf ou ci/ic ou rien";
								return false;
							}
						}
						else
						{
							if (mtfRespawn.NextKnownTeam == SpawnableTeamType.ChaosInsurgency)
							{
								mtfRespawn._timeForNextSequence = 0f;
								response = $"Spawn. Chaos Insurgency";
								return true;
							}
							else
							{
								mtfRespawn._timeForNextSequence = 0f;
								response = $"Spawn. Nine Tailed Fox";
								return true;
							}
						}
					}
				case "next":
					{
						if (player != null && !player.CheckPermission("sanya.next"))
						{
							response = "Permission denied.";
							return false;
						}
						int respawntime = (int)Math.Truncate(RespawnManager.CurrentSequence() == RespawnManager.RespawnSequencePhase.RespawnCooldown ? RespawnManager.Singleton._timeForNextSequence - RespawnManager.Singleton._stopwatch.Elapsed.TotalSeconds : 0);
						var mtfRespawn = RespawnManager.Singleton;
						if (arguments.Count > 3)
						{
							if (arguments.At(1).ToLower() == "ci" || arguments.At(1).ToLower() == "ic")
							{
								mtfRespawn.NextKnownTeam = SpawnableTeamType.ChaosInsurgency;
								response = $"Is Success:{mtfRespawn.NextKnownTeam == SpawnableTeamType.ChaosInsurgency}\n ";
								response += $"Prochains renforts : {respawntime / 60:00}:{respawntime % 60:00}";
								return true;
							}
							else if (arguments.At(1).ToLower() == "mtf" || arguments.At(1).ToLower() == "ntf")
							{
								mtfRespawn.NextKnownTeam = SpawnableTeamType.NineTailedFox;
								response = $"Is Success:{mtfRespawn.NextKnownTeam == SpawnableTeamType.NineTailedFox}";
								response += $"Prochains renforts : {respawntime / 60:00}:{respawntime % 60:00}";
								return true;
							}
							else
							{
								response = "ntf/mtf ou ci/ic";
								return false;
							}
						}
						else
						{
							if (mtfRespawn.NextKnownTeam == SpawnableTeamType.ChaosInsurgency)
							{
								response = $"\nProchain Respawn = ChaosInsurgency";
								response += $"\nProchains renforts : {respawntime / 60:00}:{respawntime % 60:00}";
								return true;
							}
							else
							{
								response = $"\nProchain Respawn = NineTailedFox";
								response += $"\nProchains renforts : {respawntime / 60:00}:{respawntime % 60:00}";
								return true;
							}
						}
					}
				case "van":
					{
						if (player != null && !player.CheckPermission("sanya.van"))
						{
							response = "Permission denied.";
							return false;
						}
						Respawn.SummonChaosInsurgencyVan(false);
						response = "Van as comming";
						return true;
					}
				case "heli":
					{
						if (player != null && !player.CheckPermission("sanya.heli"))
						{
							response = "Permission denied.";
							return false;
						}
						Respawn.SummonNtfChopper();
						response = "Heli as comming";
						return true;
					}
				default:
					{
						response = "invalid params.";
						return false;
					}
			}
		}
	}
}
