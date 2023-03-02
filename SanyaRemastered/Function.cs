using Exiled.API.Features;
using Exiled.API.Features.Items;
using Hints;
using InventorySystem.Items.ThrowableProjectiles;
using InventorySystem.Items.Usables.Scp330;
using MEC;
using Mirror;
using NorthwoodLib.Pools;
using PlayerRoles;
using PlayerRoles.PlayableScps.Scp079.Cameras;
using RemoteAdmin;
using Respawning;
using RoundRestarting;
using SanyaRemastered.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using UnityEngine;
using Utils.Networking;
using Camera = Exiled.API.Features.Camera;

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
                DiscordLog.DiscordLog.Instance.LOG += ":airplane_arriving: Arrêt  du bombardement\n";
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
                    Log.Info($"[AirSupportBomb] The AirBomb as stop");
                    yield break;
                }
                AirBombWait--;
                yield return Timing.WaitForSeconds(1);
            }
            if (!IsAirBombGoing)
                yield break;
            IsActuallyBombGoing = true;
            Log.Info($"[AirSupportBomb] booting...");
            DiscordLog.DiscordLog.Instance.LOG += ":airplane: Bombardement en cours\n";
            //AudioPlayer.API.AudioController.PlayFromFile("/home/scp/.config/EXILED/Configs/AudioAPI/Siren.mp3", 60, true);
            Cassie.MessageTranslated("danger . the outside zone emergency termination sequence activated \n the air bomb cant be Avoid", SanyaRemastered.Instance.Translation.CustomSubtitles.AirbombStarting);

            yield return Timing.WaitForSeconds(5f);
            Log.Info($"[AirSupportBomb] charging...");
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
            Log.Info($"[AirSupportBomb] throwing...");
            while (IsAirBombGoing)
            {
                List<Vector3> randampos = OutsideRandomAirbombPos.Load().OrderBy(x => Guid.NewGuid()).ToList();
                foreach (var pos in randampos)
                {
                    Methods.Explode(pos);
                    yield return Timing.WaitForSeconds(0.1f);
                }
                if (TimeEnd != -1 && TimeEnd <= Time.deltaTime)
                {
                    IsAirBombGoing = false;
                    Log.Info($"[AirSupportBomb] TimeBombing:{TimeEnd}");
                    break;
                }
                yield return Timing.WaitForSeconds(0.25f);
            }

            Cassie.MessageTranslated("outside zone termination completed", SanyaRemastered.Instance.Translation.CustomSubtitles.AirbombEnded);
            IsActuallyBombGoing = false;
            DiscordLog.DiscordLog.Instance.LOG += ":airplane_arriving: Arrêt  du bombardement\n";
            Log.Info($"[AirSupportBomb] Ended.");
            //AudioPlayer.API.AudioController.LoopMusic = false;
            yield break;
        }
    }
    internal static class Methods
    {
        public static void SpawnDummyModel(Vector3 position, RoleTypeId role, string nick, Quaternion rotation, Vector3 scale)
        {
            try
            {
                GameObject gameObject = UnityEngine.Object.Instantiate(NetworkManager.singleton.playerPrefab);
                CharacterClassManager characterClassManager = gameObject.GetComponent<CharacterClassManager>();
                QueryProcessor queryProcessor = gameObject.GetComponent<QueryProcessor>();
                gameObject.transform.localScale = scale;
                gameObject.transform.rotation = rotation;
                gameObject.transform.position = position;
                characterClassManager._hub.roleManager.ServerSetRole(role, RoleChangeReason.RemoteAdmin);
                characterClassManager.GodMode = true;
                gameObject.GetComponent<NicknameSync>().Network_myNickSync = nick;
                NetworkServer.Spawn(gameObject);
            }
            catch (Exception ex)
            { Log.Error("Error In create Dummy " + ex); }
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