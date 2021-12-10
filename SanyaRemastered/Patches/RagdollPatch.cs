using Exiled.API.Features;
using HarmonyLib;
using Mirror;
using PlayerStatsSystem;
using UnityEngine;

namespace SanyaRemastered.Patches
{
    [HarmonyPatch(typeof(Ragdoll), nameof(Ragdoll.ServerSpawnRagdoll))]
    public static class PreventRagdollPatch
    {
        public static bool Prefix(ReferenceHub hub, DamageHandlerBase handler)
        {
            if (!NetworkServer.active || hub == null)
            {
                return false;
            }

            GameObject model_ragdoll = hub.characterClassManager.CurRole.model_ragdoll;
            Ragdoll ragdoll;
            if (model_ragdoll == null || !Object.Instantiate(model_ragdoll).TryGetComponent(out ragdoll))
            {
                return false;
            }
            ragdoll.NetworkInfo = new RagdollInfo(hub, handler, model_ragdoll.transform.localPosition, model_ragdoll.transform.localRotation);
            //PlayerSizeForTheRagdoll
            Player Owner = Player.Get(hub);
            ragdoll.transform.localScale = new Vector3(Owner.Scale.x * ragdoll.transform.localScale.x,
                                              Owner.Scale.y * ragdoll.transform.localScale.y,
                                              Owner.Scale.z * ragdoll.transform.localScale.z);

            NetworkServer.Spawn(ragdoll.gameObject,(NetworkConnection) null);
            return false;
            /*
            
            if (SanyaRemastered.Instance.Config.Scp106RemoveRagdoll && ragdollInfo.Tool == DamageTypes.Scp106
                || SanyaRemastered.Instance.Config.Scp096RemoveRagdoll && ragdollInfo.Tool == DamageTypes.Scp096) return false;

            if (SanyaRemastered.Instance.Config.Scp049Real && ragdollInfo.Tool != DamageTypes.Scp049) allowRecall = false;

            if (SanyaRemastered.Instance.Config.TeslaDestroyName && ragdollInfo.Tool == DamageTypes.Tesla) ownerNick = "inconue";
            Role role = __instance.hub.characterClassManager.Classes.SafeGet(classId);
            if (role.model_ragdoll == null)
                return false;



            GameObject gameObject = Object.Instantiate(role.model_ragdoll, pos + role.ragdoll_offset.position, Quaternion.Euler(rot.eulerAngles + role.ragdoll_offset.rotation));
            if (Owner.TryGetSessionVariable("NewRole", out System.Tuple<string, string> newrole))
                role.fullName = newrole.Item2;


            Mirror.NetworkServer.Spawn(gameObject);
            Ragdoll component = gameObject.GetComponent<Ragdoll>();
            component.Networkowner = new Ragdoll.Info(ownerID, ownerNick, ragdollInfo, role, playerId);
            component.NetworkallowRecall = allowRecall;
            component.NetworkPlayerVelo = velocity;
            component.NetworkSCP096Death = _096Death;
            return false;*/
        }
    }
}