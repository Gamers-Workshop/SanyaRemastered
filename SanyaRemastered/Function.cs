using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using Hints;
using MEC;
using Mirror;
using RemoteAdmin;
using SanyaPlugin.Data;
using SanyaRemastered.Data;
using UnityEngine;
using Dissonance.Integrations.MirrorIgnorance;
using UnityEngine.Networking;
using Utf8Json;
using Respawning;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using System.Threading;
using Object = UnityEngine.Object;
using SanyaRemastered;
using CustomPlayerEffects;

namespace SanyaPlugin.Functions
{
	internal static class PlayerDataManager
	{
		public static Dictionary<string, PlayerData> playersData = new Dictionary<string, PlayerData>();

		public static PlayerData LoadPlayerData(string userid)
		{
			string targetuseridpath = Path.Combine(SanyaPlugin.Instance.Config.DataDirectory, $"{userid}.txt");
			if (!Directory.Exists(SanyaPlugin.Instance.Config.DataDirectory)) Directory.CreateDirectory(SanyaPlugin.Instance.Config.DataDirectory);
			if (!File.Exists(targetuseridpath)) return new PlayerData(DateTime.Now, userid, true, 0, 0, 0);
			else return ParsePlayerData(targetuseridpath);
		}

		public static void SavePlayerData(PlayerData data)
		{
			string targetuseridpath = Path.Combine(SanyaPlugin.Instance.Config.DataDirectory, $"{data.userid}.txt");

			if (!Directory.Exists(SanyaPlugin.Instance.Config.DataDirectory)) Directory.CreateDirectory(SanyaPlugin.Instance.Config.DataDirectory);

			string[] textdata = new string[] {
				data.lastUpdate.ToString("yyyy-MM-ddTHH:mm:sszzzz"),
				data.userid,
				data.limited.ToString(),
				data.level.ToString(),
				data.exp.ToString(),
				data.playingcount.ToString()
			};

			File.WriteAllLines(targetuseridpath, textdata);
		}

		private static PlayerData ParsePlayerData(string path)
		{
			var text = File.ReadAllLines(path);
			return new PlayerData(
				DateTime.Parse(text[0]),
				text[1],
				bool.Parse(text[2]),
				int.Parse(text[3]),
				int.Parse(text[4]),
				int.Parse(text[5])
				);
		}
	}

	internal static class ShitChecker
	{
		private static readonly string whitelist_path = Path.Combine(SanyaPlugin.Instance.Config.DataDirectory, "VPN-Whitelist.txt");
		public static HashSet<IPAddress> whitelist = new HashSet<IPAddress>();
		private static readonly string blacklist_path = Path.Combine(SanyaPlugin.Instance.Config.DataDirectory, "VPN-Blacklist.txt");
		public static HashSet<IPAddress> blacklist = new HashSet<IPAddress>();

