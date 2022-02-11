using HarmonyLib;
using InventorySystem.Items.Usables.Scp244;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static HarmonyLib.AccessTools;

namespace SanyaRemastered.Patches
{
    [HarmonyPatch(typeof(Scp244HypothermiaHandler), nameof(Scp244HypothermiaHandler.HandlePlayer))]
    public static class NaoIssue
    {
        public static bool Prefix(ReferenceHub ply)
        {
			if (!ply.characterClassManager.IsAlive)
			{
				return false;
			}
			float num = 0f;
			CustomPlayerEffects.Hypothermia effect = ply.playerEffectsController.GetEffect<CustomPlayerEffects.Hypothermia>();
			foreach (Scp244DeployablePickup scp244DeployablePickup in Scp244DeployablePickup.Instances)
			{
				num += scp244DeployablePickup.FogPercentForPoint(ply.PlayerCameraReference.position);
			}
			byte b = (byte)Mathf.Clamp(Mathf.RoundToInt(num * 10f), 0, 255);
			if (effect.Intensity != b)
			{
				effect.Intensity = b;
			}
			return false;
		}
    }
}