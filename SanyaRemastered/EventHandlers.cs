using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using CustomPlayerEffects;
using Grenades;
using LightContainmentZoneDecontamination;
using LiteNetLib.Utils;
using MEC;
using Mirror;
using SanyaRemastered.Data;
using SanyaRemastered.Patches;
using UnityEngine;
using Utf8Json;
using SanyaPlugin.Data;
using SanyaPlugin.Functions;
using Respawning;
using Exiled.Events.EventArgs;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using System.Linq;
using Exiled.API.Extensions;
using Assets._Scripts.Dissonance;
using Exiled.API.Enums;

namespace SanyaPlugin
{
	public class EventHandlers
	{
		public EventHandlers(SanyaPlugin plugin) => this.plugin = plugin;
		internal readonly SanyaPlugin plugin;
		internal List<CoroutineHandle> roundCoroutines = new List<CoroutineHandle>();
		internal bool loaded = false;

		/** Infosender **/
		private readonly UdpClient udpClient = new UdpClient();
		internal Task sendertask;
		private bool senderdisabled = false;
		internal async Task SenderAsync()
		{
			Log.Debug($"[Infosender_Task] Started.");

			while (true)
			{
				try
				{
					if (SanyaPlugin.instance.Config.InfosenderIp == "none")
					{
						Log.Info($"[Infosender_Task] Disabled(config:({SanyaPlugin.instance.Config.InfosenderIp}). breaked.");
						senderdisabled = true;
						break;
					}

					if (!this.loaded)
					{
						Log.Debug($"[Infosender_Task] Plugin not loaded. Skipped...");
						await Task.Delay(TimeSpan.FromSeconds(30));
					}

					Serverinfo cinfo = new Serverinfo();

					DateTime dt = DateTime.Now;
					cinfo.Time = dt.ToString("yyyy-MM-ddTHH:mm:sszzzz");
					cinfo.Gameversion = CustomNetworkManager.CompatibleVersions[0];
					cinfo.Modversion = "Coucou sa n'existe plus";
					cinfo.Sanyaversion = "SanyaRemastered";
					cinfo.Gamemode = eventmode.ToString();
					cinfo.Name = ServerConsole.singleton.RefreshServerName();
					cinfo.Ip = ServerConsole.Ip;
					cinfo.Port = ServerConsole.Port;
					cinfo.Playing = PlayerManager.players.Count;
					cinfo.Maxplayer = CustomNetworkManager.slots;
					cinfo.Duration = RoundSummary.roundTime;

					if (cinfo.Playing > 0)
					{
						foreach (GameObject player in PlayerManager.players)
						{
							Playerinfo ply = new Playerinfo
							{
								Name = ReferenceHub.GetHub(player).nicknameSync.MyNick,
								Userid = ReferenceHub.GetHub(player).characterClassManager.UserId,
								Ip = ReferenceHub.GetHub(player).queryProcessor._ipAddress,
								Role = ReferenceHub.GetHub(player).characterClassManager.CurClass.ToString(),
								Rank = ReferenceHub.GetHub(player).serverRoles.MyText
							};

							cinfo.Players.Add(ply);
						}
					}

					string json = JsonSerializer.ToJsonString(cinfo);

					byte[] sendBytes = Encoding.UTF8.GetBytes(json);
					udpClient.Send(sendBytes, sendBytes.Length, SanyaPlugin.instance.Config.InfosenderIp, SanyaPlugin.instance.Config.InfosenderPort);
					Log.Debug($"[Infosender_Task] {SanyaPlugin.instance.Config.InfosenderIp}:{SanyaPlugin.instance.Config.InfosenderPort}");
				}
				catch (Exception e)
				{
					throw e;
				}
				await Task.Delay(TimeSpan.FromSeconds(30));
			}
		}

		/** AuthChecker **/
		internal const byte BypassFlags = (1 << 1) | (1 << 3);
		internal static readonly NetDataReader reader = new NetDataReader();
		internal static readonly NetDataWriter writer = new NetDataWriter();
		internal static readonly Dictionary<string, string> kickedbyChecker = new Dictionary<string, string>();

		/** Update **/
		internal IEnumerator<float> EverySecond()
		{
			while (true)
			{
				try
				{
					//Traitre
					if (SanyaPlugin.instance.Config.TraitorLimit > 0)
					{
						foreach (var player in Player.List)
						{
							if ((player.Team == Team.MTF || player.Team == Team.CHI)
								&& player.IsCuffed
								&& Vector3.Distance(espaceArea, player.Position) <= Escape.radius
								&& RoundSummary.singleton.CountTeam(player.Team) <= SanyaPlugin.instance.Config.TraitorLimit)
							{
								switch (player.Team)
								{
									case Team.MTF:
										if (UnityEngine.Random.Range(0, 100) <= SanyaPlugin.instance.Config.TraitorChancePercent)
										{
											Log.Info($"[_EverySecond:Traitor] {player.Nickname} : MTF->CHI");
											player.ReferenceHub.characterClassManager.SetPlayersClass(RoleType.ChaosInsurgency, player.ReferenceHub.gameObject);
										}
										else
										{
											Log.Info($"[_EverySecond:Traitor] {player.Nickname} : Traitor Failed(by percent)");
											player.ReferenceHub.characterClassManager.SetPlayersClass(RoleType.Spectator, player.GameObject);
										}
										break;
									case Team.CHI:
										if (UnityEngine.Random.Range(0, 100) <= SanyaPlugin.instance.Config.TraitorChancePercent)
										{
											Log.Info($"[_EverySecond:Traitor] {player.Nickname} : CHI->MTF");
											player.ReferenceHub.characterClassManager.SetPlayersClass(RoleType.NtfCadet, player.GameObject);
										}
										else
										{
											Log.Info($"[_EverySecond:Traitor] {player.Nickname} : Traitor Failed(by percent)");
											player.ReferenceHub.characterClassManager.SetPlayersClass(RoleType.Spectator, player.GameObject);
										}
										break;
								}
							}
						}
					}

					//ItemCleanup
					if (SanyaPlugin.instance.Config.ItemCleanup> 0)
					{
						List<GameObject> nowitems = null;

						foreach (var i in ItemCleanupPatch.items)
						{
							if (Time.time - i.Value > SanyaPlugin.instance.Config.ItemCleanup&& i.Key != null)
							{
								if (nowitems == null) nowitems = new List<GameObject>();
								Log.Debug($"[ItemCleanupPatch] Cleanup:{i.Key.transform.position} {Time.time - i.Value} > {SanyaPlugin.instance.Config.ItemCleanup}");
								nowitems.Add(i.Key);
							}
						}

						if (nowitems != null)
						{
							foreach (var x in nowitems)
							{
								ItemCleanupPatch.items.Remove(x);
								NetworkServer.Destroy(x);
							}
						}
					}

					//Rétablissement du courant lors d'une panne de courant
					if (eventmode == SANYA_GAME_MODE.NIGHT && IsEnableBlackout && Generator079.mainGenerator.forcedOvercharge)
					{
						IsEnableBlackout = false;
					}

					//SCP-079's Spot Humans
					if (SanyaPlugin.instance.Config.scp079_spot)
					{
						foreach (var scp079 in Scp079PlayerScript.instances)
						{
							if (scp079.iAm079)
							{
								foreach (var player in Player.List)
								{
									if (player.ReferenceHub.characterClassManager.IsHuman() && scp079.currentCamera.CanLookToPlayer(player.ReferenceHub))
									{
										player.ReferenceHub.playerStats.TargetBloodEffect(player.ReferenceHub.playerStats.connectionToClient, Vector3.zero, 0.1f);
										foreach (var scp in Player.List.Where(n => n.Team == Team.SCP))
										{

											// NEXT
										}
									}
								}
							}
						}
					}
				}
				catch (Exception e)
				{
					Log.Error($"[EverySecond] {e}");
				}
				//Chaque seconde
				yield return Timing.WaitForSeconds(1f);
			}
		}
		internal IEnumerator<float> FixedUpdate()
		{
			while (true)
			{
				try
				{
					//Blackouter
					if (flickerableLight != null && IsEnableBlackout && flickerableLight.remainingFlicker < 0f && !flickerableLight.IsDisabled())
					{
						Log.Debug($"{UnityEngine.Object.FindObjectOfType<FlickerableLight>().remainingFlicker}");
						Log.Debug($"[Blackouter] Fired.");
						Generator079.mainGenerator.RpcCustomOverchargeForOurBeautifulModCreators(10f, false);
					}
					//SCP-939VoiceChatVision
					if (plugin.Config.Scp939CanSeeVoiceChatting)
					{
						List<ReferenceHub> scp939 = null;
						List<ReferenceHub> humans = new List<ReferenceHub>();
						foreach (var player in ReferenceHub.GetAllHubs().Values)
						{
							if (player.characterClassManager.CurRole.team != Team.RIP && player.TryGetComponent(out Radio radio) && (radio.isVoiceChatting || radio.isTransmitting))
							{
								player.footstepSync._visionController.MakeNoise(25f);
							}

							if (player.characterClassManager.CurRole.roleId.Is939())
							{
								if (scp939 == null)
									scp939 = new List<ReferenceHub>();
								scp939.Add(player);
							}

							if (player.characterClassManager.IsHuman())
								humans.Add(player);
						}
					}
				}
				catch (Exception e)
				{
					Log.Error($"[FixedUpdate] {e}");
				}
				//FixedUpdateの次フレームへ
				yield return Timing.WaitForOneFrame;
			}
		}

