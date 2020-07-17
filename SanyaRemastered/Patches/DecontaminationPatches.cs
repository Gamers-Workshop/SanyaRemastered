using HarmonyLib;
using UnityEngine;
/*
[HarmonyPatch(typeof(AnnounceDecontaminationEvent), nameof(AnnounceDecontaminationEvent.AnnouncementId), MethodType.Setter)]
public static class EXILEDAnnounceDecontaminationEvent
{
	public static bool Prefix(AnnounceDecontaminationEvent __instance, ref int value)
	{
		AccessTools.Field(typeof(AnnounceDecontaminationEvent), "announcementId").SetValue(__instance, Mathf.Clamp(value, 0, 6));
		return false;
	}
}*/