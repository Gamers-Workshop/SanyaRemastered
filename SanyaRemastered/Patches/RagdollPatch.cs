using HarmonyLib;

[HarmonyPatch(typeof(RagdollManager), nameof(RagdollManager.SpawnRagdoll))]
public static class PreventRagdollPatch
{
	public static bool Prefix(RagdollManager __instance, PlayerStats.HitInfo ragdollInfo)
	{
		if (SanyaPlugin.SanyaPlugin.Instance.Config.Scp939RemoveRagdoll && ragdollInfo.GetDamageType() == DamageTypes.Scp939) return false;
		else if (SanyaPlugin.SanyaPlugin.Instance.Config.Scp096RemoveRagdoll && ragdollInfo.GetDamageType() == DamageTypes.Scp096) return false;
		else return true;
	}
}