using System;
using CommandSystem;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using HarmonyLib;
using MEC;
using Mirror;
using Mirror.LiteNetLib4Mirror;
using RemoteAdmin;
using Respawning;
using SanyaPlugin.Functions;
using UnityEngine;

namespace SanyaPlugin.Commands
{
	[CommandHandler(typeof(GameConsoleCommandHandler))]
	[CommandHandler(typeof(RemoteAdminCommandHandler))]
	class Commands : ICommand
	{
		public string Command { get; } = "sanya";

		public string[] Aliases { get; } = new string[] { "sn" };

		public string Description { get; } = "SanyaPlugin Commands";

		private bool isActwatchEnabled = false;

		public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
		{
			Log.Debug($"[Commands] Sender:{sender.LogName} args:{arguments.Count}", SanyaPlugin.Instance.Config.IsDebugged);

			Player player = null;
			if(sender is PlayerCommandSender playerCommandSender) player = Player.Get(playerCommandSender.SenderId);

			if(arguments.Count == 0)
			{
				response = "sanya plugins command.";
				return true;
			}

			switch(arguments.FirstElement().ToLower())
			{
				case "test":
					{
						response = "test ok.";
						return true;
					}
				case "hud":
					{
						if (player != null && !player.CheckPermission("sanya.hud"))
						{
							response = "Permission denied.";
							return false;
						}
						var comp = player.GameObject.GetComponent<SanyaPluginComponent>();
						response = $"ok.{comp.DisableHud} -> ";
						comp.DisableHud = !comp.DisableHud;
						response += $"{comp.DisableHud}";
						return true;
					}
				case "hand":
					{
						if (player != null && !player.CheckPermission("sanya.hand"))
						{
							response = "Permission denied.";
							return false;
						}
						if (arguments.Count > 1)
						{
							var hub = ReferenceHub.GetHub(int.Parse(arguments.At(1)));
							UnityEngine.Object.FindObjectOfType<SCPSL.Halloween.Scp330>()?.SpawnHands(hub);
							response = "ok.";
							return true;
						}
						else if(player != null)
						{
							UnityEngine.Object.FindObjectOfType<SCPSL.Halloween.Scp330>()?.SpawnHands(player.ReferenceHub);
							response = "ok.";
							return true;
						}
						else
						{
							response = "this command cant use on server console.";
							return false;
						}
					}
				case "ping":
					{
						if (player != null && !player.CheckPermission("sanya.ping"))
						{
							response = "Permission denied.";
							return false;
						}
						response = "Pings:\n";

						foreach(var ply in Player.List)
						{
							response += $"{ply.Nickname} : {LiteNetLib4MirrorServer.Peers[ply.Connection.connectionId].Ping}ms\n";
						}
						return true;
					}
				case "actwatch":
					{
						if (player != null && !player.CheckPermission("sanya.actwatch"))
						{
							response = "Permission denied.";
							return false;
						}
						if (player == null) {
							response = "Only can use with RemoteAdmin.";
							return false; 
						}

						if(!isActwatchEnabled)
						{
							player.SendCustomSync(player.ReferenceHub.networkIdentity, typeof(PlayerEffectsController), (writer) => {
								writer.WritePackedUInt64(1ul);
								writer.WritePackedUInt32((uint)1);
								writer.WriteByte((byte)SyncList<byte>.Operation.OP_SET);
								writer.WritePackedUInt32((uint)3);
								writer.WriteByte((byte)1);
							}, null);
							isActwatchEnabled = true;
						}
						else
						{
							player.SendCustomSync(player.ReferenceHub.networkIdentity, typeof(PlayerEffectsController), (writer) => {
								writer.WritePackedUInt64(1ul);
								writer.WritePackedUInt32((uint)1);
								writer.WriteByte((byte)SyncList<byte>.Operation.OP_SET);
								writer.WritePackedUInt32((uint)3);
								writer.WriteByte((byte)0);
							}, null);
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
						Generator079.mainGenerator.ServerOvercharge(8f, false);
						response = "ok.";
						return true;
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
									foreach (var ply in Player.List)
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
						response = SanyaPlugin.Instance.Config.GetConfigs();
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
				case "playgen":
					{
						if (player != null && !player.CheckPermission("sanya.dev"))
						{
							response = "Permission denied.";
							return false;
						}
						if (byte.TryParse(arguments.At(1), out byte sound))
						{
							Methods.PlayGenerator079sound(sound);

						}
						response = $"Ambien sound \n";
						return true;
					}
				case "listdoor":
					{
						response = $"RoomList\n";
						foreach (var doors in Map.Doors)
						{
							response += $"{doors.doorType} : {doors.name} : {doors.DoorName} \n";
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
						SanyaPlugin.Instance.Config.GetConfigs();
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
						foreach (var i in Player.List)
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
						if (arguments.At(1) == "start")
						{
							if (int.TryParse(arguments.At(2), out int duration))
							{
								if (int.TryParse(arguments.At(3), out int duration2))
								{
									Coroutines.AirSupportBomb(false, duration, duration2);
									response = $"The AirBombing start in {duration / 60}:{duration % 60:00} and stop in {duration2 / 60}:{duration2 % 60:00}";
									return true;
								}
								else
								{
									Coroutines.AirSupportBomb(false, duration);
									response = $"The AirBombing start in {duration / 60}:{duration % 60:00}!";
									return true;
								}
							}
							else
							{
								Coroutines.AirSupportBomb(true);
								response = "Started!";
								return true;
							}
						}
						else if (arguments.At(1) == "stop")
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
								Coroutines.StartContainClassD(false, duration);
								response = $"The classD are lock for {duration / 60}:{duration % 60}";
								return true;
							}
							else if (arguments.At(1) == "false" || arguments.At(1) == "stop")
							{
								Coroutines.StartContainClassD(true);
								response = "Stop!";
								return true;
							}
							else if (arguments.At(1) == "true" || arguments.At(1) == "start")
							{
								Coroutines.StartContainClassD(false);
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
							if (arguments.At(1) == "all")
							{
								if (player != null && !player.CheckPermission("sanya.allexplode"))
								{
									response = "Permission denied.";
									return false;
								}
								foreach (var ply in Player.List)
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
							if (arguments.At(1) == "all")
							{
								if (player != null && !player.CheckPermission("sanya.allball"))
								{
									response = "Permission denied.";
									return false;
								}
								foreach (var ply in Player.List)
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
							if (arguments.At(1) == "all")
							{
								if (player != null && !player.CheckPermission("sanya.allgrenade"))
								{
									response = "Permission denied.";
									return false;
								}
								foreach (var ply in Player.List)
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
						else if (arguments.At(1) == "all")
						{
							if (player != null && !player.CheckPermission("sanya.allclearinv"))
							{
								response = "Permission denied.";
								return false;
							}
							foreach (var ply in Player.List)
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
								foreach (var ply in Player.List)
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
						else if (arguments.At(1) == "all")
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
								foreach (var ply in Player.List)
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
						if (arguments.Count > 2)
						{
							if (arguments.At(1) == "unlock")
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
							else if (arguments.At(1) == "door")
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
							else if (arguments.At(1) == "set")
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
							else if (arguments.At(1) == "once")
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
							else if (arguments.At(1) == "eject")
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
							if (arguments.At(1) == "ci" || arguments.At(1) == "ic")
							{
								mtfRespawn.NextKnownTeam = SpawnableTeamType.ChaosInsurgency;
								mtfRespawn.Start();
								response = $"force spawn ChaosInsurgency";
								return true;
							}
							else if (arguments.At(1) == "mtf" || arguments.At(1) == "ntf")
							{
								mtfRespawn.NextKnownTeam = SpawnableTeamType.NineTailedFox;
								mtfRespawn.Start();
								response = $"force spawn NineTailedFox";
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
								mtfRespawn.Start();
								response = $"Spawn. Chaos Insurgency";
								return true;
							}
							else
							{
								mtfRespawn.Start();
								response = $"Spawn. Nine Tailed Fox";
								return true;
							}
						}
					}
				case "next":
					{
						if (player != null && !player.CheckPermission("sanya.next") || !player.CheckPermission("sanya.spawn"))
						{
							response = "Permission denied.";
							return false;
						}
						int respawntime = (int)Math.Truncate(RespawnManager.CurrentSequence() == RespawnManager.RespawnSequencePhase.RespawnCooldown ? RespawnManager.Singleton._timeForNextSequence - RespawnManager.Singleton._stopwatch.Elapsed.TotalSeconds : 0);
						var mtfRespawn = RespawnManager.Singleton;
						if (arguments.Count > 3)
						{
							if (arguments.At(1) == "ci" || arguments.At(1) == "ic")
							{
								mtfRespawn.NextKnownTeam = SpawnableTeamType.ChaosInsurgency;
								response = $"Is Success:{mtfRespawn.NextKnownTeam == SpawnableTeamType.ChaosInsurgency}\n ";
								response += $"Prochains renforts : {respawntime / 60:00}:{respawntime % 60:00}";
								return true;
							}
							else if (arguments.At(1) == "mtf" || arguments.At(1) == "ntf")
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