		/** Flag Params **/
		private int detonatedDuration = -1;
		private Vector3 espaceArea = new Vector3(177.5f, 985.0f, 29.0f);
		private readonly int grenade_pickup_mask = 1049088;
		private int prevMaxAHP = 0;
		/** RoundVar **/
		private FlickerableLight flickerableLight = null;
		private bool IsEnableBlackout = false;

		/** EventModeVar **/
		internal static SANYA_GAME_MODE eventmode = SANYA_GAME_MODE.NULL;
		private Vector3 LCZArmoryPos;

		public void OnWaintingForPlayers()
		{
			loaded = true;

			if (sendertask?.Status != TaskStatus.Running && sendertask?.Status != TaskStatus.WaitingForActivation && !senderdisabled)
				sendertask = SenderAsync().StartSender();

			roundCoroutines.Add(Timing.RunCoroutine(EverySecond(), Segment.FixedUpdate));
			roundCoroutines.Add(Timing.RunCoroutine(FixedUpdate(), Segment.FixedUpdate));

			PlayerDataManager.playersData.Clear();
			ItemCleanupPatch.items.Clear();
			Coroutines.isAirBombGoing = false;

			detonatedDuration = -1;
			IsEnableBlackout = false;

			flickerableLight = UnityEngine.Object.FindObjectOfType<FlickerableLight>();

			if (SanyaPlugin.instance.Config.classd_container_locked)
			{
				foreach (var door in UnityEngine.Object.FindObjectsOfType<Door>())
				{
					if (door.name.Contains("PrisonDoor"))
					{
						door.lockdown = true;
						Timing.WaitForSeconds(SanyaPlugin.instance.Config.classd_container_Unlocked);
						door.UpdateLock();
					}
				}
			}

			if (SanyaPlugin.instance.Config.TeslaRange != 5.5f)
			{
				foreach (var tesla in UnityEngine.Object.FindObjectsOfType<TeslaGate>())
				{
					tesla.sizeOfTrigger = SanyaPlugin.instance.Config.TeslaRange;
				}
			}


			eventmode = (SANYA_GAME_MODE)Methods.GetRandomIndexFromWeight(SanyaPlugin.instance.Config.EventModeWeight.ToArray());
			switch (eventmode)
			{
				case SANYA_GAME_MODE.NIGHT:
					{
						break;
					}
				case SANYA_GAME_MODE.CLASSD_INSURGENCY:
					{
						foreach (var room in Map.Rooms)
						{
							if (room.Name == "LCZ_Armory")
							{
								LCZArmoryPos = room.Position + new Vector3(0, 2, 0);
							}
						}
						break;
					}
				default:
					{
						eventmode = SANYA_GAME_MODE.NORMAL;
						break;
					}
			}

			if (ReferenceHub.Hubs.ContainsKey(PlayerManager.localPlayer)) ReferenceHub.Hubs.Remove(PlayerManager.localPlayer);

			Log.Info($"[OnWaintingForPlayers] Waiting for Players... EventMode:{eventmode}");
		}

		public void OnRoundStart()
		{
			Log.Info($"[OnRoundStart] Round Start!");

			switch (eventmode)
			{
				case SANYA_GAME_MODE.NIGHT:
					{
						IsEnableBlackout = true;
						roundCoroutines.Add(Timing.RunCoroutine(Coroutines.StartNightMode()));
						break;
					}
			}
		}

		public void OnRoundEnd(RoundEndedEventArgs ev)
		{
			Log.Info($"[OnRoundEnd] Round Ended.");

			if (SanyaPlugin.instance.Config.DataEnabled)
			{
				foreach (Player Eplayer in Player.List)
				{
					ReferenceHub player = Eplayer.ReferenceHub;
					if (string.IsNullOrEmpty(player.characterClassManager.UserId)) continue;

					if (PlayerDataManager.playersData.ContainsKey(player.characterClassManager.UserId))
					{
						if (player.characterClassManager.CurClass == RoleType.Spectator)
						{
							PlayerDataManager.playersData[player.characterClassManager.UserId].AddExp(SanyaPlugin.instance.Config.level_exp_other);
						}
						else
						{
							PlayerDataManager.playersData[player.characterClassManager.UserId].AddExp(SanyaPlugin.instance.Config.LevelExpWin);
						}
					}
				}

				foreach (var data in PlayerDataManager.playersData.Values)
				{
					data.lastUpdate = DateTime.Now;
					data.playingcount++;
					PlayerDataManager.SavePlayerData(data);
				}
			}

			if (SanyaPlugin.instance.Config.GodmodeAfterEndround)
			{
				foreach (Player player in Player.List)
				{
					ReferenceHub referenceHub = player.ReferenceHub;
					referenceHub.characterClassManager.GodMode = true;
				}
			}

			Coroutines.isAirBombGoing = false;
		}

		public void OnRoundRestart()
		{
			Log.Info($"[OnRoundRestart] Restarting...");

			foreach (var cor in roundCoroutines)
				Timing.KillCoroutines(cor);
			roundCoroutines.Clear();
		}

		public void OnWarheadStart(StartingEventArgs ev)
		{
			Log.Debug($"[OnWarheadStart] {ev.Player?.Nickname}");

			if (SanyaPlugin.instance.Config.CassieSubtitle)
			{
				bool isresumed = AlphaWarheadController._resumeScenario != -1;
				double left = isresumed ? AlphaWarheadController.Host.timeToDetonation : AlphaWarheadController.Host.timeToDetonation - 4;
				double count = Math.Truncate(left / 10.0) * 10.0;

				if (!isresumed)
				{
					Methods.SendSubtitle(Subtitles.AlphaWarheadStart.Replace("{0}", count.ToString()), 15);
				}
				else
				{
					Methods.SendSubtitle(Subtitles.AlphaWarheadResume.Replace("{0}", count.ToString()), 10);
				}
			}
		}

		public void OnWarheadCancel(StoppingEventArgs ev)
		{
			Log.Debug($"[OnWarheadCancel] {ev.Player?.Nickname}");

			if (AlphaWarheadController.Host._isLocked) return;

			if (SanyaPlugin.instance.Config.CassieSubtitle)
			{
				Methods.SendSubtitle(Subtitles.AlphaWarheadCancel, 7);
			}

			if (SanyaPlugin.instance.Config.CloseDoorsOnNukecancel)
			{
				foreach (var door in UnityEngine.Object.FindObjectsOfType<Door>())
				{
					if (door.warheadlock)
					{
						if (door.isOpen)
						{
							door.RpcDoSound();
						}
						door.moving.moving = true;
						door.SetState(false);
					}
				}
			}
		}

		public void OnDetonated()
		{
			Log.Debug($"[OnDetonated] Detonated:{RoundSummary.roundTime / 60:00}:{RoundSummary.roundTime % 60:00}");

			if (SanyaPlugin.instance.Config.OutsidezoneTerminationTimeAfterNuke != 0)
			{ 
			roundCoroutines.Add(Timing.RunCoroutine(Coroutines.AirSupportBomb(false,SanyaPlugin.instance.Config.OutsidezoneTerminationTimeAfterNuke)));
			}
		}
		public void OnAnnounceDecont(AnnouncingDecontaminationEventArgs ev)
		{
			Log.Debug($"[OnAnnounceDecont] {ev.Id} {DecontaminationController.Singleton._stopUpdating}");
			
			if (SanyaPlugin.instance.Config.CassieSubtitle)
			{
				
				ev.IsGlobal = true;

				switch (ev.Id)
				{
					case 0:
						{
							foreach(Player player in Player.List)
							{
								if (player.CurrentRoom.Name.StartsWith("LCZ"))
								Methods.SendSubtitle(player,Subtitles.DecontaminationInit, 20);
							}
							break;
						}
					case 1:
						{
							foreach (Player player in Player.List)
							{
								if (player.CurrentRoom.Name.StartsWith("LCZ"))
									Methods.SendSubtitle(Subtitles.DecontaminationMinutesCount.Replace("{0}", "10"), 15);
							}
							break;
						}
					case 2:
						{
							foreach (Player player in Player.List)
							{
								if (player.CurrentRoom.Name.StartsWith("LCZ"))
									Methods.SendSubtitle(Subtitles.DecontaminationMinutesCount.Replace("{0}", "5"), 15);
							}
							break;
						}
					case 3:
						{
							foreach (Player player in Player.List)
							{
								if (player.CurrentRoom.Name.StartsWith("LCZ"))
									Methods.SendSubtitle(Subtitles.DecontaminationMinutesCount.Replace("{0}", "1"), 15);
							}
							break;
						}
					case 4:
						{
							foreach (Player player in Player.List)
							{
								if (player.CurrentRoom.Name.StartsWith("LCZ"))
									Methods.SendSubtitle(Subtitles.Decontamination30s, 45);
							}
							break;
						}
					case 5:
						{
							//no announce
							break;
						}
					case 6:
						{
							Methods.SendSubtitle(Subtitles.DecontaminationLockdown, 15);
							break;
						}
				}
			}
		}

