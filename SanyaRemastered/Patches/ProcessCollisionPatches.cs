using HarmonyLib;
using InventorySystem.Items.Pickups;
using PlayerStatsSystem;
using UnityEngine;

namespace SanyaRemastered.Patches
{
    [HarmonyPatch(typeof(CollisionDetectionPickup), nameof(CollisionDetectionPickup.ProcessCollision))]
    public static class ProcessCollisionPatches
    {
        public static bool Prefix(CollisionDetectionPickup __instance, Collision collision)
        {
            float sqrMagnitude = collision.relativeVelocity.sqrMagnitude;
            float num = __instance.Info.Weight * sqrMagnitude / 2f;
            __instance.MakeCollisionSound(sqrMagnitude);

            if (num <= 30f || !collision.collider.TryGetComponent(out BreakableWindow breakableWindow)) return false;

            float damage = num * 0.25f;
            breakableWindow.Damage(damage, new ScpDamageHandler(__instance.PreviousOwner.Hub, damage, DeathTranslations.Crushed), Vector3.zero);

            return false;
        }
    }
}
