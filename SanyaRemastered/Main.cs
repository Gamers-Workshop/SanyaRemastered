using System;
using Exiled.API.Enums;
using Exiled.API.Features;
using HarmonyLib;

using ServerEvents = Exiled.Events.Handlers.Server;
using MapEvents = Exiled.Events.Handlers.Map;
using WarheadEvents = Exiled.Events.Handlers.Warhead;
using PlayerEvents = Exiled.Events.Handlers.Player;
using Scp079Events = Exiled.Events.Handlers.Scp079;
using Scp914Events = Exiled.Events.Handlers.Scp914;
using Scp106Events = Exiled.Events.Handlers.Scp106;
using Scp096Events = Exiled.Events.Handlers.Scp096;
using System.Collections.Generic;
using NorthwoodLib.Pools;
using System.Reflection.Emit;
using Mirror;
using System.Reflection;

namespace SanyaRemastered
{
	public class SanyaRemastered : Plugin<Configs>
	{
		public override string Name => "SanyaRemastered";
		public override string Prefix => "sanya";
		public override string Author => "sanyae2439";
		public override PluginPriority Priority => (PluginPriority) 1;

		public static SanyaRemastered Instance { get; private set; }
		public EventHandlers Handlers { get; private set; }
		private Harmony Harmony = new Harmony("dev.Yamato");
		public Random Random { get; } = new Random();
		private int patchCounter;

		private readonly MethodInfo _prefixToPatch = AccessTools.Method("Exiled.Events.Patches.Events.Player.ActivatingWarheadPanel:Prefix");
		private readonly HarmonyMethod _transpiler = new HarmonyMethod(typeof(SanyaRemastered), nameof(ExiledPrefixPatch));

		public SanyaRemastered() => Instance = this;

		public override void OnEnabled()
		{
			if (!Config.IsEnabled) return;

			base.OnEnabled();

			RegistEvents();
			Config.ParseConfig();

			Harmony.Patch(_prefixToPatch, transpiler: _transpiler);
			RegistPatch();

			Log.Info($"[OnEnabled] SanyaRemastered({Version}) Enabled Complete.");
		}

		public override void OnDisabled()
		{
			base.OnDisabled();

			foreach (var cor in Handlers.RoundCoroutines)
				MEC.Timing.KillCoroutines(cor);
			Handlers.RoundCoroutines.Clear();

			UnRegistEvents();

			Harmony.Unpatch(_prefixToPatch, _transpiler.method);
			UnRegistPatch();

			Log.Info($"[OnDisable] SanyaRemastered({Version}) Disabled Complete.");
		}

		private void RegistEvents()
		{
			Handlers = new EventHandlers(this);
			new Scp096Helper();
			ServerEvents.SendingConsoleCommand += Handlers.OnCommand;
			ServerEvents.WaitingForPlayers += Handlers.OnWaintingForPlayers;
			ServerEvents.RoundStarted += Handlers.OnRoundStart;
			ServerEvents.RoundEnded += Handlers.OnRoundEnd;
			ServerEvents.RestartingRound += Handlers.OnRoundRestart;
			ServerEvents.RespawningTeam += Handlers.OnTeamRespawn;
			
			WarheadEvents.Starting += Handlers.OnWarheadStart;
			WarheadEvents.Stopping += Handlers.OnWarheadCancel;
			WarheadEvents.Detonated += Handlers.OnDetonated;
			
			MapEvents.AnnouncingDecontamination += Handlers.OnAnnounceDecont;
			MapEvents.AnnouncingNtfEntrance += Handlers.OnAnnounceNtf;

			MapEvents.PlacingDecal += Handlers.OnPlacingDecal;
			MapEvents.ExplodingGrenade += Handlers.OnExplodingGrenade;
			MapEvents.GeneratorActivated += Handlers.OnGeneratorFinish;
			
			PlayerEvents.InteractingElevator += Handlers.OnInteractingElevator;
			PlayerEvents.PreAuthenticating += Handlers.OnPreAuth;
			PlayerEvents.Verified += Handlers.OnPlayerVerified;
			PlayerEvents.Destroying += Handlers.OnPlayerDestroying;
			PlayerEvents.ChangingRole += Handlers.OnPlayerSetClass;
			PlayerEvents.Spawning += Handlers.OnPlayerSpawn;
			PlayerEvents.Hurting += Handlers.OnPlayerHurt;
			PlayerEvents.Died += Handlers.OnDied;
			PlayerEvents.FailingEscapePocketDimension  += Handlers.OnPocketDimDeath;
			PlayerEvents.MedicalItemUsed += Handlers.OnPlayerUsedMedicalItem;
			PlayerEvents.TriggeringTesla += Handlers.OnPlayerTriggerTesla;
			PlayerEvents.InteractingDoor += Handlers.OnPlayerDoorInteract;
			PlayerEvents.InteractingLocker += Handlers.OnPlayerLockerInteract;
			PlayerEvents.SyncingData += Handlers.OnSyncingData;
			PlayerEvents.IntercomSpeaking += Handlers.OnIntercomSpeaking;

			PlayerEvents.Shooting += Handlers.OnShoot;
			PlayerEvents.UnlockingGenerator += Handlers.OnGeneratorUnlock;
			PlayerEvents.EjectingGeneratorTablet += Handlers.OnEjectingGeneratorTablet;
			PlayerEvents.OpeningGenerator  += Handlers.OnGeneratorOpen;
			PlayerEvents.ClosingGenerator += Handlers.OnGeneratorClose;
			PlayerEvents.InsertingGeneratorTablet += Handlers.OnGeneratorInsert;
			PlayerEvents.ActivatingWarheadPanel += Handlers.OnActivatingWarheadPanel;

			Scp106Events.CreatingPortal += Handlers.On106MakePortal;
			Scp106Events.Teleporting += Handlers.On106Teleport;
			Scp079Events.GainingLevel += Handlers.On079LevelGain;
			Scp914Events.UpgradingItems += Handlers.On914Upgrade;
		}