		/*public static IEnumerator<float> CheckVPN(PreAuthenticatingEventArgs ev)
		{
			IPAddress address = ev.Request.RemoteEndPoint.Address;

			if (IsWhiteListed(address) || IsBlacklisted(address))
			{
				Log.Debug($"[VPNChecker] Already Checked:{address}", SanyaPlugin.Instance.Config.IsDebugged);
				yield break;
			}

			using (UnityWebRequest unityWebRequest = UnityWebRequest.Get($"https://v2.api.iphub.info/ip/{address}"))
			{
				unityWebRequest.SetRequestHeader("X-Key", SanyaPlugin.Instance.Config.KickVpnApikey);
				yield return Timing.WaitUntilDone(unityWebRequest.SendWebRequest());
				if (!unityWebRequest.isNetworkError)
				{
					var data = JsonSerializer.Deserialize<VPNData>(unityWebRequest.downloadHandler.text);

					Log.Info($"[VPNChecker] Checking:{address}:{ev.UserId} ({data.CountryCode}/{data.Isp})");

					if (data.Block == 0 || data.Block == 2)
					{
						Log.Info($"[VPNChecker] Passed:{address} UserId:{ev.UserId}");
						AddWhitelist(address);
						yield break;
					}
					else if (data.Block == 1)
					{
						Log.Info($"[VPNChecker] VPN Detected:{address} UserId:{ev.UserId}");
						AddBlacklist(address);

						var player = Player.Get(ev.UserId);
						if (player != null)
						{
							ServerConsole.Disconnect(player.Connection, Subtitles.VPNKickMessage);
						}
						if (!EventHandlers.kickedbyChecker.ContainsKey(ev.UserId))
							EventHandlers.kickedbyChecker.Add(ev.UserId, "vpn");
						yield break;
					}
					else
					{
						Log.Error($"[VPNChecker] Error({unityWebRequest.responseCode}):block == {data.Block}");
					}
				}
				else
				{
					Log.Error($"[VPNChecker] Error({unityWebRequest.responseCode}):{unityWebRequest.error}");
					yield break;
				}
			}
		}
		*/
		public static IEnumerator<float> CheckIsLimitedSteam(string userid)
		{
			PlayerData data = null;

			string xmlurl = string.Concat(
				"https://steamcommunity.com/profiles/",
				userid.Replace("@steam", string.Empty),
				"?xml=1"
			);
			using (UnityWebRequest unityWebRequest = UnityWebRequest.Get(xmlurl))
			{
				yield return Timing.WaitUntilDone(unityWebRequest.SendWebRequest());
				if (!unityWebRequest.isNetworkError)
				{
					XmlReaderSettings xmlReaderSettings = new XmlReaderSettings() { IgnoreComments = true, IgnoreWhitespace = true };
					XmlReader xmlReader = XmlReader.Create(new MemoryStream(unityWebRequest.downloadHandler.data), xmlReaderSettings);
					while (xmlReader.Read())
					{
						if (xmlReader.ReadToFollowing("isLimitedAccount"))
						{
							string isLimited = xmlReader.ReadElementContentAsString();
							if (isLimited == "0")
							{
								Log.Info($"[SteamCheck] OK:{userid}");
								if (data != null)
								{
									data.limited = false;
									PlayerDataManager.SavePlayerData(data);
								}
								yield break;
							}
							else
							{
								Log.Warn($"[SteamCheck] NG:{userid}");
								var player = Player.Get(userid);
								if (player != null)
								{
									ServerConsole.Disconnect(player.Connection, Subtitles.LimitedKickMessage);
								}

								if (!EventHandlers.kickedbyChecker.ContainsKey(userid))
									EventHandlers.kickedbyChecker.Add(userid, "steam");

								yield break;
							}
						}
						else
						{
							Log.Warn($"[SteamCheck] Falied(NoProfile):{userid}");
							var player = Player.Get(userid);
							if (player != null)
							{
								ServerConsole.Disconnect(player.Connection, Subtitles.NoProfileKickMessage);
							}
							if (!EventHandlers.kickedbyChecker.ContainsKey(userid))
								EventHandlers.kickedbyChecker.Add(userid, "steam");
							yield break;
						}
					}
				}
				else
				{
					Log.Error($"[SteamCheck] Failed(NetworkError):{userid}:{unityWebRequest.error}");
					yield break;
				}
			}
			yield break;
		}

		public static void LoadLists()
		{
			whitelist.Clear();
			blacklist.Clear();

			if (!File.Exists(whitelist_path))
				File.WriteAllText(whitelist_path, null);
			if (!File.Exists(blacklist_path))
				File.WriteAllText(blacklist_path, null);

			foreach (var line in File.ReadAllLines(whitelist_path))
			{
				if (IPAddress.TryParse(line, out var address))
				{
					whitelist.Add(address);
				}
			}

			foreach (var line2 in File.ReadAllLines(blacklist_path))
			{
				if (IPAddress.TryParse(line2, out var address2))
				{
					blacklist.Add(address2);
				}
			}
		}

		public static void AddWhitelist(IPAddress address)
		{
			whitelist.Add(address);
			using (StreamWriter writer = File.AppendText(whitelist_path))
			{
				writer.WriteLine(address);
			}
		}

		public static bool IsWhiteListed(IPAddress address)
		{
			return whitelist.Contains(address);
		}

