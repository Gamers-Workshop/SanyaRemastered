using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using Hints;
using MEC;
using Mirror;
using RemoteAdmin;
using SanyaRemastered.Data;
using UnityEngine;
using Dissonance.Integrations.MirrorIgnorance;
using UnityEngine.Networking;
using Respawning;
using Exiled.API.Features;
using SanyaRemastered.DissonanceControl;
using NorthwoodLib.Pools;
using System.Text;
using System.Collections.ObjectModel;
using Interactables.Interobjects.DoorUtils;

namespace SanyaRemastered.Functions
{
	internal static class PlayerDataManager
	{
		public static Dictionary<string, PlayerData> playersData = new Dictionary<string, PlayerData>();

		public static PlayerData LoadPlayerData(string userid)
		{
			string targetuseridpath = Path.Combine(SanyaRemastered.Instance.Config.DataDirectory, $"{userid}.txt");
			if (!Directory.Exists(SanyaRemastered.Instance.Config.DataDirectory)) Directory.CreateDirectory(SanyaRemastered.Instance.Config.DataDirectory);
			if (!File.Exists(targetuseridpath)) return new PlayerData(DateTime.Now, userid, true, 0, 0, 0);
			else return ParsePlayerData(targetuseridpath);
		}

		public static void SavePlayerData(PlayerData data)
		{
			string targetuseridpath = Path.Combine(SanyaRemastered.Instance.Config.DataDirectory, $"{data.userid}.txt");

			if (!Directory.Exists(SanyaRemastered.Instance.Config.DataDirectory)) Directory.CreateDirectory(SanyaRemastered.Instance.Config.DataDirectory);

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
		private static readonly string whitelist_path = Path.Combine(SanyaRemastered.Instance.Config.DataDirectory, "VPN-Whitelist.txt");
		public static HashSet<IPAddress> whitelist = new HashSet<IPAddress>();
		private static readonly string blacklist_path = Path.Combine(SanyaRemastered.Instance.Config.DataDirectory, "VPN-Blacklist.txt");
		public static HashSet<IPAddress> blacklist = new HashSet<IPAddress>();

		/*public static IEnumerator<float> CheckVPN(PreAuthenticatingEventArgs ev)
		{
			IPAddress address = ev.Request.RemoteEndPoint.Address;

			if (IsWhiteListed(address) || IsBlacklisted(address))
			{
				Log.Debug($"[VPNChecker] Already Checked:{address}", SanyaRemastered.Instance.Config.IsDebugged);
				yield break;
			}

			using (UnityWebRequest unityWebRequest = UnityWebRequest.Get($"https://v2.api.iphub.info/ip/{address}"))
			{
				unityWebRequest.SetRequestHeader("X-Key", SanyaRemastered.Instance.Config.KickVpnApikey);
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
		public static bool isActuallyBombGoing = false;
		public static int AirBombWait = 0;

		public static bool ContainClassD = false;

		public static IEnumerator<float> BigHitmark(MicroHID microHID)
		{
			yield return Timing.WaitForSeconds(0.1f);
			microHID.TargetSendHitmarker(false);
			yield break;
		}
		public static IEnumerator<float> CloseNukeCap()
		{
			var outsite = UnityEngine.Object.FindObjectOfType<AlphaWarheadOutsitePanel>();
			if (!outsite.keycardEntered) yield break;
			yield return Timing.WaitForSeconds(0.1f);
				outsite.NetworkkeycardEntered = false;
			yield break;
		}
		public static IEnumerator<float> Calm096(PlayableScps.Scp096 scp)
		{
			if (scp.Enraged && !scp._targets.Any())
			{
				scp.PlayerState = PlayableScps.Scp096PlayerState.Calming;
			}
			yield break;
		}
		public static IEnumerator<float> StartContainClassD(bool stop, float TimeLock = 0)
		{
			if (stop)
			{
				ContainClassD = false;
				foreach (var door in UnityEngine.Object.FindObjectsOfType<DoorVariant>())
				{
					if (door.name.Contains("PrisonDoor"))
					{
						door.NetworkActiveLocks = (ushort)DoorLockMode.FullLock;
						yield break;
					}
				}
			}
			else 
			{
				ContainClassD = true;
				while (ContainClassD)
				{
					foreach (var door in UnityEngine.Object.FindObjectsOfType<DoorVariant>())
					{
						if (door.name.Contains("PrisonDoor"))
						{
							door.NetworkActiveLocks = (ushort)DoorLockMode.FullLock;
							yield return Timing.WaitForSeconds(TimeLock);
							door.NetworkActiveLocks = (ushort)DoorLockMode.CanOpen;
							yield break;
						}
					}
				}
			}
		}
		public static IEnumerator<float> AirSupportBomb(bool stop, int timewait = 0, float TimeEnd = -1f)
		{
			AirBombWait = timewait;
			if (isAirBombGoing && stop)
			{
				isAirBombGoing = false;
				isActuallyBombGoing = false;
				RespawnEffectsController.PlayCassieAnnouncement($"The Outside Zone emergency termination sequence as been stop .", false, true);
				if (SanyaRemastered.Instance.Config.CassieSubtitle)
				{
					Methods.SendSubtitle(Subtitles.AirbombStop, 10);
				}
				Log.Info("[AirSupportBomb] The AirBomb as stop");
				yield break;
			}
			else if (isAirBombGoing && !stop)
			{
				Log.Info("[AirSupportBomb] The AirBomb as already true");
				yield break;
			}
			isAirBombGoing = true;
			while (AirBombWait > 0)
			{
				Log.Debug("Démarage AirSupport timewait");
				if (AirBombWait == 60f || AirBombWait == 120f || AirBombWait == 300f || AirBombWait == 600f || AirBombWait == 1800f || AirBombWait == 3600f)
				{
					RespawnEffectsController.PlayCassieAnnouncement($"Alert . The Outside Zone emergency termination sequence activated in t minus {AirBombWait / 60} minutes .", false, true);
					if (SanyaRemastered.Instance.Config.CassieSubtitle)
					{
						Methods.SendSubtitle(Subtitles.AirbombStartingWaitMinutes.Replace("{0}", (AirBombWait / 60).ToString()), 10);
					}
				}
				else if (AirBombWait == 30f)
				{
					RespawnEffectsController.PlayCassieAnnouncement($"Alert . The Outside Zone emergency termination sequence activated in t minus 30 seconds .", false, true);
					if (SanyaRemastered.Instance.Config.CassieSubtitle)
					{
						Methods.SendSubtitle(Subtitles.AirbombStartingWait30s, 10);
					}
				}
				else if (AirBombWait == 0)
				{
					break;
				}
				if (!isAirBombGoing)
				{
					RespawnEffectsController.PlayCassieAnnouncement($"The Outside Zone emergency termination sequence as been stop .", false, true);
					if (SanyaRemastered.Instance.Config.CassieSubtitle)
					{
						Methods.SendSubtitle(Subtitles.AirbombStop, 10);
					}
					Log.Info($"[AirSupportBomb] The AirBomb as stop");
					yield break;
				}
				AirBombWait--;
				yield return Timing.WaitForSeconds(1);
			}
			if (isAirBombGoing)
			{
				isActuallyBombGoing = true;
				Log.Info($"[AirSupportBomb] booting...");
				try
				{
					if (!DissonanceCommsControl.IsReady)
						DissonanceCommsControl.Init();

					if (DissonanceCommsControl.dissonanceComms._capture.MicrophoneName == "Siren.raw")
						DissonanceCommsControl.dissonanceComms._capture.RestartTransmissionPipeline("Command");
					else
						DissonanceCommsControl.dissonanceComms._capture.MicrophoneName = "Siren.raw";
				}
				catch (Exception)
				{

				}
	
				RespawnEffectsController.PlayCassieAnnouncement("danger . outside zone emergency termination sequence activated .", false, true);
				if (SanyaRemastered.Instance.Config.CassieSubtitle)
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
							isActuallyBombGoing = false;
							try
							{
								if (!DissonanceCommsControl.IsReady)
									DissonanceCommsControl.Init();

								if (DissonanceCommsControl.dissonanceComms._capture.MicrophoneName == "")
									DissonanceCommsControl.dissonanceComms._capture.RestartTransmissionPipeline("Command");
								else
									DissonanceCommsControl.dissonanceComms._capture.MicrophoneName = "";
							}
							catch (Exception)
							{

							}
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
						yield return Timing.WaitForSeconds(0.1f);
					}
					if (TimeEnd != -1)
					{
						if (TimeEnd <= Time.deltaTime)
						{
							isAirBombGoing = false;
							Log.Info($"[AirSupportBomb] TimeBombing:{TimeEnd}");
							break;
						}
					}
					yield return Timing.WaitForSeconds(0.25f);
				}

				try
				{
					if (!DissonanceCommsControl.IsReady)
						DissonanceCommsControl.Init();

					if (DissonanceCommsControl.dissonanceComms._capture.MicrophoneName == "")
						DissonanceCommsControl.dissonanceComms._capture.RestartTransmissionPipeline("Command");
					else
						DissonanceCommsControl.dissonanceComms._capture.MicrophoneName = "";
				}
				catch (Exception)
				{

				}

				if (SanyaRemastered.Instance.Config.CassieSubtitle)
					Methods.SendSubtitle(Subtitles.AirbombEnded, 10);
				RespawnEffectsController.PlayCassieAnnouncement("outside zone termination completed .", false, true);
				isActuallyBombGoing = false;
				Log.Info($"[AirSupportBomb] Ended.");
				yield break;
			}
		}
		public static IEnumerator<float> Scp106CustomTeleport(Scp106PlayerScript scp106PlayerScript, Vector3 position)
		{
			if (!scp106PlayerScript.goingViaThePortal)
			{
				scp106PlayerScript.RpcTeleportAnimation();
				scp106PlayerScript.goingViaThePortal = true;
				yield return Timing.WaitForSeconds(3.5f);
				scp106PlayerScript._hub.playerMovementSync.OverridePosition(position, 0f, false);
				yield return Timing.WaitForSeconds(3.5f);
				if (AlphaWarheadController.Host.detonated && scp106PlayerScript.transform.position.y < 800f)
					scp106PlayerScript._hub.playerStats.HurtPlayer(new PlayerStats.HitInfo(9000f, "WORLD", DamageTypes.Nuke, 0), scp106PlayerScript.gameObject, true);
				scp106PlayerScript.goingViaThePortal = false;
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
			component.FullInitData(gm, position, Quaternion.Euler(component.throwStartAngle), Vector3.zero, component.throwAngularVelocity, player == null ? Team.TUT : player.characterClassManager.CurRole.team);
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
			PlayerManager.localPlayer.GetComponent<AmbientSoundPlayer>().RpcPlaySound(Mathf.Clamp(id, 0, 32));
		}

		public static void TargetShake(this ReferenceHub target, bool achieve)
		{
			NetworkWriter writer = NetworkWriterPool.GetWriter();
			writer.WriteBoolean(achieve);
			target.TargetSendRpc(AlphaWarheadController.Host, nameof(AlphaWarheadController.RpcShake), writer);
			NetworkWriterPool.Recycle(writer);
		}
		public static bool IsStuck(Vector3 pos)
		{
			bool result = false;
			foreach (Collider collider in Physics.OverlapBox(pos, new Vector3(0.4f, 1f, 0.4f), new Quaternion(0f, 0f, 0f, 0f)))
			{
				bool flag = collider.name.Contains("Hitbox") || collider.name.Contains("mixamorig") || collider.name.Equals("Player") || collider.name.Equals("PlyCenter") || collider.name.Equals("Antijumper");
				if (!flag)
				{
					Log.Warn($"Detect:{collider.name}");
					result = true;
				}
			}
			return result;
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

		public static void SendCustomSyncObject(this Player target, NetworkIdentity behaviorOwner, Type targetType, Action<NetworkWriter> customAction)
		{
			/* 
			Cant be use if you dont understand(ill make more use easily soonTM)
			Example(SyncList) [EffectOnlySCP207]:
			player.SendCustomSync(player.ReferenceHub.networkIdentity, typeof(PlayerEffectsController), (writer) => {
				writer.WritePackedUInt64(1ul);								// DirtyObjectsBit
				writer.WritePackedUInt32((uint)1);							// DirtyIndexCount
				writer.WriteByte((byte)SyncList<byte>.Operation.OP_SET);	// Operations
				writer.WritePackedUInt32((uint)0);							// EditIndex
				writer.WriteByte((byte)1);									// Item
			});
			*/
			NetworkWriter writer = NetworkWriterPool.GetWriter();
			NetworkWriter writer2 = NetworkWriterPool.GetWriter();
			MakeCustomSyncWriter(behaviorOwner, targetType, customAction, null, writer, writer2);
			NetworkServer.SendToClientOfPlayer(target.ReferenceHub.networkIdentity, new UpdateVarsMessage() { netId = behaviorOwner.netId, payload = writer.ToArraySegment() });
			NetworkWriterPool.Recycle(writer);
			NetworkWriterPool.Recycle(writer2);
		}