		public void OnPreAuth(PreAuthenticatingEventArgs ev)
		{
			Log.Debug($"[OnPreAuth] {ev.Request.RemoteEndPoint.Address}:{ev.UserId}");

			if ((SanyaPlugin.instance.Config.KickSteamLimited|| SanyaPlugin.instance.Config.KickVpn) && !ev.UserId.Contains("@northwood", StringComparison.InvariantCultureIgnoreCase))
			{
				reader.SetSource(ev.Request.Data.RawData, 20);
				if (reader.TryGetBytesWithLength(out var sb) && reader.TryGetString(out var s) &&
					reader.TryGetULong(out var e) && reader.TryGetByte(out var flags))
				{
					if ((flags & BypassFlags) > 0)
					{
						Log.Warn($"[OnPreAuth] User have bypassflags. {ev.UserId}");
						return;
					}
				}
			}

			if (SanyaPlugin.instance.Config.DataEnabled && !PlayerDataManager.playersData.ContainsKey(ev.UserId))
			{
				PlayerDataManager.playersData.Add(ev.UserId, PlayerDataManager.LoadPlayerData(ev.UserId));
			}

			if (SanyaPlugin.instance.Config.KickVpn)
			{
				if (ShitChecker.IsBlacklisted(ev.Request.RemoteEndPoint.Address))
				{
					//ev.IsAllowed = false;
					writer.Reset();
					writer.Put((byte)10);
					writer.Put(Subtitles.VPNKickMessageShort);
					ev.Request.Reject(writer);
					return;
				}

				roundCoroutines.Add(Timing.RunCoroutine(ShitChecker.CheckVPN(ev)));
			}

			if (SanyaPlugin.instance.Config.KickSteamLimited&& ev.UserId.Contains("@steam", StringComparison.InvariantCultureIgnoreCase))
			{
				roundCoroutines.Add(Timing.RunCoroutine(ShitChecker.CheckIsLimitedSteam(ev.UserId)));
			}
		}

		public void OnPlayerJoin(JoinedEventArgs ev)
		{
			if (ev.Player.ReferenceHub.characterClassManager.IsHost) return;
			Log.Info($"[OnPlayerJoin] {ev.Player.Nickname} ({ev.Player.ReferenceHub.queryProcessor._ipAddress}:{ev.Player.UserId})");

			if (kickedbyChecker.TryGetValue(ev.Player.UserId, out var reason))
			{
				string reasonMessage = string.Empty;
				if (reason == "steam")
					reasonMessage = Subtitles.LimitedKickMessage;
				else if (reason == "vpn")
					reasonMessage = Subtitles.VPNKickMessage;

				ServerConsole.Disconnect(ev.Player.ReferenceHub.characterClassManager.connectionToClient, reasonMessage);
				kickedbyChecker.Remove(ev.Player.UserId);
				return;
			}

			if (!string.IsNullOrEmpty(SanyaPlugin.instance.Config.MotdMessage))
			{
				Methods.SendSubtitle(SanyaPlugin.instance.Config.MotdMessage.Replace("[name]", ev.Player.Nickname), 10, ev.Player.ReferenceHub);
			}

			if (SanyaPlugin.instance.Config.DataEnabled
				&& SanyaPlugin.instance.Config.LevelEnabled
				&& PlayerDataManager.playersData.TryGetValue(ev.Player.UserId, out PlayerData data))
			{
				Timing.RunCoroutine(Coroutines.GrantedLevel(ev.Player.ReferenceHub, data), Segment.FixedUpdate);
			}

			if (SanyaPlugin.instance.Config.DisableAllChat)
			{
				if (!(SanyaPlugin.instance.Config.DisableChatBypassWhitelist&& WhiteList.IsOnWhitelist(ev.Player.UserId)))
				{
					ev.Player.ReferenceHub.characterClassManager.NetworkMuted = true;
				}
			}

			//MuteFixer
			foreach (Player ExiledPlayer in Player.List)
			{
				ReferenceHub player = ExiledPlayer.ReferenceHub;
				if (ExiledPlayer.IsMuted)
					player.characterClassManager.SetDirtyBit(1uL);
			}
			//SpeedFixer
			ServerConfigSynchronizer.Singleton.SetDirtyBit(2uL);
			ServerConfigSynchronizer.Singleton.SetDirtyBit(4uL);
		}
		
		public void OnPlayerLeave(LeftEventArgs ev)
		{
			if (ev.Player.IsHost) return;
			Log.Debug($"[OnPlayerLeave] {ev.Player.Nickname} ({ev.Player.ReferenceHub.queryProcessor._ipAddress}:{ev.Player.UserId})");

			if (SanyaPlugin.instance.Config.DataEnabled&& !string.IsNullOrEmpty(ev.Player.UserId))
			{
				if (PlayerDataManager.playersData.ContainsKey(ev.Player.UserId))
				{
					PlayerDataManager.playersData.Remove(ev.Player.UserId);
				}
			}
		}
		#region A regler
		/*public void OnStartItems(StartItemsEvent ev)
		{
			if (ev.Player.IsHost()) return;
			Log.Debug($"[OnStartItems] {ev.Player.GetNickname()} -> {ev.Role}");

			if (Configs.defaultitems.TryGetValue(ev.Role, out List<ItemType> itemconfig) && itemconfig.Count > 0)
			{
				ev.StartItems = itemconfig;
			}

			if (itemconfig != null && itemconfig.Contains(ItemType.None))
			{
				ev.StartItems.Clear();
			}

			switch (eventmode)
			{
				case SANYA_GAME_MODE.CLASSD_INSURGENCY:
					{
						if (ev.Role == RoleType.ClassD && Configs.classd_insurgency_classd_inventory.Count > 0)
						{
							ev.StartItems = Configs.classd_insurgency_classd_inventory;
						}
						if (ev.Role == RoleType.Scientist && Configs.classd_insurgency_scientist_inventory.Count > 0)
						{
							ev.StartItems = Configs.classd_insurgency_scientist_inventory;
						}
						break;
					}
			}
		}
		*/

#endregion A regler
		public void OnPlayerSetClass(ChangingRoleEventArgs ev)
		{
			if (ev.Player.IsHost) return;
			Log.Debug($"[OnPlayerSetClass] {ev.Player.Nickname} -> {ev.NewRole}");

			if (SanyaPlugin.instance.Config.Scp079ExtendEnabled && ev.NewRole == RoleType.Scp079)
			{
				roundCoroutines.Add(Timing.CallDelayed(10f, () => ev.Player.ReferenceHub.SendTextHint(Subtitles.Extend079First, 10)));
			}

			if (SanyaPlugin.instance.Config.Scp049RecoveryAmount > 0 && ev.NewRole == RoleType.Scp0492)
			{
				foreach (Player Exiledscp049 in Player.List)
				{
					if(Exiledscp049.Role == RoleType.Scp049)
						Exiledscp049.ReferenceHub.playerStats.HealHPAmount(SanyaPlugin.instance.Config.Scp049RecoveryAmount);
				}
			}
			if (SanyaPlugin.instance.Config.scp049_add_time_res_success && ev.NewRole == RoleType.Scp0492)
			{
				foreach (Player Exiledspec in Player.List)
				{
					if (Exiledspec.Role == RoleType.Spectator)
						Methods.AddDeathTimeForScp049(Exiledspec.ReferenceHub);
				}
			}
			//Scp939Extend
			if (ev.NewRole.Is939())
			{
				if (prevMaxAHP == 0) prevMaxAHP = ev.Player.ReferenceHub.playerStats.maxArtificialHealth;
				ev.Player.ReferenceHub.playerStats.NetworkmaxArtificialHealth = 0;
				ev.Player.ReferenceHub.playerStats.NetworkartificialHpDecay = 0f;
				ev.Player.ReferenceHub.playerStats.NetworkartificialNormalRatio = 1f;
			}
			else if (ev.Player.ReferenceHub.characterClassManager._prevId.Is939())
			{
				ev.Player.ReferenceHub.playerStats.NetworkmaxArtificialHealth = this.prevMaxAHP;
				ev.Player.ReferenceHub.playerStats.NetworkartificialHpDecay = 0.75f;
				ev.Player.ReferenceHub.playerStats.NetworkartificialNormalRatio = 0.7f;
			}
		}
	