		public static void AddBlacklist(IPAddress address)
		{
			blacklist.Add(address);
			using (StreamWriter writer = File.AppendText(blacklist_path))
			{
				writer.WriteLine(address);
			}
		}

		public static bool IsBlacklisted(IPAddress address)
		{
			return blacklist.Contains(address);
		}
	}

	internal static class Coroutines
	{
		public static bool isAirBombGoing = false;
		public static bool ContainClassD = false;

		public static IEnumerator<float> BigHitmark(MicroHID microHID)
		{
			yield return Timing.WaitForSeconds(0.1f);
			microHID.TargetSendHitmarker(false);
			yield break;
		}
		public static IEnumerator<float> StartContainClassD(bool stop, float TimeLock = 0)
		{
			if (stop)
			{
				ContainClassD = false;
				foreach (var door in UnityEngine.Object.FindObjectsOfType<Door>())
				{
					if (door.name.Contains("PrisonDoor"))
					{
						door.SetLock(false);
						yield break;
					}
				}
			}
			else 
			{
				ContainClassD = true;
				while (ContainClassD)
				{
					foreach (var door in UnityEngine.Object.FindObjectsOfType<Door>())
					{
						if (door.name.Contains("PrisonDoor"))
						{
							door.SetLock(true);
							yield return Timing.WaitForSeconds(TimeLock);
							door.SetLock(false);
							yield break;
						}
					}
				}
			}
		}
		public static IEnumerator<float> AirSupportBomb(bool stop, int timewait = 0, float TimeEnd = -1)
		{
			if (isAirBombGoing && stop)
			{
				isAirBombGoing = false;
				RespawnEffectsController.PlayCassieAnnouncement($"The Outside Zone emergency termination sequence as been stop .", false, true);
				if (SanyaPlugin.Instance.Config.CassieSubtitle)
				{
					Methods.SendSubtitle(Subtitles.AirbombStop, 10);
				}
				Log.Info("[AirSupportBomb] The AirBomb as stop");
				yield break;
			}
			else if (isAirBombGoing && !stop)
			{
				Log.Info("[AirSupportBomb] The AirBomb as stop");
				yield break;
			}
			isAirBombGoing = true;
			while (timewait >= 0)
			{
				Log.Debug("Démarage AirSupport timewait");
				if (timewait == 60f || timewait == 120f || timewait == 300f || timewait == 600f || timewait == 1800f || timewait == 3600f)
				{
					RespawnEffectsController.PlayCassieAnnouncement($"Alert . The Outside Zone emergency termination sequence activated in t minus {timewait / 60} minutes .", false, true);
					if (SanyaPlugin.Instance.Config.CassieSubtitle)
					{
						Methods.SendSubtitle(Subtitles.AirbombStartingWaitMinutes.Replace("{0}", (timewait / 60).ToString()), 10);
					}
					Log.Debug($"Time wait {timewait / 60} seconds");
				}
				else if (timewait == 30f)
				{
					RespawnEffectsController.PlayCassieAnnouncement($"Alert . The Outside Zone emergency termination sequence activated in t minus 30 seconds .", false, true);
					if (SanyaPlugin.Instance.Config.CassieSubtitle)
					{
						Methods.SendSubtitle(Subtitles.AirbombStartingWait30s, 10);
					}
					Log.Debug("reste 30 seconde");
				}
				else if (timewait == 0)
				{
					Log.Debug("reste 0 seconde");
					break;
				}
				if (!isAirBombGoing)
				{
					RespawnEffectsController.PlayCassieAnnouncement($"The Outside Zone emergency termination sequence as been stop .", false, true);
					if (SanyaPlugin.Instance.Config.CassieSubtitle)
					{
						Methods.SendSubtitle(Subtitles.AirbombStop, 10);
					}
					Log.Info($"[AirSupportBomb] The AirBomb as stop");
					yield break;
				}
				Log.Debug("-1 seconde");
				timewait--;
				yield return Timing.WaitForSeconds(1);
			}
			if (isAirBombGoing)
			{
				Log.Info($"[AirSupportBomb] booting...");
				RespawnEffectsController.PlayCassieAnnouncement("danger . outside zone emergency termination sequence activated .", false, true);
				if (SanyaPlugin.Instance.Config.CassieSubtitle)
				{
					Methods.SendSubtitle(Subtitles.AirbombStarting, 10);
				}
				yield return Timing.WaitForSeconds(5f);
				Log.Info($"[AirSupportBomb] charging...");
				{
					int waitforready = 5;
					while (waitforready >= 0)
					{
						Methods.PlayAmbientSound(7);
						waitforready--;
						if (!isAirBombGoing)
						{
							yield break;
						}
						yield return Timing.WaitForSeconds(1f);
					}
				}
				Log.Info($"[AirSupportBomb] throwing...");
				while (isAirBombGoing)
				{
					List<Vector3> randampos = OutsideRandomAirbombPos.Load().OrderBy(x => Guid.NewGuid()).ToList();
					foreach (var pos in randampos)
					{
						Methods.SpawnGrenade(pos, false, 0.1f);
						yield return Timing.WaitForSeconds(0.2f);
					}
					if (TimeEnd != -1)
					{
						if (TimeEnd <= Time.time)
						{
							isAirBombGoing = false;
							Log.Info($"[AirSupportBomb] TimeBombing:{TimeEnd}");
							break;
						}
					}
					yield return Timing.WaitForSeconds(0.25f);
				}
				if (SanyaPlugin.Instance.Config.CassieSubtitle)
				{
					Methods.SendSubtitle(Subtitles.AirbombEnded, 10);
				}
				RespawnEffectsController.PlayCassieAnnouncement("outside zone termination completed .", false, true);
				Log.Info($"[AirSupportBomb] Ended.");
				yield break;
			}
		}
		public static IEnumerator<float> Scp106WalkingThrough(Player player)
		{
			yield return Timing.WaitForOneFrame;

			if (!Physics.Raycast(player.Position, -Vector3.up, 50f, player.ReferenceHub.scp106PlayerScript.teleportPlacementMask))
			{
				player.Position = Map.GetRandomSpawnPoint(RoleType.Scp106);
				yield break;
			}

			Vector3 forward = player.CameraTransform.forward;
			forward.Set(forward.x * 0.1f, 0f, forward.z * 0.1f);

			var hits = Physics.RaycastAll(player.Position, forward, 50f, 1);
			if (hits.Length < 2) yield break;
			if (hits[0].distance > 1f) yield break;

			if (!Physics.Raycast(hits.Last().point + forward, forward * -1f, out var BackHits, 50f, 1)) yield break;

			if (!PlayerMovementSync.FindSafePosition(BackHits.point, out var pos, true)) yield break;
			player.ReferenceHub.playerMovementSync.WhitelistPlayer = true;
			yield return Timing.WaitForOneFrame;
			player.ReferenceHub.fpc.NetworkforceStopInputs = true;
			player.AddItem(ItemType.SCP268);
			player.ReferenceHub.playerEffectsController.EnableEffect<Scp268>();
			player.ReferenceHub.playerEffectsController.EnableEffect<Deafened>();
			player.ReferenceHub.playerEffectsController.ChangeEffectIntensity<Visuals939>(1);
			SanyaPlugin.Instance.Handlers.last106walkthrough.Restart();

			while (true)
			{
				if (player.Position == pos || player.Role != RoleType.Scp106)
				{
					player.ReferenceHub.fpc.NetworkforceStopInputs = false;
					player.ClearInventory();
					player.ReferenceHub.playerEffectsController.DisableEffect<Deafened>();
					player.ReferenceHub.playerEffectsController.DisableEffect<Visuals939>();
					yield return Timing.WaitForOneFrame;
					player.ReferenceHub.playerMovementSync.WhitelistPlayer = false;
					yield break;
				}
				player.Position = Vector3.MoveTowards(player.Position, pos, 0.25f);
				yield return Timing.WaitForOneFrame;
			}
		}
	}
	internal static class Methods
	{
		public static HttpClient httpClient = new HttpClient();

