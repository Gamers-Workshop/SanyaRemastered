/*
using CustomPlayerEffects;
using EXILED;
using EXILED.Extensions;
using Harmony;
using Mirror;
using PlayableScps;
using SanyaPlugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions.Must;
using Utf8Json.Internal.DoubleConversion;

namespace SanyaRemastered.Patches
{
    [HarmonyPatch(typeof(PlayerPositionManager), nameof(PlayerPositionManager.ReceiveData))]
    class PlayerMovementPatches
    {
        public static bool Prefix(PlayerPositionManager __instance)
        {
			if (!__instance._isReadyToWork || __instance._usedData == 0)
			{
				return false;
			}
			if (__instance._myCcm != null)
			{
				for (int i = 0; i < __instance._usedData; i++)
				{
					PlayerPositionData playerPositionData = __instance._receivedData[i];

					ReferenceHub referenceHub;
					if (ReferenceHub.TryGetHub(playerPositionData.playerID, out referenceHub))
					{
						if (!referenceHub.isLocalPlayer)
						{
							CharacterClassManager characterClassManager = referenceHub.characterClassManager;
							if (__instance._myCcm.Classes.CheckBounds(__instance._myCcm.CurClass) && (characterClassManager.CurClass != RoleType.Scp173 || !__instance._myCcm.IsHuman()) && Vector3.Distance(referenceHub.transform.position, playerPositionData.position) < 10f)
							{
								Vector3 VecMove = referenceHub.gameObject.GetComponent<Stamina>()._prevPosition - playerPositionData.position;
								VecMove = VecMove /  2;
								playerPositionData.position = referenceHub.gameObject.GetComponent<Stamina>()._prevPosition + VecMove;
								referenceHub.playerMovementSync.OverridePosition(playerPositionData.position, playerPositionData.rotation);
								Transform transform;
								(transform = referenceHub.transform).position = Vector3.Lerp(referenceHub.transform.position, playerPositionData.position, 0.2f);
								__instance.SetRotation(characterClassManager, Quaternion.Lerp(Quaternion.Euler(transform.rotation.eulerAngles), Quaternion.Euler(Vector3.up * playerPositionData.rotation), 0.3f));
							}
							else
							{
								referenceHub.transform.position = playerPositionData.position;
								__instance.SetRotation(characterClassManager, Quaternion.Euler(0f, playerPositionData.rotation, 0f));
							}
						}
						if (!NetworkServer.active)
						{
							PlayerMovementSync playerMovementSync = referenceHub.playerMovementSync;
							playerMovementSync.RealModelPosition = playerPositionData.position;
							playerMovementSync.Rotations = new Vector2(playerMovementSync.Rotations.x, playerPositionData.rotation);
						}
					}
				}
				__instance._usedData = 0;
				return false;
			}
			__instance._myCcm = PlayerManager.localPlayer.GetComponent<CharacterClassManager>();
			return false;

		}


	}

	[HarmonyPatch(typeof(PlayerPositionManager), nameof(PlayerPositionManager.ReceiveData))]
	class PlayerMovementReceiveDataPatches
	{
		public static bool Prefix(PlayerPositionManager __instance)
		{
			if (!__instance._isReadyToWork || __instance._usedData == 0)
			{
				return false;
			}
			if (__instance._myCcm != null)
			{
				for (int i = 0; i < __instance._usedData; i++)
				{
					PlayerPositionData playerPositionData = __instance._receivedData[i];

					ReferenceHub referenceHub;
					if (ReferenceHub.TryGetHub(playerPositionData.playerID, out referenceHub))
					{
						if (!referenceHub.isLocalPlayer)
						{
							CharacterClassManager characterClassManager = referenceHub.characterClassManager;
							if (__instance._myCcm.Classes.CheckBounds(__instance._myCcm.CurClass) && (characterClassManager.CurClass != RoleType.Scp173 || !__instance._myCcm.IsHuman()) && Vector3.Distance(referenceHub.transform.position, playerPositionData.position) < 10f)
							{
								Vector3 VecMove = referenceHub.gameObject.GetComponent<Stamina>()._prevPosition - playerPositionData.position;
								VecMove = VecMove / 2;
								playerPositionData.position = referenceHub.gameObject.GetComponent<Stamina>()._prevPosition + VecMove;
								referenceHub.playerMovementSync.OverridePosition(playerPositionData.position, playerPositionData.rotation);
								Transform transform;
								(transform = referenceHub.transform).position = Vector3.Lerp(referenceHub.transform.position, playerPositionData.position, 0.2f);
								__instance.SetRotation(characterClassManager, Quaternion.Lerp(Quaternion.Euler(transform.rotation.eulerAngles), Quaternion.Euler(Vector3.up * playerPositionData.rotation), 0.3f));
							}
							else
							{
								referenceHub.transform.position = playerPositionData.position;
								__instance.SetRotation(characterClassManager, Quaternion.Euler(0f, playerPositionData.rotation, 0f));
							}
						}
						if (!NetworkServer.active)
						{
							PlayerMovementSync playerMovementSync = referenceHub.playerMovementSync;
							playerMovementSync.RealModelPosition = playerPositionData.position;
							playerMovementSync.Rotations = new Vector2(playerMovementSync.Rotations.x, playerPositionData.rotation);
						}
					}
				}
				__instance._usedData = 0;
				return false;
			}
			__instance._myCcm = PlayerManager.localPlayer.GetComponent<CharacterClassManager>();
			return false;

		}

		[HarmonyPatch(typeof(PlayerPositionManager), nameof(PlayerPositionManager.FixedUpdate))]

		class PlayerMovementUpdatePatches
		{
			public static bool Prefix(PlayerPositionManager __instance)
			{
				__instance.ReceiveData();
				return false;

			}


		}
	}

		[HarmonyPatch(typeof(PlayerMovementSync), nameof(PlayerMovementSync.AntiCheatKillPlayer))]
	public static class AntiCheatKillDisablePatch
	{
		public static bool Prefix(PlayerMovementSync __instance, string message)
		{
			Log.Warn($"[AntiCheatKill] {__instance._hub.GetNickname()} detect AntiCheat:{message}");
			if(Configs.beta_anticheat_disable)
				return false;
			else
				return true;
		}
	}
}*/