using Exiled.API.Features;
using HarmonyLib;
using InventorySystem.Items.Pickups;
using InventorySystem.Items.ThrowableProjectiles;
using MEC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SanyaRemastered.Patches
{
    public static class TeslaInstantBurstPatches
    {
        [HarmonyPatch(typeof(TeslaGate), nameof(TeslaGate.UserCode_RpcInstantBurst))]
        public static void Postfix(TeslaGate __instance)
        {
            if (SanyaRemastered.Instance.Config.ExplodingGrenadeTesla)
            {
                Timing.RunCoroutine(TeslaPatches2.ExplodeGrenadeInTesla(__instance,true), Segment.FixedUpdate);
            }
        }

    }
    public static class PlayTeslaAnimationPatches
    {
        [HarmonyPatch(typeof(TeslaGate), nameof(TeslaGate.UserCode_RpcPlayAnimation))]
        public static void Postfix(TeslaGate __instance)
        {
            if (SanyaRemastered.Instance.Config.ExplodingGrenadeTesla)
            {
                Timing.RunCoroutine(TeslaPatches2.ExplodeGrenadeInTesla(__instance, false), Segment.FixedUpdate);
            }
        } 
    }
    public static class TeslaPatches2
    {
        public static IEnumerator<float> ExplodeGrenadeInTesla(TeslaGate __instance, bool Instant)
        {
            if (!Instant)
                yield return Timing.WaitForSeconds(0.5f);
            ExplodeGrenade(__instance);
            yield return Timing.WaitForSeconds(0.25f);
            ExplodeGrenade(__instance);
            yield return Timing.WaitForSeconds(0.25f);
            ExplodeGrenade(__instance);
            yield break;
        }
        public static void ExplodeGrenade(TeslaGate __instance)
        {
            foreach (GameObject gameObject in __instance.killers)
            {
                if (!(gameObject == null))
                {
                    Log.Info("__instance.sizeOfKiller = " + __instance.sizeOfKiller);
                    foreach (Collider collider in Physics.OverlapBox(gameObject.transform.position + Vector3.up * (__instance.sizeOfKiller.y / 2), new Vector3(__instance.sizeOfKiller.x, __instance.sizeOfKiller.y * 2, __instance.sizeOfKiller.z) / 2f, default))
                    {
                        Log.Debug("Collider :" + collider.gameObject);
                        if (collider.TryGetComponent<ItemPickupBase>(out var pickup))
                        {
                            Log.Debug("pickup :" + pickup.Info.ItemId);
                            if (pickup.Info.ItemId == (ItemType.GrenadeHE | ItemType.GrenadeFlash))
                            {
                                Functions.Methods.SpawnGrenade(pickup.Info.Position, pickup.Info.ItemId, 0.1f);
                                pickup.DestroySelf();
                            }
                            var thrownProjectile = pickup.transform.GetComponentInParent<ThrownProjectile>();
                            Log.Debug("ThrownProjectile is null: " + thrownProjectile == null, SanyaRemastered.Instance.Config.IsDebugged);
                            if (thrownProjectile != null && thrownProjectile.Info.ItemId != ItemType.SCP018)
                            {
                                Log.Debug("NoSCP018", SanyaRemastered.Instance.Config.IsDebugged);
                                var timeGrenade = pickup.transform.GetComponentInParent<TimeGrenade>();
                                Log.Debug("timeGrenade is null: " + timeGrenade == null, SanyaRemastered.Instance.Config.IsDebugged);
                                if (timeGrenade != null)
                                {
                                    timeGrenade.TargetTime = 0.1f;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}