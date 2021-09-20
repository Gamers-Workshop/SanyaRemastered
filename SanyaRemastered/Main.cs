using System;
using Exiled.API.Enums;
using Exiled.API.Features;
using HarmonyLib;

using ServerEvents = Exiled.Events.Handlers.Server;
using MapEvents = Exiled.Events.Handlers.Map;
using WarheadEvents = Exiled.Events.Handlers.Warhead;
using ItemEvents = Exiled.Events.Handlers.Item;

using PlayerEvents = Exiled.Events.Handlers.Player;
using Scp079Events = Exiled.Events.Handlers.Scp079;
using Scp914Events = Exiled.Events.Handlers.Scp914;
using Scp106Events = Exiled.Events.Handlers.Scp106;
using Scp096Events = Exiled.Events.Handlers.Scp096;
using Scp049Events = Exiled.Events.Handlers.Scp049;

using System.Collections.Generic;
using NorthwoodLib.Pools;
using System.Reflection.Emit;
using Mirror;
using System.Reflection;
using System.Diagnostics;

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

		private readonly MethodInfo _methodToPatch = AccessTools.Method("PlayerInteract:CallCmdSwitchAWButton");

		public SanyaRemastered() => Instance = this;
		private Ram.MemoryService _memoryService;

		public override void OnEnabled()
		{
			if (!Config.IsEnabled) return;
			
			int processId = Process.GetCurrentProcess().Id;
			_memoryService = new Ram.MemoryService(processId);
			//Only run if memory is going to be asked for
			if (Config.RamInfo)
			{
				_memoryService.BackgroundWorker.RunWorkerAsync();
			}

			base.OnEnabled();

			RegistEvents();

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

			UnRegistPatch();

			Log.Info($"[OnDisable] SanyaRemastered({Version}) Disabled Complete.");
		}

		private void RegistEvents()
		{
			Handlers = new EventHandlers(this);

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

			MapEvents.ExplodingGrenade += Handlers.OnExplodingGrenade;
			MapEvents.GeneratorActivated += Handlers.OnGeneratorFinish;

			PlayerEvents.PreAuthenticating += Handlers.OnPreAuth;
			PlayerEvents.Verified += Handlers.OnPlayerVerified;
			PlayerEvents.Destroying += Handlers.OnPlayerDestroying;
			PlayerEvents.ChangingRole += Handlers.OnPlayerSetClass;
			PlayerEvents.Spawning += Handlers.OnPlayerSpawn;
			PlayerEvents.Hurting += Handlers.OnPlayerHurt;
			PlayerEvents.Died += Handlers.OnDied;
			PlayerEvents.EscapingPocketDimension -= Handlers.OnEscapingPocketDimension;
			PlayerEvents.FailingEscapePocketDimension  += Handlers.OnPocketDimDeath;
			PlayerEvents.ItemUsed += Handlers.OnPlayerItemUsed;
			PlayerEvents.TriggeringTesla += Handlers.OnPlayerTriggerTesla;
			PlayerEvents.InteractingDoor += Handlers.OnPlayerDoorInteract;
			PlayerEvents.InteractingLocker += Handlers.OnPlayerLockerInteract;
			PlayerEvents.InteractingElevator += Handlers.OnInteractingElevator;
			PlayerEvents.IntercomSpeaking += Handlers.OnIntercomSpeaking;

			PlayerEvents.Shooting += Handlers.OnShoot;
			PlayerEvents.SyncingData += Handlers.OnSyncingData;
			PlayerEvents.ActivatingWarheadPanel += Handlers.OnActivatingWarheadPanel;
			PlayerEvents.UnlockingGenerator += Handlers.OnGeneratorUnlock;
			PlayerEvents.StoppingGenerator += Handlers.OnStoppingGenerator;
			PlayerEvents.OpeningGenerator  += Handlers.OnGeneratorOpen;
			PlayerEvents.ClosingGenerator += Handlers.OnGeneratorClose;
			PlayerEvents.ActivatingGenerator += Handlers.OnActivatingGenerator;
			PlayerEvents.Handcuffing += Handlers.OnHandcuffing;

			Scp079Events.GainingLevel += Handlers.On079LevelGain;
			Scp106Events.CreatingPortal += Handlers.On106MakePortal;
			Scp106Events.Teleporting += Handlers.On106Teleport;
			Scp914Events.UpgradingPlayer += Handlers.On914UpgradingPlayer;

			Scp096Events.AddingTarget += Handlers.On096AddingTarget;
			Scp096Events.Enraging += Handlers.On096Enraging;
			Scp096Events.CalmingDown += Handlers.On096CalmingDown;
			Scp049Events.FinishingRecall += Handlers.On049FinishingRecall;
			Scp049Events.StartingRecall += Handlers.On049StartingRecall;
		}

		private void UnRegistEvents()
		{
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

			MapEvents.ExplodingGrenade -= Handlers.OnExplodingGrenade;
			MapEvents.GeneratorActivated -= Handlers.OnGeneratorFinish;

			PlayerEvents.PreAuthenticating -= Handlers.OnPreAuth;
			PlayerEvents.Verified -= Handlers.OnPlayerVerified;
			PlayerEvents.Destroying -= Handlers.OnPlayerDestroying;
			PlayerEvents.ChangingRole -= Handlers.OnPlayerSetClass;
			PlayerEvents.Spawning -= Handlers.OnPlayerSpawn;
			PlayerEvents.Hurting -= Handlers.OnPlayerHurt;
			PlayerEvents.Died -= Handlers.OnDied;
			PlayerEvents.EscapingPocketDimension -= Handlers.OnEscapingPocketDimension;
			PlayerEvents.FailingEscapePocketDimension -= Handlers.OnPocketDimDeath;
			PlayerEvents.ItemUsed -= Handlers.OnPlayerItemUsed;
			PlayerEvents.TriggeringTesla -= Handlers.OnPlayerTriggerTesla;
			PlayerEvents.InteractingDoor -= Handlers.OnPlayerDoorInteract;
			PlayerEvents.InteractingLocker -= Handlers.OnPlayerLockerInteract;
			PlayerEvents.InteractingElevator -= Handlers.OnInteractingElevator;
			PlayerEvents.IntercomSpeaking -= Handlers.OnIntercomSpeaking;

			PlayerEvents.Shooting -= Handlers.OnShoot;
			PlayerEvents.SyncingData -= Handlers.OnSyncingData;
			PlayerEvents.ActivatingWarheadPanel -= Handlers.OnActivatingWarheadPanel;
			PlayerEvents.UnlockingGenerator -= Handlers.OnGeneratorUnlock;
			PlayerEvents.StoppingGenerator -= Handlers.OnStoppingGenerator;
			PlayerEvents.OpeningGenerator -= Handlers.OnGeneratorOpen;
			PlayerEvents.ClosingGenerator -= Handlers.OnGeneratorClose;
			PlayerEvents.ActivatingGenerator -= Handlers.OnActivatingGenerator;
			PlayerEvents.Handcuffing -= Handlers.OnHandcuffing;

			Scp079Events.GainingLevel -= Handlers.On079LevelGain;
			Scp106Events.CreatingPortal -= Handlers.On106MakePortal;
			Scp106Events.Teleporting -= Handlers.On106Teleport;
			Scp914Events.UpgradingPlayer -= Handlers.On914UpgradingPlayer;

			Scp096Events.AddingTarget -= Handlers.On096AddingTarget;
			Scp096Events.Enraging -= Handlers.On096Enraging;
			Scp096Events.CalmingDown -= Handlers.On096CalmingDown;
			Scp049Events.FinishingRecall -= Handlers.On049FinishingRecall;
			Scp049Events.StartingRecall -= Handlers.On049StartingRecall;
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
			Harmony.UnpatchAll(Harmony.Id);
		}
	}
}