		// API, dont change
		public static int GetComponentIndex(NetworkIdentity identity, Type type)
		{
			return Array.FindIndex(identity.NetworkBehaviours, (x) => x.GetType() == type);
		}

		// API, dont change
		public static ulong GetDirtyBit(Type targetType, string PropertyName)
		{
			var bytecodes = targetType.GetProperty(PropertyName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)?.GetSetMethod().GetMethodBody().GetILAsByteArray();
			return bytecodes[Array.FindLastIndex(bytecodes, x => x == System.Reflection.Emit.OpCodes.Ldc_I8.Value) + 1];
		}

		// API, dont change
		public static System.Reflection.MethodInfo GetWriteExtension(object value)
		{
			Type type = value.GetType();
			switch (Type.GetTypeCode(type))
			{
				case TypeCode.String:
					return typeof(NetworkWriterExtensions).GetMethod(nameof(NetworkWriterExtensions.WriteString));
				case TypeCode.Boolean:
					return typeof(NetworkWriterExtensions).GetMethod(nameof(NetworkWriterExtensions.WriteBoolean));
				case TypeCode.Int16:
					return typeof(NetworkWriterExtensions).GetMethod(nameof(NetworkWriterExtensions.WriteInt16));
				case TypeCode.Int32:
					return typeof(NetworkWriterExtensions).GetMethod(nameof(NetworkWriterExtensions.WritePackedInt32));
				case TypeCode.UInt16:
					return typeof(NetworkWriterExtensions).GetMethod(nameof(NetworkWriterExtensions.WriteUInt16));
				case TypeCode.Byte:
					return typeof(NetworkWriterExtensions).GetMethod(nameof(NetworkWriterExtensions.WriteByte));
				case TypeCode.SByte:
					return typeof(NetworkWriterExtensions).GetMethod(nameof(NetworkWriterExtensions.WriteSByte));
				case TypeCode.Single:
					return typeof(NetworkWriterExtensions).GetMethod(nameof(NetworkWriterExtensions.WriteSingle));
				case TypeCode.Double:
					return typeof(NetworkWriterExtensions).GetMethod(nameof(NetworkWriterExtensions.WriteDouble));
				default:
					if (type == typeof(Vector3))
						return typeof(NetworkWriterExtensions).GetMethod(nameof(NetworkWriterExtensions.WriteVector3));
					if (type == typeof(Vector2))
						return typeof(NetworkWriterExtensions).GetMethod(nameof(NetworkWriterExtensions.WriteVector2));
					if (type == typeof(GameObject))
						return typeof(NetworkWriterExtensions).GetMethod(nameof(NetworkWriterExtensions.WriteGameObject));
					if (type == typeof(Quaternion))
						return typeof(NetworkWriterExtensions).GetMethod(nameof(NetworkWriterExtensions.WriteQuaternion));
					if (type == typeof(BreakableWindow.BreakableWindowStatus))
						return typeof(BreakableWindowStatusSerializer).GetMethod(nameof(BreakableWindowStatusSerializer.WriteBreakableWindowStatus));
					if (type == typeof(Grenades.RigidbodyVelocityPair))
						return typeof(Grenades.RigidbodyVelocityPairSerializer).GetMethod(nameof(Grenades.RigidbodyVelocityPairSerializer.WriteRigidbodyVelocityPair));
					if (type == typeof(ItemType))
						return typeof(NetworkWriterExtensions).GetMethod(nameof(NetworkWriterExtensions.WritePackedInt32));
					if (type == typeof(PlayerMovementSync.RotationVector))
						return typeof(RotationVectorSerializer).GetMethod(nameof(RotationVectorSerializer.WriteRotationVector));
					if (type == typeof(Pickup.WeaponModifiers))
						return typeof(WeaponModifiersSerializer).GetMethod(nameof(WeaponModifiersSerializer.WriteWeaponModifiers));
					if (type == typeof(Offset))
						return typeof(OffsetSerializer).GetMethod(nameof(OffsetSerializer.WriteOffset));
					return null;
			}
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

		public static GameObject SpawnDummy(RoleType role, Vector3 pos, Quaternion rot)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(NetworkManager.singleton.spawnPrefabs.FirstOrDefault(p => p.gameObject.name == "Player"));
			CharacterClassManager ccm = gameObject.GetComponent<CharacterClassManager>();
			ccm.CurClass = role;
			ccm.RefreshPlyModel();
			gameObject.GetComponent<NicknameSync>().Network_myNickSync = "Yamato";
			gameObject.GetComponent<QueryProcessor>().NetworkPlayerId = 9999;
			gameObject.transform.position = pos;
			gameObject.transform.rotation = rot;
			NetworkServer.Spawn(gameObject);
			return gameObject;
		}
	}
	public static class Keycard
	{
		[Flags]
		public enum Permissions
		{
			None = 0x0,
			Checkpoints = 0x1,
			ExitGates = 0x2,
			Intercom = 0x4,
			AlphaWarhead = 0x8,
			ContainmentLevelOne = 0x10,
			ContainmentLevelTwo = 0x20,
			ContainmentLevelThree = 0x40,
			ArmoryLevelOne = 0x80,
			ArmoryLevelTwo = 0x100,
			ArmoryLevelThree = 0x200,
			ScpOverride = 0x400,

