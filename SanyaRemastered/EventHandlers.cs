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
using Scp914;

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
		private bool Senderdisabled = false;
		internal async Task SenderAsync()
		{
			Log.Debug($"[Infosender_Task] Started.");

			while (true)
			{
				try
				{
					if (SanyaPlugin.Instance.Config.InfosenderIp == "none")
					{
						Log.Info($"[Infosender_Task] Disabled(config:({SanyaPlugin.Instance.Config.InfosenderIp}). breaked.");
						Senderdisabled = true;
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
					udpClient.Send(sendBytes, sendBytes.Length, SanyaPlugin.Instance.Config.InfosenderIp, SanyaPlugin.Instance.Config.InfosenderPort);
					Log.Debug($"[Infosender_Task] {SanyaPlugin.Instance.Config.InfosenderIp}:{SanyaPlugin.Instance.Config.InfosenderPort}");
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
					//ItemCleanup
					if (SanyaPlugin.Instance.Config.ItemCleanup > 0)
					{
						List<GameObject> nowitems = null;

						foreach (var i in ItemCleanupPatch.items)
						{
							if (Time.time - i.Value > SanyaPlugin.Instance.Config.ItemCleanup && i.Key != null)
							{
								if (nowitems == null) nowitems = new List<GameObject>();
								Log.Debug($"[ItemCleanupPatch] Cleanup:{i.Key.transform.position} {Time.time - i.Value} > {SanyaPlugin.Instance.Config.ItemCleanup}");
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
					//SCP-939VoiceChatVision
					if (plugin.Config.Scp939CanSeeVoiceChatting != 0)
					{
						List<ReferenceHub> scp939 = null;
						List<ReferenceHub> humans = new List<ReferenceHub>();
						foreach (var player in ReferenceHub.GetAllHubs().Values)
						{
							if (player.characterClassManager.CurRole.team != Team.RIP && player.TryGetComponent(out Radio radio) && (radio.g_voice))
							{
								player.footstepSync._visionController.MakeNoise(radio.noiseSource.volume * plugin.Config.Scp939CanSeeVoiceChatting);
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
		private readonly int grenade_pickup_mask = 1049088;
		private int prevMaxAHP = 0;

		public void OnWaintingForPlayers()
		{
			loaded = true;

			if (sendertask?.Status != TaskStatus.Running && sendertask?.Status != TaskStatus.WaitingForActivation
				&& plugin.Config.InfosenderIp != "none" && plugin.Config.InfosenderPort != -1)
				sendertask = SenderAsync().StartSender();

			roundCoroutines.Add(Timing.RunCoroutine(EverySecond(), Segment.FixedUpdate));
			roundCoroutines.Add(Timing.RunCoroutine(FixedUpdate(), Segment.FixedUpdate));

			PlayerDataManager.playersData.Clear();
			ItemCleanupPatch.items.Clear();
			Coroutines.isAirBombGoing = false;


			if (SanyaPlugin.Instance.Config.TeslaRange != 5.5f)
			{
				foreach (var tesla in UnityEngine.Object.FindObjectsOfType<TeslaGate>())
				{
					tesla.sizeOfTrigger = SanyaPlugin.Instance.Config.TeslaRange;
				}
			}
			
			Log.Info($"[OnWaintingForPlayers] Waiting for Players...");
		}

		public void OnRoundStart()
		{
			Log.Info($"[OnRoundStart] Round Start!");

			if (SanyaPlugin.Instance.Config.ClassD_container_locked)
			{
				Coroutines.StartContainClassD();
			}
		}

		public void OnRoundEnd(RoundEndedEventArgs ev)
		{
			Log.Info($"[OnRoundEnd] Round Ended.{ev.TimeToRestart}");

			if (SanyaPlugin.Instance.Config.DataEnabled)
			{
				foreach (Player Eplayer in Player.List)
				{
					ReferenceHub player = Eplayer.ReferenceHub;
					if (string.IsNullOrEmpty(player.characterClassManager.UserId)) continue;

					if (PlayerDataManager.playersData.ContainsKey(player.characterClassManager.UserId))
					{
						if (player.characterClassManager.CurClass == RoleType.Spectator)
						{
							PlayerDataManager.playersData[player.characterClassManager.UserId].AddExp(SanyaPlugin.Instance.Config.Level_exp_other);
						}
						else
						{
							PlayerDataManager.playersData[player.characterClassManager.UserId].AddExp(SanyaPlugin.Instance.Config.LevelExpWin);
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

			if (SanyaPlugin.Instance.Config.GodmodeAfterEndround)
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

			if (SanyaPlugin.Instance.Config.CassieSubtitle)
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

			if (SanyaPlugin.Instance.Config.CassieSubtitle)
			{
				Methods.SendSubtitle(Subtitles.AlphaWarheadCancel, 7);
			}

			if (SanyaPlugin.Instance.Config.CloseDoorsOnNukecancel)
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

			if (SanyaPlugin.Instance.Config.OutsidezoneTerminationTimeAfterNuke != 0)
			{
				roundCoroutines.Add(Timing.RunCoroutine(Coroutines.AirSupportBomb(false, SanyaPlugin.Instance.Config.OutsidezoneTerminationTimeAfterNuke)));
			}
		}
		public void OnAnnounceDecont(AnnouncingDecontaminationEventArgs ev)
		{
			Log.Debug($"[OnAnnounceDecont] {ev.Id} {DecontaminationController.Singleton._stopUpdating}");

			if (SanyaPlugin.Instance.Config.CassieSubtitle)
			{

				ev.IsGlobal = true;

				switch (ev.Id)
				{
					case 0:
						{
							foreach (Player player in Player.List)
							{
								if (player.CurrentRoom.Name.StartsWith("LCZ"))
									Methods.SendSubtitle(player, Subtitles.DecontaminationInit, 20);
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

			if ((SanyaPlugin.Instance.Config.KickSteamLimited || SanyaPlugin.Instance.Config.KickVpn) && !ev.UserId.Contains("@northwood", StringComparison.InvariantCultureIgnoreCase))
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

			if (SanyaPlugin.Instance.Config.DataEnabled && !PlayerDataManager.playersData.ContainsKey(ev.UserId))
			{
				PlayerDataManager.playersData.Add(ev.UserId, PlayerDataManager.LoadPlayerData(ev.UserId));
			}

			if (SanyaPlugin.Instance.Config.KickVpn)
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

			if (SanyaPlugin.Instance.Config.KickSteamLimited && ev.UserId.Contains("@steam", StringComparison.InvariantCultureIgnoreCase))
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

			if (!string.IsNullOrEmpty(SanyaPlugin.Instance.Config.MotdMessage))
			{
				Methods.SendSubtitle(SanyaPlugin.Instance.Config.MotdMessage.Replace("[name]", ev.Player.Nickname), 10, ev.Player.ReferenceHub);
			}

			if (SanyaPlugin.Instance.Config.DataEnabled
				&& SanyaPlugin.Instance.Config.LevelEnabled
				&& PlayerDataManager.playersData.TryGetValue(ev.Player.UserId, out PlayerData data))
			{
				Timing.RunCoroutine(Coroutines.GrantedLevel(ev.Player.ReferenceHub, data), Segment.FixedUpdate);
			}

			if (SanyaPlugin.Instance.Config.DisableAllChat)
			{
				if (!(SanyaPlugin.Instance.Config.DisableChatBypassWhitelist && WhiteList.IsOnWhitelist(ev.Player.UserId)))
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

			if (SanyaPlugin.Instance.Config.DataEnabled && !string.IsNullOrEmpty(ev.Player.UserId))
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

			if (SanyaPlugin.Instance.Config.Scp079ExtendEnabled && ev.NewRole == RoleType.Scp079)
			{
				roundCoroutines.Add(Timing.CallDelayed(10f, () => ev.Player.ReferenceHub.SendTextHint(Subtitles.Extend079First, 10)));
			}

			if (SanyaPlugin.Instance.Config.Scp049RecoveryAmount > 0 && ev.NewRole == RoleType.Scp0492)
			{
				foreach (Player Exiledscp049 in Player.List)
				{
					if (Exiledscp049.Role == RoleType.Scp049)
						Exiledscp049.ReferenceHub.playerStats.HealHPAmount(SanyaPlugin.Instance.Config.Scp049RecoveryAmount);
				}
			}
			if (SanyaPlugin.Instance.Config.Scp049_add_time_res_success && ev.NewRole == RoleType.Scp0492)
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
			ev.Player.ReferenceHub.fpc.staminaController.RemainingStamina += 1;
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
				if (SanyaPlugin.Instance.Config.HitmarkGrenade
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
						ev.Amount *= SanyaPlugin.Instance.Config.UspDamageMultiplierScp;
					}
					else
					{
						ev.Amount *= SanyaPlugin.Instance.Config.UspDamageMultiplierHuman;
						ev.Target.ReferenceHub.playerEffectsController.EnableEffect<Disabled>(10f);
					}
				}

				//939Bleeding
				if (SanyaPlugin.Instance.Config.Scp939AttackBleeding && ev.DamageType == DamageTypes.Scp939)
				{
					ev.Target.ReferenceHub.playerEffectsController.EnableEffect<Hemorrhage>(SanyaPlugin.Instance.Config.Scp939AttackBleedingTime);
				}

				//HurtBlink173
				if (SanyaPlugin.Instance.Config.Scp173ForceBlinkPercent > 0 && ev.Target.Role == RoleType.Scp173 && UnityEngine.Random.Range(0, 100) < SanyaPlugin.Instance.Config.Scp173ForceBlinkPercent)
				{
					Methods.Blink();
				}

				//SCPsDivisor
				if (ev.DamageType != DamageTypes.Wall
					&& ev.DamageType != DamageTypes.Nuke
					&& ev.DamageType != DamageTypes.Decont)
				{
					switch (ev.Target.Role)
					{
						case RoleType.Scp173:
							ev.Amount /= SanyaPlugin.Instance.Config.Scp173DamageDivisor;
							break;
						case RoleType.Scp106:
							if (ev.DamageType == DamageTypes.Grenade) ev.Amount /= SanyaPlugin.Instance.Config.Scp106GrenadeDivisor;
							if (ev.DamageType != DamageTypes.MicroHid
								&& ev.DamageType != DamageTypes.Tesla)
								ev.Amount /= SanyaPlugin.Instance.Config.Scp106DamageDivisor;
							break;
						case RoleType.Scp049:
							ev.Amount /= SanyaPlugin.Instance.Config.Scp049DamageDivisor;
							break;
						case RoleType.Scp096:
							ev.Amount /= SanyaPlugin.Instance.Config.Scp096DamageDivisor;
							break;
						case RoleType.Scp0492:
							ev.Amount /= SanyaPlugin.Instance.Config.Scp0492DamageDivisor;
							break;
						case RoleType.Scp93953:
						case RoleType.Scp93989:
							ev.Amount /= SanyaPlugin.Instance.Config.Scp939DamageDivisor;
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

			if (SanyaPlugin.Instance.Config.DataEnabled)
			{
				if (!string.IsNullOrEmpty(ev.Killer.UserId)
					&& ev.Target.UserId != ev.Killer.UserId
					&& PlayerDataManager.playersData.ContainsKey(ev.Killer.UserId))
				{
					PlayerDataManager.playersData[ev.Killer.UserId].AddExp(SanyaPlugin.Instance.Config.LevelExpKill);
				}

				if (PlayerDataManager.playersData.ContainsKey(ev.Target.UserId))
				{
					PlayerDataManager.playersData[ev.Target.UserId].AddExp(SanyaPlugin.Instance.Config.LevelExpDeath);
				}
			}

			if (ev.HitInformations.GetDamageType() == DamageTypes.Scp173 && ev.Killer.Role == RoleType.Scp173 && SanyaPlugin.Instance.Config.Scp173RecoveryAmount > 0)
			{
				ev.Killer.ReferenceHub.playerStats.HealHPAmount(SanyaPlugin.Instance.Config.Scp173RecoveryAmount);
			}
			if (ev.HitInformations.GetDamageType() == DamageTypes.Scp096 && ev.Killer.Role == RoleType.Scp096 && SanyaPlugin.Instance.Config.Scp096RecoveryAmount > 0)
			{
				ev.Killer.ReferenceHub.playerStats.HealHPAmount(SanyaPlugin.Instance.Config.Scp096RecoveryAmount);
			}
			if (ev.HitInformations.GetDamageType() == DamageTypes.Scp939 && (ev.Killer.Role == RoleType.Scp93953 || ev.Killer.Role == RoleType.Scp93989) && SanyaPlugin.Instance.Config.Scp939RecoveryAmount > 0)
			{
				ev.Killer.ReferenceHub.playerStats.HealHPAmount(SanyaPlugin.Instance.Config.Scp939RecoveryAmount);
				ev.Target.ReferenceHub.inventory.Clear();
			}
			if (ev.HitInformations.GetDamageType() == DamageTypes.Scp0492 && ev.Killer.Role == RoleType.Scp0492 && SanyaPlugin.Instance.Config.Scp0492RecoveryAmount > 0)
			{
				ev.Killer.ReferenceHub.playerStats.HealHPAmount(SanyaPlugin.Instance.Config.Scp0492RecoveryAmount);
			}

			if (SanyaPlugin.Instance.Config.HitmarkKilled
				&& ev.Killer.Team != Team.SCP
				&& !string.IsNullOrEmpty(ev.Killer.UserId)
				&& ev.Killer.UserId != ev.Target.UserId)
			{
				Timing.RunCoroutine(Coroutines.BigHitmark(ev.Killer.GameObject.GetComponent<MicroHID>()));
			}

			if (SanyaPlugin.Instance.Config.CassieSubtitle
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
					ticket.Add(SpawnableTeamType.ChaosInsurgency, SanyaPlugin.Instance.Config.Tickets_ci_classd_died_count);
					if (ev.Killer.Team == Team.MTF || ev.Killer.Team == Team.RSC) ticket.Add(SpawnableTeamType.NineTailedFox, SanyaPlugin.Instance.Config.Tickets_mtf_classd_killed_count);
					break;
				case Team.RSC:
					ticket.Add(SpawnableTeamType.NineTailedFox, SanyaPlugin.Instance.Config.Tickets_mtf_scientist_died_count);
					if (ev.Killer.Team == Team.CHI || ev.Killer.Team == Team.CDP) ticket.Add(SpawnableTeamType.NineTailedFox, SanyaPlugin.Instance.Config.Tickets_ci_scientist_killed_count);
					break;
				case Team.MTF:
					if (ev.Killer.Team == Team.SCP) ticket.Add(SpawnableTeamType.NineTailedFox, SanyaPlugin.Instance.Config.Tickets_mtf_killed_by_scp_count);
					break;
				case Team.CHI:
					if (ev.Killer.Team == Team.SCP) ticket.Add(SpawnableTeamType.ChaosInsurgency, SanyaPlugin.Instance.Config.Tickets_ci_killed_by_scp_count);
					break;
			}
		}

		public void OnPocketDimDeath(FailingEscapePocketDimensionEventArgs ev)
		{
			Log.Debug($"[OnPocketDimDeath] {ev.Player.Nickname}");

			if (SanyaPlugin.Instance.Config.DataEnabled)
			{
				foreach (Player player in Player.List)
				{
					if (player.Role == RoleType.Scp106)
					{
						if (PlayerDataManager.playersData.ContainsKey(player.UserId))
						{
							PlayerDataManager.playersData[player.UserId].AddExp(SanyaPlugin.Instance.Config.LevelExpKill);
						}
					}
				}
			}

			if (SanyaPlugin.Instance.Config.Scp106RecoveryAmount > 0)
			{
				foreach (Player player in Player.List)
				{
					if (player.Role == RoleType.Scp106)
					{
						player.ReferenceHub.playerStats.HealHPAmount(SanyaPlugin.Instance.Config.Scp106RecoveryAmount);
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
			if (SanyaPlugin.Instance.Config.TeslaTriggerableTeams.Count == 0
				|| SanyaPlugin.Instance.Config.TeslaTriggerableTeams.Contains(ev.Player.Team))
			{
				if (SanyaPlugin.Instance.Config.TeslaTriggerableDisarmed || ev.Player.CufferId == -1)
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
			Log.Debug($"[OnPlayerDoorInteract] {ev.Player.Nickname}:{ev.Door.DoorName}:{ev.Door.doorType}");

			if (SanyaPlugin.Instance.Config.InventoryKeycardActivation && ev.Player.Team != Team.SCP && !ev.Player.ReferenceHub.serverRoles.BypassMode && !ev.Door.locked)
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
			if (SanyaPlugin.Instance.Config.Scp049_2DontOpenDoorAnd106 && (ev.Player.Role == RoleType.Scp0492 || ev.Player.Role == RoleType.Scp106))
			{
				ev.IsAllowed = false;			
			}

			//Mini fix
			if (ev.Door.DoorName.Contains("CHECKPOINT") && (ev.Door.decontlock || ev.Door.warheadlock) && !ev.Door.isOpen)
			{
				ev.Door.SetStateWithSound(true);
			}
		}

		public void OnPlayerLockerInteract(InteractingLockerEventArgs ev)
		{
			Log.Debug($"[OnPlayerLockerInteract] {ev.Player.Nickname}:{ev.Locker.name}");
			if (SanyaPlugin.Instance.Config.InventoryKeycardActivation)
			{
				foreach (var item in ev.Player.ReferenceHub.inventory.items)
				{
					if (ev.Player.ReferenceHub.inventory.GetItemByID(item.id).permissions.Contains("PEDESTAL_ACC"))
					{
						ev.IsAllowed = true;
					}
				}
			}
			if (SanyaPlugin.Instance.Config.Scp049_2DontOpenDoorAnd106 && (ev.Player.Role == RoleType.Scp0492 || ev.Player.Role == RoleType.Scp106))
			{
				ev.IsAllowed = false;
			}
			if (SanyaPlugin.Instance.Config.Scp939And096DontOpenlockerAndGenerator && (ev.Player.Role == RoleType.Scp93953 || ev.Player.Role == RoleType.Scp93989 || ev.Player.Role == RoleType.Scp096))
			{
				ev.IsAllowed = false;
			}
		}
		public void OnPlayerChangeAnim(SyncingDataEventArgs ev)
		{
			if (ev.Player.IsHost || ev.Player.ReferenceHub.animationController.curAnim == ev.CurrentAnimation) return;

			if (SanyaPlugin.Instance.Config.Scp079ExtendEnabled && ev.Player.Role == RoleType.Scp079)
			{
				if (ev.CurrentAnimation == 1)
					ev.Player.ReferenceHub.SendTextHint(Subtitles.ExtendEnabled, 3);
				else
					ev.Player.ReferenceHub.SendTextHint(Subtitles.ExtendDisabled, 3);
			}

			if (SanyaPlugin.Instance.Config.StaminaLostJump != -1f
				&& ev.CurrentAnimation == 2
				&& ev.Player.ReferenceHub.characterClassManager.IsHuman()
				&& !ev.Player.ReferenceHub.fpc.staminaController._invigorated.Enabled
				&& !ev.Player.ReferenceHub.fpc.staminaController._scp207.Enabled
				)
			{
				ev.Player.ReferenceHub.fpc.staminaController.RemainingStamina -= SanyaPlugin.Instance.Config.StaminaLostJump;
				ev.Player.ReferenceHub.fpc.staminaController._regenerationTimer = 0f;
			}
			if (SanyaPlugin.Instance.Config.StaminaEffect)
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

			if (SanyaPlugin.Instance.Config.StopRespawnAfterDetonated && AlphaWarheadController.Host.detonated)
			{
				ev.Players.Clear();
			}
			if (SanyaPlugin.Instance.Config.GodmodeAfterEndround && !RoundSummary.RoundInProgress())
			{
				ev.Players.Clear();
			}
			if (Coroutines.isAirBombGoing)
			{
				ev.Players.Clear();
			}
		}
		public void OnExplodingGrenade (ExplodingGrenadeEventArgs ev)
		{
			Log.Debug($"[OnExplodingGrenade]  : {ev.Grenade.transform.position}");
			if (SanyaPlugin.Instance.Config.GrenadeEffect)
			{
				foreach (var ply in Player.List)
				{
					var dis = Vector3.Distance(ev.Grenade.transform.position, ply.Position);
					if (dis <= 4)
					{ 
						ply.ReferenceHub.playerEffectsController.EnableEffect<Deafened>(10f,true);
					}
				}
			}
		}
		public void OnActivatingWarheadPanel(ActivatingWarheadPanelEventArgs ev)
		{
			Log.Debug($"[OnActivatingWarheadPanel] Nickname : {ev.Player.Nickname} Permissions : {ev.Permissions} Type : {ev.GetType()}  Allowed : {ev.IsAllowed}");
			if (SanyaPlugin.Instance.Config.Scp049_2DontOpenDoorAnd106 && (ev.Player.Role == RoleType.Scp0492 || ev.Player.Role == RoleType.Scp106))
			{
				ev.IsAllowed = false;
			}
			if (SanyaPlugin.Instance.Config.Scp939And096DontOpenlockerAndGenerator && (ev.Player.Role == RoleType.Scp93953 || ev.Player.Role == RoleType.Scp93989 || ev.Player.Role == RoleType.Scp096))
			{
				ev.IsAllowed = false;
			}
		}
		public void OnGeneratorUnlock(UnlockingGeneratorEventArgs ev)
		{
			Log.Debug($"[OnGeneratorUnlock] {ev.Player.Nickname} -> {ev.Generator.CurRoom}");
			if (SanyaPlugin.Instance.Config.InventoryKeycardActivation && !ev.Player.ReferenceHub.serverRoles.BypassMode)
			{
				foreach (var item in ev.Player.ReferenceHub.inventory.items)
				{
					if (ev.Player.ReferenceHub.inventory.GetItemByID(item.id).permissions.Contains("ARMORY_LVL_2"))
					{
						ev.IsAllowed = true;
					}
				}
			}
			if (ev.IsAllowed && SanyaPlugin.Instance.Config.GeneratorUnlockOpen)
			{
				ev.Generator._doorAnimationCooldown = 1.5f;
				ev.Generator.NetworkisDoorOpen = true;
				ev.Generator.RpcDoSound(true);
			}
		}

		public void OnGeneratorOpen(OpeningGeneratorEventArgs ev)
		{
			Log.Debug($"[OnGeneratorOpen] {ev.Player.Nickname} -> {ev.Generator.CurRoom}");
			if (ev.Generator.prevFinish && SanyaPlugin.Instance.Config.GeneratorFinishLock)
			{
				ev.IsAllowed = false;
			}
			if (SanyaPlugin.Instance.Config.Scp049_2DontOpenDoorAnd106 && (ev.Player.Role == RoleType.Scp0492 || ev.Player.Role == RoleType.Scp106))
			{
				ev.IsAllowed = false;
				ev.Player.ReferenceHub.SendTextHint("Fait pas ça stp", 3);
			}
			if (SanyaPlugin.Instance.Config.Scp939And096DontOpenlockerAndGenerator && (ev.Player.Role == RoleType.Scp93953 || ev.Player.Role == RoleType.Scp93989 || ev.Player.Role == RoleType.Scp096))
			{
				ev.IsAllowed = false;
				ev.Player.ReferenceHub.SendTextHint("Fait pas ça stp", 3);
			}
		}

		public void OnGeneratorClose(ClosingGeneratorEventArgs ev)
		{
			Log.Debug($"[OnGeneratorClose] {ev.Player.Nickname} -> {ev.Generator.CurRoom}");
			if (ev.IsAllowed && ev.Generator.isTabletConnected && SanyaPlugin.Instance.Config.GeneratorActivatingClose)
			{
				ev.IsAllowed = false;
			}
			if (SanyaPlugin.Instance.Config.Scp049_2DontOpenDoorAnd106 && (ev.Player.Role == RoleType.Scp0492 || ev.Player.Role == RoleType.Scp106))
			{
				ev.IsAllowed = false;
				ev.Player.ReferenceHub.SendTextHint("Fait pas ça stp", 3);
			}
			if (SanyaPlugin.Instance.Config.Scp939And096DontOpenlockerAndGenerator && (ev.Player.Role == RoleType.Scp93953 || ev.Player.Role == RoleType.Scp93989 || ev.Player.Role == RoleType.Scp096))
			{
				ev.IsAllowed = false;
				ev.Player.ReferenceHub.SendTextHint("Fait pas ça stp", 3);
			}
		}
		public void OnEjectingGeneratorTablet(InsertingGeneratorTabletEventArgs ev)
		{
			Log.Debug($"[OnEjectingGeneratorTablet] {ev.Player.Nickname} -> {ev.Generator.CurRoom}");
			if (SanyaPlugin.Instance.Config.Scp049_2DontOpenDoorAnd106 && (ev.Player.Role == RoleType.Scp0492 || ev.Player.Role == RoleType.Scp106))
			{
				ev.IsAllowed = false;
				ev.Player.ReferenceHub.SendTextHint("Fait pas ça stp", 3);

			}
			if (SanyaPlugin.Instance.Config.Scp939And096DontOpenlockerAndGenerator && (ev.Player.Role == RoleType.Scp93953 || ev.Player.Role == RoleType.Scp93989 || ev.Player.Role == RoleType.Scp096))
			{
				ev.IsAllowed = false;
				ev.Player.ReferenceHub.SendTextHint("Fait pas ça stp", 3);
			}
		}
		public void OnGeneratorInsert(InsertingGeneratorTabletEventArgs ev)
		{
			Log.Debug($"[OnGeneratorInsert] {ev.Player.Nickname} -> {ev.Generator.CurRoom}");
		}

		public void OnGeneratorFinish(GeneratorActivatedEventArgs ev)
		{
			Log.Debug($"[OnGeneratorFinish] {ev.Generator.CurRoom}");
			if (SanyaPlugin.Instance.Config.GeneratorFinishLock) ev.Generator.NetworkisDoorOpen = false;

			int curgen = Generator079.mainGenerator.NetworktotalVoltage + 1;
			if (SanyaPlugin.Instance.Config.CassieSubtitle && !Generator079.mainGenerator.forcedOvercharge)
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
		}
		public void OnInteractingElevator(InteractingElevatorEventArgs ev)
		{
			Log.Debug($"[OnInteractingElevator] Player : {ev.Player}  Type : {ev.Elevator.GetType()}");
			if (SanyaPlugin.Instance.Config.Scp049_2DontOpenDoorAnd106 && (ev.Player.Role == RoleType.Scp0492))
			{
				ev.IsAllowed = false;
			}
		}
		public void OnPlacingDecal(PlacingDecalEventArgs ev)
		{
			Log.Debug($"[OnPlacingDecal] position : {ev.Position} Owner: {ev.Owner} Type: {ev.Type}");
			if (SanyaPlugin.Instance.Config.Coroding106)
			{
				List<Vector3> DecalList = new List<Vector3>{ev.Position};
				while (SanyaPlugin.Instance.Config.Coroding106)
				{
					foreach (var ply in Player.List)
					{
						foreach (var Decal in DecalList)
						{
							var dis = Vector3.Distance(ply.Position, Decal);
							if (dis <= 1)
							{
								ply.ReferenceHub.playerEffectsController.EnableEffect<SinkHole>(0.1f);
								ply.ReferenceHub.playerEffectsController.EnableEffect<Corroding>(0.1f);
							}
						}
					}
				}
			}
		}
		public void On079LevelGain(GainingLevelEventArgs ev)
		{
			Log.Debug($"[On079LevelGain] {ev.Player.Nickname} : {ev.NewLevel}");

			if (SanyaPlugin.Instance.Config.Scp079ExtendEnabled)
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
			Log.Debug($"[On106MakePortal] {ev.Player.Nickname}:{ev.Position}");
		}

		public void On106Teleport(TeleportingEventArgs ev)
		{
			Log.Debug($"[On106Teleport] {ev.Player.Nickname}:{ev.PortalPosition}");
		//	if (SanyaPlugin.instance.Config.Scp106PortalExtensionEnabled) ;
		}
		public void OnEnraging(EnragingEventArgs ev)
		{
			Log.Debug($"[On106MakePortal] {ev.Player.Nickname}");

		}
	public void On914Upgrade(UpgradingItemsEventArgs ev)
		{
			Log.Debug($"[On914Upgrade] {ev.KnobSetting} Players:{ev.Players.Count} Items:{ev.Items.Count}");

			if (SanyaPlugin.Instance.Config.Scp914Effect)
			{
				switch (ev.KnobSetting)
				{
					case Scp914Knob.Rough:
						foreach (var player in ev.Players)
						{
							var Death = new PlayerStats.HitInfo(99999, "Scp-914", DamageTypes.RagdollLess, 0);
							player.ReferenceHub.playerStats.HurtPlayer(Death, player.ReferenceHub.gameObject);
							if (player.Team != Team.SCP)
							player.ReferenceHub.SendTextHint("Un cadavre gravement mutilé a été trouvé à l'intérieur de SCP-914. Le sujet a évidemment été affiné par le SCP-914 sur le réglage Rough.", 30);
						}
						break;
					case Scp914Knob.Coarse:
						foreach (var player in ev.Players)
						{
							if (player.Role == RoleType.Scp93953 || player.Role == RoleType.Scp93989)
							{
								var Death = new PlayerStats.HitInfo(99999, "Scp-914", DamageTypes.RagdollLess, 0);
								player.ReferenceHub.playerStats.HurtPlayer(Death, player.ReferenceHub.gameObject);
							}
							if (player.Team != Team.SCP)
							{
								var Hit = new PlayerStats.HitInfo(70, "Scp-914", DamageTypes.RagdollLess, 0);
								player.ReferenceHub.playerStats.HurtPlayer(Hit, player.ReferenceHub.gameObject);
								player.ReferenceHub.playerEffectsController.EnableEffect<Hemorrhage>();
								player.ReferenceHub.playerEffectsController.EnableEffect<Bleeding>();
								player.ReferenceHub.playerEffectsController.EnableEffect<Disabled>();
								player.ReferenceHub.SendTextHint("Vous remarquez d'innombrables petites incisions dans votre corps.", 10);
							}
						}
						break;
					case Scp914Knob.OneToOne:
						foreach (var player in ev.Players)
						{
							if (player.Role == RoleType.Scp93953)
							{
								var Health = player.Health;
								player.SetRole(RoleType.Scp93989);
								var Hit = new PlayerStats.HitInfo(player.MaxHealth - Health, "Scp-914", DamageTypes.RagdollLess, 0);
								player.ReferenceHub.playerStats.HurtPlayer(Hit, player.ReferenceHub.gameObject);
								break;
							}
							if (player.Role == RoleType.Scp93989)
							{
								var Health = player.Health;
								player.SetRole(RoleType.Scp93953);
								var Hit = new PlayerStats.HitInfo(player.MaxHealth - Health, "Scp-914", DamageTypes.RagdollLess, 0);
								player.ReferenceHub.playerStats.HurtPlayer(Hit, player.ReferenceHub.gameObject);
								break;
							}
							if (player.Team != Team.SCP)
							{
								{
									//Reverse ammo
									var Nato556 = player.GetAmmo(Exiled.API.Enums.AmmoType.Nato556);
									var Nato762 = player.GetAmmo(Exiled.API.Enums.AmmoType.Nato762);
									var Nato9 = player.GetAmmo(Exiled.API.Enums.AmmoType.Nato9);
									player.SetAmmo(Exiled.API.Enums.AmmoType.Nato556, Nato762);
									player.SetAmmo(Exiled.API.Enums.AmmoType.Nato762, Nato9);
									player.SetAmmo(Exiled.API.Enums.AmmoType.Nato9, Nato556);
								}
								{

								}
							}
						}
						break;
					case Scp914Knob.Fine:
						foreach (var player in ev.Players)
						{
							player.ReferenceHub.fpc.sprintToggle = true;
							player.ReferenceHub.fpc.effectScp207.Intensity += 4;
						}
						break;
					case Scp914Knob.VeryFine:
						foreach (var player in ev.Players)
						{
							var Death = new PlayerStats.HitInfo(99999, "Scp-914", DamageTypes.RagdollLess, 0);
							player.ReferenceHub.playerStats.HurtPlayer(Death, player.ReferenceHub.gameObject);
							player.ReferenceHub.SendTextHint("L'analyse chimique de la substance a l'intérieur de SCP-914 reste non concluante.", 30);
						}
						break;
				}
			}

		}
		public void OnShoot(ShootingEventArgs ev)
		{
			Log.Debug($"[OnShoot] {ev.Shooter.Nickname} -{ev.Position}-> {ev.Target?.name}");

			if ((SanyaPlugin.Instance.Config.Grenade_shoot_fuse || SanyaPlugin.Instance.Config.Item_shoot_move)
				&& ev.Position != Vector3.zero
				&& Physics.Linecast(ev.Shooter.Position, ev.Position, out RaycastHit raycastHit, grenade_pickup_mask))
			{
				if (SanyaPlugin.Instance.Config.Item_shoot_move)
				{
					var pickup = raycastHit.transform.GetComponentInParent<Pickup>();
					if (pickup != null && pickup.Rb != null)
					{
						pickup.Rb.AddExplosionForce(Vector3.Distance(ev.Position, ev.Shooter.Position), ev.Shooter.Position, 500f, 3f, ForceMode.Impulse);
					}
				}

				if (SanyaPlugin.Instance.Config.Grenade_shoot_fuse)
				{
					var grenade = raycastHit.transform.GetComponentInParent<FragGrenade>();
					if (grenade != null)
					{
						grenade.NetworkfuseTime = 0.1f;
					}
				}
			}
			if (SanyaPlugin.Instance.Config.StaminaLostLogicer > 0f
				&& ev.Shooter.ReferenceHub.characterClassManager.IsHuman()
				&& ItemType.GunLogicer == ev.Shooter.CurrentItem.id
				&& ev.IsAllowed
				&& !ev.Shooter.ReferenceHub.fpc.staminaController._invigorated.Enabled
				&& !ev.Shooter.ReferenceHub.fpc.staminaController._scp207.Enabled
				)
			{

			}
		}
		public void OnCommand(SendingConsoleCommandEventArgs ev)
		{
			string[] args = ev.Arguments.ToArray();
			Log.Debug($"[OnCommand] Player : {ev.Player} command:{ev.Name} args:{args.Length}");
			string effort = $"{ev.Name} ";
			foreach (string s in ev.Arguments)
				effort += $"{s} ";

			args = effort.Split(' ');
			if (args[0] == "nuke")
			{
				
				/*
				if (RoundSummary.singleton.CountRole(RoleType.Scp106) != 0)
				{
					ev.Player.Broadcast(10, "SCP-106");
				}
				if (RoundSummary.singleton.CountRole(RoleType.Scientist) != 0)
				{
					ev.Player.Broadcast(10, "Scientifique");
				}
				if (player)*/
			}
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
								ReturnStr = SanyaPlugin.Instance.Config.GetConfigs();
								break;
							}
						case "reload":
							{
								if (!perm.CheckPermission("sanya.reload"))
								{
									ev.Sender.RemoteAdminMessage("Permission denied.");
									return;
								}
								SanyaPlugin.Instance.Config.GetConfigs();
								if (SanyaPlugin.Instance.Config.KickVpn) ShitChecker.LoadLists();
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