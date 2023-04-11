using AudioPlayer;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Hints;
using InventorySystem.Items.ThrowableProjectiles;
using InventorySystem.Items.Usables.Scp330;
using MapEditorReborn.API.Features.Objects;
using MapEditorReborn.API.Features.Serializable;
using MEC;
using Mirror;
using NorthwoodLib.Pools;
using PlayerRoles;
using PlayerRoles.PlayableScps.Scp079.Cameras;
using RemoteAdmin;
using RoundRestarting;
using SCPSLAudioApi.AudioCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Utils.Networking;
using Random = UnityEngine.Random;

namespace SanyaRemastered.Functions
{
    internal static class Coroutines
    {
        public static bool IsAirBombGoing { get; set; }
        public static bool IsActuallyBombGoing { get; set; }
        public static int AirBombWait { get; set; }

        public static IEnumerator<float> RestartServer()
        {
            yield return Timing.WaitForSeconds(30f);
            if (Player.List.IsEmpty())
            {
                ServerStatic.StopNextRound = ServerStatic.NextRoundAction.Restart;
                RoundRestart.ChangeLevel(true);
                yield break;
            }
            if (ServerStatic.StopNextRound is ServerStatic.NextRoundAction.Restart)
            {
                ServerStatic.StopNextRound = ServerStatic.NextRoundAction.DoNothing;
                ServerConsole.AddOutputEntry(default(ServerOutput.ExitActionResetEntry)); 
                yield break;
            }
            ServerStatic.StopNextRound = ServerStatic.NextRoundAction.Restart;
            ServerConsole.AddOutputEntry(default(ServerOutput.ExitActionRestartEntry));
        }
        public static IEnumerator<float> AirSupportBomb(bool stop, int timewait = 0, float TimeEnd = -1f)
        {
            AirBombWait = timewait;
            if (IsAirBombGoing)
            {
                if (!stop)
                    yield break;
                IsAirBombGoing = false;
                IsActuallyBombGoing = false;
                AirBombWait = 0;

                Cassie.MessageTranslated("The Outside Zone emergency termination sequence as been stoped", SanyaRemastered.Instance.Translation.CustomSubtitles.AirbombStop);
                try
                {
                    Methods.DiscordLog(":airplane_arriving: Arrêt  du bombardement\n");
                }
                catch { }
                yield break;
            }

            DiscordLog.DiscordLog.Instance.LOG += $":airplane_departure: Départ du bombardement dans {AirBombWait / 60:00}min {AirBombWait % 60:00}sec\n";

            IsAirBombGoing = true;
            while (AirBombWait > 0)
            {
                Log.Debug("Démarage AirSupport timewait");
                if (AirBombWait is 60 or 120 or 300 or 600 or 1800 or 3600)
                {
                    Cassie.MessageTranslated($"Alert . The Outside Zone emergency termination sequence activated in t minus {AirBombWait / 60} minutes .", SanyaRemastered.Instance.Translation.CustomSubtitles.AirbombStartingWaitMinutes.Replace("{0}", (AirBombWait / 60).ToString()));
                }
                else if (AirBombWait is 30)
                {
                    Cassie.MessageTranslated($"Alert . The Outside Zone emergency termination sequence activated in t minus 30 seconds .", SanyaRemastered.Instance.Translation.CustomSubtitles.AirbombStartingWait30s);
                }
                else if (AirBombWait is 0)
                {
                    break;
                }
                if (!IsAirBombGoing)
                {
                    Cassie.MessageTranslated("The Outside Zone emergency termination sequence as been stop", SanyaRemastered.Instance.Translation.CustomSubtitles.AirbombStop);
                    DiscordLog.DiscordLog.Instance.LOG += ":airplane_arriving: Arrêt  du bombardement\n";
                    Log.Debug($"[AirSupportBomb] The AirBomb as stop");
                    yield break;
                }
                AirBombWait--;
                yield return Timing.WaitForSeconds(1);
            }
            if (!IsAirBombGoing)
                yield break;
            IsActuallyBombGoing = true;
            Log.Debug($"[AirSupportBomb] booting...");
            try
            {
                Methods.DiscordLog(":airplane: Bombardement en cours\n");
            }
            catch { }

            try
            {
                Methods.PlaySirenAudio();
            }
            catch { }
            Cassie.MessageTranslated("danger . the outside zone emergency termination sequence activated \n the air bomb cant be Avoid", SanyaRemastered.Instance.Translation.CustomSubtitles.AirbombStarting);

            yield return Timing.WaitForSeconds(5f);
            Log.Debug($"[AirSupportBomb] charging...");
            {
                int waitforready = 5;
                while (waitforready >= 0)
                {
                    Map.PlayAmbientSound(7);
                    waitforready--;
                    if (!IsAirBombGoing)
                    {
                        IsActuallyBombGoing = false;
                    }
                    yield return Timing.WaitForSeconds(1f);
                }
            }
            Log.Debug($"[AirSupportBomb] throwing...");
            while (IsAirBombGoing)
            {
                for (int i = 0; i < 4; i++)
                    Methods.Explode(Methods.RandomSurface());
                yield return Timing.WaitForSeconds(0.1f);
                if (TimeEnd != -1 && TimeEnd <= Time.deltaTime)
                {
                    IsAirBombGoing = false;
                    Log.Debug($"[AirSupportBomb] TimeBombing:{TimeEnd}");
                    break;
                }
                yield return Timing.WaitForSeconds(0.1f);
            }

            Cassie.MessageTranslated("outside zone termination completed", SanyaRemastered.Instance.Translation.CustomSubtitles.AirbombEnded);
            IsActuallyBombGoing = false;
            try
            {
                Methods.DiscordLog(":airplane_arriving: Arrêt  du bombardement\n");
            }
            catch { }
            Log.Debug($"[AirSupportBomb] Ended.");
            try
            {
                Methods.StopSirenAudio();
            }
            catch { }
            yield break;
        }
    }
    internal static class Methods
    {
        internal static Dictionary<float, GameObject> SurfaceBombArea = new();
        internal static float MaxChance;
        public static Vector3 RandomSurface()
        {
            try
            {
                if (SurfaceBombArea.IsEmpty())
                {
                    SchematicObjectDataList Map = MapEditorReborn.API.Features.MapUtils.GetSchematicDataByName("SufaceBomb");
                    if (Map == null)
                        Log.Error("Fail to load (SufaceBomb)");
                    foreach (SchematicBlockData block in Map.Blocks)
                    {
                        GameObject Primitive = GameObject.CreatePrimitive(PrimitiveType.Plane);
                        Primitive.transform.position = block.Position;
                        Primitive.transform.rotation = Quaternion.Euler(block.Rotation);
                        Primitive.transform.localScale = block.Scale;
                        SurfaceBombArea.Add(block.Scale.x * block.Scale.z, Primitive);
                        MaxChance += block.Scale.x * block.Scale.z;
                    }
                }

                float RandomChance = Random.Range(0f, MaxChance);
                GameObject gameObject = null;
                foreach (KeyValuePair<float, GameObject> Area in SurfaceBombArea)
                {
                    RandomChance -= Area.Key;
                    if (RandomChance <= 0)
                    {
                        gameObject = Area.Value;
                        break;
                    }
                }

                // Get the MeshFilter and Mesh of the GameObject
                MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();

                Mesh mesh = meshFilter.mesh;

                // Choose a random triangle from the Mesh
                int triangleIndex = Random.Range(0, mesh.triangles.Length / 3);
                int index1 = mesh.triangles[triangleIndex * 3];
                int index2 = mesh.triangles[triangleIndex * 3 + 1];
                int index3 = mesh.triangles[triangleIndex * 3 + 2];

                // Get the vertices of the triangle
                Vector3 vertex1 = mesh.vertices[index1];
                Vector3 vertex2 = mesh.vertices[index2];
                Vector3 vertex3 = mesh.vertices[index3];

                // Choose a random point on the triangle
                float r1 = Random.Range(0f, 1f);
                float r2 = Random.Range(0f, 1f);

                if (r1 + r2 > 1f)
                {
                    r1 = 1f - r1;
                    r2 = 1f - r2;
                }

                Vector3 randomPoint = vertex1 + r1 * (vertex2 - vertex1) + r2 * (vertex3 - vertex1);

                // Convert the point from local space to world space
                randomPoint = gameObject.transform.TransformPoint(randomPoint) + Vector3.up * 0.25f;

                return randomPoint;
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return Vector3.zero;
            }
        }
        public static void DiscordLog(string message)
    => AudioPlayer.API.AudioController.PlayAudioFromFile(SanyaRemastered.Instance.Config.AudioSoundAirBomb, true, 10, id: Server.Host.Id);