			Pedestal = 0x800
		}

		public static readonly ReadOnlyDictionary<string, Permissions> BackwardsCompatibility = new ReadOnlyDictionary<string, Permissions>(new Dictionary<string, Permissions>
		{
			["CONT_LVL_1"] = Permissions.ContainmentLevelOne,
			["CONT_LVL_2"] = Permissions.ContainmentLevelTwo,
			["CONT_LVL_3"] = Permissions.ContainmentLevelThree,

			["ARMORY_LVL_1"] = Permissions.ArmoryLevelOne,
			["ARMORY_LVL_2"] = Permissions.ArmoryLevelTwo,
			["ARMORY_LVL_3"] = Permissions.ArmoryLevelThree,

			["INCOM_ACC"] = Permissions.Intercom,
			["CHCKPOINT_ACC"] = Permissions.Checkpoints,
			["EXIT_ACC"] = Permissions.ExitGates,

			["PEDESTAL_ACC"] = Permissions.Pedestal,
		});

		public static Permissions ToTruthyPermissions(this KeycardPermissions keycardPermissions) => (Permissions)keycardPermissions;

		public static Permissions ToTruthyPermissions(string permission)
		{
			if (string.IsNullOrEmpty(permission))
				return Permissions.None;

			BackwardsCompatibility.TryGetValue(permission, out var p);
			return p;
		}