		public void OnPlayerSpawn(SpawningEventArgs ev)
		{
			if (ev.Player.IsHost) return;
			Log.Debug($"[OnPlayerSpawn] {ev.Player.Nickname} -{ev.RoleType}-> {ev.Position}");

			switch (eventmode)
			{
				case SANYA_GAME_MODE.CLASSD_INSURGENCY:
					{
						if (ev.RoleType == RoleType.ClassD)
						{
							ev.Position = LCZArmoryPos;
						}
						break;
					}
			}
		}
		public void OnPlayerHurt(HurtingEventArgs ev)
		{
			if (ev.Target.IsHost || ev.Target.Role == RoleType.Spectator || ev.Target.ReferenceHub.characterClassManager.GodMode || ev.Target.ReferenceHub.characterClassManager.SpawnProtected) return;
			Log.Debug($"[OnPlayerHurt:Before] {ev.Attacker?.Nickname}[{ev.Attacker?.Role}] -{ev.HitInformations.GetDamageName()}({ev.HitInformations.Amount})-> {ev.Target?.Nickname}[{ev.Target?.Role}]");

			if (ev.Attacker == null) return;
				
			if (ev.DamageType != DamageTypes.Nuke
				&& ev.DamageType != DamageTypes.Decont
				&& ev.DamageType != DamageTypes.Wall
				&& ev.DamageType != DamageTypes.Tesla
				&& ev.DamageType != DamageTypes.Scp207)
			{
				//GrenadeHitmark
				if (SanyaPlugin.instance.Config.HitmarkGrenade
					&& ev.DamageType == DamageTypes.Grenade
					&& ev.Target.UserId != ev.Attacker.UserId)
				{
					ev.Attacker.ReferenceHub.ShowHitmarker();
				}

				//USPMultiplier
				if (ev.DamageType == DamageTypes.Usp)
				{
					if (ev.Target.ReferenceHub.characterClassManager.IsAnyScp())
					{
						ev.Amount *= SanyaPlugin.instance.Config.UspDamageMultiplierScp;
					}
					else
					{
						ev.Amount *= SanyaPlugin.instance.Config.UspDamageMultiplierHuman;
						ev.Target.ReferenceHub.playerEffectsController.EnableEffect<Disabled>(10f);
					}
				}

				//939Bleeding
				if (SanyaPlugin.instance.Config.Scp939AttackBleeding && ev.DamageType == DamageTypes.Scp939)
				{
					ev.Target.ReferenceHub.playerEffectsController.EnableEffect<Hemorrhage>(SanyaPlugin.instance.Config.Scp939AttackBleedingTime);
				}

				//HurtBlink173
				if (SanyaPlugin.instance.Config.Scp173ForceBlinkPercent > 0 && ev.Target.Role == RoleType.Scp173 && UnityEngine.Random.Range(0, 100) < SanyaPlugin.instance.Config.Scp173ForceBlinkPercent)
				{
					Methods.Blink();
				}

				//CuffedDivisor
				if (ev.Target.IsCuffed)
				{
					ev.Amount /= SanyaPlugin.instance.Config.CuffedDamageMultiplier;
				}

				//SCPsDivisor
				if (ev.DamageType != DamageTypes.Wall
					&& ev.DamageType != DamageTypes.Nuke
					&& ev.DamageType != DamageTypes.Decont)
				{
					switch (ev.Target.Role)
					{
						case RoleType.Scp173:
							ev.Amount /= SanyaPlugin.instance.Config.Scp173DamageMultiplier;
							break;
						case RoleType.Scp106:
							if (ev.DamageType == DamageTypes.Grenade) ev.Amount /= SanyaPlugin.instance.Config.Scp106GrenadeMultiplier;
							if (ev.DamageType != DamageTypes.MicroHid 
								&& ev.DamageType != DamageTypes.Tesla)
								ev.Amount /= SanyaPlugin.instance.Config.Scp106DamageMultiplier;
							break;
						case RoleType.Scp049:
							ev.Amount /= SanyaPlugin.instance.Config.Scp049DamageMultiplier;
							break;
						case RoleType.Scp096:
							ev.Amount /= SanyaPlugin.instance.Config.Scp096DamageMultiplier;
							break;
						case RoleType.Scp0492:
							ev.Amount /= SanyaPlugin.instance.Config.Scp0492DamageMultiplier;
							break;
						case RoleType.Scp93953:
						case RoleType.Scp93989:
							ev.Amount /= SanyaPlugin.instance.Config.Scp939DamageMultiplier;
							break;
					}
				}
			}

			Log.Debug($"[OnPlayerHurt:After] {ev.Attacker?.Nickname}[{ev.Attacker?.Role}] -{ev.HitInformations.GetDamageName()}({ev.HitInformations.Amount})-> {ev.Target?.Nickname}[{ev.Target?.Role}]");
		}

		public void OnPlayerDeath(DiedEventArgs ev)
		{
			if (ev.Target.IsHost || ev.Target.Role == RoleType.Spectator || ev.Target.ReferenceHub.characterClassManager.GodMode || ev.Target.ReferenceHub.characterClassManager.SpawnProtected) return;
			Log.Debug($"[OnPlayerDeath] {ev.Killer?.Nickname}[{ev.Killer?.Role}] -{ev.HitInformations.GetDamageName()}-> {ev.Target?.Nickname}[{ev.Target?.Role}]");

			if (ev.Killer == null) return;

			if (SanyaPlugin.instance.Config.DataEnabled)
			{
				if (!string.IsNullOrEmpty(ev.Killer.UserId)
					&& ev.Target.UserId != ev.Killer.UserId
					&& PlayerDataManager.playersData.ContainsKey(ev.Killer.UserId))
				{
					PlayerDataManager.playersData[ev.Killer.UserId].AddExp(SanyaPlugin.instance.Config.LevelExpKill);
				}

				if (PlayerDataManager.playersData.ContainsKey(ev.Target.UserId))
				{
					PlayerDataManager.playersData[ev.Target.UserId].AddExp(SanyaPlugin.instance.Config.LevelExpDeath);
				}
			}

			if (ev.HitInformations.GetDamageType() == DamageTypes.Scp173 && ev.Killer.Role == RoleType.Scp173 && SanyaPlugin.instance.Config.Scp173RecoveryAmount > 0)
			{
				ev.Killer.ReferenceHub.playerStats.HealHPAmount(SanyaPlugin.instance.Config.Scp173RecoveryAmount);
			}
			if (ev.HitInformations.GetDamageType() == DamageTypes.Scp096 && ev.Killer.Role == RoleType.Scp096 && SanyaPlugin.instance.Config.Scp096RecoveryAmount> 0)
			{
				ev.Killer.ReferenceHub.playerStats.HealHPAmount(SanyaPlugin.instance.Config.Scp096RecoveryAmount);
			}
			if (ev.HitInformations.GetDamageType() == DamageTypes.Scp939 && (ev.Killer.Role == RoleType.Scp93953 || ev.Killer.Role == RoleType.Scp93989) && SanyaPlugin.instance.Config.Scp939RecoveryAmount> 0)
			{
				ev.Killer.ReferenceHub.playerStats.HealHPAmount(SanyaPlugin.instance.Config.Scp939RecoveryAmount);
				ev.Target.ReferenceHub.inventory.Clear();
			}
			if (ev.HitInformations.GetDamageType() == DamageTypes.Scp0492 && ev.Killer.Role == RoleType.Scp0492 && SanyaPlugin.instance.Config.Scp0492RecoveryAmount> 0)
			{
				ev.Killer.ReferenceHub.playerStats.HealHPAmount(SanyaPlugin.instance.Config.Scp0492RecoveryAmount);
			}

			if (SanyaPlugin.instance.Config.HitmarkKilled
				&& ev.Killer.Team != Team.SCP
				&& !string.IsNullOrEmpty(ev.Killer.UserId)
				&& ev.Killer.UserId != ev.Target.UserId)
			{
				Timing.RunCoroutine(Coroutines.BigHitmark(ev.Killer.GameObject.GetComponent<MicroHID>()));
			}

			if (SanyaPlugin.instance.Config.CassieSubtitle
				&& ev.Target.Team == Team.SCP
				&& ev.Target.Role != RoleType.Scp0492)
			{
				string fullname = CharacterClassManager._staticClasses.Get(ev.Target.Role).fullName;
				string str;
				if (ev.HitInformations.GetDamageType() == DamageTypes.Tesla)
				{
					str = Subtitles.SCPDeathTesla.Replace("{0}", fullname);
				}
				else if (ev.HitInformations.GetDamageType() == DamageTypes.Nuke)
				{
					str = Subtitles.SCPDeathWarhead.Replace("{0}", fullname);
				}
				else if (ev.HitInformations.GetDamageType() == DamageTypes.Decont)
				{
					str = Subtitles.SCPDeathDecont.Replace("{0}", fullname);
				}
				else
				{
					Team killerTeam = ev.Killer.Team;
					foreach (Player Exiledi in Player.List)
					{
						if (Exiledi.ReferenceHub.queryProcessor.PlayerId == ev.HitInformations.PlayerId)
						{
							killerTeam = Exiledi.Team;
						}
					}
					Log.Debug($"[CheckTeam] ply:{ev.Target.ReferenceHub.queryProcessor.PlayerId} kil:{ev.Killer.ReferenceHub.queryProcessor.PlayerId} plyid:{ev.HitInformations.PlayerId} killteam:{killerTeam}");

					if (killerTeam == Team.CDP)
					{
						str = Subtitles.SCPDeathTerminated.Replace("{0}", fullname).Replace("{1}", "un classe-D");
					}
					else if (killerTeam == Team.CHI)
					{
						str = Subtitles.SCPDeathTerminated.Replace("{0}", fullname).Replace("{1}", "l'insurection du chaos");
					}
					else if (killerTeam == Team.RSC)
					{
						str = Subtitles.SCPDeathTerminated.Replace("{0}", fullname).Replace("{1}", "un scientifique");
					}
					else if (killerTeam == Team.MTF)
					{
						
						string unit = ev.Killer.ReferenceHub.characterClassManager.CurUnitName;
						str = Subtitles.SCPDeathContainedMTF.Replace("{0}", fullname).Replace("{1}", unit);
					}
					else
					{
						str = Subtitles.SCPDeathUnknown.Replace("{0}", fullname);
					}
				}

				int count = 0;
				bool isFound079 = false;
				bool isForced = false;
				foreach (var i in Player.List)
				{
					if (ev.Target.UserId == i.UserId) continue;
					if (i.Team == Team.SCP) count++;
					if (i.Role == RoleType.Scp079) isFound079 = true;
				}

				Log.Debug($"[Check079] SCPs:{count} isFound079:{isFound079} totalvol:{Generator079.mainGenerator.totalVoltage} forced:{Generator079.mainGenerator.forcedOvercharge}");
				if (count == 1
					&& isFound079
					&& Generator079.mainGenerator.totalVoltage < 4
					&& !Generator079.mainGenerator.forcedOvercharge
					&& ev.HitInformations.GetDamageType() != DamageTypes.Nuke)
				{
					isForced = true;
					str = str.Replace("{-1}", "\n《Tout les SCP ont été sécurisé.\nLa séquence de reconfinement de SCP-079 a commencé\nLa Heavy Containement Zone vas surcharger dans t-moins 1 minutes.》");
				}
				else
				{
					str = str.Replace("{-1}", string.Empty);
				}

				Methods.SendSubtitle(str, (ushort)(isForced ? 30 : 10));
			}

			if (ev.HitInformations.GetDamageType() == DamageTypes.Tesla || ev.HitInformations.GetDamageType() == DamageTypes.Nuke)
			{
				ev.Target.ReferenceHub.inventory.Clear();
			}

			//Ticket Extend a revoir
			var ticket = RespawnTickets.Singleton._tickets;
			switch (ev.Killer.Team)
			{
				case Team.CDP:
					ticket.Add(SpawnableTeamType.ChaosInsurgency, SanyaPlugin.instance.Config.tickets_ci_classd_died_count);
					if (ev.Killer.Team == Team.MTF || ev.Killer.Team == Team.RSC) ticket.Add(SpawnableTeamType.NineTailedFox, SanyaPlugin.instance.Config.tickets_mtf_classd_killed_count);
					break;
				case Team.RSC:
					ticket.Add(SpawnableTeamType.NineTailedFox ,SanyaPlugin.instance.Config.tickets_mtf_scientist_died_count);
					if (ev.Killer.Team == Team.CHI || ev.Killer.Team == Team.CDP) ticket.Add(SpawnableTeamType.NineTailedFox, SanyaPlugin.instance.Config.tickets_ci_scientist_killed_count);
					break;
				case Team.MTF:
					if (ev.Killer.Team == Team.SCP) ticket.Add(SpawnableTeamType.NineTailedFox, SanyaPlugin.instance.Config.tickets_mtf_killed_by_scp_count);
					break;
				case Team.CHI:
					if (ev.Killer.Team == Team.SCP) ticket.Add(SpawnableTeamType.ChaosInsurgency ,SanyaPlugin.instance.Config.tickets_ci_killed_by_scp_count);
					break;
			}
		}

