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
using MapGeneration.Distributors;
using Exiled.API.Extensions;
using Extensions = SanyaRemastered.Functions.Extensions;
using Scp914;
using Utils.Networking;
using AdminToys;
using InventorySystem.Items.Pickups;
using InventorySystem;

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
		private ItemPickupBase targetitem = null;
		private GameObject targetstation = null;
		private GameObject targetTarget = null;
		private PrimitiveObjectToy targetPrimitive = null;
		private LightSourceToy targetLight = null;

		public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
		{
			Log.Debug($"[Commands] Sender:{sender.LogName} args:{arguments.Count}", SanyaRemastered.Instance.Config.IsDebugged);

			Player player = null;
			if(sender is PlayerCommandSender playerCommandSender) player = Player.Get(playerCommandSender.SenderId);

			if (arguments.Count == 0)
			{
				response = "sanya plugins command params: <ping/override/actwatch/106/914/nukecap/nukelock/femur/blackout/addscps/ammo/forcend/now/config>";
				return true;
			}

			switch (arguments.FirstElement().ToLower())
			{
				case "test":
					{
						response = $"test ok.";
						return true;
					}
				/*case "rainbow"://SCP330 Command
                    {
						try
                        {
							new InventorySystem.Items.Usables.Scp330.CandyRainbow.ExplosionMessage
							{
								ExplosionPosition = player.Position
							}.SendToAuthenticated(0);
							response = "Is work ?? i think";
							return true;
						}
						catch (Exception ex)
                        {
							response = "Is not work Spaghetti bolognaise\n" + ex;
							return false;
						}
					}*/
				case "spawnobject":
                    {
						if (player != null && !player.CheckPermission("sanya.dev"))
						{
							response = "Permission denied.";
							return false;
						}
						NetworkServer.Spawn(UnityEngine.Object.Instantiate(GameObject.CreatePrimitive(PrimitiveType.Cube), player.Position, player.GameObject.transform.rotation));
						response = "test ok.";
						return true;
					}
				case "audio":
                    {
						if (player != null && !player.CheckPermission("sanya.dev"))
						{
							response = "Permission denied.";
							return false;
						}
						//Methods.PlayFileRaw("/home/scp/.config/EXILED/Configs/AudioAPI/049_Ringo_Ringo_Roses.raw", 9997, 1, true, player.Position);

						response = "test ok.";
						return true;
					}
				case "identitytree":
					{
						response = "ok.";
						foreach (var identity in UnityEngine.Object.FindObjectsOfType<NetworkIdentity>())
						{
							Log.Warn($"{identity.transform.name} (layer{identity.transform.gameObject.layer})");
							Log.Warn($"HasComponents:");
							foreach (var i in identity.transform.gameObject.GetComponents<Component>())
							{
								Log.Warn($"    {i?.name}:{i?.GetType()}");
							}
							Log.Warn($"HasComponentsInChildren:");
							foreach (var i in identity.transform.gameObject.GetComponentsInChildren<Component>())
							{
								Log.Warn($"    {i?.name}:{i?.GetType()}");
							}
							Log.Warn($"HasComponentsInParent:");
							foreach (var i in identity.transform.gameObject.GetComponentsInParent<Component>())
							{
								Log.Warn($"    {i?.name}:{i?.GetType()}");
							}
						}
						return true;
					}
				case "identitypos":
					{
						response = "ok.";
						foreach (var identity in UnityEngine.Object.FindObjectsOfType<NetworkIdentity>())
						{
							Log.Warn($"{identity.transform.name}{identity.transform.position}");
						}
						return true;
					}
				case "avlcol":
					{
						response = "Available colors:\n";
						foreach (var i in ReferenceHub.HostHub.serverRoles.NamedColors.OrderBy(x => x.Restricted))
							response += $"[#{i.ColorHex}] {i.Name,-13} {(i.Restricted ? "Restricted" : "Not Restricted")}\n";
						return true;
					}
				case "lighttest":
					{
						if (targetLight == null)
						{
							var prefab = CustomNetworkManager.singleton.spawnPrefabs.First(x => x.name.Contains("LightSource"));
							var pobject = UnityEngine.Object.Instantiate(prefab.GetComponent<LightSourceToy>());
							targetLight = pobject;

							NetworkServer.Spawn(pobject.gameObject, ownerConnection: null);
						}

						targetLight.transform.position = new UnityEngine.Vector3(float.Parse(arguments.At(1)), float.Parse(arguments.At(2)), float.Parse(arguments.At(3)));
						targetLight.NetworkLightIntensity = float.Parse(arguments.At(4));
						targetLight.NetworkLightRange = float.Parse(arguments.At(5));
						targetLight.NetworkLightShadows = bool.Parse(arguments.At(6));
						response = $"lighttest.";
						return true;
					}
				case "walltest":
					{
						if (targetPrimitive == null)
						{
							var prefab = CustomNetworkManager.singleton.spawnPrefabs.First(x => x.name.Contains("Primitive"));
							var pobject = UnityEngine.Object.Instantiate(prefab.GetComponent<PrimitiveObjectToy>());

							pobject.NetworkScale = Vector3.one;
							pobject.NetworkMaterialColor = Color.black;
							targetPrimitive = pobject;

							NetworkServer.Spawn(pobject.gameObject, ownerConnection: null);
						}

						targetPrimitive.NetworkPrimitiveType = PrimitiveType.Cube;
						targetPrimitive.transform.position = new UnityEngine.Vector3(float.Parse(arguments.At(1)), float.Parse(arguments.At(2)), float.Parse(arguments.At(3)));
						targetPrimitive.transform.localScale = new UnityEngine.Vector3(float.Parse(arguments.At(4)), float.Parse(arguments.At(5)), float.Parse(arguments.At(6)));
						response = $"walltest.";
						return true;
					}
				case "targettest":
					{
						if (targetTarget == null)
						{
							var gameObject = UnityEngine.Object.Instantiate(CustomNetworkManager.singleton.spawnPrefabs.First(x => x.name.Contains("dboyTarget")),
								new UnityEngine.Vector3(float.Parse(arguments.At(1)), float.Parse(arguments.At(2)), float.Parse(arguments.At(3))),
								Quaternion.Euler(Vector3.up * float.Parse(arguments.At(4))));
							targetTarget = gameObject;
							NetworkServer.Spawn(gameObject);
						}
						else
						{
							NetworkServer.Destroy(targetTarget);
							targetTarget = null;
						}
						response = $"targettest.";
						return true;
					}
				case "itemtest":
					{
						if (targetitem == null)
						{
							var itemtype = (ItemType)Enum.Parse(typeof(ItemType), arguments.At(1));
							var itemBase = InventoryItemLoader.AvailableItems[itemtype];
							var pickup = UnityEngine.Object.Instantiate(itemBase.PickupDropModel,
								new UnityEngine.Vector3(float.Parse(arguments.At(2)), float.Parse(arguments.At(3)), float.Parse(arguments.At(4))),
								Quaternion.Euler(Vector3.up * float.Parse(arguments.At(5))));
							pickup.Info.ItemId = itemtype;
							pickup.Info.Weight = itemBase.Weight;
							pickup.Info.Locked = true;
							pickup.GetComponent<Rigidbody>().useGravity = false;
							pickup.transform.localScale = new UnityEngine.Vector3(float.Parse(arguments.At(6)), float.Parse(arguments.At(7)), float.Parse(arguments.At(8)));

							targetitem = pickup;
							ItemDistributor.SpawnPickup(pickup);
						}
						else
						{
							NetworkServer.Destroy(targetitem.gameObject);
							targetitem = null;
						}
						response = $"itemtest.";
						return true;
					}
				case "worktest":
					{
						if (targetstation == null)
						{
							var prefab = CustomNetworkManager.singleton.spawnPrefabs.First(x => x.name.Contains("Station"));
							var station = UnityEngine.Object.Instantiate(prefab,
								new UnityEngine.Vector3(float.Parse(arguments.At(1)), float.Parse(arguments.At(2)), float.Parse(arguments.At(3))),
								Quaternion.Euler(Vector3.up * float.Parse(arguments.At(4))));
							station.transform.localScale = new UnityEngine.Vector3(float.Parse(arguments.At(5)), float.Parse(arguments.At(6)), float.Parse(arguments.At(7)));
							targetstation = station;
							NetworkServer.Spawn(station);
						}
						else
						{
							NetworkServer.Destroy(targetstation);
							targetstation = null;
						}
						response = $"worktest.";
						return true;
					}
				case "doortest":
					{
						if (targetdoor == null)
						{
							var prefab = UnityEngine.Object.FindObjectsOfType<DoorSpawnpoint>().First(x => x.TargetPrefab.name.Contains("HCZ"));
							var door = UnityEngine.Object.Instantiate(prefab.TargetPrefab, new UnityEngine.Vector3(float.Parse(arguments.At(1)), float.Parse(arguments.At(2)), float.Parse(arguments.At(3))), Quaternion.Euler(Vector3.up * 180f));
							door.transform.localScale = new UnityEngine.Vector3(float.Parse(arguments.At(4)), float.Parse(arguments.At(5)), float.Parse(arguments.At(6)));
							targetdoor = door;
							NetworkServer.Spawn(door.gameObject);
						}
						else
						{
							NetworkServer.Destroy(targetdoor.gameObject);
							targetdoor = null;
						}
						response = $"doortest.";
						return true;
					}
				case "checkobj":
					{
						if (player != null && !player.CheckPermission("sanya.dev"))
						{
							response = "Permission denied.";
							return false;
						}
						response = "ok.";
						if (Physics.Raycast(player.Position + player.CameraTransform.forward, player.CameraTransform.forward, out var casy,25f))
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
						return true;
					}
				case "checkobjdel":
					{
						if (player != null && !player.CheckPermission("sanya.dev"))
						{
							response = "Permission denied.";
							return false;
						}
						response = "ok.";
						if (Physics.Raycast(player.Position + player.CameraTransform.forward, player.CameraTransform.forward, out var casy, 25f))
						{
							Log.Warn($"{casy.transform.name} (layer{casy.transform.gameObject.layer})");
							Log.Warn($"HasComponents:");
							foreach (var i in casy.transform.gameObject.GetComponents<Component>())
							{
								if (i.transform.gameObject.GetComponents<NetworkIdentity>() != null)
								{
									Log.Warn($"    {i.name}:{i.GetType()}");
									GameObject.Destroy(i.gameObject);
								}
							}
							Log.Warn($"HasComponentsInChildren:");
							foreach (var i in casy.transform.gameObject.GetComponentsInChildren<Component>())
							{
								if (i.transform.gameObject.GetComponents<NetworkIdentity>() != null)
								{
									Log.Warn($"    {i.name}:{i.GetType()}");
									GameObject.Destroy(i.gameObject);
								}
							}
                            Log.Warn($"HasComponentsInParent:");
                            foreach (var i in casy.transform.gameObject.GetComponentsInParent<Component>())
                            {
								if (i.transform.gameObject.GetComponents<NetworkIdentity>() != null)
								{
									Log.Warn($"    {i.name}:{i.GetType()}");
								}
							}
							GameObject.Destroy(casy.transform.gameObject);
						}
						return true;
					}
				case "box":
					{
						{
							if (player != null && !player.CheckPermission("sanya.hint"))
							{
								response = "Permission denied.";
								return false;
							}

							if (arguments.At(1).ToLower() == "all")
							{
								if (player != null && !player.CheckPermission("sanya.hintall"))
								{
									response = "Permission denied.";
									return false;
								}
								foreach (Player ply in Player.List.Where((p) => p.Role != RoleType.None))
								{
									ply.OpenReportWindow(Extensions.FormatArguments(arguments, 2));
								}
								response = $"La box avec : {Extensions.FormatArguments(arguments, 2)} a bien été envoyé a tout le monde ";
								return true;
							}
							
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
								response = $"Votre message a bien été envoyé à :\n";
								foreach (Player ply in PlyList)
								{
									ply.OpenReportWindow(Extensions.FormatArguments(arguments, 3));
									response += $" - {ply.Nickname}\n";
								}
								return true;
							}
							else
							{
								response = $"Sanya box <player/all> <message> // Sanya box <id.id.id> <message>";
								return false;
							}
						}
					}
				case "scale":
					{
						if (player != null && !player.CheckPermission("sanya.scale"))
						{
							response = "Permission denied.";
							return false;
						}
						if (arguments.At(1) == null)
                        {
							response = "[Scale] <player> <x> <y> <z>.";
							return false;
						}
						Player target = Player.Get(arguments.At(1));
						if (target != null)
						{
							if (arguments.Count > 4
								&& float.TryParse(arguments.At(2), out float x)
								&& float.TryParse(arguments.At(3), out float y)
								&& float.TryParse(arguments.At(4), out float z))
							{
								Vector3 pos = new Vector3(x, y, z);
								target.Scale = pos;
								response = $"{target.Nickname} as been scale to {pos} ok.";
								return true;
							}
							else
							{
								response = "[Scale] il manque la taille <x> <y> <z>.";
								return false;
							}
						}

						response = "[Scale] Il manque le joueur <player> <x> <y> <z>.";
						return false;
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

						if (arguments.At(1).ToLower() == "all")
						{
							if (player != null && !player.CheckPermission("sanya.hintall"))
							{
								response = "Permission denied.";
								return false;
							}
							else if (arguments.Count > 2 && ulong.TryParse(arguments.At(2), out ulong duration))
							{
								foreach (Player ply in Player.List)
								{
									if (ply.ReferenceHub.TryGetComponent<SanyaRemasteredComponent>(out var Component))
										Component.AddHudCenterDownText(Extensions.FormatArguments(arguments, 3), duration);
									else
										Log.Debug($"{ply.Nickname} don't have SanyaRemasteredComponent");
								}
								response = $"Le Hint {Extensions.FormatArguments(arguments, 2)} a bien été envoyé a tout le monde ";
								return true;
							}
							else
							{
								response = $"Sanya hint <player/all> <durée> <message> // Sanya hint <id.id.id> <durée> <message>";
								return false;
							}
						}
						else if (arguments.Count > 2 && ulong.TryParse(arguments.At(2), out ulong duration))
						{
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
									response = $"Votre message a bien été envoyé à :\n";
									foreach (Player ply in PlyList)
									{
										if (ply.ReferenceHub.TryGetComponent<SanyaRemasteredComponent>(out var Component))
                                        {
											Component.AddHudCenterDownText(Extensions.FormatArguments(arguments, 3), duration);
											response += $" - {ply.Nickname}\n";
										}
										else
											Log.Debug($"{ply.Nickname} don't have SanyaRemasteredComponent"); 
									}
									return true;
								}
								else
								{
									response = $"Sanya hint <player/all> <durée> <message> // Sanya hint <id.id.id> <durée> <message>";
									return false;
								}
							}
						}
						else
						{
							response = "Sanya hint <player> <durée> <message>";
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
                        try
						{
							var comp = player.GameObject.GetComponent<SanyaRemasteredComponent>();
							foreach (Player p in Player.List)
								p.GameObject.GetComponent<SanyaRemasteredComponent>().DisableHud = bool.Parse(arguments.At(2).ToLower());
							response = $"all hud is = {bool.Parse(arguments.At(1).ToLower())}";
						} 
						catch (Exception) 
						{
							response = "hud true/false";
							return false;
						}

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
						if (player != null && !player.CheckPermission("sanya.actwatch"))
						{
							response = "Permission denied.";
							return false;
						}
						if (player == null)
						{
							response = "Only can use with RemoteAdmin.";
							return false;
						}

						if (!isActwatchEnabled)
						{
							MirrorExtensions.SendFakeSyncObject(player, player.ReferenceHub.networkIdentity, typeof(PlayerEffectsController), (writer) =>
							{
								writer.WriteUInt64(1ul);
								writer.WriteUInt32((uint)1);
								writer.WriteByte((byte)SyncList<byte>.Operation.OP_SET);
								writer.WriteUInt32((uint)19);
								writer.WriteByte((byte)1);
							});
							isActwatchEnabled = true;
						}
						else
						{
							MirrorExtensions.SendFakeSyncObject(player, player.ReferenceHub.networkIdentity, typeof(PlayerEffectsController), (writer) =>
							{
								writer.WriteUInt64(1ul);
								writer.WriteUInt32(1);
								writer.WriteByte((byte)SyncList<byte>.Operation.OP_SET);
								writer.WriteUInt32(19);
								writer.WriteByte(0);
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
							Scp914Controller Scp914 = UnityEngine.Object.FindObjectOfType<Scp914Controller>();
							if (arguments.At(1).ToLower() == "use")
							{
								if(!Scp914._isUpgrading)
								{
									Scp914._remainingCooldown = Scp914._totalSequenceTime;
									Scp914._isUpgrading = true;
									Scp914._itemsAlreadyUpgraded = false;
									Scp914.RpcPlaySound(1);
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
								response = $"ok. [{Scp914._knobSetting}] -> ";
								Scp914._remainingCooldown = Scp914._knobChangeCooldown;
								Type typeFromHandle = typeof(Scp914KnobSetting);
								Scp914KnobSetting scp914KnobSetting = Scp914._knobSetting + 1;
								Scp914.Network_knobSetting = scp914KnobSetting;
								if (!Enum.IsDefined(typeFromHandle, scp914KnobSetting))
								{
									Scp914.Network_knobSetting = Scp914KnobSetting.Rough;
								}
								Scp914.RpcPlaySound(0);

								response += $"[{Scp914._knobSetting}]";
								return true;
							}
							else if (Enum.TryParse(arguments.At(1),out Scp914KnobSetting knob))
                            {
								response = $"ok. [{Scp914.Network_knobSetting}] -> ";
								Scp914.Network_knobSetting = knob;
								response += $"[{Scp914.Network_knobSetting}]";
								return true;
							}
							else
							{
								response = "invalid parameters. (use/knob) or (Rough/Coarse/OneToOne/Fine/VeryFine)";
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
						if (arguments.Count > 1 && arguments.At(1).ToLower() == "hcz")
						{
							if (float.TryParse(arguments.At(2), out float duration))
								foreach (FlickerableLightController flickerableLightController in FlickerableLightController.Instances)
								{
                                    if (RoomIdentifier.RoomsByCoordinatess.TryGetValue(RoomIdUtils.PositionToCoords(flickerableLightController.transform.position), out RoomIdentifier roomIdentifier2) && roomIdentifier2.Zone == MapGeneration.FacilityZone.HeavyContainment && flickerableLightController.TryGetComponent(out Scp079Interactable scp079Interactable) && scp079Interactable.type == Scp079Interactable.InteractableType.LightController)
                                    {
                                        flickerableLightController.ServerFlickerLights(duration);
                                    }
                                }

							response = "HCZ blackout!";
							return true;
						}
						if (arguments.Count > 1 && arguments.At(1).ToLower() == "all")
						{
							if (arguments.Count > 2 && float.TryParse(arguments.At(2), out float duration))
                            {
								foreach (FlickerableLightController flickerableLightController in FlickerableLightController.Instances)
								{
                                    if (RoomIdentifier.RoomsByCoordinatess.TryGetValue(RoomIdUtils.PositionToCoords(flickerableLightController.transform.position), out RoomIdentifier roomIdentifier2) && flickerableLightController.TryGetComponent(out Scp079Interactable scp079Interactable) && scp079Interactable.type == Scp079Interactable.InteractableType.LightController)
                                    {
                                        flickerableLightController.ServerFlickerLights(duration);
                                    }
                                }
								response = "ALL blackout!";
								return true;
							}
							response = "need an duration";
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
				case "infammo":
				case "infiniteammo":
					{
						if (player != null && !player.CheckPermission("sanya.ammo"))
						{
							response = "Permission denied.";
							return false;
						}
						if (arguments.At(1).ToLower() == "all")
                        {
							foreach (Player p in Player.List.Where(x=>!x.SessionVariables.ContainsKey("InfAmmo")))
									p.SessionVariables.Add("InfAmmo", null);
							response = "Tous les joueurs on l'infinite ammo";
							return true;
						}
						else if (arguments.At(1).ToLower() == "clear")
						{
							foreach (Player p in Player.List.Where(x => x.SessionVariables.ContainsKey("InfAmmo")))
								p.SessionVariables.Remove("InfAmmo");
							response = "Plus aucun joueurs n'as le infinite ammo";
							return true;
						}
						else if (arguments.At(1).ToLower() == "list")
						{
							response = "Liste des joueurs avec infinite ammo";
							foreach (Player p in Player.List.Where(x => x.SessionVariables.ContainsKey("InfAmmo")))
								response += "\n  - " + p.Nickname;
							return true;
						}
						if (arguments.Count > 0)
						{
							Player target = Player.Get(arguments.At(1));
							if (target != null)
                            {
								if (target.SessionVariables.ContainsKey("InfAmmo"))
									target.SessionVariables.Remove("InfAmmo");
								else
									target.SessionVariables.Add("InfAmmo", null);
								response = $"Inf Ammo: {target.SessionVariables.ContainsKey("InfAmmo")}.";
								return true;
							}
							response = $"Inf Ammo: Can't Find the player";
							return false;
						}
						else
						{
							if (player.SessionVariables.ContainsKey("InfAmmo"))
								player.SessionVariables.Remove("InfAmmo");
							else
								player.SessionVariables.Add("InfAmmo", null);
							response = $"Inf Ammo: {player.SessionVariables.ContainsKey("InfAmmo")}.";
							return true;
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
						if (player != null && !player.CheckPermission("sanya.playambiant"))
						{
							response = "Permission denied.";
							return false;
						}
						if (arguments.Count > 1 && int.TryParse(arguments.At(1), out int sound))
						{
							Methods.PlayAmbientSound(sound);
						}
						response = $"Ambien sound \n";
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
							response += $"[{i.Id}]{i.Nickname}({i.UserId})<{i.Role}/{i.Health}HP> {i?.CurrentRoom}\n";
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
							if (arguments.Count() > 2 && int.TryParse(arguments.At(2), out int duration))
							{
								if (arguments.Count() > 3 && float.TryParse(arguments.At(3), out float duration2))
								{
									SanyaRemastered.Instance.Handlers.RoundCoroutines.Add(Timing.RunCoroutine(Coroutines.AirSupportBomb(false, duration, duration2), Segment.FixedUpdate));
									response = $"The AirBombing start in {duration / 60}:{duration % 60:00} and stop in {(int)duration2 / 60}:{(int)duration2 % 60:00}";
									return true;
								}
								else
								{
									SanyaRemastered.Instance.Handlers.RoundCoroutines.Add(Timing.RunCoroutine(Coroutines.AirSupportBomb(false, duration), Segment.FixedUpdate));
									response = $"The AirBombing start in {duration / 60}:{duration % 60:00}!";
									return true;
								}
							}
							else
							{
								SanyaRemastered.Instance.Handlers.RoundCoroutines.Add(Timing.RunCoroutine(Coroutines.AirSupportBomb(false), Segment.FixedUpdate));
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
				case "speed":
					if (player != null && !player.CheckPermission("sanya.speed"))
					{
						response = "Permission denied.";
						return false;
					}
					{
						if (arguments.Count > 2)
						{
							Player target = Player.Get(arguments.At(1));
							if (target != null && target.Role != RoleType.Spectator)
							{
								if (arguments.At(2).ToLower() == "walk")
								{
									if (arguments.Count() > 3 && float.TryParse(arguments.At(3), out float speed))
									{
										target.ChangeWalkingSpeed(speed);
										response = $"Change the walk speed to {speed}";
										return true;
									}
								}
								else if (arguments.At(2).ToLower() == "sprint")
								{
									if (arguments.Count() > 3 && float.TryParse(arguments.At(3), out float speed))
									{
										target.ChangeRunningSpeed(speed);
										response = $"Change the sprint speed to {speed}";
										return true;
									}
								}
								else if (arguments.At(2).ToLower() == "all")
								{
									if (arguments.Count() > 3 && float.TryParse(arguments.At(3), out float speed))
									{
										target.ChangeWalkingSpeed(speed);
										target.ChangeRunningSpeed(speed);
										response = $"Change the speed to {speed}";
										return true;
									}
								}
								response = "[speed] missing args <all/Player> <walk/sprint/all> <speed>";
								return false;
							}
							else if (arguments.At(1).ToLower() == "all")
							{
								if (player != null && !player.CheckPermission("sanya.allspeed"))
								{
									response = "Permission denied.";
									return false;
								}
								if (arguments.At(2).ToLower() == "walk")
								{
									if (arguments.Count() > 3 && float.TryParse(arguments.At(3), out float speed))
									{
										foreach (var ply in Player.List.Where((p) => p.Team != Team.RIP))
										{
											target.ChangeWalkingSpeed(speed);
										}
										response = $"Change the walk speed to {speed}";
										return true;
									}
								}
								else if (arguments.At(2).ToLower() == "sprint")
								{
									if (arguments.Count() > 3 && float.TryParse(arguments.At(3), out float speed))
									{
										foreach (var ply in Player.List.Where((p) => p.Team != Team.RIP))
										{
											target.ChangeRunningSpeed(speed);
										}
										response = $"Change the sprint speed to {speed}";
										return true;
									}
								}
								else if (arguments.At(2).ToLower() == "all")
								{
									if (arguments.Count() > 3 && float.TryParse(arguments.At(3), out float speed))
									{
										foreach (var ply in Player.List.Where((p) => p.Team != Team.RIP))
										{
											target.ChangeWalkingSpeed(speed);
											target.ChangeRunningSpeed(speed);
										}
										response = $"Change the speed to {speed}";
										return true;
									}
								}
								response = "fail to change the speed <all/Player> <walk/sprint/all> <speed>";
								return false;
							}
							else
							{
								response = "[speed] missing target.";
								return false;
							}
						}
						else
						{
							if (player != null)
							{
								if (arguments.At(1).ToLower() == "walk")
								{
									if (arguments.Count() > 2 && float.TryParse(arguments.At(2), out float speed))
									{
										player.ChangeWalkingSpeed(speed);
										response = $"Change the walk speed to {speed}";
										return true;
									}
								}
								else if (arguments.At(2).ToLower() == "sprint")
								{
									if (arguments.Count() > 2 && float.TryParse(arguments.At(2), out float speed))
									{
										player.ChangeRunningSpeed(speed);
										response = $"Change the sprint speed to {speed}";
										return true;
									}
								}
								else if (arguments.At(2).ToLower() == "all")
								{
									if (arguments.Count() > 3 && float.TryParse(arguments.At(3), out float speed))
									{
										player.ChangeWalkingSpeed(speed);
										player.ChangeRunningSpeed(speed);
										response = $"Change the speed to {speed}";
										return true;
									}
								}
								response = $"please take <walk/sprint/all> <speed>";
								return false;
							}
							else
							{
								response = "[speed] missing target.";
								return false;
							}
						}
					}
				case "color":
					{
						if (player != null && !player.CheckPermission("sanya.color"))
						{
							response = "Permission denied.";
							return false;
						}
						if (arguments.Count == 1)
						{
							response = "Usage: lightcolor r g b or lightcolor reset";
							return false;
						}
						if (arguments.At(1).ToLower() == "all")
                        {
							if (arguments.Count > 2 && arguments.At(2).ToLower() == "reset")
							{
								foreach (var i in FlickerableLightController.Instances)
								{
									i.WarheadLightColor = FlickerableLightController.DefaultWarheadColor;
									i.WarheadLightOverride = false;
								}
								response = "reset ok.";
								return true;
							}
							if (arguments.Count > 2 && arguments.At(2).ToLower() == "rand")
							{
								System.Random rng = new System.Random();
								foreach (var i in FlickerableLightController.Instances)
								{
									i.WarheadLightColor = new Color((float)rng.NextDouble(), (float)rng.NextDouble(), (float)rng.NextDouble());
									i.WarheadLightOverride = true;
								}
								response = "random color ok.";
								return true;
							}
							if (arguments.Count > 4
								&& float.TryParse(arguments.At(2), out var r)
								&& float.TryParse(arguments.At(3), out var g)
								&& float.TryParse(arguments.At(4), out var b))
							{
								foreach (var i in FlickerableLightController.Instances)
								{
									i.WarheadLightColor = new Color(r / 255f, g / 255f, b / 255f);
									i.WarheadLightOverride = true;
								}
								response = $"color set:{r},{g},{b}";
								return true;
							}
							response = $"lightcolor: invalid params.";
							return false;
						}
						else if (arguments.At(1).ToLower() == "set")
						{
							foreach (Room room in Map.Rooms)
							{
								if (room.Type.ToString().Contains(arguments.At(2)))
								{
									if (arguments.Count > 5
									&& float.TryParse(arguments.At(3), out var r)
									&& float.TryParse(arguments.At(4), out var g)
									&& float.TryParse(arguments.At(5), out var b))
									{
										room.Color = new Color(r / 255f, g / 255f, b / 255f);
									}
								}
							}
						}
						else if (arguments.At(1) == "reset")
						{
							foreach (Room room in Map.Rooms)
							{
								if (room.Type.ToString().ToLower().Contains(arguments.At(2).ToLower()))
								{
									room.ResetColor();
								}
							}
						}


						response = $"lightcolor: invalid params.";
						return false;
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
								Methods.SpawnGrenade(target.Position, ItemType.GrenadeHE, 0.1f, player);
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
								foreach (var ply in Player.List.Where((p) => p.Team != Team.RIP))
								{
									Methods.SpawnGrenade(ply.Position, ItemType.GrenadeHE, 0.1f, player);
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
								Methods.SpawnGrenade(player.Position, ItemType.GrenadeHE, 0.1f, player);
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
							if (target != null && target.Team != Team.RIP)
							{
								Methods.SpawnGrenade(target.Position,ItemType.SCP018,-1, player);
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
								foreach (var ply in Player.List.Where((p) => p.Team != Team.RIP))
								{
									Methods.SpawnGrenade(target.Position, ItemType.SCP018, -1, player);
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
								Methods.SpawnGrenade(player.Position, ItemType.SCP018, -1, player);
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
							if (target != null && target.Team != Team.RIP)
							{
								Methods.SpawnGrenade(target.Position, ItemType.GrenadeHE, -1f, player);
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
								foreach (var ply in Player.List.Where((p) => p.Team != Team.RIP))
								{
									Methods.SpawnGrenade(ply.Position, ItemType.GrenadeHE, -1f, player);
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
								Methods.SpawnGrenade(player.Position, ItemType.GrenadeHE, 0f, player);
								response = $"success. target:{Player.Get(player.ReferenceHub.gameObject).Nickname}";
								return true;
							}
							else
							{
								response = "[grenade] missing target.";
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
						if (target != null && target.Team != Team.RIP)
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
							foreach (var ply in Player.List.Where((p) => p.Team != Team.RIP))
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
				case "tppos":
					{
						if (player != null && !player.CheckPermission("sanya.tppos"))
						{
							response = "Permission denied.";
							return false;
						}
						Player target = Player.Get(arguments.At(1));
						if (target != null)
						{
							if (arguments.Count > 4
								&& float.TryParse(arguments.At(2), out float x)
								&& float.TryParse(arguments.At(3), out float y)
								&& float.TryParse(arguments.At(4), out float z))
							{
								Vector3 pos = new Vector3(x, y, z);
								target.Position = pos;
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
							if (arguments.Count > 4
								&& float.TryParse(arguments.At(2), out float x)
								&& float.TryParse(arguments.At(3), out float y)
								&& float.TryParse(arguments.At(4), out float z))
							{
								Vector3 pos = new Vector3(x, y, z);
								foreach (var ply in Player.List.Where((p) => p.Team != Team.RIP))
									ply.Position = pos;
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
								foreach (var generator in Recontainer079.AllGenerators)
								{
									generator.ServerSetFlag(Scp079Generator.GeneratorFlags.Unlocked, true);
								}
								response = "gen unlocked.";
								return true;
							}
							else if (arguments.At(1).ToLower() == "door")
							{
								foreach (var generator in Recontainer079.AllGenerators)
								{
									generator.ServerSetFlag(Scp079Generator.GeneratorFlags.Open, !generator.HasFlag(generator._flags, Scp079Generator.GeneratorFlags.Open));
									generator._targetCooldown = generator._doorToggleCooldownTime;
								}
								response = $"gen doors interacted.";
								return true;
							}
							else if (arguments.At(1).ToLower() == "set")
							{
								foreach (var generator in Recontainer079.AllGenerators.Where(x => !x.Engaged))
								{
									if (generator != null)
									{
										generator.Engaged = true;
										generator._currentTime = 1000;
										generator.Network_flags = (byte)Scp079Generator.GeneratorFlags.Engaged;
										response = "set once.";
									}
								}
								response = "gen set.";
								return true;
							}
							else if (arguments.At(1).ToLower() == "once")
							{
								var gen = Recontainer079.AllGenerators.FirstOrDefault(x => !x.Engaged);

								if (gen != null)
								{
									gen.Engaged = true;
									gen._currentTime = 1000;
									gen.Network_flags = (byte)Scp079Generator.GeneratorFlags.Engaged;
									response = "set once.";
									return true;
								}
								response = "All generator ";
								return false;
							}
							else if (arguments.At(1).ToLower() == "eject")
							{
								foreach (var generator in Recontainer079.AllGenerators)
								{
									if (generator.Activating)
									{
										generator.Activating = false;
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
