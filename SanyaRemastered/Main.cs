using System;
using System.Collections.Generic;
using System.IO;
using Exiled.API.Features;
using HarmonyLib;
using MEC;
using SanyaPlugin.Functions;
using Handlers = Exiled.Events.Handlers;

namespace SanyaPlugin
{
	public class SanyaPlugin : Plugin<SanyaRemastered.Configs>
	{
		public override string Name { get; } = "SanyaPlugin";
		public static readonly string harmonyId = "jp.sanyae2439.SanyaPlugin";
		public static readonly string Version = "2.0.1b";
		public static readonly string TargetVersion = "1.12.20";
		public static readonly string DataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Plugins", "SanyaPlugin");
		//public static SanyaRemastered.Configs Config;
		public static SanyaPlugin Instance { get; private set; }
		public EventHandlers EventHandlers;
		public Harmony harmony;
		public static Random random = new Random();
		public Dictionary<ReferenceHub, DateTime> cooldowngaz = new Dictionary<ReferenceHub, DateTime>();
		public SanyaPlugin() => Instance = this;

		public override void OnEnabled()
		{
			//Config = new SanyaRemastered.Configs();
			if (Config.KickVpn) ShitChecker.LoadLists();

			EventHandlers = new EventHandlers(this);
			Handlers.Server.SendingRemoteAdminCommand += EventHandlers.OnRACommand;
			Handlers.Server.SendingConsoleCommand += EventHandlers.OnCommand;
			Handlers.Server.WaitingForPlayers += EventHandlers.OnWaintingForPlayers;
			Handlers.Server.RoundStarted += EventHandlers.OnRoundStart;
			Handlers.Server.RoundEnded += EventHandlers.OnRoundEnd;
			Handlers.Server.RestartingRound += EventHandlers.OnRoundRestart;
			Handlers.Server.RespawningTeam += EventHandlers.OnTeamRespawn;
			
			Handlers.Warhead.Starting += EventHandlers.OnWarheadStart;
			Handlers.Warhead.Stopping += EventHandlers.OnWarheadCancel;
			Handlers.Warhead.Detonated += EventHandlers.OnDetonated;
			
			Handlers.Map.AnnouncingDecontamination += EventHandlers.OnAnnounceDecont;
			Handlers.Map.PlacingDecal += EventHandlers.OnPlacingDecal;
			Handlers.Map.ExplodingGrenade += EventHandlers.OnExplodingGrenade;
			Handlers.Map.GeneratorActivated += EventHandlers.OnGeneratorFinish;
			
			Handlers.Player.InteractingElevator += EventHandlers.OnInteractingElevator;
			Handlers.Player.PreAuthenticating += EventHandlers.OnPreAuth;
			Handlers.Player.Joined += EventHandlers.OnPlayerJoin;
			Handlers.Player.Left += EventHandlers.OnPlayerLeave;
			Handlers.Player.ChangingRole += EventHandlers.OnPlayerSetClass;
			Handlers.Player.Spawning += EventHandlers.OnPlayerSpawn;
			Handlers.Player.Hurting += EventHandlers.OnPlayerHurt;
			Handlers.Player.Died += EventHandlers.OnPlayerDeath;
			Handlers.Player.FailingEscapePocketDimension  += EventHandlers.OnPocketDimDeath;
			Handlers.Player.MedicalItemUsed += EventHandlers.OnPlayerUsedMedicalItem;
			Handlers.Player.TriggeringTesla += EventHandlers.OnPlayerTriggerTesla;
			Handlers.Player.InteractingDoor += EventHandlers.OnPlayerDoorInteract;
			Handlers.Player.InteractingLocker += EventHandlers.OnPlayerLockerInteract;
			Handlers.Player.SyncingData += EventHandlers.OnPlayerChangeAnim;
			
			Handlers.Player.Shooting += EventHandlers.OnShoot;
			Handlers.Player.UnlockingGenerator += EventHandlers.OnGeneratorUnlock;
			Handlers.Player.EjectingGeneratorTablet += EventHandlers.OnEjectingGeneratorTablet;
			Handlers.Player.OpeningGenerator  += EventHandlers.OnGeneratorOpen;
			Handlers.Player.ClosingGenerator += EventHandlers.OnGeneratorClose;
			Handlers.Player.InsertingGeneratorTablet += EventHandlers.OnGeneratorInsert;
			Handlers.Player.ActivatingWarheadPanel += EventHandlers.OnActivatingWarheadPanel;

			Handlers.Scp106.CreatingPortal += EventHandlers.On106MakePortal;
			Handlers.Scp106.Teleporting += EventHandlers.On106Teleport;
			Handlers.Scp079.GainingLevel += EventHandlers.On079LevelGain;
			Handlers.Scp914.UpgradingItems += EventHandlers.On914Upgrade;
			Handlers.Scp096.Enraging += EventHandlers.OnEnraging;

			harmony = new HarmonyLib.Harmony(harmonyId);
			harmony.PatchAll();

			EventHandlers.sendertask = EventHandlers.SenderAsync().StartSender();
			/*
			ServerConsole.singleton.NameFormatter.Commands.Add("mtf_tickets", (List<string> args) => Methods.GetMTFTickets().ToString());
			ServerConsole.singleton.NameFormatter.Commands.Add("ci_tickets", (List<string> args) => Methods.GetCITickets().ToString());
			*/
			Log.Info($"[OnEnabled] SanyaPlugin({Version}) Enabled.");
			base.OnEnabled();
		}