		private void UnRegistEvents()
		{
			ServerEvents.SendingConsoleCommand -= Handlers.OnCommand;
			ServerEvents.WaitingForPlayers -= Handlers.OnWaintingForPlayers;
			ServerEvents.RoundStarted -= Handlers.OnRoundStart;
			ServerEvents.RoundEnded -= Handlers.OnRoundEnd;
			ServerEvents.RestartingRound -= Handlers.OnRoundRestart;
			ServerEvents.RespawningTeam -= Handlers.OnTeamRespawn;
			
			WarheadEvents.Starting -= Handlers.OnWarheadStart;
			WarheadEvents.Stopping -= Handlers.OnWarheadCancel;
			WarheadEvents.Detonated -= Handlers.OnDetonated;
			
			MapEvents.AnnouncingDecontamination -= Handlers.OnAnnounceDecont;
			MapEvents.AnnouncingNtfEntrance -= Handlers.OnAnnounceNtf;

			MapEvents.PlacingDecal -= Handlers.OnPlacingDecal;
			MapEvents.ExplodingGrenade -= Handlers.OnExplodingGrenade;
			MapEvents.GeneratorActivated -= Handlers.OnGeneratorFinish;
			
			PlayerEvents.InteractingElevator -= Handlers.OnInteractingElevator;
			PlayerEvents.PreAuthenticating -= Handlers.OnPreAuth;
			PlayerEvents.Verified -= Handlers.OnPlayerVerified;
			PlayerEvents.Destroying -= Handlers.OnPlayerDestroying;
			PlayerEvents.ChangingRole -= Handlers.OnPlayerSetClass;
			PlayerEvents.Spawning -= Handlers.OnPlayerSpawn;
			PlayerEvents.Hurting -= Handlers.OnPlayerHurt;
			PlayerEvents.Died -= Handlers.OnDied;
			PlayerEvents.FailingEscapePocketDimension -= Handlers.OnPocketDimDeath;
			PlayerEvents.MedicalItemUsed -= Handlers.OnPlayerUsedMedicalItem;
			PlayerEvents.TriggeringTesla -= Handlers.OnPlayerTriggerTesla;
			PlayerEvents.InteractingDoor -= Handlers.OnPlayerDoorInteract;
			PlayerEvents.InteractingLocker -= Handlers.OnPlayerLockerInteract;
			PlayerEvents.IntercomSpeaking += Handlers.OnIntercomSpeaking;

			PlayerEvents.Shooting -= Handlers.OnShoot;
			PlayerEvents.SyncingData -= Handlers.OnSyncingData;
			PlayerEvents.UnlockingGenerator -= Handlers.OnGeneratorUnlock;
			PlayerEvents.OpeningGenerator -= Handlers.OnGeneratorOpen;
			PlayerEvents.ClosingGenerator -= Handlers.OnGeneratorClose;
			PlayerEvents.InsertingGeneratorTablet -= Handlers.OnGeneratorInsert;
			PlayerEvents.ActivatingWarheadPanel -= Handlers.OnActivatingWarheadPanel;

			Scp106Events.CreatingPortal -= Handlers.On106MakePortal;
			Scp106Events.Teleporting -= Handlers.On106Teleport;
			Scp079Events.GainingLevel -= Handlers.On079LevelGain;
			Scp914Events.UpgradingItems -= Handlers.On914Upgrade;

			Handlers = null;
		}

		private void RegistPatch()
		{
			try
			{
				Harmony = new Harmony(Author + "." + Name + ++patchCounter);
				Harmony.PatchAll();
			}
			catch (Exception ex)
			{
				Log.Error($"[RegistPatch] Patching Failed : {ex}");
			}
		}

		private void UnRegistPatch()
		{
			Harmony.UnpatchAll();
		}
		private static IEnumerable<CodeInstruction> ExiledPrefixPatch(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
		{
			var newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);

			Predicate<CodeInstruction> searchPredicate = i => i.opcode == OpCodes.Ldloc_1;

			var index = newInstructions.FindIndex(searchPredicate);
			var label = newInstructions[index + 1].operand;

			newInstructions.RemoveRange(index, 2);

			newInstructions.InsertRange(index, new[]
			{
				new CodeInstruction(OpCodes.Ldarg_0),
				new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PlayerInteract), nameof(PlayerInteract._inv))),
				new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Inventory), nameof(Inventory.items))),
				new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(SyncList<Inventory.SyncItemInfo>), nameof(SyncList<Inventory.SyncItemInfo>.Count))),
				new CodeInstruction(OpCodes.Ldc_I4_0),
				new CodeInstruction(OpCodes.Ceq),
				new CodeInstruction(OpCodes.Brfalse, label)
			});

			index = newInstructions.FindIndex(index, searchPredicate);
			label = newInstructions[newInstructions.FindIndex(index, i => i.opcode == OpCodes.Br_S || i.opcode == OpCodes.Br)].operand;

			var notNullLabel = generator.DefineLabel();
			newInstructions[index].WithLabels(notNullLabel);
			newInstructions.InsertRange(index, new[]
			{
				new CodeInstruction(OpCodes.Ldloc_1),
				new CodeInstruction(OpCodes.Brtrue, notNullLabel),
				new CodeInstruction(OpCodes.Ldc_I4_0),
				new CodeInstruction(OpCodes.Br, label)
			});

			for (var z = 0; z < newInstructions.Count; z++)
				yield return newInstructions[z];

			ListPool<CodeInstruction>.Shared.Return(newInstructions);
		}
	}
}