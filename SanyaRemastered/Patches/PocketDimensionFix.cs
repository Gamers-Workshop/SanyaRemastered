using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanyaRemastered.Patches
{
    [HarmonyPatch(typeof(PocketDimensionGenerator), nameof(PocketDimensionGenerator.GenerateRandom))]
    public static class PocketDimensionFix
    {
        public static bool Prefix()
        {
            return GameCore.ConfigFile.ServerConfig.GetBool("pd_refresh_exit", true);
		}
    }
}
