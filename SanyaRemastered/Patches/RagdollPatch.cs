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
        }
    }
}