		public override void OnDisabled()
		{
			harmony.UnpatchAll();

			foreach (var cor in EventHandlers.roundCoroutines)
				Timing.KillCoroutines(cor);
			EventHandlers.roundCoroutines.Clear();

			Handlers.Server.SendingRemoteAdminCommand -= EventHandlers.OnRACommand;
			Handlers.Server.SendingConsoleCommand -= EventHandlers.OnCommand;
			Handlers.Server.WaitingForPlayers -= EventHandlers.OnWaintingForPlayers;
			Handlers.Server.RoundStarted -= EventHandlers.OnRoundStart;
			Handlers.Server.RoundEnded -= EventHandlers.OnRoundEnd;
			Handlers.Server.RestartingRound -= EventHandlers.OnRoundRestart;
			Handlers.Server.RespawningTeam -= EventHandlers.OnTeamRespawn;
			
			Handlers.Warhead.Starting -= EventHandlers.OnWarheadStart;
			Handlers.Warhead.Stopping -= EventHandlers.OnWarheadCancel;
			Handlers.Warhead.Detonated -= EventHandlers.OnDetonated;
			
			Handlers.Map.AnnouncingDecontamination -= EventHandlers.OnAnnounceDecont;
			Handlers.Map.PlacingDecal -= EventHandlers.OnPlacingDecal;
			Handlers.Map.ExplodingGrenade -= EventHandlers.OnExplodingGrenade;
			Handlers.Map.GeneratorActivated -= EventHandlers.OnGeneratorFinish;
			
			Handlers.Player.InteractingElevator -= EventHandlers.OnInteractingElevator;
			Handlers.Player.PreAuthenticating -= EventHandlers.OnPreAuth;
			Handlers.Player.Joined -= EventHandlers.OnPlayerJoin;
			Handlers.Player.Left -= EventHandlers.OnPlayerLeave;
			Handlers.Player.ChangingRole -= EventHandlers.OnPlayerSetClass;
			Handlers.Player.Spawning -= EventHandlers.OnPlayerSpawn;
			Handlers.Player.Hurting -= EventHandlers.OnPlayerHurt;
			Handlers.Player.Died -= EventHandlers.OnPlayerDeath;
			Handlers.Player.FailingEscapePocketDimension -= EventHandlers.OnPocketDimDeath;
			Handlers.Player.MedicalItemUsed -= EventHandlers.OnPlayerUsedMedicalItem;
			Handlers.Player.TriggeringTesla -= EventHandlers.OnPlayerTriggerTesla;
			Handlers.Player.InteractingDoor -= EventHandlers.OnPlayerDoorInteract;
			Handlers.Player.InteractingLocker -= EventHandlers.OnPlayerLockerInteract;
			
			Handlers.Player.Shooting -= EventHandlers.OnShoot;
			Handlers.Player.SyncingData -= EventHandlers.OnPlayerChangeAnim;
			Handlers.Player.UnlockingGenerator -= EventHandlers.OnGeneratorUnlock;
			Handlers.Player.OpeningGenerator -= EventHandlers.OnGeneratorOpen;
			Handlers.Player.ClosingGenerator -= EventHandlers.OnGeneratorClose;
			Handlers.Player.InsertingGeneratorTablet -= EventHandlers.OnGeneratorInsert;
			Handlers.Player.ActivatingWarheadPanel -= EventHandlers.OnActivatingWarheadPanel;
			
			Handlers.Scp106.CreatingPortal -= EventHandlers.On106MakePortal;
			Handlers.Scp106.Teleporting -= EventHandlers.On106Teleport;
			Handlers.Scp079.GainingLevel -= EventHandlers.On079LevelGain;
			Handlers.Scp914.UpgradingItems -= EventHandlers.On914Upgrade;
			Handlers.Scp096.Enraging -= EventHandlers.OnEnraging;

			EventHandlers = null;

			Log.Info($"[OnDisable] SanyaPlugin({Version}) Disabled.");
		}

        public override void OnReloaded()
        {
			Log.Info($"[OnReload] SanyaPlugin({Version}) Reloaded.");
			base.OnReloaded();
        }
	}
}