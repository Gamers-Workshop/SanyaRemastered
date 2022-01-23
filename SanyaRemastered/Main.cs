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
using Scp173Events = Exiled.Events.Handlers.Scp173;

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
		public EventHandlers.ServerHandlers ServerHandlers { get; private set; }
		public EventHandlers.PlayerHandlers PlayerHandlers { get; private set; }
		public EventHandlers.ScpHandlers ScpHandlers { get; private set; }

		private Harmony Harmony = new Harmony("dev.Yamato");
		public Random Random { get; } = new Random();
		private int patchCounter;

		public SanyaRemastered() => Instance = this;
		private Ram.MemoryService _memoryService;

		public string SpecialTextIntercom = null;
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
        public override void OnReloaded()
        {
			if (!Config.IsEnabled) return;

			int processId = Process.GetCurrentProcess().Id;
			_memoryService = new Ram.MemoryService(processId);
			//Only run if memory is going to be asked for
			if (Config.RamInfo)
			{
				_memoryService.BackgroundWorker.RunWorkerAsync();
			}
			RegistEvents();
			RegistPatch();

			foreach (Player p in Player.List)
				if (!p.GameObject.TryGetComponent<SanyaRemasteredComponent>(out _))
					p.GameObject.AddComponent<SanyaRemasteredComponent>();

			base.OnReloaded();
        }
        public override void OnDisabled()
		{
			base.OnDisabled();

			foreach (var cor in ServerHandlers.RoundCoroutines)
				MEC.Timing.KillCoroutines(cor);
			ServerHandlers.RoundCoroutines.Clear();

			UnRegistEvents();

			UnRegistPatch();

			Log.Info($"[OnDisable] SanyaRemastered({Version}) Disabled Complete.");
		}

		private void RegistEvents()
		{
			ServerHandlers = new EventHandlers.ServerHandlers(this);
			PlayerHandlers = new EventHandlers.PlayerHandlers(this);
			ScpHandlers = new EventHandlers.ScpHandlers(this);

			//
			ServerEvents.WaitingForPlayers += ServerHandlers.OnWaintingForPlayers;
			ServerEvents.RoundStarted += ServerHandlers.OnRoundStart;
			ServerEvents.RoundEnded += ServerHandlers.OnRoundEnd;
			ServerEvents.RestartingRound += ServerHandlers.OnRoundRestart;
			ServerEvents.RespawningTeam += ServerHandlers.OnTeamRespawn;
			
			WarheadEvents.Starting += ServerHandlers.OnWarheadStart;
			WarheadEvents.Stopping += ServerHandlers.OnWarheadCancel;
			WarheadEvents.Detonated += ServerHandlers.OnDetonated;
			
			MapEvents.AnnouncingDecontamination += ServerHandlers.OnAnnounceDecont;
			MapEvents.Decontaminating += ServerHandlers.OnDecontaminating;
			MapEvents.AnnouncingNtfEntrance += ServerHandlers.OnAnnounceNtf;

			MapEvents.ExplodingGrenade += ServerHandlers.OnExplodingGrenade;
			MapEvents.GeneratorActivated += ServerHandlers.OnGeneratorFinish;
			MapEvents.PlacingBulletHole += ServerHandlers.OnPlacingBulletHole;
			MapEvents.DamagingWindow += ServerHandlers.OnDamagingWindow;

			PlayerEvents.PreAuthenticating += PlayerHandlers.OnPreAuth;
			PlayerEvents.Verified += PlayerHandlers.OnPlayerVerified;
			PlayerEvents.Destroying += PlayerHandlers.OnPlayerDestroying;
			PlayerEvents.ChangingRole += PlayerHandlers.OnPlayerSetClass;
			PlayerEvents.Spawning += PlayerHandlers.OnPlayerSpawn;
			PlayerEvents.Hurting += PlayerHandlers.OnPlayerHurt;

			PlayerEvents.Died += PlayerHandlers.OnDied;
			PlayerEvents.FailingEscapePocketDimension  += PlayerHandlers.OnPocketDimDeath;
			PlayerEvents.ThrowingItem += PlayerHandlers.OnThrowingItem;
			PlayerEvents.UsingItem += PlayerHandlers.OnPlayerUsingItem;
			PlayerEvents.ItemUsed += PlayerHandlers.OnPlayerItemUsed;
			PlayerEvents.TriggeringTesla += PlayerHandlers.OnPlayerTriggerTesla;
			PlayerEvents.InteractingDoor += PlayerHandlers.OnPlayerDoorInteract;
			PlayerEvents.InteractingLocker += PlayerHandlers.OnPlayerLockerInteract;
			PlayerEvents.InteractingElevator += PlayerHandlers.OnInteractingElevator;
			PlayerEvents.IntercomSpeaking += PlayerHandlers.OnIntercomSpeaking;

			PlayerEvents.Shooting += PlayerHandlers.OnShooting;
			PlayerEvents.UsingMicroHIDEnergy += PlayerHandlers.OnUsingMicroHIDEnergy;
			PlayerEvents.Jumping -= PlayerHandlers.OnJumping;
			PlayerEvents.ActivatingWarheadPanel += PlayerHandlers.OnActivatingWarheadPanel;
			PlayerEvents.UnlockingGenerator += PlayerHandlers.OnGeneratorUnlock;
			PlayerEvents.StoppingGenerator += PlayerHandlers.OnStoppingGenerator;
			PlayerEvents.OpeningGenerator  += PlayerHandlers.OnGeneratorOpen;
			PlayerEvents.ClosingGenerator += PlayerHandlers.OnGeneratorClose;
			PlayerEvents.ActivatingGenerator += PlayerHandlers.OnActivatingGenerator;
			PlayerEvents.Handcuffing += PlayerHandlers.OnHandcuffing;
			PlayerEvents.ProcessingHotkey += PlayerHandlers.OnProcessingHotkey;

			Scp079Events.GainingLevel += ScpHandlers.On079LevelGain;
			Scp914Events.UpgradingPlayer += ScpHandlers.On914UpgradingPlayer;

			Scp096Events.AddingTarget += ScpHandlers.On096AddingTarget;
			Scp096Events.Enraging += ScpHandlers.On096Enraging;
			Scp096Events.CalmingDown += ScpHandlers.On096CalmingDown;

			Scp049Events.FinishingRecall += ScpHandlers.On049FinishingRecall;
		}

		private void UnRegistEvents()
		{
			ServerEvents.WaitingForPlayers -= ServerHandlers.OnWaintingForPlayers;
			ServerEvents.RoundStarted -= ServerHandlers.OnRoundStart;
			ServerEvents.RoundEnded -= ServerHandlers.OnRoundEnd;
			ServerEvents.RestartingRound -= ServerHandlers.OnRoundRestart;
			ServerEvents.RespawningTeam -= ServerHandlers.OnTeamRespawn;
			
			WarheadEvents.Starting -= ServerHandlers.OnWarheadStart;
			WarheadEvents.Stopping -= ServerHandlers.OnWarheadCancel;
			WarheadEvents.Detonated -= ServerHandlers.OnDetonated;
			
			MapEvents.AnnouncingDecontamination -= ServerHandlers.OnAnnounceDecont;
			MapEvents.AnnouncingNtfEntrance -= ServerHandlers.OnAnnounceNtf;

			MapEvents.ExplodingGrenade -= ServerHandlers.OnExplodingGrenade;
			MapEvents.GeneratorActivated -= ServerHandlers.OnGeneratorFinish;
			MapEvents.PlacingBulletHole -= ServerHandlers.OnPlacingBulletHole;
			MapEvents.DamagingWindow += ServerHandlers.OnDamagingWindow;

			PlayerEvents.PreAuthenticating -= PlayerHandlers.OnPreAuth;
			PlayerEvents.Verified -= PlayerHandlers.OnPlayerVerified;
			PlayerEvents.Destroying -= PlayerHandlers.OnPlayerDestroying;
			PlayerEvents.ChangingRole -= PlayerHandlers.OnPlayerSetClass;
			PlayerEvents.Spawning -= PlayerHandlers.OnPlayerSpawn;
			PlayerEvents.Hurting -= PlayerHandlers.OnPlayerHurt;
			PlayerEvents.Died -= PlayerHandlers.OnDied;
			PlayerEvents.FailingEscapePocketDimension -= PlayerHandlers.OnPocketDimDeath;
			PlayerEvents.ThrowingItem -= PlayerHandlers.OnThrowingItem;
			PlayerEvents.UsingItem -= PlayerHandlers.OnPlayerUsingItem;
			PlayerEvents.ItemUsed -= PlayerHandlers.OnPlayerItemUsed;
			PlayerEvents.TriggeringTesla -= PlayerHandlers.OnPlayerTriggerTesla;
			PlayerEvents.InteractingDoor -= PlayerHandlers.OnPlayerDoorInteract;
			PlayerEvents.InteractingLocker -= PlayerHandlers.OnPlayerLockerInteract;
			PlayerEvents.InteractingElevator -= PlayerHandlers.OnInteractingElevator;
			PlayerEvents.IntercomSpeaking -= PlayerHandlers.OnIntercomSpeaking;

			PlayerEvents.Shooting -= PlayerHandlers.OnShooting;
			PlayerEvents.UsingMicroHIDEnergy -= PlayerHandlers.OnUsingMicroHIDEnergy;
			PlayerEvents.Jumping -= PlayerHandlers.OnJumping;
			PlayerEvents.ActivatingWarheadPanel -= PlayerHandlers.OnActivatingWarheadPanel;
			PlayerEvents.UnlockingGenerator -= PlayerHandlers.OnGeneratorUnlock;
			PlayerEvents.StoppingGenerator -= PlayerHandlers.OnStoppingGenerator;
			PlayerEvents.OpeningGenerator -= PlayerHandlers.OnGeneratorOpen;
			PlayerEvents.ClosingGenerator -= PlayerHandlers.OnGeneratorClose;
			PlayerEvents.ActivatingGenerator -= PlayerHandlers.OnActivatingGenerator;
			PlayerEvents.Handcuffing -= PlayerHandlers.OnHandcuffing;
			PlayerEvents.ProcessingHotkey -= PlayerHandlers.OnProcessingHotkey;

			Scp079Events.GainingLevel -= ScpHandlers.On079LevelGain;
			Scp914Events.UpgradingPlayer -= ScpHandlers.On914UpgradingPlayer;

			Scp096Events.AddingTarget -= ScpHandlers.On096AddingTarget;
			Scp096Events.Enraging -= ScpHandlers.On096Enraging;
			Scp096Events.CalmingDown -= ScpHandlers.On096CalmingDown;
			Scp049Events.FinishingRecall -= ScpHandlers.On049FinishingRecall;
			ServerHandlers = null;
			PlayerHandlers = null;
			ScpHandlers = null;
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