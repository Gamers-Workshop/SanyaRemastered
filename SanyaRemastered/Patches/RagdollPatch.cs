using Exiled.API.Features;
using HarmonyLib;
using UnityEngine;

namespace SanyaRemastered.Patches
{
    [HarmonyPatch(typeof(RagdollManager), nameof(RagdollManager.SpawnRagdoll))]
    public static class PreventRagdollPatch
    {
        public static bool Prefix(RagdollManager __instance, Vector3 pos, Quaternion rot, Vector3 velocity, int classId, PlayerStats.HitInfo ragdollInfo, bool allowRecall, string ownerID, string ownerNick, int playerId, bool _096Death = false)
        {
            if (SanyaRemastered.Instance.Config.Scp106RemoveRagdoll && ragdollInfo.Tool == DamageTypes.Scp106
                || SanyaRemastered.Instance.Config.Scp096RemoveRagdoll && ragdollInfo.Tool == DamageTypes.Scp096) return false;

            if (SanyaRemastered.Instance.Config.Scp049Real && ragdollInfo.Tool != DamageTypes.Scp049) allowRecall = false;

            if (SanyaRemastered.Instance.Config.TeslaDestroyName && ragdollInfo.Tool == DamageTypes.Tesla) ownerNick = "inconue";

            Role role = __instance.hub.characterClassManager.Classes.SafeGet(classId);
            if (role.model_ragdoll == null)
            {
                return false;
            }
            GameObject gameObject = Object.Instantiate(role.model_ragdoll, pos + role.ragdoll_offset.position, Quaternion.Euler(rot.eulerAngles + role.ragdoll_offset.rotation));
            
            gameObject.transform.localScale = new Vector3(Player.Get(playerId).Scale.x * gameObject.transform.localScale.x, 
                                                          Player.Get(playerId).Scale.y * gameObject.transform.localScale.y, 
                                                          Player.Get(playerId).Scale.z * gameObject.transform.localScale.z);

            Mirror.NetworkServer.Spawn(gameObject);
            Ragdoll component = gameObject.GetComponent<Ragdoll>();
            component.Networkowner = new Ragdoll.Info(ownerID, ownerNick, ragdollInfo, role, playerId);
            component.NetworkallowRecall = allowRecall;
            component.NetworkPlayerVelo = velocity;
            component.NetworkSCP096Death = _096Death;
            return false;
        }
    }
}