        public static void PlaySirenAudio() 
            => AudioPlayer.API.AudioController.PlayAudioFromFile(SanyaRemastered.Instance.Config.AudioSoundAirBomb, true, 10, id: Server.Host.Id);
        public static void StopSirenAudio() => AudioPlayer.API.AudioController.LoopAudio(false, Server.Host.Id);
        public static void AddPlayerAudio(Player player)
        {
            FakeConnectionList fakeConnection = new()
            {
                fakeConnection = new(player.Id),
                audioplayer = AudioPlayerBase.Get(player.ReferenceHub),
                hubPlayer = player.ReferenceHub,
            };

            fakeConnection.audioplayer.Volume = 1f;
            Plugin.plugin.FakeConnectionsIds.Add(player.Id, fakeConnection);
        }
        public static void RemovePlayerAudio(Player player) => Plugin.plugin.FakeConnectionsIds.Remove(player.Id);
        public static ReferenceHub SpawnDummyModel(Vector3 position, RoleTypeId role, string nick, Vector3 rotation, Vector3 scale)
        {
            try
            {
                GameObject newPlayer = UnityEngine.Object.Instantiate(NetworkManager.singleton.playerPrefab);
                FakeConnection fakeConnection = new();
                ReferenceHub hubPlayer = newPlayer.GetComponent<ReferenceHub>();
                NetworkServer.AddPlayerForConnection(fakeConnection, newPlayer);
                hubPlayer.characterClassManager.InstanceMode = ClientInstanceMode.Unverified;
                hubPlayer.characterClassManager._hub.roleManager.ServerSetRole(role, RoleChangeReason.RemoteAdmin);
                hubPlayer.characterClassManager.GodMode = true;
                newPlayer.GetComponent<NicknameSync>().Network_myNickSync = nick;
                newPlayer.transform.localScale = scale;
                newPlayer.transform.rotation = Quaternion.Euler(rotation);
                newPlayer.transform.position = position;
                return hubPlayer;
            }
            catch (Exception ex)
            { 
                Log.Error("Error In create Dummy " + ex);
                return null;
            }
        }
        public static bool IsStuck(Vector3 pos)
        {
            bool result = false;
            foreach (Collider collider in Physics.OverlapBox(pos, new Vector3(0.4f, 1f, 0.4f), new Quaternion(0f, 0f, 0f, 0f)))
            {
                bool flag = collider.name.Contains("Hitbox") || collider.name.Contains("mixamorig") || collider.name.Equals("Player") || collider.name.Equals("PlyCenter") || collider.name.Equals("Antijumper");
                if (!flag)
                {
                    Log.Debug($"Detect:{collider.name}");
                    result = true;
                }
            }
            return result;
        }
        public static void SpawnGrenade(Vector3 position, ItemType Grenade, float fusedur = -1, Player hub = null)
        {
            hub ??= Server.Host;
            
            switch (Grenade) 
            {
                case ItemType.GrenadeFlash:
                    FlashGrenade Flash = (FlashGrenade)Item.Create(ItemType.GrenadeFlash);
                    if (fusedur is not -1)
                        Flash.FuseTime = fusedur;
                    Flash.SpawnActive(position, hub);
                    return;
                case ItemType.GrenadeHE:
                    ExplosiveGrenade grenade = (ExplosiveGrenade)Item.Create(ItemType.GrenadeHE);
                    if (fusedur is not -1)
                        grenade.FuseTime = fusedur;
                    grenade.SpawnActive(position, hub);
                    return;
                case ItemType.SCP018:
                    ExplosiveGrenade SCP018 = (ExplosiveGrenade)Item.Create(ItemType.SCP018);
                    if (fusedur is not -1)
                        SCP018.FuseTime = fusedur;
                    SCP018.SpawnActive(position, hub);
                    return;
                default:
                    return;
            }
        }
        public static void Explode(Vector3 position, ReferenceHub hub = null)
        {
            hub ??= Server.Host.ReferenceHub;
            if (!CandyPink.TryGetGrenade(out ExplosionGrenade settingsReference))
                return;

            new CandyPink.CandyExplosionMessage
            {
                Origin = position
            }.SendToAuthenticated(0);
            ExplosionGrenade.Explode(new Footprinting.Footprint(hub), position, settingsReference);
        }
        public static void SetParentAndOffset(this Transform target, Transform parent, Vector3 local)
        {
            target.SetParent(parent);
            target.position = parent.position;
            target.transform.localPosition = local;
            var localoffset = parent.transform.TransformVector(target.localPosition);
            target.localPosition = Vector3.zero;
            target.position += localoffset;
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
        public static void SendTextHintNotEffect(this Player player, string text, float time) 
            => player.ReferenceHub.hints.Show(new TextHint(text, new HintParameter[] { new StringHintParameter(string.Empty) }, null, time));

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
        
        public static void MoveNetworkIdentityObject(NetworkIdentity identity, Vector3 pos)
        {
            identity.gameObject.transform.position = pos;
            ObjectDestroyMessage objectDestroyMessage = new()
            {
                netId = identity.netId
            };
            foreach (var ply in Player.List)
            {
                ply.Connection.Send(objectDestroyMessage, 0);
                typeof(NetworkServer).GetMethod("SendSpawnMessage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).Invoke(null, new object[] { identity, ply.Connection });
            }
        }
        public static bool CanLookToPlayer(this Scp079Camera camera, ReferenceHub player)
        {
            Player EPlayer = Player.Dictionary[player.gameObject];
            if (EPlayer.Role.Type is RoleTypeId.Spectator or RoleTypeId.Scp079 or RoleTypeId.None)
                return false;

            Vector3 vector = player.transform.position - camera.transform.position;
            float num = Vector3.Dot(camera._cameraAnchor.transform.forward, vector);

            return (num >= 0f && num * num / vector.sqrMagnitude > 0.4225f)
                && Physics.Raycast(camera.transform.position, vector, out RaycastHit raycastHit, 100f, -117407543)
                && raycastHit.transform.name == player.name;
        }

        // API, dont change
        public static int GetComponentIndex(NetworkIdentity identity, Type type) => Array.FindIndex(identity.NetworkBehaviours, (x) => x.GetType() == type);

        // API, dont change
        public static ulong GetDirtyBit(Type targetType, string PropertyName)
        {
            var bytecodes = targetType.GetProperty(PropertyName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)?.GetSetMethod().GetMethodBody().GetILAsByteArray();
            return bytecodes[Array.FindLastIndex(bytecodes, x => x == System.Reflection.Emit.OpCodes.Ldc_I8.Value) + 1];
        }

       
        public static void SendTargetRPCInternal(NetworkConnection conn, Type invokeClass, string rpcName, NetworkWriter writer, int channelId, uint netid, int componentindex)
        {
            if (!NetworkServer.active)
            {
                Debug.LogError("TargetRPC Function " + rpcName + " called on client.");
                return;
            }
            RpcMessage msg = new()
            {
                netId = netid,
                componentIndex = componentindex,
                functionHash = GetMethodHash(invokeClass, rpcName),
                payload = writer.ToArraySegment()
            };
            conn.Send(msg, channelId);
        }
        private static int GetMethodHash(Type invokeClass, string methodName) => invokeClass.FullName.GetStableHashCode() * 503 + methodName.GetStableHashCode();
    }
    internal static class Extensions
    {
        public static void SendHitmarker(this Player player, float size = 1f) => Hitmarker.SendHitmarker(player.Connection, size);

        public static float GetHealthAmountPercent(this Player player) => 100f - player.Health / player.MaxHealth * 100f;
        public static bool IsExmode(this Player player) => player.SessionVariables.ContainsKey("scp079_advanced_mode");

        public static bool IsList(this Type type) => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);

        public static bool IsDictionary(this Type type) => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>);

        public static Type GetListArgs(this Type type) => type.GetGenericArguments()[0];

        public static T GetRandomOne<T>(this List<T> list) => list[UnityEngine.Random.Range(0, list.Count)];

        public static T Random<T>(this IEnumerable<T> ie) => ie.Any() ? (ie.ElementAt(SanyaRemastered.Instance.Random.Next(ie.Count()))) : default;

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