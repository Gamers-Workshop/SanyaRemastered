/*using System;
using System.Collections.Generic;
using CommandSystem;
using CustomPlayerEffects;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
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

		public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
		{
			Log.Debug($"[Commands] Sender:{sender.LogName} args:{arguments.Count}", SanyaPlugin.Instance.Config.IsDebugged);

			Player player = null;
			if (sender is PlayerCommandSender playerCommandSender) player = Player.Get(playerCommandSender.SenderId);

			if (player != null && !player.CheckPermission("sanya.command"))
			{
				response = "Permission denied.";
				return false;
			}

			if (arguments.Count == 0)
			{
				response = "sanya plugins command.";
				return true;
			}

			switch (arguments.FirstElement().ToLower())
			{
				case "ping":
					{
						if (player != null && !player.CheckPermission("sanya.ping"))
						{
							response = "Permission denied.";
							return false;
						}
						response = "Pings:\n";

						foreach (var ply in Player.List)
						{
							response += $"{ply.Nickname} : {LiteNetLib4MirrorServer.Peers[ply.Connection.connectionId].Ping}ms\n";
						}
						return true;
					}
				case "addscps":
					{
						if (player != null && !player.CheckPermission("sanya.addscps"))
						{
							response = "Permission denied.";
							return false;
						}
						response = $"ok.{RoundSummary.singleton.classlistStart.scps_except_zombies++}";
						return true;
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
				case "showconfig":
					{
						if (player != null && !player.CheckPermission("sanya.showconfig"))
						{
							response = "Permission denied.";
							return false;
						}
						response = SanyaPlugin.Instance.Config.GetConfigs();
						break;
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
						break;
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
						break;
					}
				case "air":
					{
						if (player != null && !player.CheckPermission("sanya.airbomb"))
						{
							response = "Permission denied.";
							return false;
						}
						if (args[1] == "start")
						{
							if (int.TryParse(args[2], out int duration))
							{
								if (int.TryParse(args[3], out int duration2))
								{
									roundCoroutines.Add(Timing.RunCoroutine(Coroutines.AirSupportBomb(false, duration, duration2)));
									response = $"The AirBombing start in {duration / 60}:{duration % 60:00} and stop in {duration2 / 60}:{duration2 % 60:00}";
									break;
								}
								else
								{
									roundCoroutines.Add(Timing.RunCoroutine(Coroutines.AirSupportBomb(false, duration)));
									response = $"The AirBombing start in {duration / 60}:{duration % 60:00}!";
									break;
								}
							}
							else
							{
								roundCoroutines.Add(Timing.RunCoroutine(Coroutines.AirSupportBomb(false)));
								response = "Started!";
								break;
							}
						}
						else if (args[1] == "stop")
						{
							roundCoroutines.Add(Timing.RunCoroutine(Coroutines.AirSupportBomb(true)));
							Coroutines.isAirBombGoing = false;
							response = $"Stop ok.";
							break;
						}
						else
						{
							response = $"sanya air start/stop";
							break;
						}
					}
				case "stoprespawn":
				case "stopres":
					{
						if (player != null && !player.CheckPermission("sanya.stoprespawn"))
						{
							response = "Permission denied.";
							return false;
						}
						if (args.Length > 2 && args[2] == "true")
						{
							StopRespawn = true;
							response = $"StopRespawn = {StopRespawn}";
						}
						else if (args.Length > 2 && args[2] == "false")
						{
							StopRespawn = false;
							response = $"StopRespawn = {StopRespawn}";
						}
						else
						{
							response = $"StopRespawn = {StopRespawn}";
						}
						break;
					}
				case "stopticket":
					{
						if (player != null && !player.CheckPermission("sanya.stopticket"))
						{
							response = "Permission denied.";
							return false;
						}
						if (args.Length > 2 && args[2] == "true")
						{
							StopTicket = true;
							response = $"StopTicket = {StopTicket}";
						}
						else if (args.Length > 2 && args[2] == "false")
						{
							StopTicket = false;
							response = $"StopTicket = {StopTicket}";
						}
						else
						{
							response = $"StopTicket = {StopTicket}";
						}
						break;
					}
				case "914":
					{
						if (player != null && !player.CheckPermission("sanya.914"))
						{
							response = "Permission denied.";
							return false;
						}
						if (args.Length > 2)
						{
							if (!Scp914.Scp914Machine.singleton.working)
							{

								if (args[2] == "use")
								{
									Scp914.Scp914Machine.singleton.RpcActivate(NetworkTime.time);
									response = $"Used : {Scp914.Scp914Machine.singleton.knobState}";
								}
								else if (args[2] == "knob")
								{
									Scp914.Scp914Machine.singleton.ChangeKnobStatus();
									response = $"Knob Changed to:{Scp914.Scp914Machine.singleton.knobState}";
								}
								else
								{
									isSuccess = false;
									response = "[914] Wrong Parameters.";
								}
							}
							else
							{
								isSuccess = false;
								response = "[914] SCP-914 is working now.";
							}
						}
						else
						{
							isSuccess = false;
							response = "[914] Parameters : 914 <use/knob>";
						}
						break;
					}
				case "nukecap":
					{
						if (player != null && !player.CheckPermission("sanya.nukecap"))
						{
							response = "Permission denied.";
							return false;
						}
						var outsite = GameObject.Find("OutsitePanelScript")?.GetComponent<AlphaWarheadOutsitePanel>();
						outsite.NetworkkeycardEntered = !outsite.keycardEntered;
						response = $"{outsite?.keycardEntered}";
						break;
					}
				case "femur":
					{
						if (player != null && !player.CheckPermission("sanya.femur"))
						{
							response = "Permission denied.";
							return false;
						}
						PlayerManager.localPlayer.GetComponent<PlayerInteract>()?.RpcContain106(PlayerManager.localPlayer);
						response = "FemurScreamer!";
						break;
					}
				case "dlock":
					{
						if (player != null && !player.CheckPermission("sanya.dlock"))
						{
							response = "Permission denied.";
							return false;
						}
						{
							if (int.TryParse(args[2], out int duration))
							{
								roundCoroutines.Add(Timing.RunCoroutine(Coroutines.StartContainClassD(false, duration)));
								response = $"The classD are lock for {duration / 60}:{duration % 60}";
								break;
							}
							else if (args[2] == "false" || args[2] == "stop")
							{
								roundCoroutines.Add(Timing.RunCoroutine(Coroutines.StartContainClassD(true)));
								response = "Stop!";
								break;
							}
							else if (args[2] == "true" || args[2] == "start")
							{
								roundCoroutines.Add(Timing.RunCoroutine(Coroutines.StartContainClassD(false)));
								response = "Started!";
								break;
							}
							else
							{
								isSuccess = false;
								response = "dlock {durée du lock} ou start/stop";
								break;
							}
						}
					}
				case "explode":
					{
						if (player != null && !player.CheckPermission("sanya.explode"))
						{
							response = "Permission denied.";
							return false;
						}
						if (args.Length > 2)
						{
							Player target = Player.Get(args[2]);
							if (target != null && target.Role != RoleType.Spectator)
							{
								Methods.SpawnGrenade(target.Position, false, 0.1f, target.ReferenceHub);
								response = $"success. target:{target.Nickname}";
								break;
							}
							if (args[2] == "all")
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
								break;
							}
							else
							{
								isSuccess = false;
								response = "[explode] missing target.";
								break;
							}
						}
						else
						{
							if (player != null)
							{
								Methods.SpawnGrenade(player.transform.position, false, 0.1f, player);
								response = $"success. target:{Player.Get(player.gameObject).Nickname}";
								break;
							}
							else
							{
								isSuccess = false;
								response = "[explode] missing target.";
								break;
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
						if (args.Length > 2)
						{
							Player target = Player.Get(args[2]);
							if (target != null && target.Role != RoleType.Spectator)
							{
								Methods.Spawn018(target.ReferenceHub);
								response = $"success. target:{target.Nickname}";
								break;
							}
							if (args[2] == "all")
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
								break;
							}
							else
							{
								isSuccess = false;
								response = "[ball] missing target.";
								break;
							}
						}
						else
						{
							if (player != null)
							{
								Methods.Spawn018(player);
								response = $"success. target:{Player.Get(player.gameObject).Nickname}";
								break;
							}
							else
							{
								isSuccess = false;
								response = "[ball] missing target.";
								break;
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
						if (args.Length > 2)
						{
							Player target = Player.Get(args[2]);
							if (target != null && target.Role != RoleType.Spectator)
							{
								Methods.SpawnGrenade(target.Position, false, -1f, target.ReferenceHub);
								response = $"success. target:{target.Nickname}";
								break;
							}
							if (args[2] == "all")
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
								break;
							}
							else
							{
								isSuccess = false;
								response = "[grenade] missing target.";
								break;
							}
						}
						else
						{
							if (player != null)
							{
								Methods.SpawnGrenade(player.transform.position, false, -1f, player);
								response = $"success. target:{Player.Get(player.gameObject).Nickname}";
								break;
							}
							else
							{
								isSuccess = false;
								response = "[ball] missing target.";
								break;
							}
						}
					}
				case "ammo":
					{
						if (player != null && !player.CheckPermission("sanya.ammo"))
						{
							response = "Permission denied.";
							return false;
						}
						if (args.Length > 2)
						{
							Player target = Player.Get(args[2]);
							if (target != null && target.Role != RoleType.Spectator)
							{
								if (uint.TryParse(args[3], out uint Nato556) &&
									uint.TryParse(args[4], out uint Nato762) &&
									uint.TryParse(args[5], out uint Nato9))
								{
									target.SetAmmo(Exiled.API.Enums.AmmoType.Nato556, Nato556);
									target.SetAmmo(Exiled.API.Enums.AmmoType.Nato762, Nato762);
									target.SetAmmo(Exiled.API.Enums.AmmoType.Nato9, Nato9);
									response = $"{target.Nickname}  {Nato556}:{Nato762}:{Nato9}";
									break;
								}
								else
								{
									response = "sanya ammo {player} (5.56) (7.62) (9mm).";
									break;
								}
							}
							if (args[2] == "all")
							{
								if (player != null && !player.CheckPermission("sanya.allammo"))
								{
									response = "Permission denied.";
									return false;
								}
								if (uint.TryParse(args[3], out uint Nato556)
									&& uint.TryParse(args[4], out uint Nato762)
									&& uint.TryParse(args[5], out uint Nato9))
								{
									foreach (var ply in Player.List)
									{
										ply.SetAmmo(Exiled.API.Enums.AmmoType.Nato556, Nato556);
										ply.SetAmmo(Exiled.API.Enums.AmmoType.Nato762, Nato762);
										ply.SetAmmo(Exiled.API.Enums.AmmoType.Nato9, Nato9);
									}
									response = $"ammo set {Nato556}:{Nato762}:{Nato9}";
									break;
								}
								else
								{
									response = "sanya ammo all (5.56) (7.62) (9mm)";
									break;
								}
							}
							else
							{
								isSuccess = false;
								response = "sanya (player id ou all) ";
								break;
							}
						}
						else
						{
							response = "Failed to set. (cant use from SERVER)";
							break;
						}
					}
				case "clearinv":
					{
						if (player != null && !player.CheckPermission("sanya.clearinv"))
						{
							response = "Permission denied.";
							return false;
						}
						Player target = Player.UserIdsCache[args[2]];
						if (target != null && target.Role != RoleType.Spectator)
						{
							target.ClearInventory();
							response = $"Clear Inventory : {target.Nickname}";
							break;
						}
						else if (args[2] == "all")
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
							break;
						}
						else if (ev.Sender != null)
						{
							ev.Sender.ClearInventory();
							response = "Your Inventory as been clear";
							break;
						}
						else
						{
							response = $"sanya clearinv <target/all>";
							break;
						}
					}
				case "cleareffect":
					{
						if (player != null && !player.CheckPermission("sanya.cleareffect"))
						{
							response = "Permission denied.";
							return false;
						}
						if (args.Length > 2)
						{

							Player target = Player.UserIdsCache[args[2]];
							if (target != null && target.Role != RoleType.Spectator)
							{
								foreach (KeyValuePair<Type, PlayerEffect> keyValuePair in target.ReferenceHub.playerEffectsController.AllEffects.ToArray())
								{
									PlayerEffect effect = keyValuePair.Value;
									effect.ServerDisable();
								}
								response = $"ALL EFFECT AS BEEN CLEAR FOR {target.Nickname}";
								break;
							}
							else
							{
								isSuccess = false;
								response = "Fail";
								break;
							}
						}
						if (args[2] == "all")
						{
							foreach (var ply in Player.List)
							{
								foreach (KeyValuePair<Type, PlayerEffect> keyValuePair in ply.ReferenceHub.playerEffectsController.AllEffects.ToArray())
								{
									PlayerEffect effect = keyValuePair.Value;
									effect.ServerDisable();
								}
							}
							response = "ALL EFFECT OF ALL PLAYER AS BEEN CLEAR";
							break;
						}
						if (player != null)
						{
							foreach (KeyValuePair<Type, PlayerEffect> keyValuePair in player.playerEffectsController.AllEffects.ToArray())
							{
								PlayerEffect effect = keyValuePair.Value;
								effect.ServerDisable();
							}
							response = "ALL YOUR EFFECT AS BEEN CLEAR";
							break;
						}
						else
						{
							response = "Failed to set.";
							break;
						}
					}
				case "dummy":
					{
						if (player != null && !player.CheckPermission("sanya.dummy"))
						{
							response = "Permission denied.";
							return false;
						}
						if (args.Length > 2)
						{
							Player target = Player.Get(args[2]);
							var roletype = target.Role;
							if (target != null && target.Role != RoleType.Spectator)
							{
								Methods.SpawnDummy(target.Role, target.Position, target.ReferenceHub.transform.rotation);
								response = $"{target.Role}'s Dummy Created. pos:{target.Position} rot:{target.ReferenceHub.transform.rotation}";
								break;
							}
							if (args[2] == "all")
							{
								if (player != null && !player.CheckPermission("sanya.alldummy"))
								{
									response = "Permission denied.";
									return false;
								}
								foreach (var ply in Player.List)
								{
									Methods.SpawnDummy(ply.Role, ply.Position, ply.ReferenceHub.transform.rotation);
								}
								response = "success spawn grenade on all player";
								break;
							}
							else
							{
								isSuccess = false;
								response = "[explode] missing target.";
								break;
							}
						}
						else
						{
							if (player != null)
							{
								Methods.SpawnDummy(RoleType.ClassD, player.Position, player.transform.rotation);
								response = $"{player.Role}'s Dummy Created. pos:{player.Position} rot:{player.transform.rotation}";
								break;
							}
							else
							{
								isSuccess = false;
								response = "[explode] missing target.";
								break;
							}
						}
					}
				case "tppos":
					{
						if (player != null && !player.CheckPermission("sanya.tppos"))
						{
							response = "Permission denied.";
							return false;
						}
						ReferenceHub target = Player.UserIdsCache[args[2]].ReferenceHub;
						if (target != null)
						{
							if (float.TryParse(args[3], out float x)
								&& float.TryParse(args[4], out float y)
								&& float.TryParse(args[5], out float z))
							{
								Vector3 pos = new Vector3(x, y, z);
								target.playerMovementSync.OverridePosition(pos, 0f, true);
								response = $"TP to {pos}.";
							}
							else
							{
								isSuccess = false;
								response = "[tppos] manque les coordonés <x> <y> <z>.";
							}
						}
						else if (args[2] == "all")
						{
							if (player != null && !player.CheckPermission("sanya.alltppos"))
							{
								response = "Permission denied.";
								return false;
							}
							if (float.TryParse(args[3], out float x)
								&& float.TryParse(args[4], out float y)
								&& float.TryParse(args[5], out float z))
							{
								Vector3 pos = new Vector3(x, y, z);
								foreach (var ply in Player.List)
								{
									ply.ReferenceHub.playerMovementSync.OverridePosition(pos, 0f, true);
								}
								response = $"TP to {pos}.";
							}
							else
							{
								isSuccess = false;
								response = "[tppos] manque les coordonés <x> <y> <z>.";
							}
						}
						else
						{
							isSuccess = false;
							response = "[tppos] manque la cible.";
						}
						break;
					}
				case "gen":
					{
						if (player != null && !player.CheckPermission("sanya.gen"))
						{
							response = "Permission denied.";
							return false;
						}
						if (args.Length > 2)
						{
							if (args[2] == "unlock")
							{
								foreach (var generator in Generator079.Generators)
								{
									generator.NetworkisDoorUnlocked = true;
									generator.NetworkisDoorOpen = true;
									generator._doorAnimationCooldown = 0.5f;
								}
								response = "gen unlocked.";
							}
							else if (args[2] == "door")
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
							}
							else if (args[2] == "set")
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
							}
							else if (args[2] == "once")
							{
								Generator079 gen = Generator079.Generators.FindAll(x => !x.prevFinish).GetRandomOne();

								if (gen != null)
								{
									gen.NetworkisDoorUnlocked = true;
									gen.NetworkisTabletConnected = true;
									gen.NetworkisDoorOpen = true;
								}
								response = "set once.";
							}
							else if (args[2] == "eject")
							{
								foreach (var generator in Generator079.Generators)
								{
									if (generator.isTabletConnected)
									{
										generator.EjectTablet();
									}
								}
								response = "gen ejected.";
							}
							else
							{
								isSuccess = false;
								response = "[gen] Wrong Parameters.";
							}
						}
						else
						{
							isSuccess = false;
							response = "[gen] Parameters : gen <unlock/door/set/once/eject>";
						}
						break;
					}
				case "spawn":
					{
						if (player != null && !player.CheckPermission("sanya.spawn"))
						{
							response = "Permission denied.";
							return false;
						}
						var mtfRespawn = RespawnManager.Singleton;
						if (args.Length > 2)
						{
							if (args[2] == "ci" || args[2] == "ic")
							{
								mtfRespawn.NextKnownTeam = SpawnableTeamType.ChaosInsurgency;
								mtfRespawn._started = true;
								response = $"force spawn ChaosInsurgency";
								break;
							}
							else if (args[2] == "mtf" || args[2] == "ntf")
							{
								mtfRespawn.NextKnownTeam = SpawnableTeamType.NineTailedFox;
								mtfRespawn._started = true;
								response = $"force spawn NineTailedFox";
								break;
							}
							else
							{
								response = $"ntf/mtf ou ci/ic ou rien";
								break;
							}
						}
						else
						{
							if (mtfRespawn.NextKnownTeam == SpawnableTeamType.ChaosInsurgency)
							{
								mtfRespawn._timeForNextSequence = 0;
								response = $"Spawn. Chaos Insurgency";
								break;
							}
							else
							{
								mtfRespawn._timeForNextSequence = 0;
								response = $"Spawn. Nine Tailed Fox";
								break;
							}
						}
					}
				case "next":
					{
						if (player != null && !player.CheckPermission("sanya.next") && player != null && !player.CheckPermission("sanya.spawn"))
						{
							response = "Permission denied.";
							return false;
						}
						var mtfRespawn = RespawnManager.Singleton;
						if (args.Length > 2)
						{
							if (args[2] == "time")
							{
								//NextSpawn
								response = $"Futur Commande";
								break;
							}
							if (args[2] == "ci" || args[2] == "ic")
							{
								mtfRespawn.NextKnownTeam = SpawnableTeamType.ChaosInsurgency;
								response = $"Is Success:{mtfRespawn.NextKnownTeam == SpawnableTeamType.ChaosInsurgency}";
								break;
							}
							else if (args[2] == "mtf" || args[2] == "ntf")
							{
								mtfRespawn.NextKnownTeam = SpawnableTeamType.NineTailedFox;
								response = $"Is Success:{mtfRespawn.NextKnownTeam == SpawnableTeamType.NineTailedFox}";
								break;
							}
							else
							{
								isSuccess = false;
								response = "ntf/mtf ou ci/ic";
								break;
							}
						}
						else
						{
							if (mtfRespawn.NextKnownTeam == SpawnableTeamType.ChaosInsurgency)
							{
								response = $"Prochain Respawn = ChaosInsurgency";
								break;
							}
							else
							{
								response = $"Prochain Respawn = NineTailedFox";
								break;
							}
						}
					}
				/*case "van":
					{
						if (player != null && !player.CheckPermission("sanya.deco"))
						{
							response = "Permission denied.";
							return false;
						}

						response = "Van Called!";
						break;
					}
				case "heli":
					{
						if (player != null && !player.CheckPermission("sanya.deco"))
						{
							response = "Permission denied.";
							return false;
						}

						response = "Heli Called!";
						break;
					}*//*
				default:
					{
						response = "invalid params.";
						return false;
					}
			}
		}
	}
}
*/