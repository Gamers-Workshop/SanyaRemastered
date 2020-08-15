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

namespace SanyaPlugin.Functions
{
	internal static class PlayerDataManager
	{
		public static Dictionary<string, PlayerData> playersData = new Dictionary<string, PlayerData>();

		public static PlayerData LoadPlayerData(string userid)
		{
			string targetuseridpath = Path.Combine(SanyaPlugin.DataPath, $"{userid}.txt");
			if (!Directory.Exists(SanyaPlugin.DataPath)) Directory.CreateDirectory(SanyaPlugin.DataPath);
			if (!File.Exists(targetuseridpath)) return new PlayerData(DateTime.Now, userid, true, 0, 0, 0);
			else return ParsePlayerData(targetuseridpath);
		}

		public static void SavePlayerData(PlayerData data)
		{
			string targetuseridpath = Path.Combine(SanyaPlugin.DataPath, $"{data.userid}.txt");

			if (!Directory.Exists(SanyaPlugin.DataPath)) Directory.CreateDirectory(SanyaPlugin.DataPath);

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

		public static void ResetLimitedFlag()
		{
			foreach (var file in Directory.GetFiles(SanyaPlugin.DataPath))
			{
				var data = LoadPlayerData(file.Replace(".txt", string.Empty));
				Log.Warn($"{data.userid}:{data.limited}");
				data.limited = true;
				SavePlayerData(data);
			}
		}
	}

	internal static class ShitChecker
	{
		private static string whitelist_path = Path.Combine(SanyaPlugin.DataPath, "VPN-Whitelist.txt");
		public static HashSet<IPAddress> whitelist = new HashSet<IPAddress>();
		private static string blacklist_path = Path.Combine(SanyaPlugin.DataPath, "VPN-Blacklist.txt");
		public static HashSet<IPAddress> blacklist = new HashSet<IPAddress>();

		public static IEnumerator<float> CheckVPN(PreAuthenticatingEventArgs ev)
		{
			IPAddress address = ev.Request.RemoteEndPoint.Address;

			if (IsWhiteListed(address) || IsBlacklisted(address))
			{
				Log.Debug($"[VPNChecker] Already Checked:{address}");
				yield break;
			}

			/*using (UnityWebRequest unityWebRequest = UnityWebRequest.Get($"https://v2.api.iphub.info/ip/{address}"))
			{
				unityWebRequest.SetRequestHeader("X-Key", Configs.kick_vpn_apikey);
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

						ReferenceHub player = Player.GetPlayer(ev.UserId);
						if (player != null)
						{
							ServerConsole.Disconnect(player.characterClassManager.connectionToClient, Subtitles.VPNKickMessage);
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
			}*/
		}

