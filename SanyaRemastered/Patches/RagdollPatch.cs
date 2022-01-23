using Exiled.API.Enums;
using Exiled.API.Features;
using HarmonyLib;
using Mirror;
using PlayerStatsSystem;
using UnityEngine;

namespace SanyaRemastered.Patches
{
    [HarmonyPatch(typeof(Ragdoll), nameof(Ragdoll.ServerSpawnRagdoll))] // Fait kick le joueur 
    public static class PreventRagdollPatch
    {
        public static bool Prefix(ReferenceHub hub, DamageHandlerBase handler)
        {
            try 
            {
                if (!NetworkServer.active || hub == null)
                {
                    return false;
                }
                GameObject model_ragdoll = hub.characterClassManager.CurRole.model_ragdoll;
                if (model_ragdoll == null || !Object.Instantiate(model_ragdoll).TryGetComponent(out Ragdoll ragdoll))
                {
                    return false;
                }
                Player player = Player.Get(hub);
                DamageHandler damage = new DamageHandler(player, handler);
                string nick = hub.nicknameSync.DisplayName;
                double time = NetworkTime.time;
                //Disable Ragdoll
                if (SanyaRemastered.Instance.Config.Scp106RemoveRagdoll && damage.Type == DamageType.Scp106
                    || SanyaRemastered.Instance.Config.Scp096RemoveRagdoll && damage.Type == DamageType.Scp096) return false;
                //Disable Recall By 079
                if (SanyaRemastered.Instance.Config.Scp049Real && damage.Type != DamageType.Scp049) time = double.MinValue;
                //TeslaDestroyTheNameOfThePlayer
                if (SanyaRemastered.Instance.Config.TeslaDestroyName && damage.Type == DamageType.Tesla) nick = "inconue";


                ragdoll.NetworkInfo = new RagdollInfo(hub, handler, player.Role, model_ragdoll.transform.localPosition + hub.transform.position, model_ragdoll.transform.localRotation * hub.transform.rotation, nick, time);

                //PlayerSizeForTheRagdoll
                Player Owner = Player.Get(hub);
                ragdoll.transform.localScale = new Vector3(Owner.Scale.x * ragdoll.transform.localScale.x,
                                                  Owner.Scale.y * ragdoll.transform.localScale.y,
                                                  Owner.Scale.z * ragdoll.transform.localScale.z);

                NetworkServer.Spawn(ragdoll.gameObject);
                return false;
            }
            catch(System.Exception ex)
            {
                Log.Error(ex);
                return true;
            }

        }
    }
}