		public void OnPocketDimDeath(FailingEscapePocketDimensionEventArgs ev)
		{
			Log.Debug($"[OnPocketDimDeath] {ev.Player.Nickname}");

			if (SanyaPlugin.instance.Config.DataEnabled)
			{
				foreach (Player player in Player.List)
				{
					if (player.Role == RoleType.Scp106)
					{
						if (PlayerDataManager.playersData.ContainsKey(player.UserId))
						{
							PlayerDataManager.playersData[player.UserId].AddExp(SanyaPlugin.instance.Config.LevelExpKill);
						}
					}
				}
			}

			if (SanyaPlugin.instance.Config.Scp106RecoveryAmount > 0)
			{
				foreach (Player player in Player.List)
				{
					if (player.Role == RoleType.Scp106)
					{
						player.ReferenceHub.playerStats.HealHPAmount(SanyaPlugin.instance.Config.Scp106RecoveryAmount);
						player.ReferenceHub.ShowHitmarker();
					}
				}
			}
		}

		public void OnPlayerUsedMedicalItem(UsedMedicalItemEventArgs ev)
		{
			Log.Debug($"[OnPlayerUsedMedicalItem] {ev.Player.Nickname} -> {ev.Item}");

			if (ev.Item == ItemType.Medkit)
			{
				ev.Player.ReferenceHub.playerEffectsController.DisableEffect<Hemorrhage>();
				ev.Player.ReferenceHub.playerEffectsController.DisableEffect<Bleeding>();
				ev.Player.ReferenceHub.playerEffectsController.DisableEffect<Burned>();
			}
			if (ev.Item == ItemType.Adrenaline)
			{
				ev.Player.ReferenceHub.playerEffectsController.DisableEffect<Panic>();
				ev.Player.ReferenceHub.playerEffectsController.DisableEffect<Disabled>();
			}
			if (ev.Item == ItemType.SCP500)
			{
				ev.Player.ReferenceHub.playerEffectsController.DisableEffect<Hemorrhage>();
				ev.Player.ReferenceHub.playerEffectsController.DisableEffect<Bleeding>();
				ev.Player.ReferenceHub.playerEffectsController.DisableEffect<Disabled>();
				ev.Player.ReferenceHub.playerEffectsController.DisableEffect<Burned>();
				ev.Player.ReferenceHub.playerEffectsController.DisableEffect<Poisoned>();
			}
		}

		public void OnPlayerTriggerTesla(TriggeringTeslaEventArgs ev)
		{
			Log.Debug($"[OnPlayerTriggerTesla] {ev.IsInHurtingRange}:{ev.Player.Nickname}");
			if (SanyaPlugin.instance.Config.TeslaTriggerableTeams.Count == 0
				|| SanyaPlugin.instance.Config.TeslaTriggerableTeams.Contains(ev.Player.Team))
			{
				if (SanyaPlugin.instance.Config.TeslaTriggerableDisarmed || ev.Player.CufferId == -1)
				{
					ev.IsTriggerable = true;
				}
				else
				{
					ev.IsTriggerable = false;
				}
			}
			else
			{
				ev.IsTriggerable = false;
			}
		}

		public void OnPlayerDoorInteract(InteractingDoorEventArgs ev)
		{
			Log.Debug($"[OnPlayerDoorInteract] {ev.Player.Nickname}:{ev.Door.DoorName}:{ev.Door.PermissionLevels}");

			if (SanyaPlugin.instance.Config.InventoryKeycardActivation && ev.Player.Team != Team.SCP && !ev.Player.ReferenceHub.serverRoles.BypassMode && !ev.Door.locked)
			{
				foreach (var item in ev.Player.ReferenceHub.inventory.items)
				{
					foreach (var permission in ev.Player.ReferenceHub.inventory.GetItemByID(item.id).permissions)
					{
						if (ev.Door.backwardsCompatPermissions.TryGetValue(permission, out var flag) && ev.Door.PermissionLevels.HasPermission(flag))
						{
							ev.IsAllowed = true;
						}
					}
				}
			}

			//Mini fix
			if (ev.Door.DoorName.Contains("CHECKPOINT") && (ev.Door.decontlock || ev.Door.warheadlock) && !ev.Door.isOpen)
			{
				ev.Door.SetStateWithSound(true);
			}
		}

		public void OnPlayerLockerInteract(InteractingLockerEventArgs ev)
		{
			Log.Debug($"[OnPlayerLockerInteract] {ev.Player.Nickname}:{ev.Id}");
			if (SanyaPlugin.instance.Config.InventoryKeycardActivation)
			{
				foreach (var item in ev.Player.ReferenceHub.inventory.items)
				{
					if (ev.Player.ReferenceHub.inventory.GetItemByID(item.id).permissions.Contains("PEDESTAL_ACC"))
					{
						ev.IsAllowed = true;
					}
				}
			}
		}
		public void OnPlayerChangeAnim(SyncingDataEventArgs ev)
		{
			if (ev.Player.IsHost || ev.Player.ReferenceHub.animationController.curAnim == ev.CurrentAnimation) return;

			if (SanyaPlugin.instance.Config.Scp079ExtendEnabled && ev.Player.Role == RoleType.Scp079)
			{
				if (ev.CurrentAnimation == 1)
					ev.Player.ReferenceHub.SendTextHint(Subtitles.ExtendEnabled, 3);
				else
					ev.Player.ReferenceHub.SendTextHint(Subtitles.ExtendDisabled, 3);
			}

			if (SanyaPlugin.instance.Config.StaminaLostJump != -1f
				&& ev.CurrentAnimation == 2
				&& ev.Player.ReferenceHub.characterClassManager.IsHuman()
				&& !ev.Player.ReferenceHub.fpc.staminaController._invigorated.Enabled
				&& !ev.Player.ReferenceHub.fpc.staminaController._scp207.Enabled
				)
			{
				ev.Player.ReferenceHub.fpc.staminaController.RemainingStamina -= SanyaPlugin.instance.Config.StaminaLostJump;
				ev.Player.ReferenceHub.fpc.staminaController._regenerationTimer = 0f;
			}
			if (SanyaPlugin.instance.Config.StaminaEffect)
			if (ev.Player.ReferenceHub.fpc.staminaController.RemainingStamina <= 0f
				&& ev.Player.ReferenceHub.characterClassManager.IsHuman()
				&& !ev.Player.ReferenceHub.fpc.staminaController._invigorated.Enabled
				&& !ev.Player.ReferenceHub.fpc.staminaController._scp207.Enabled)
			{
				ev.Player.ReferenceHub.playerEffectsController.EnableEffect<Disabled>(3f);
			}
		}

		public void OnTeamRespawn(RespawningTeamEventArgs ev)
		{
			Log.Debug($"[OnTeamRespawn] Queues:{ev.Players.Count} IsCI:{ev.NextKnownTeam == SpawnableTeamType.ChaosInsurgency} MaxAmount:{ev.MaximumRespawnAmount}");

			if (SanyaPlugin.instance.Config.StopRespawnAfterDetonated&& AlphaWarheadController.Host.detonated)
			{
				ev.Players.Clear();
			}
			if (SanyaPlugin.instance.Config.GodmodeAfterEndround && !RoundSummary.RoundInProgress())
			{
				ev.Players.Clear();
			}
			if (Coroutines.isAirBombGoing)
			{
				ev.Players.Clear();
			}
		}

		public void OnGeneratorUnlock(UnlockingGeneratorEventArgs ev)
		{
			Log.Debug($"[OnGeneratorUnlock] {ev.Player.Nickname} -> {ev.Generator.CurRoom}");
			if (SanyaPlugin.instance.Config.InventoryKeycardActivation && !ev.Player.ReferenceHub.serverRoles.BypassMode)
			{
				foreach (var item in ev.Player.ReferenceHub.inventory.items)
				{
					if (ev.Player.ReferenceHub.inventory.GetItemByID(item.id).permissions.Contains("ARMORY_LVL_2"))
					{
						ev.IsAllowed = true;
					}
				}
			}

			if (ev.IsAllowed && SanyaPlugin.instance.Config.GeneratorUnlockOpen)
			{
				ev.Generator._doorAnimationCooldown = 1.5f;
				ev.Generator.NetworkisDoorOpen = true;
				ev.Generator.RpcDoSound(true);
			}
		}