		public static Permissions ToTruthyPermissions(string[] permissions)
		{
			var p = Permissions.None;
			for (var z = 0; z < permissions.Length; z++)
				p |= ToTruthyPermissions(permissions[z]);

			return p;
		}

		public static bool HasFlagFast(this Permissions permissions, Permissions flag, bool requireAll) => requireAll ? (permissions & flag) == flag : (permissions & flag) != 0;
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
			if (player.Team == Team.SCP && target == Team.TUT || player.Team == Team.TUT && target == Team.SCP) return false;
			return target == Team.SCP
				|| target == Team.TUT
				|| ((player.Team != Team.MTF && player.Team != Team.RSC) || (target != Team.MTF && target != Team.RSC))
				&&
				((player.Team != Team.CDP && player.Team != Team.CHI) || (target != Team.CDP && target != Team.CHI))
			;
		}

		public static int GetHealthAmountPercent(this Player player)
		{
			return (int)(100f - (player.ReferenceHub.playerStats.GetHealthPercent() * 100f));
		}
		public static void SendToTargetSound(this Player player)
		{
			NetworkServer.SendToClientOfPlayer(player.ReferenceHub.networkIdentity, new PlayableScps.Messages.Scp096ToTargetMessage(player.ReferenceHub));
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

		/*public static bool HasPermission(this Door.AccessRequirements value, Door.AccessRequirements flag)
		{
			return (value & flag) == flag;
		}*/

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
			return ie.ElementAt(SanyaRemastered.Instance.Random.Next(ie.Count()));
		}
		public static string FormatArguments(ArraySegment<string> sentence, int index)
		{
			StringBuilder SB = StringBuilderPool.Shared.Rent();
			foreach (string word in sentence.Segment(index))
			{
				SB.Append(word);
				SB.Append(" ");
			}
			string msg = SB.ToString();
			StringBuilderPool.Shared.Return(SB);
			return msg;
		}
	}
}