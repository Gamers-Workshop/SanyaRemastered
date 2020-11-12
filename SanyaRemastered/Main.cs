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
using SanyaRemastered;

namespace SanyaPlugin
{
	public class SanyaPlugin : Plugin<Configs>
	{
		public override string Name => "SanyaPlugin";
		public override string Prefix => "sanya";
		public override string Author => "sanyae2439";
		public override PluginPriority Priority => PluginPriority.Default;
		public override Version Version => new Version(2, 9, 1);
		public override Version RequiredExiledVersion => new Version(2, 1, 9);

		public static SanyaPlugin Instance { get; private set; }
		public EventHandlers Handlers { get; private set; }
		public Harmony Harmony { get; private set; }
		public Random Random { get; } = new Random();
		private int patchCounter;

		public SanyaPlugin() => Instance = this;

		public override void OnEnabled()
		{
			if (!Config.IsEnabled) return;

			base.OnEnabled();

			RegistEvents();
			Config.ParseConfig();

			RegistPatch();

			Log.Info($"[OnEnabled] SanyaPlugin({Version}) Enabled Complete.");
		}

		public override void OnDisabled()
		{
			base.OnDisabled();

			foreach (var cor in Handlers.roundCoroutines)
				MEC.Timing.KillCoroutines(cor);
			Handlers.roundCoroutines.Clear();

			UnRegistEvents();
			UnRegistPatch();

			Log.Info($"[OnDisable] SanyaPlugin({Version}) Disabled Complete.");
		}

		private void RegistEvents()
		{
			Handlers = new EventHandlers(this);
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
			MapEvents.PlacingDecal += Handlers.OnPlacingDecal;
			MapEvents.ExplodingGrenade += Handlers.OnExplodingGrenade;
			MapEvents.GeneratorActivated += Handlers.OnGeneratorFinish;
			
			PlayerEvents.InteractingElevator += Handlers.OnInteractingElevator;
			PlayerEvents.PreAuthenticating += Handlers.OnPreAuth;
			PlayerEvents.Joined += Handlers.OnPlayerJoin;
			PlayerEvents.Left += Handlers.OnPlayerLeave;
			PlayerEvents.ChangingRole += Handlers.OnPlayerSetClass;
			PlayerEvents.Spawning += Handlers.OnPlayerSpawn;
			PlayerEvents.Hurting += Handlers.OnPlayerHurt;
			PlayerEvents.Died += Handlers.OnPlayerDeath;
			PlayerEvents.FailingEscapePocketDimension  += Handlers.OnPocketDimDeath;
			PlayerEvents.MedicalItemUsed += Handlers.OnPlayerUsedMedicalItem;
			PlayerEvents.TriggeringTesla += Handlers.OnPlayerTriggerTesla;
			PlayerEvents.InteractingDoor += Handlers.OnPlayerDoorInteract;
			PlayerEvents.InteractingLocker += Handlers.OnPlayerLockerInteract;
			PlayerEvents.SyncingData += Handlers.OnSyncingData;
			
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
			Scp096Events.Enraging += Handlers.OnEnraging;
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
			MapEvents.PlacingDecal -= Handlers.OnPlacingDecal;
			MapEvents.ExplodingGrenade -= Handlers.OnExplodingGrenade;
			MapEvents.GeneratorActivated -= Handlers.OnGeneratorFinish;
			
			PlayerEvents.InteractingElevator -= Handlers.OnInteractingElevator;
			PlayerEvents.PreAuthenticating -= Handlers.OnPreAuth;
			PlayerEvents.Joined -= Handlers.OnPlayerJoin;
			PlayerEvents.Left -= Handlers.OnPlayerLeave;
			PlayerEvents.ChangingRole -= Handlers.OnPlayerSetClass;
			PlayerEvents.Spawning -= Handlers.OnPlayerSpawn;
			PlayerEvents.Hurting -= Handlers.OnPlayerHurt;
			PlayerEvents.Died -= Handlers.OnPlayerDeath;
			PlayerEvents.FailingEscapePocketDimension -= Handlers.OnPocketDimDeath;
			PlayerEvents.MedicalItemUsed -= Handlers.OnPlayerUsedMedicalItem;
			PlayerEvents.TriggeringTesla -= Handlers.OnPlayerTriggerTesla;
			PlayerEvents.InteractingDoor -= Handlers.OnPlayerDoorInteract;
			PlayerEvents.InteractingLocker -= Handlers.OnPlayerLockerInteract;
			
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
			Scp096Events.Enraging -= Handlers.OnEnraging;
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
	}
}