		public void OnGeneratorOpen(OpeningGeneratorEventArgs ev)
		{
			Log.Debug($"[OnGeneratorOpen] {ev.Player.Nickname} -> {ev.Generator.CurRoom}");
			if (ev.Generator.prevFinish && SanyaPlugin.instance.Config.GeneratorFinishLock) ev.IsAllowed = false;
		}

		public void OnGeneratorClose(ClosingGeneratorEventArgs ev)
		{
			Log.Debug($"[OnGeneratorClose] {ev.Player.Nickname} -> {ev.Generator.CurRoom}");
			if (ev.IsAllowed && ev.Generator.isTabletConnected && SanyaPlugin.instance.Config.GeneratorActivatingClose) ev.IsAllowed = false;
		}

		public void OnGeneratorInsert(InsertingGeneratorTabletEventArgs ev)
		{
			Log.Debug($"[OnGeneratorInsert] {ev.Player.Nickname} -> {ev.Generator.CurRoom}");
		}

		public void OnGeneratorFinish(GeneratorActivatedEventArgs ev)
		{
			Log.Debug($"[OnGeneratorFinish] {ev.Generator.CurRoom}");
			if (SanyaPlugin.instance.Config.GeneratorFinishLock) ev.Generator.NetworkisDoorOpen = false;

			int curgen = Generator079.mainGenerator.NetworktotalVoltage + 1;
			if (SanyaPlugin.instance.Config.CassieSubtitle&& !Generator079.mainGenerator.forcedOvercharge)
			{
				if (curgen < 5)
				{
					Methods.SendSubtitle(Subtitles.GeneratorFinish.Replace("{0}", curgen.ToString()), 10);
				}
				else
				{
					Methods.SendSubtitle(Subtitles.GeneratorComplete, 20);
				}
			}

			if (eventmode == SANYA_GAME_MODE.NIGHT && curgen >= 3 && IsEnableBlackout)
			{
				IsEnableBlackout = false;
			}

		}
	public void OnPlacingDecal(PlacingDecalEventArgs ev)
		{
			Log.Debug($"[OnPlacingDecal] position : {ev.Position} Owner: {ev.Owner} Type: {ev.Type}");
		}
	public void On079LevelGain(GainingLevelEventArgs ev)
		{
			Log.Debug($"[On079LevelGain] {ev.Player.Nickname} : {ev.NewLevel}");

			if (SanyaPlugin.instance.Config.Scp079ExtendEnabled)
			{
				switch (ev.NewLevel)
				{
					case 1:
						ev.Player.ReferenceHub.SendTextHint(Subtitles.Extend079Lv2, 10);
						break;
					case 2:
						ev.Player.ReferenceHub.SendTextHint(Subtitles.Extend079Lv3, 10);
						break;
					case 3:
						ev.Player.ReferenceHub.SendTextHint(Subtitles.Extend079Lv4, 10);
						break;
				}
			}
		}
		public void On106MakePortal(CreatingPortalEventArgs ev)
		{
			Log.Debug($"[On106MakePortal] {ev.Player.Nickname}:{ev.Position}:{ev.Player.ReferenceHub.IsExmode()}");

			if (SanyaPlugin.instance.Config.Scp106PortalExtensionEnabled && ev.Player.Role == RoleType.Scp106)
			{
			//	List<Room> Map.Rooms;
			}
		}

		public void On106Teleport(TeleportingEventArgs ev)
		{
			Log.Debug($"[On106Teleport] {ev.Player.Nickname}:{ev.PortalPosition}:{ev.Player.ReferenceHub.IsExmode()}");
		}