		public static void SpawnGrenade(Vector3 position, bool isFlash = false, float fusedur = -1, ReferenceHub player = null)
		{
			if (player == null) player = ReferenceHub.GetHub(PlayerManager.localPlayer);
			var gm = player.GetComponent<Grenades.GrenadeManager>();
			Grenades.Grenade component = UnityEngine.Object.Instantiate(gm.availableGrenades[isFlash ? (int)GRENADE_ID.FLASH_NADE : (int)GRENADE_ID.FRAG_NADE].grenadeInstance).GetComponent<Grenades.Grenade>();
			if (fusedur != -1) component.fuseDuration = fusedur;
			component.FullInitData(gm, position, Quaternion.Euler(component.throwStartAngle), Vector3.zero, component.throwAngularVelocity);
			NetworkServer.Spawn(component.gameObject);
		}

		public static void Spawn018(ReferenceHub player)
		{
			var gm = player.GetComponent<Grenades.GrenadeManager>();
			var component = UnityEngine.Object.Instantiate(gm.availableGrenades[(int)GRENADE_ID.SCP018_NADE].grenadeInstance).GetComponent<Grenades.Scp018Grenade>();
			component.InitData(gm,
				new Vector3(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f)),
				new Vector3(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f)));
			NetworkServer.Spawn(component.gameObject);
		}

		public static int GetRandomIndexFromWeight(int[] list)
		{
			int sum = 0;

			foreach (int i in list)
			{
				if (i <= 0) continue;
				sum += i;
			}

			int random = UnityEngine.Random.Range(0, sum);
			for (int i = 0; i < list.Length; i++)
			{
				if (list[i] <= 0) continue;

				if (random < list[i])
				{
					return i;
				}
				random -= list[i];
			}
			return -1;
		}

		public static void SendSubtitle(string text, ushort time, ReferenceHub target = null)
		{
			Broadcast brd = PlayerManager.localPlayer.GetComponent<Broadcast>();
			if (target != null)
			{
				brd.TargetClearElements(target.characterClassManager.connectionToClient);
				brd.TargetAddElement(target.characterClassManager.connectionToClient, text, time, Broadcast.BroadcastFlags.Normal);
			}
			else
			{
				brd.RpcClearElements();
				brd.RpcAddElement(text, time, Broadcast.BroadcastFlags.Normal);
			}
		}
		public static void SendSubtitle(Player player, string text, ushort time, ReferenceHub target = null)
		{
			Broadcast brd = player.GameObject.GetComponent<Broadcast>();
			if (target != null)
			{
				brd.TargetClearElements(target.characterClassManager.connectionToClient);
				brd.TargetAddElement(target.characterClassManager.connectionToClient, text, time, Broadcast.BroadcastFlags.Normal);
			}
			else
			{
				brd.RpcClearElements();
				brd.RpcAddElement(text, time, Broadcast.BroadcastFlags.Normal);
			}
		}
		public static void PlayGenerator079sound(byte curr)
		{
			UnityEngine.GameObject.FindObjectOfType<Generator079>().CallRpcNotify(curr);
		}

		public static void PlayAmbientSound(int id)
		{
			PlayerManager.localPlayer.GetComponent<AmbientSoundPlayer>().RpcPlaySound(Mathf.Clamp(id, 0, 32));
		}

		public static void PlayRandomAmbient()
		{
			PlayAmbientSound(UnityEngine.Random.Range(0, 31));
		}

		public static void TargetShake(this ReferenceHub target, bool achieve)
		{
			NetworkWriter writer = NetworkWriterPool.GetWriter();
			writer.WriteBoolean(achieve);
			target.TargetSendRpc(AlphaWarheadController.Host, nameof(AlphaWarheadController.RpcShake), writer);
			NetworkWriterPool.Recycle(writer);
		}

		public static void TargetSendRpc<T>(this ReferenceHub sendto, T target, string rpcName, NetworkWriter writer) where T : NetworkBehaviour
		{
			var msg = new RpcMessage
			{
				netId = target.netId,
				componentIndex = target.ComponentIndex,
				functionHash = target.GetType().FullName.GetStableHashCode() * 503 + rpcName.GetStableHashCode(),
				payload = writer.ToArraySegment()
			};
			sendto?.characterClassManager.connectionToClient.Send(msg, 0);
		}
		public static void SendCustomSync(this Player player, NetworkIdentity behaviorOwner, Type targetType, Action<NetworkWriter> customSyncObject, Action<NetworkWriter> customSyncVar)
		{
			/* 
			
			Example(SyncVar) [TargetOnlyBadge]:
			player.SendCustomSync(player.networkIdentity, typeof(ServerRoles), null, (targetwriter) =>
			{
				targetwriter.WritePackedUInt64(2UL);
				targetwriter.WriteString("test");
			});

			Example(SyncList) [EffectOnlySCP207]:
			player.SendCustomSync(player.ReferenceHub.networkIdentity, typeof(PlayerEffectsController), (writer) => {
				writer.WritePackedUInt64(1ul);								// DirtyObjectsBit
				writer.WritePackedUInt32((uint)1);							// DirtyIndexCount
				writer.WriteByte((byte)SyncList<byte>.Operation.OP_SET);	// Operations
				writer.WritePackedUInt32((uint)0);							// EditIndex
				writer.WriteByte((byte)1);									// Item
			}, null);

			*/
			NetworkWriter writer = NetworkWriterPool.GetWriter();
			NetworkWriter writer2 = NetworkWriterPool.GetWriter();
			MakeCustomSyncWriter(behaviorOwner, targetType, customSyncObject, customSyncVar, writer, writer2);
			NetworkServer.SendToClientOfPlayer(player.ReferenceHub.networkIdentity, new UpdateVarsMessage() { netId = behaviorOwner.netId, payload = writer.ToArraySegment() });
			NetworkWriterPool.Recycle(writer);
			NetworkWriterPool.Recycle(writer2);
		}
		public static void MakeCustomSyncWriter(NetworkIdentity behaviorOwner, Type targetType, Action<NetworkWriter> customSyncObject, Action<NetworkWriter> customSyncVar, NetworkWriter owner, NetworkWriter observer)
		{
			ulong dirty = 0ul;
			ulong dirty_o = 0ul;
			NetworkBehaviour behaviour = null;
			for (int i = 0; i < behaviorOwner.NetworkBehaviours.Length; i++)
			{
				behaviour = behaviorOwner.NetworkBehaviours[i];
				if (behaviour.GetType() == targetType)
				{
					dirty |= 1UL << i;
					if (behaviour.syncMode == SyncMode.Observers) dirty_o |= 1UL << i;
				}
			}
			owner.WritePackedUInt64(dirty);
			observer.WritePackedUInt64(dirty & dirty_o);

			int position = owner.Position;
			owner.WriteInt32(0);
			int position2 = owner.Position;

			if (customSyncObject != null)
				customSyncObject.Invoke(owner);
			else
				behaviour.SerializeObjectsDelta(owner);

			customSyncVar?.Invoke(owner);

			int position3 = owner.Position;
			owner.Position = position;
			owner.WriteInt32(position3 - position2);
			owner.Position = position3;

			if (dirty_o != 0ul)
			{
				ArraySegment<byte> arraySegment = owner.ToArraySegment();
				observer.WriteBytes(arraySegment.Array, position, owner.Position - position);
			}
		}
		public static void AddDeathTimeForScp049(ReferenceHub target)
		{
			PlayerManager.localPlayer.GetComponent<RagdollManager>().SpawnRagdoll(
							Vector3.zero,
							target.transform.rotation,
							Vector3.zero,
							(int)RoleType.ClassD,
							new PlayerStats.HitInfo(-1, "Scp049Reviver", DamageTypes.Scp049, -1),
							true,
							target.GetComponent<MirrorIgnorancePlayer>().PlayerId,
							target.nicknameSync.DisplayName,
							target.queryProcessor.PlayerId
						);
		}

		public static bool CanLookToPlayer(this Camera079 camera, ReferenceHub player)
		{
			Player EPlayer = Player.Dictionary[player.gameObject];
			if (EPlayer.Role == RoleType.Spectator || EPlayer.Role == RoleType.Scp079 || EPlayer.Role == RoleType.None)
				return false;

			Vector3 vector = player.transform.position - camera.transform.position;
			float num = Vector3.Dot(camera.head.transform.forward, vector);

			return (num >= 0f && num * num / vector.sqrMagnitude > 0.4225f)
				&& Physics.Raycast(camera.transform.position, vector, out RaycastHit raycastHit, 100f, -117407543)
				&& raycastHit.transform.name == player.name;
		}

		public static void Blink()
		{
			foreach (var scp173 in UnityEngine.Object.FindObjectsOfType<Scp173PlayerScript>())
			{
				scp173.RpcBlinkTime();
			}
		}
		public static int GetMTFTickets()
		{
			if (CustomLiteNetLib4MirrorTransport.DelayConnections) return -1;
			return RespawnTickets.Singleton.GetAvailableTickets(SpawnableTeamType.NineTailedFox);
		}

		public static int GetCITickets()
		{
			if (CustomLiteNetLib4MirrorTransport.DelayConnections) return -1;
			return RespawnTickets.Singleton.GetAvailableTickets(SpawnableTeamType.NineTailedFox);
		}

		public static void SendCustomSyncVar(this ReferenceHub player, NetworkIdentity behaviorOwner, Type targetType, Action<NetworkWriter> customSyncVar)
		{
			/* 
			Example:
			player.SendCustomSyncVar(player.networkIdentity, typeof(ServerRoles), (targetwriter) =>
			{
				targetwriter.WritePackedUInt64(2UL);
				targetwriter.WriteString("test");
			});
			 */
			NetworkWriter writer = NetworkWriterPool.GetWriter();
			NetworkWriter writer2 = NetworkWriterPool.GetWriter();
			MakeCustomSyncVarWriter(behaviorOwner, targetType, customSyncVar, writer, writer2);
			NetworkServer.SendToClientOfPlayer(player.networkIdentity, new UpdateVarsMessage() { netId = behaviorOwner.netId, payload = writer.ToArraySegment() });
			NetworkWriterPool.Recycle(writer);
			NetworkWriterPool.Recycle(writer2);
		}
		public static void MakeCustomSyncVarWriter(NetworkIdentity behaviorOwner, Type targetType, Action<NetworkWriter> customSyncVar, NetworkWriter owner, NetworkWriter observer)
		{
			ulong dirty = 0ul;
			ulong dirty_o = 0ul;
			NetworkBehaviour behaviour = null;
			for (int i = 0; i < behaviorOwner.NetworkBehaviours.Length; i++)
			{
				behaviour = behaviorOwner.NetworkBehaviours[i];
				if (behaviour.GetType() == targetType)
				{
					dirty |= 1UL << i;
					if (behaviour.syncMode == SyncMode.Observers) dirty_o |= 1UL << i;
				}
			}
			owner.WritePackedUInt64(dirty);
			observer.WritePackedUInt64(dirty & dirty_o);

			int position = owner.Position;
			owner.WriteInt32(0);
			int position2 = owner.Position;

			behaviour.SerializeObjectsDelta(owner);
			customSyncVar(owner);
			int position3 = owner.Position;
			owner.Position = position;
			owner.WriteInt32(position3 - position2);
			owner.Position = position3;

			if (dirty_o != 0ul)
			{
				ArraySegment<byte> arraySegment = owner.ToArraySegment();
				observer.WriteBytes(arraySegment.Array, position, owner.Position - position);
			}
		}
		public static void RpcCassieAnnouncement(RespawnEffectsController resp, NetworkConnection conn, string words, bool makeHold, bool makeNoise)
		{
			NetworkWriter writer = NetworkWriterPool.GetWriter();
			writer.WriteString(words);
			writer.WriteBoolean(makeHold);
			writer.WriteBoolean(makeNoise);
			SendTargetRPCInternal(conn, typeof(RespawnEffectsController), "RpcCassieAnnouncement", writer, 0, resp.netId, resp.ComponentIndex);
			NetworkWriterPool.Recycle(writer);
		}
		public static void SendTargetRPCInternal(NetworkConnection conn, Type invokeClass, string rpcName, NetworkWriter writer, int channelId, uint netid, int componentindex)
		{
			if (!NetworkServer.active)
			{
				Debug.LogError("TargetRPC Function " + rpcName + " called on client.");
				return;
			}
			RpcMessage msg = new RpcMessage
			{
				netId = netid,
				componentIndex = componentindex,
				functionHash = GetMethodHash(invokeClass, rpcName),
				payload = writer.ToArraySegment()
			};
			conn.Send<RpcMessage>(msg, channelId);
		}
		private static int GetMethodHash(Type invokeClass, string methodName)
		{
			return invokeClass.FullName.GetStableHashCode() * 503 + methodName.GetStableHashCode();
		}

		internal static void SpawnDummy(RoleType role, Vector3 position, Quaternion rotation, string name = "Yamato")
		{
			GameObject obj =
				Object.Instantiate(
					NetworkManager.singleton.spawnPrefabs.FirstOrDefault(p => p.gameObject.name == "Player"));
			CharacterClassManager ccm = obj.GetComponent<CharacterClassManager>();
			if (ccm == null)
				Log.Error("CCM is null, doufus. You need to do this the harder way.");
			ccm.CurClass = role;
			obj.GetComponent<NicknameSync>().Network_myNickSync = name;
			obj.GetComponent<QueryProcessor>().PlayerId = 9999;
			obj.GetComponent<QueryProcessor>().NetworkPlayerId = 9999;
			obj.transform.localScale = new Vector3(1, 1, 1);
			obj.transform.position = position;
			obj.transform.rotation = rotation;
			NetworkServer.Spawn(obj);
		}
	}
	internal static class Extensions
	{
		public static Task StartSender(this Task task)
		{
			return task.ContinueWith((x) => { Log.Error($"[Sender] {x}"); }, TaskContinuationOptions.OnlyOnFaulted);
		}
		public static bool IsHuman(this Player player)
		{
			return player.Team != Team.SCP && player.Team != Team.RIP;
		}

		public static bool IsEnemy(this Player player, Team target)
		{
			if (player.Role == RoleType.Spectator || player.Role == RoleType.None || player.Team == target)
				return false;

			return target == Team.SCP ||
				((player.Team != Team.MTF && player.Team != Team.RSC) || (target != Team.MTF && target != Team.RSC))
				&&
				((player.Team != Team.CDP && player.Team != Team.CHI) || (target != Team.CDP && target != Team.CHI))
			;
		}

		public static int GetHealthAmountPercent(this Player player)
		{
			return (int)(100f - (player.ReferenceHub.playerStats.GetHealthPercent() * 100f));
		}

		public static void ShowHitmarker(this ReferenceHub player)
		{
			player.GetComponent<Scp173PlayerScript>().TargetHitMarker(player.characterClassManager.connectionToClient);
		}

		public static void SendTextHintNotEffect(this Player player, string text, float time)
		{
			player.ReferenceHub.hints.Show(new TextHint(text, new HintParameter[] { new StringHintParameter(text) }, null, time));
		}

		public static IEnumerable<Camera079> GetNearCams(this ReferenceHub player)
		{
			Player EPlayer = Player.Dictionary[player.gameObject];
			foreach (var cam in Scp079PlayerScript.allCameras)
			{
				var dis = Vector3.Distance(EPlayer.Position, cam.transform.position);
				if (dis <= 15f)
				{
					yield return cam;
				}
			}
		}
		public static bool IsExmode(this Player player) => player.ReferenceHub.animationController.curAnim == 1;

		public static bool HasPermission(this Door.AccessRequirements value, Door.AccessRequirements flag)
		{
			return (value & flag) == flag;
		}

		public static bool IsList(this Type type)
		{
			return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);
		}

		public static bool IsDictionary(this Type type)
		{
			return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>);
		}

		public static Type GetListArgs(this Type type)
		{
			return type.GetGenericArguments()[0];
		}

		public static T GetRandomOne<T>(this List<T> list)
		{
			return list[UnityEngine.Random.Range(0, list.Count)];
		}

		public static T Random<T>(this IEnumerable<T> ie)
		{
			if (!ie.Any()) return default;
			return ie.ElementAt(SanyaPlugin.Instance.Random.Next(ie.Count()));
		}
	}
}