
namespace SanyaRemastered.Patches
{
    //[HarmonyPatch(typeof(AnimationController), nameof(AnimationController.PlaySound))]
    class AnimationControllerPatches
    {
        public static bool Prefix(AnimationController __instance, int id, bool isGun)
        {
            if (__instance.isLocalPlayer)
            {
                return false;
            }
            if (isGun)
            {
                __instance.gunSource.PlayOneShot(__instance.clips[id].audio);
                return false;
            }
            __instance.runSource.PlayOneShot(__instance.clips[id].audio);
            return false;
        }
    }
}