		public void On914Upgrade(UpgradingItemsEventArgs ev)
		{
			Log.Debug($"[On914Upgrade] {ev.KnobSetting} Players:{ev.Players.Count} Items:{ev.Items.Count}");

			if (SanyaPlugin.instance.Config.Scp914IntakeDeath)
			{
				foreach (var player in ev.Players)
				{
					player.ReferenceHub.inventory.Clear();
					var info = new PlayerStats.HitInfo(914914, "WORLD", DamageTypes.RagdollLess, 0);
					player.ReferenceHub.playerStats.HurtPlayer(info, player.ReferenceHub.gameObject);
				}
			}
		}
		public void OnShoot(ShootingEventArgs ev)
		{
			Log.Debug($"[OnShoot] {ev.Shooter.Nickname} -{ev.Position}-> {ev.Target?.name}");

			if ((SanyaPlugin.instance.Config.grenade_shoot_fuse || SanyaPlugin.instance.Config.item_shoot_move)
				&& ev.Position != Vector3.zero
				&& Physics.Linecast(ev.Shooter.Position, ev.Position, out RaycastHit raycastHit, grenade_pickup_mask))
			{
				if (SanyaPlugin.instance.Config.item_shoot_move)
				{
					var pickup = raycastHit.transform.GetComponentInParent<Pickup>();
					if (pickup != null && pickup.Rb != null)
					{
						pickup.Rb.AddExplosionForce(Vector3.Distance(ev.Position, ev.Shooter.Position), ev.Shooter.Position, 500f, 3f, ForceMode.Impulse);
					}
				}

				if (SanyaPlugin.instance.Config.grenade_shoot_fuse)
				{
					var grenade = raycastHit.transform.GetComponentInParent<FragGrenade>();
					if (grenade != null)
					{
						grenade.NetworkfuseTime = 0.1f;
					}
				}
			}
			if (SanyaPlugin.instance.Config.StaminaLostLogicer != -1f
				&& ev.Shooter.ReferenceHub.characterClassManager.IsHuman()
				&& ItemType.GunLogicer == ev.Shooter.CurrentItem.id
				&& ev.IsAllowed
				&& !ev.Shooter.ReferenceHub.fpc.staminaController._invigorated.Enabled
				&& !ev.Shooter.ReferenceHub.fpc.staminaController._scp207.Enabled
				)
			{
				ev.Shooter.ReferenceHub.fpc.staminaController.RemainingStamina -= SanyaPlugin.instance.Config.StaminaLostLogicer;
				ev.Shooter.ReferenceHub.fpc.staminaController._regenerationTimer = 0f;
			}
		}
		public void OnCommand (SendingConsoleCommandEventArgs ev)
		{
			Log.Debug($"Player : {ev.Player} Name : {ev.Name}");
		}
		public void OnRACommand(SendingRemoteAdminCommandEventArgs ev)
		{
			
			string[] args = ev.Arguments.ToArray();
			Log.Debug($"[OnCommand] sender:{ev.CommandSender.SenderId} command:{ev.Name} args:{args.Length}");
			string effort = $"{ev.Name} ";
			foreach (string s in ev.Arguments)
				effort += $"{s} ";

			args = effort.Split(' ');
			if (args[0].ToLower() == "sanya")
			{
				ReferenceHub player = ev.CommandSender.SenderId == "SERVER CONSOLE" || ev.CommandSender.SenderId == "GAME CONSOLE" ? Player.Dictionary[PlayerManager.localPlayer].ReferenceHub : ev.Sender.ReferenceHub;
				Player perm = Player.Dictionary[player.gameObject];
				if (args.Length > 1)
				{
					string ReturnStr;
					bool isSuccess = true;
					switch (args[1].ToLower())
					{
						case "test":
							{
								ReturnStr = "test ok.";
								break;
							}
						case "resynceffect":
							{
								if (!perm.CheckPermission("sanya.resynceffect"))
								{
									ev.Sender.RemoteAdminMessage("Permission denied.");
									return;
								}
								foreach (var ply in Player.List)
								{
									ply.ReferenceHub.playerEffectsController.Resync();
								}
								ReturnStr = "Resync ok.";
								break;
							}
						case "check":
							{
								if (!perm.CheckPermission("sanya.check"))
								{
									ev.Sender.RemoteAdminMessage("Permission denied.");
									return;
								}
								ReturnStr = $"\nPlayers List ({PlayerManager.players.Count})\n";
								foreach (var i in Player.List)
								{
									ReturnStr += $"{i.Nickname} {i.Position}\n";
									foreach (var effect in i.ReferenceHub.playerEffectsController.syncEffectsIntensity)
										ReturnStr += $"{effect}";
									ReturnStr += "\n";
								}
								ReturnStr.Trim();
								break;
							}
						case "showconfig":
							{
								ReturnStr = SanyaPlugin.instance.Config.GetConfigs();
								break;
							}
						case "reload":
							{
								if (!perm.CheckPermission("sanya.reload"))
								{
									ev.Sender.RemoteAdminMessage("Permission denied.");
									return;
								}
								SanyaPlugin.instance.Config.GetConfigs();
								if (SanyaPlugin.instance.Config.KickVpn) ShitChecker.LoadLists();
								ReturnStr = "reload ok";
								break;
							}
						case "list":
							{
								if (!perm.CheckPermission("sanya.list"))
								{
									ev.Sender.RemoteAdminMessage("Permission denied.");
									return;
								}
								ReturnStr = $"Players List ({PlayerManager.players.Count})\n";
								foreach (var i in Player.List)
								{
									ReturnStr += $"[{i.Id}]{i.Nickname}({i.UserId})<{i.Role}/{i.Health}HP> {i.Position}\n";
								}
								ReturnStr.Trim();
								break;
							}
						case "startair":
							{
								if (!perm.CheckPermission("sanya.airbomb"))
								{
									ev.Sender.RemoteAdminMessage("Permission denied.");
									return;
								}
								if (args.Length > 2)
								{
									if (int.TryParse(args[2], out int duration))
									{
										if (int.TryParse(args[3], out int duration2))
										{
											roundCoroutines.Add(Timing.RunCoroutine(Coroutines.AirSupportBomb(false,duration, duration2)));
											ReturnStr = $"The AirBombing start in {duration / 60}:{duration % 60} and stop in {duration2 / 60}:{duration2 % 60:00}";
											break;
										}
										else
										{
											roundCoroutines.Add(Timing.RunCoroutine(Coroutines.AirSupportBomb(false,duration)));
											ReturnStr = $"The AirBombing start in {duration / 60}:{duration % 60:00}!";
											break;
										}
									}
									else
									{
										isSuccess = false;
										ReturnStr = "startair {durée avant que ça démare} {durée du bombardement}";
										break;
									}
								}
								else
								{
									roundCoroutines.Add(Timing.RunCoroutine(Coroutines.AirSupportBomb()));
									ReturnStr = "Started!";
									break;
								}
							}
						case "stopair":
							{
								if (!perm.CheckPermission("sanya.airbomb"))
								{
									ev.Sender.RemoteAdminMessage("Permission denied.");
									return;
								}
								roundCoroutines.Add(Timing.RunCoroutine(Coroutines.AirSupportBomb(true)));
								Coroutines.isAirBombGoing = false;
								ReturnStr = $"Stop ok. now:{Coroutines.isAirBombGoing}";
								break;
							}
						case "914":
							{
								if (!perm.CheckPermission("sanya.914"))
								{
									ev.Sender.RemoteAdminMessage("Permission denied.");
									return;
								}
								if (args.Length > 2)
								{
									if (!Scp914.Scp914Machine.singleton.working)
									{

										if (args[2] == "use")
										{
											Scp914.Scp914Machine.singleton.RpcActivate(NetworkTime.time);
											ReturnStr = $"Used : {Scp914.Scp914Machine.singleton.knobState}";
										}
										else if (args[2] == "knob")
										{
											Scp914.Scp914Machine.singleton.ChangeKnobStatus();
											ReturnStr = $"Knob Changed to:{Scp914.Scp914Machine.singleton.knobState}";
										}
										else
										{
											isSuccess = false;
											ReturnStr = "[914] Wrong Parameters.";
										}
									}
									else
									{
										isSuccess = false;
										ReturnStr = "[914] SCP-914 is working now.";
									}
								}
								else
								{
									isSuccess = false;
									ReturnStr = "[914] Parameters : 914 <use/knob>";
								}
								break;
							}
						case "nukecap":
							{
								if (!perm.CheckPermission("sanya.nukecap"))
								{
									ev.Sender.RemoteAdminMessage("Permission denied.");
									return;
								}
								var outsite = GameObject.Find("OutsitePanelScript")?.GetComponent<AlphaWarheadOutsitePanel>();
								outsite.NetworkkeycardEntered = !outsite.keycardEntered;
								ReturnStr = $"{outsite?.keycardEntered}";
								break;
							}
						case "blackout":
							{
								if (!perm.CheckPermission("sanya.blackout"))
								{
									ev.Sender.RemoteAdminMessage("Permission denied.");
									return;
								}
								if (args.Length > 2 && args[2] == "hcz")
								{
									if (float.TryParse(args[3], out float duration))
										Generator079.mainGenerator.RpcCustomOverchargeForOurBeautifulModCreators(duration, true);
									ReturnStr = "HCZ blackout!";
								}
								if (args.Length > 2 && args[2] == "all")
								{
									if (float.TryParse(args[3], out float duration))
										Generator079.mainGenerator.RpcCustomOverchargeForOurBeautifulModCreators(duration, false);
									ReturnStr = "ALL blackout!";
								}
								else
									ReturnStr = "all ou hcz";
								break;
							}
						case "femur":
							{
								if (!perm.CheckPermission("sanya.femur"))
								{
									ev.Sender.RemoteAdminMessage("Permission denied.");
									return;
								}
								PlayerManager.localPlayer.GetComponent<PlayerInteract>()?.RpcContain106(PlayerManager.localPlayer);
								ReturnStr = "FemurScreamer!";
								break;
							}
						case "explode":
							{
								if (!perm.CheckPermission("sanya.explode"))
								{
									ev.Sender.RemoteAdminMessage("Permission denied.");
									return;
								}
								if (args.Length > 2)
								{
									Player target = Player.Get(args[2]);
									if (target != null && target.Role != RoleType.Spectator)
									{
										Methods.SpawnGrenade(target.Position, false, 0.1f, target.ReferenceHub);
										ReturnStr = $"success. target:{target.Nickname}";
										break;
									}
									if (args[2] == "all")
									{
										foreach (var ply in Player.List)
										{
											Methods.SpawnGrenade(ply.Position, false, 0.1f, ply.ReferenceHub);
										}
										ReturnStr = "success spawn grenade on all player";
										break;
									}
									else
									{
										isSuccess = false;
										ReturnStr = "[explode] missing target.";
										break;
									}
								}
								else
								{
									if (player != null)
									{
										Methods.SpawnGrenade(player.transform.position, false, 0.1f, player);
										ReturnStr = $"success. target:{Player.Get(player.gameObject).Nickname}";
										break;
									}
									else
									{
										isSuccess = false;
										ReturnStr = "[explode] missing target.";
										break;
									}
								}
							}
						case "ammo":
							{
								if (!perm.CheckPermission("sanya.ammo"))
								{
									ev.Sender.RemoteAdminMessage("Permission denied.");
									return;
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
											ReturnStr = $"{target.Nickname}  {Nato556}:{Nato762}:{Nato9}";
											break;
										}
										else
										{ 
											ReturnStr = "sanya ammo {player} (5.56) (7.62) (9mm).";
											break;
										}
									}
									if (args[2] == "all")
									{		
										if (uint.TryParse(args[3], out uint Nato556)
											&& uint.TryParse(args[4], out uint Nato762)
											&& uint.TryParse(args[5], out uint Nato9))
										{
											foreach (var ply in Player.List)
											{
												ply.SetAmmo(Exiled.API.Enums.AmmoType.Nato556 , Nato556);
												ply.SetAmmo(Exiled.API.Enums.AmmoType.Nato762, Nato762);
												ply.SetAmmo(Exiled.API.Enums.AmmoType.Nato9, Nato9);
											}
											ReturnStr = $"ammo set {Nato556}:{Nato762}:{Nato9}";
											break;
										}
										else
										{ 
											ReturnStr = "sanya ammo all (5.56) (7.62) (9mm)";
											break;
										}
									}
									else
									{
										isSuccess = false;
										ReturnStr = "sanya (player id ou all) ";
										break;
									}
								}
								else
								{
									ReturnStr = "Failed to set. (cant use from SERVER)";
									break;
								}
							}
						case "clearinv":
							{
								if (!perm.CheckPermission("sanya.clearinv"))
								{
									ev.Sender.RemoteAdminMessage("Permission denied.");
									return;
								}
								if (args.Length > 2)
								{
									Player target = Player.UserIdsCache[args[2]];
									if (target != null && target.Role != RoleType.Spectator)
									{
										target.ClearInventory();
										ReturnStr = $"Clear Inventory: {target.Nickname}";
										break;
									}
									if (args[2] == "all")
									{
										foreach (var ply in Player.List)
										{
											ply.ClearInventory();
										}
										ReturnStr = "INVENTORY OF ALL PLAYER AS BEEN CLEAR";
										break;
									}
									else
									{
										isSuccess = false;
										ReturnStr = "Fail";
										break;
									}
								}
								if (player != null)
								{
									ev.Sender.ClearInventory();
									ReturnStr = "Your Inventory as been clear";
									break;
								}
								else
								{
									ReturnStr = $"sanya clearinv {Player.Get(player).Nickname}";
									break;
								}
							}
						case "cleareffect":
							{
								if (!perm.CheckPermission("sanya.cleareffect"))
								{
									ev.Sender.RemoteAdminMessage("Permission denied.");
									return;
								}
								if (args.Length > 2)
								{
									
									Player target = Player.UserIdsCache[args[2]];
									if (target != null && target.Role != RoleType.Spectator)
									{
										foreach(KeyValuePair<Type, PlayerEffect> keyValuePair in target.ReferenceHub.playerEffectsController.AllEffects.ToArray())
                                        {
											PlayerEffect effect = keyValuePair.Value;
											effect.ServerDisable();
										}
										ReturnStr = $"ALL EFFECT AS BEEN CLEAR FOR {target.Nickname}";
										break;
									}
									else
									{
										isSuccess = false;
										ReturnStr = "Fail";
										break;
									}
								}
								if (player != null)
								{
									foreach (KeyValuePair<Type, PlayerEffect> keyValuePair in player.playerEffectsController.AllEffects.ToArray())
									{
										PlayerEffect effect = keyValuePair.Value;
										effect.ServerDisable();
									}
									ReturnStr = "ALL YOUR EFFECT AS BEEN CLEAR";
									break;
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
									ReturnStr = "ALL EFFECT OF ALL PLAYER AS BEEN CLEAR";
									break;
								}
								else
								{
									ReturnStr = "Failed to set."; 
									break;
								}
							}/*
						case "dummy":
							{
								if (!perm.CheckPermission("sanya.dummy"))
								{
									ev.Sender.RemoteAdminMessage("Permission denied.");
									return;
								}
								if (args.Length > 2)
								{
									Player target = Player.Get(args[2]);
									var roletype = target.Role;
									if (target != null && target.Role != RoleType.Spectator)
									{
									Methods.SpawnDummy(target.Role , target.Position, target.ReferenceHub.transform.rotation);
									ReturnStr = $"{target.Role}'s Dummy Created. pos:{target.Position} rot:{target.ReferenceHub.transform.rotation}";
									break;
									}
									if (args[2] == "all")
									{
										foreach (var ply in Player.List)
										{
											Methods.SpawnDummy(ply.Role,ply.Position,ply.ReferenceHub.transform.rotation);
										}
										ReturnStr = "success spawn grenade on all player";
										break;
									}
									else
									{
										isSuccess = false;
										ReturnStr = "[explode] missing target.";
										break;
									}
								}
								else
								{
									if (player != null)
									{
										Methods.SpawnDummy(RoleType.ClassD , perm.Position, player.transform.rotation);
										ReturnStr = $"{perm.Role}'s Dummy Created. pos:{perm.Position} rot:{player.transform.rotation}";
										break;
									}
									else
									{
										isSuccess = false;
										ReturnStr = "[explode] missing target.";
										break;
									}
								}
							}*/
						case "ev":
							{
								if (!Player.Dictionary[player.gameObject].CheckPermission("sanya.ev"))
								{
									ev.Sender.RemoteAdminMessage("Permission denied.");
									return;
								}
								foreach (Lift lift in UnityEngine.Object.FindObjectsOfType<Lift>())
								{
									lift.UseLift();
								}
								ReturnStr = "EV Used.";
								break;
							}
						case "roompos":
							{
								if (!Player.Dictionary[player.gameObject].CheckPermission("sanya.roompos"))
								{
									ev.Sender.RemoteAdminMessage("Permission denied.");
									return;
								}
								string output = "\n";
								foreach (var rid in UnityEngine.Object.FindObjectsOfType<Rid>())
								{
									output += $"{rid.id} : {rid.transform.position}\n";
								}
								ReturnStr = output;
								break;
							}
						case "tppos":
							{
								if (!Player.Dictionary[player.gameObject].CheckPermission("sanya.tppos"))
								{
									ev.Sender.RemoteAdminMessage("Permission denied.");
									return;
								}
								if (args.Length > 5)
								{
									ReferenceHub target = Player.UserIdsCache[args[2]].ReferenceHub;
									if (target != null)
									{
										if (float.TryParse(args[3], out float x)
											&& float.TryParse(args[4], out float y)
											&& float.TryParse(args[5], out float z))
										{
											Vector3 pos = new Vector3(x, y, z);
											target.playerMovementSync.OverridePosition(pos, 0f, true);
											ReturnStr = $"TP to {pos}.";
										}
										else
										{
											isSuccess = false;
											ReturnStr = "[tppos] Wrong parameters.";
										}
									}
									else
									{
										isSuccess = false;
										ReturnStr = "[tppos] missing target.";
									}
								}
								else
								{
									isSuccess = false;
									ReturnStr = "[tppos] parameters : tppos <player> <x> <y> <z>";
								}

								break;
							}
						case "gen":
							{
								if (!Player.Dictionary[player.gameObject].CheckPermission("sanya.gen"))
								{
									ev.Sender.RemoteAdminMessage("Permission denied.");
									return;
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
										ReturnStr = "gen unlocked.";
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
										ReturnStr = $"gen doors interacted.";
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
										ReturnStr = "gen set.";
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
										ReturnStr = "set once.";
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
										ReturnStr = "gen ejected.";
									}
									else
									{
										isSuccess = false;
										ReturnStr = "[gen] Wrong Parameters.";
									}
								}
								else
								{
									isSuccess = false;
									ReturnStr = "[gen] Parameters : gen <unlock/door/set/once/eject>";
								}
								break;
							}
						case "spawn":
							{
								if (!Player.Dictionary[player.gameObject].CheckPermission("sanya.spawn"))
								{
									ev.Sender.RemoteAdminMessage("Permission denied.");
									return;
								}
								var mtfRespawn = RespawnManager.Singleton;
								if (args.Length > 2)
								{
									if (args[2] == "ci" || args[2] == "ic")
									{
										mtfRespawn.Spawn();
										ReturnStr = $"force spawn ChaosInsurgency";
										break;
									}
								else if (args[2] == "mtf" || args[2] == "ntf")
									{
										mtfRespawn.Spawn();
										ReturnStr = $"force spawn NineTailedFox";
										break;
									}
									else
									{ 
									ReturnStr = $"ntf/mtf ou ci/ic ou rien";
									break;
									}
								}
								else
								{
									if (mtfRespawn.NextKnownTeam == SpawnableTeamType.ChaosInsurgency)
									{
										mtfRespawn.Spawn();
										ReturnStr = $"spawn soon. Chaos Insurgency";
										break;
									}
									else
									{
										mtfRespawn.Spawn();
										ReturnStr = $"spawn soon. Nine Tailed Fox";
										break;
									}
								}
							}/*
						case "next":
							{
								if (!Player.Dictionary[player.gameObject].CheckPermission("sanya.next"))
								{
									ev.Sender.RemoteAdminMessage("Permission denied.");
									return;
								}
								var mtfRespawn = RespawnManager.Singleton;
								if (args.Length > 2)
								{
									if (args[2] == "time")
									{
										//Intercom NextSpawn
										ReturnStr = $"Futur Commande";
										break;
									}
									if (args[2] == "ci" || args[2] == "ic")
									{
										ReturnStr = $"Is Success:{mtfRespawn.NextKnownTeam == SpawnableTeamType.ChaosInsurgency}";
										break;
									}
									else if (args[2] == "mtf" || args[2] == "ntf")
									{
										RespawnTickets.Singleton.
										ReturnStr = $"Is Success:{mtfRespawn.NextKnownTeam == SpawnableTeamType.NineTailedFox}";
										break;
									}
									else
									{
										isSuccess = false;
										ReturnStr = "ntf/mtf ou ci/ic";
										break;
									}
								}
								else
								{
									if (mtfRespawn.NextKnownTeam == SpawnableTeamType.ChaosInsurgency)
									{ 
										ReturnStr = $"Next Respawn is ChaosInsurgency";
										break;
									}
									else if (mtfRespawn.NextKnownTeam == SpawnableTeamType.NineTailedFox)
									{ 
										ReturnStr = $"Next Respawn is NineTailedFox";
										break;
									}
									else
									{
										if (mtfRespawn.NextKnownTeam == SpawnableTeamType.ChaosInsurgency)
										{
											ReturnStr = $"Next spawn is Chaos Insurgency";
											break;
										}
										else
										{
											ReturnStr = $"Next spawn is Nine Tailed Fox";
											break;
										}
									}
								}								
							}
						case "van":
							{
								if (!Player.Dictionary[player.gameObject].CheckPermission("sanya.van"))
								{
									ev.Sender.RemoteAdminMessage("Permission denied.");
									return;
								}
								RespawnManager.Singleton.GetComponent<RespawnWaveGenerator>;
								PlayerManager.localPlayer.GetComponent<MTFRespawn>()?.RpcVan();
								ReturnStr = "Van Called!";
								break;
							}
						case "heli":
							{
								if (!Player.Dictionary[player.gameObject].CheckPermission("sanya.heli"))
								{
									ev.Sender.RemoteAdminMessage("Permission denied.");
									return;
								}
								ReturnStr = "Heli Called!";
								break;
							}*/
						case "now":
							{
								if (!Player.Dictionary[player.gameObject].CheckPermission("sanya.now"))
								{
									ev.Sender.RemoteAdminMessage("Permission denied.");
									return;
								}
								ReturnStr = TimeBehaviour.CurrentTimestamp().ToString();
								break;
							}
						case "help":
							{
								ReturnStr = " Sanya Commands\nsanya showconfig    =Config du plugin\nsanya list          =liste de tout les joueur connecté \nsanya startair      =Bombardement Aérien Démarage \nsanya stopair       =Bombardement Aérien Arrét \nsanya 914(knob / use) = Change la configuration de 914 ou l'utilise\nsanya nukecap = Lève ou baise le petit cache sure la nuke \nsanya sonar = Combien il y a de joueur ennemie par rapport a votre rôle \nsanya blackout = Active un blackout de 10 secondes HCZ et LCZ \nsanya blackout hcz {durée}= Active un blackout de {durée} secondes HCZ\nsanya femur = Active que le son du re-confinement de 106 \nsanya explode[id] = Explose une grenade sur le joueur ciblé\nsanya grenade[id] = Spawn une grenade sous les pied du joueur ciblé\nsanya flash[id] = Spawn une flash sous les pied du joueur ciblé\nsanya ball[id] = Spawn une balle sous les pied du joueur ciblé\nsanya ammo [id]= Full munition pour le joueur ciblé\nsanya next(ic / ntf) = Configure le prochain spawn a MTF ou IC / MdS(MTF ou NTF c'est identique)\nsanya spawn         =Force le prochain spawn \nsanya roompos = la position X Y Z des salle avec leurs noms \nsanya tppos(id)(x)(y)(z) \nsanya pocket = Vous tp dans la pocket \nsanya van = Fait venir la voiture des chaos \nsanya heli = Fait venir un Hélico \nsanya dummy = Fait spawn un PNG \nsanya cleareffect = clear tout les effet \n Commande pour les générateur \nsanya gen unlock = Unlock tout les générateurs de SCP-079 \nsanya gen door = Ouvre tout les porte de SCP-079 \nsanya gen set = Place une tablette dans TOUT les générateur de SCP-079 \nsanya gen once = pose une tablette dans un seul générateur \nsanya gen eject = eject tout les tablette des générateur de SCP-079";
								break;
							}
						default:
							{
								ReturnStr = "Wrong Parameters.";
								isSuccess = false;
								break;
							}
					}
					ev.IsAllowed = false;
					ev.Sender.RemoteAdminMessage(ReturnStr, isSuccess);
				}
				else
				{
					ev.IsAllowed = false;
					ev.Sender.RemoteAdminMessage(string.Concat(
						"Usage : sanya help <reload / startair / stopair / list / blackout ",
						"/ roompos / tppos / pocket / gen / spawn / next / van / heli / 106 / 096 / 914 / now / ammo / test >"
						), false);
				}
			}
		}
	}
}