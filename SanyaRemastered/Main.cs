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
using Scp330Events = Exiled.Events.Handlers.Scp330;
using System.Collections.Generic;
using NorthwoodLib.Pools;
using System.Reflection.Emit;
using Mirror;
using System.Reflection;
using System.Diagnostics;

using SanyaRemastered.Configs;
using DiscordLog;
using PlayerRoles.Ragdolls;
using MEC;
using CustomPlayerEffects;

namespace SanyaRemastered
{
	public class SanyaRemastered : Plugin<Config,Translation>
	{
		public override string Name => "SanyaRemastered";
		public override string Prefix => "sanya";
		public override string Author => "sanyae2439";
		public override PluginPriority Priority => (PluginPriority) (-1);

        public static SanyaRemastered Instance { get; private set; }
		public static bool IsEnable => Instance.Config.Debug;
        public EventHandlers.ServerHandlers ServerHandlers { get; private set; }
		public EventHandlers.PlayerHandlers PlayerHandlers { get; private set; }
		public EventHandlers.ScpHandlers ScpHandlers { get; private set; }

		private Harmony Harmony = new("dev.Yamato");
		public Random Random { get; } = new Random();
		private int patchCounter;

		private Ram.MemoryService _memoryService;

		public string SpecialTextIntercom = null;
		public override void OnEnabled()
        {
            Instance = this;

            int processId = Process.GetCurrentProcess().Id;
			_memoryService = new(processId);
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
			int processId = Process.GetCurrentProcess().Id;
			_memoryService = new(processId);
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
			ServerHandlers = new(this);
			PlayerHandlers = new(this);
			ScpHandlers = new(this);

			//
			ServerEvents.WaitingForPlayers += ServerHandlers.OnWaintingForPlayers;
			ServerEvents.RoundStarted += ServerHandlers.OnRoundStart;
            ServerEvents.RoundEnded += ServerHandlers.OnRoundEnd;
			ServerEvents.RestartingRound += ServerHandlers.OnRoundRestart;
			ServerEvents.RespawningTeam += ServerHandlers.OnTeamRespawn;
			
			WarheadEvents.Stopping += ServerHandlers.OnWarheadCancel;
			WarheadEvents.Detonated += ServerHandlers.OnDetonated;
			
			MapEvents.GeneratorActivating += ServerHandlers.OnGeneratorFinish;
			MapEvents.PlacingBulletHole += ServerHandlers.OnPlacingBulletHole;
			PlayerEvents.Verified += PlayerHandlers.OnPlayerVerified;
			PlayerEvents.ChangingRole += PlayerHandlers.OnChangingRole;
			PlayerEvents.Spawned += PlayerHandlers.OnSpawned;
            PlayerEvents.Destroying += PlayerHandlers.OnPlayerDestroying;
			PlayerEvents.Hurting += PlayerHandlers.OnPlayerHurting;

			PlayerEvents.Died += PlayerHandlers.OnDied;

			ItemEvents.ChangingAmmo += PlayerHandlers.OnChangingAmmo;
			PlayerEvents.UsingMicroHIDEnergy += PlayerHandlers.OnUsingMicroHIDEnergy;
			PlayerEvents.ActivatingWarheadPanel += PlayerHandlers.OnActivatingWarheadPanel;
			PlayerEvents.UnlockingGenerator += PlayerHandlers.OnGeneratorUnlock;
			PlayerEvents.ActivatingGenerator += PlayerHandlers.OnActivatingGenerator;
			PlayerEvents.Handcuffing += PlayerHandlers.OnHandcuffing;
			Scp096Events.AddingTarget += ScpHandlers.OnAddingTarget;
        }

        private void UnRegistEvents()
		{
			ServerEvents.WaitingForPlayers -= ServerHandlers.OnWaintingForPlayers;
            ServerEvents.RoundStarted -= ServerHandlers.OnRoundStart;
            ServerEvents.RoundEnded -= ServerHandlers.OnRoundEnd;
			ServerEvents.RestartingRound -= ServerHandlers.OnRoundRestart;
			ServerEvents.RespawningTeam -= ServerHandlers.OnTeamRespawn;

			WarheadEvents.Stopping -= ServerHandlers.OnWarheadCancel;
			WarheadEvents.Detonated -= ServerHandlers.OnDetonated;
			
			MapEvents.GeneratorActivating -= ServerHandlers.OnGeneratorFinish;
			MapEvents.PlacingBulletHole -= ServerHandlers.OnPlacingBulletHole;
			PlayerEvents.Verified -= PlayerHandlers.OnPlayerVerified;
			PlayerEvents.ChangingRole -= PlayerHandlers.OnChangingRole;
            PlayerEvents.Spawned -= PlayerHandlers.OnSpawned;
            PlayerEvents.Destroying -= PlayerHandlers.OnPlayerDestroying;
			PlayerEvents.Hurting -= PlayerHandlers.OnPlayerHurting;
			PlayerEvents.Died -= PlayerHandlers.OnDied;

            ItemEvents.ChangingAmmo -= PlayerHandlers.OnChangingAmmo;
            PlayerEvents.UsingMicroHIDEnergy -= PlayerHandlers.OnUsingMicroHIDEnergy;
			PlayerEvents.ActivatingWarheadPanel -= PlayerHandlers.OnActivatingWarheadPanel;
			PlayerEvents.UnlockingGenerator -= PlayerHandlers.OnGeneratorUnlock;
			PlayerEvents.ActivatingGenerator -= PlayerHandlers.OnActivatingGenerator;
			PlayerEvents.Handcuffing -= PlayerHandlers.OnHandcuffing;
            Scp096Events.AddingTarget -= ScpHandlers.OnAddingTarget;

            ServerHandlers = null;
			PlayerHandlers = null;
			ScpHandlers = null;
		}

		private void RegistPatch()
		{
			try
			{
				Harmony = new(Author + "." + Name + ++patchCounter);
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