		public static IEnumerator<float> CheckIsLimitedSteam(string userid)
		{
			PlayerData data = null;
			if (SanyaPlugin.Instance.Config.DataEnabled && PlayerDataManager.playersData.TryGetValue(userid, out data) && !data.limited)
			{
				Log.Debug($"[SteamCheck] Already Checked:{userid}");
				yield break;
			}

			/*string xmlurl = string.Concat(
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
								ReferenceHub player = Player.GetPlayer(userid);
								if (player != null)
								{
									ServerConsole.Disconnect(player.characterClassManager.connectionToClient, Subtitles.LimitedKickMessage);
								}

								if (!EventHandlers.kickedbyChecker.ContainsKey(userid))
									EventHandlers.kickedbyChecker.Add(userid, "steam");

								yield break;
							}
						}
						else
						{
							Log.Warn($"[SteamCheck] Falied(NoProfile):{userid}");
							ReferenceHub player = Player.GetPlayer(userid);
							if (player != null)
							{
								ServerConsole.Disconnect(player.characterClassManager.connectionToClient, Subtitles.NoProfileKickMessage);
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
			yield break;*/
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

		public static IEnumerator<float> GrantedLevel(ReferenceHub player, PlayerData data)
		{
			yield return Timing.WaitForSeconds(1f);

			var group = player.serverRoles.Group?.Clone();
			string level = data.level.ToString();
			string rolestr = player.serverRoles.GetUncoloredRoleString();
			string rolecolor = player.serverRoles.MyColor;
			string badge;

			rolestr = rolestr.Replace("[", string.Empty).Replace("]", string.Empty).Replace("<", string.Empty).Replace(">", string.Empty);

			if (rolecolor == "light_red")
			{
				rolecolor = "pink";
			}

			if (data.level == -1)
			{
				level = "???";
			}

			if (string.IsNullOrEmpty(rolestr))
			{
				badge = $"Level{level}";
			}
			else
			{
				badge = $"Level{level} : {rolestr}";
			}

			if (SanyaPlugin.Instance.Config.DisableChatBypassWhitelist && WhiteList.IsOnWhitelist(Player.Dictionary[player.gameObject].UserId))
			{
				badge += " : Certifié";
			}

			if (group == null)
			{
				group = new UserGroup()
				{
					BadgeText = badge,
					BadgeColor = "default",
					HiddenByDefault = false,
					Cover = true,
					KickPower = 0,
					Permissions = 0,
					RequiredKickPower = 0,
					Shared = false
				};
			}
			else
			{
				group.BadgeText = badge;
				group.BadgeColor = rolecolor;
				group.HiddenByDefault = false;
				group.Cover = true;
			}

			player.serverRoles.SetGroup(group, false, false, true);

			Log.Debug($"[GrantedLevel] {Player.Dictionary[player.gameObject].UserId} : Level{level}");

			yield break;
		}
		public static IEnumerator<float> BigHitmark(MicroHID microHID)
		{
			yield return Timing.WaitForSeconds(0.1f);
			microHID.TargetSendHitmarker(false);
			yield break;
		}
		public static IEnumerator<float> StartContainClassD(bool stop , float TimeLock = 0)
		{
			foreach (var door in UnityEngine.Object.FindObjectsOfType<Door>())
			{
				while (!stop)
				{
					if (door.name.Contains("PrisonDoor"))
					{
						door.lockdown = true;
					}
					yield return Timing.WaitForSeconds(TimeLock);
					stop = true;
				}
				door.lockdown = false;
				stop = true;
				yield break;
			}
		}
		public static IEnumerator<float> AirSupportBomb(bool stop = false,float timewait = 0,float TimeEnd = -1)
		{
			while (timewait > 0 && !isAirBombGoing)
			{
				if (timewait == 60f || timewait == 120f || timewait == 300f || timewait == 600f || timewait == 1800f || timewait == 3600f)
				{
					RespawnEffectsController.PlayCassieAnnouncement($"Alert . The Outside Zone emergency termination sequence activated in t minus {(int)timewait / 60} minutes .", false, true);
					if (SanyaPlugin.Instance.Config.CassieSubtitle)
					{
						Methods.SendSubtitle(Subtitles.AirbombStartingWaitMinutes.Replace("{0}", ((int)timewait / 60).ToString()), 10);
					}
				}
				else if (timewait == 30f)
				{
					RespawnEffectsController.PlayCassieAnnouncement($"Alert . The Outside Zone emergency termination sequence activated in t minus 30 seconds .", false, true);
					if (SanyaPlugin.Instance.Config.CassieSubtitle)
					{
						Methods.SendSubtitle(Subtitles.AirbombStartingWait30s, 10);
					}
				}
				if (stop && timewait > 0)
				{
					RespawnEffectsController.PlayCassieAnnouncement($"The Outside Zone emergency termination sequence as been stop .", false, true);
					if (SanyaPlugin.Instance.Config.CassieSubtitle)
					{
						Methods.SendSubtitle(Subtitles.AirbombStop, 10);
					}
					Log.Info($"[AirSupportBomb] The AirBomb as stop");
					break;
				}
				timewait--;
				yield return Timing.WaitForSeconds(1);
			}
				Log.Info($"[AirSupportBomb] booting...");
				if (isAirBombGoing)
				{
					Log.Info($"[Airbomb] already booted, cancel.");
					yield break;
				}
				else
				{
					stop = true;
					isAirBombGoing = true;
				}
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
						yield return Timing.WaitForSeconds(0.075f);
					}
					if (TimeEnd != -1)
					{
						float TimeBombing = 0;
						TimeBombing += Time.deltaTime;
						if (TimeBombing <= TimeEnd)
						{
							Log.Info($"[AirSupportBomb] TimeBombing:{TimeBombing}");
							isAirBombGoing = false;
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

		public static void PlayAmbientSound(int id)
		{
			PlayerManager.localPlayer.GetComponent<AmbientSoundPlayer>().RpcPlaySound(Mathf.Clamp(id, 0, 31));
		}

		public static void PlayRandomAmbient()
		{
			PlayAmbientSound(UnityEngine.Random.Range(0, 32));
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

			RaycastHit raycastHit;
			return (num >= 0f && num * num / vector.sqrMagnitude > 0.4225f)
				&& Physics.Raycast(camera.transform.position, vector, out raycastHit, 100f, -117407543)
				&& raycastHit.transform.name == player.name;
		}

		public static void Blink()
		{
			foreach (var scp173 in UnityEngine.Object.FindObjectsOfType<Scp173PlayerScript>())
			{
				scp173.RpcBlinkTime();
			}
		}
		/*private void SpawnDummy(RoleType role, Vector3 position, Quaternion rotation, float x = 1, float y = 1, float z = 1)
		{
			GameObject obj =
				Object.Instantiate(
					NetworkManager.singleton.spawnPrefabs.FirstOrDefault(p => p.gameObject.name == "Player"));
			CharacterClassManager ccm = obj.GetComponent<CharacterClassManager>();
			if (ccm == null)
				Log.Error("CCM is null, doufus. You need to do this the harder way.");
			ccm.CurClass = role;
			ccm.RefreshPlyModel();
			obj.GetComponent<NicknameSync>().Network_myNickSync = "Yamato";
			obj.GetComponent<QueryProcessor>().PlayerId = 9999;
			obj.GetComponent<QueryProcessor>().NetworkPlayerId = 9999;
			obj.transform.localScale = new Vector3(x, y, z);
			obj.transform.position = position;
			obj.transform.rotation = rotation;
			NetworkServer.Spawn(obj);
		}*/

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
	}
	internal static class Extensions
	{
		public static Task StartSender(this Task task)
		{
			return task.ContinueWith((x) => { Log.Error($"[Sender] {x}"); }, TaskContinuationOptions.OnlyOnFaulted);
		}

		public static bool IsEnemy(this ReferenceHub player, Team target)
		{
			Player EPlayer = Player.Dictionary[player.gameObject];
			if (EPlayer.Role == RoleType.Spectator || EPlayer.Role == RoleType.None || EPlayer.Team == target)
				return false;

			return target == Team.SCP ||
				((EPlayer.Team != Team.MTF && EPlayer.Team != Team.RSC) || (target != Team.MTF && target != Team.RSC))
				&&
				((EPlayer.Team != Team.CDP && EPlayer.Team != Team.CHI) || (target != Team.CDP && target != Team.CHI))
			;
		}

		public static void ShowHitmarker(this ReferenceHub player)
		{
			player.GetComponent<Scp173PlayerScript>().TargetHitMarker(player.characterClassManager.connectionToClient);
		}

		public static void SendTextHint(this ReferenceHub player, string text, ushort time)
		{
			player.hints.Show(new TextHint(text, new HintParameter[] { new StringHintParameter("") }, new HintEffect[] { HintEffectPresets.TrailingPulseAlpha(0.5f, 1f, 0.5f, 2f, 0f, 2) }, time));
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

		public static bool IsExmode(this ReferenceHub player) => player.animationController.curAnim == 1;

		public static bool HasPermission(this Door.AccessRequirements value, Door.AccessRequirements flag)
		{
			return (value & flag) == flag;
		}

		public static T GetRandomOne<T>(this List<T> list)
		{
			return list[UnityEngine.Random.Range(0, list.Count)];
		}
		public static T Random<T>(this IEnumerable<T> ie)
		{
			if (!ie.Any()) return default;
			return ie.ElementAt(SanyaPlugin.random.Next(ie.Count()));
		}
	}
}