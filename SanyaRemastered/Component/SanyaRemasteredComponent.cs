using System;
using System.Linq;
using UnityEngine;
using Mirror.LiteNetLib4Mirror;
using Respawning;
using Exiled.API.Features;

using SanyaRemastered.Data;
using SanyaRemastered.Functions;
using System.Collections.Generic;
using SanyaRemastered;
using Targeting;
using CustomPlayerEffects;
using Exiled.API.Extensions;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Assets._Scripts.Dissonance;
using Dissonance;
using Dissonance.Integrations.MirrorIgnorance;
using MapGeneration.Distributors;
using Exiled.API.Enums;

namespace SanyaRemastered
{
	public class SanyaRemasteredComponent : MonoBehaviour
	{

		public static readonly HashSet<Player> _scplists = new HashSet<Player>();

		public bool DisableHud = false;

		private SanyaRemastered _plugin;
		private Player _player;
		private string _hudTemplate = "<line-height=95%><voffset=8.5em><align=left><size=50%><alpha=#44>([STATS])<alpha=#ff></size></align>\n<align=right>[LIST]</align><align=center>[CENTER_UP][CENTER][CENTER_DOWN][BOTTOM]";
		private float _timer = 0f;
		private int _respawnCounter = -1;
		private string _hudText = string.Empty;
		//HudCenterDown
		private string _hudCenterDownString = string.Empty;
		private float _hudCenterDownTime = -1f;
		private float _hudCenterDownTimer = 0f;
		//HudBottom
		private string _hudBottomString = string.Empty;
		private float _hudBottomTime = -1f;
		private float _hudBottomTimer = 0f;

		//public float soundvolume = -1f;
		private void Start()
		{
			_plugin = SanyaRemastered.Instance;
			_player = Player.Get(gameObject);
		}
		private void OnDestroy()
		{
			if (_scplists.Contains(_player))
				_scplists.Remove(_player);
		}

		private void FixedUpdate()
		{
			if (!_plugin.Config.IsEnabled) return;

			_timer += Time.deltaTime;

			UpdateTimers();

			UpdateContainScp();
			UpdateRespawnCounter();
			UpdateScpLists();
			UpdateHint();
			UpdateExHud();

			if (_timer > 1f)
				_timer = 0f;
		}

		public void AddHudCenterDownText(string text, ulong timer)
		{
			_hudCenterDownString = text;
			_hudCenterDownTime = timer;
			_hudCenterDownTimer = 0f;
		}
		
		public void ClearHudCenterDownText()
		{
			_hudCenterDownTime = -1f;
		}

		public void AddHudBottomText(string text, ulong timer)
		{
			_hudBottomString = text;
			_hudBottomTime = timer;
			_hudBottomTimer = 0f;
		}

		public void ClearHudBottomText()
		{
			_hudBottomTime = -1f;
		}

		private void UpdateTimers()
		{
			if (_hudCenterDownTimer < _hudCenterDownTime)
				_hudCenterDownTimer += Time.deltaTime;
			else
				_hudCenterDownString = string.Empty; 

			if (_hudBottomTimer < _hudBottomTime)
				_hudBottomTimer += Time.deltaTime;
			else
				_hudBottomString = string.Empty;
		}

		private void UpdateRespawnCounter()
		{
			if(!RoundSummary.RoundInProgress() || Warhead.IsDetonated || _player.Role != RoleType.Spectator) return;
			if(RespawnManager.CurrentSequence() == RespawnManager.RespawnSequencePhase.RespawnCooldown)
				_respawnCounter = (int)Math.Truncate(RespawnManager.Singleton._timeForNextSequence - RespawnManager.Singleton._stopwatch.Elapsed.TotalSeconds);
			else
				_respawnCounter = 0;
		}

		private void UpdateScpLists()
		{
			if ((_player.Team != Team.SCP || _player.Role == RoleType.Scp0492) && _scplists.Contains(_player))
			{
				_scplists.Remove(_player);
				return;
			}

			if (_player.Team == Team.SCP && _player.Role != RoleType.Scp0492 && !_scplists.Contains(_player))
			{
				_scplists.Add(_player);
				return;
			}
		}
		public void UpdateContainScp()
        {
			if (!SanyaRemastered.Instance.Config.ContainCommand || !_player.IsScp || !(_timer > 1f) || _player.GameObject.TryGetComponent(out ContainScpComponent _)) return;
			Contain.IsCanBeContain(_player);
		}
		public void UpdateHint()
		{
			if (DisableHud || !_plugin.Config.ExHudEnabled || !(_timer > 1f)) return;

			if (_player.TryGetSessionVariable("hint_centerdown", out Tuple<string, float> Hintcenterdown))
			{
				_hudCenterDownString = Hintcenterdown.Item1;
				_hudCenterDownTime = Hintcenterdown.Item2;
				_hudCenterDownTimer = 0f;
				_player.SessionVariables.Remove("hint_centerdown");
			}
			if (_player.TryGetSessionVariable("hint_bottom", out Tuple<string, float> Hintbottom))
            {
				_hudBottomString = Hintbottom.Item1;
				_hudBottomTime = Hintbottom.Item2;
				_hudBottomTimer = 0f;
				_player.SessionVariables.Remove("hint_bottom");
			}
		}
		private void UpdateExHud()
		{
			if (DisableHud || !_plugin.Config.ExHudEnabled || !(_timer > 1f)) return;
			string curText = _hudTemplate;
			//[LEFT_UP]
			string info = string.Empty;
			if (_player.IsInvisible)
				info += $"<b>vous êtes invisible</b> ";
			{
				curText = curText.Replace("([STATS])", info);
			}

			//[LIST]
			if (_player.Team == Team.SCP)
			{
				string list = string.Empty;
				if (_player.Role == RoleType.Scp079 && SanyaRemastered.Instance.Config.ExHudScp079Moreinfo)
				{
					list += "<color=red><u>SCP</u>\n";
					int Scp0492 = 0;
					foreach (var scp in _scplists)
						if (scp.Role == RoleType.Scp079)
							list += $"{scp.ReferenceHub.characterClassManager.CurRole.fullName}:Tier{scp.ReferenceHub.scp079PlayerScript.Lvl + 1}\n";
						else if (scp.Role != RoleType.Scp0492)
							list += $"{scp.ReferenceHub.characterClassManager.CurRole.fullName}:{scp.CurrentRoom.Type}\n";
						else
							Scp0492++;
					if (Scp0492 > 0)
						list += $"Scp049-2:{Scp0492}\n";
					list.TrimEnd('\n');
					list += "</color>";
				}
				if (_player.Role == RoleType.Scp096 && SanyaRemastered.Instance.Config.ExHudScp096 && _player.CurrentScp is PlayableScps.Scp096 Scp096 && Scp096._targets.Count() != 0)
				{
					list += "<color=red><u>SCP</u>\n";
					var TargetList = Scp096._targets.OrderBy(x => Vector3.Distance(_player.Position, x.gameObject.transform.position));
					list += $"Target : {TargetList.Count()}\n";
					list += $"Distance : {(int)Vector3.Distance(_player.Position, TargetList.First().gameObject.transform.position)}m\n";
					foreach (Room room in Map.Rooms.Where(r => r.Players.Where(p => TargetList.Contains(p.ReferenceHub)).Count() != 0))
						list += $"{room.Type} : {room.Players.Where(x => TargetList.Contains(x.ReferenceHub)).Count()}\n";
					list.TrimEnd('\n');
					list += "</color>";
				}
				curText = curText.Replace("[LIST]", FormatStringForHud(list, 7));
			}
			else
				curText = curText.Replace("[LIST]", FormatStringForHud(string.Empty, 7));

			//[CENTER_UP]
			curText = curText.Replace("[CENTER_UP]", FormatStringForHud(string.Empty, 6));

			//[CENTER]
			if (_player.Role == RoleType.Scp079 && _player.Zone == ZoneType.HeavyContainment && SanyaRemastered.Instance.Config.ExHudScp079Moreinfo)
            {
				string InfoGen = string.Empty;
				foreach (Generator gen in Generator.Get(GeneratorState.Activating))
					InfoGen += $"<color=#ffff00>({gen.Room.Type}){Mathf.FloorToInt(gen.CurrentTime) / 60:00} : {Mathf.FloorToInt(gen.CurrentTime) % 60:00}</color>\n";
				curText = curText.Replace("[CENTER]", FormatStringForHud(InfoGen, 6));
			}
			curText = curText.Replace("[CENTER]", FormatStringForHud(string.Empty, 6));

			
			//[CENTER_DOWN]
			if (!string.IsNullOrEmpty(_hudCenterDownString))
				curText = curText.Replace("[CENTER_DOWN]", FormatStringForHud(_hudCenterDownString, 5));
			else if (_player.Role == RoleType.Spectator)
			{
				if (Coroutines.isActuallyBombGoing)
					curText = curText.Replace("[CENTER_DOWN]", FormatStringForHud($"Aucun respawn tant que le bombardement est activé.", 5));
				else if (Coroutines.AirBombWait != 0 && Coroutines.AirBombWait < 60)
					curText = curText.Replace("[CENTER_DOWN]", FormatStringForHud($"Aucun respawn. Un bombardement est prévu sur le site dans {Coroutines.AirBombWait} seconde{(Coroutines.AirBombWait <= 1 ? "" : "s")} !", 5));
				else if (Warhead.IsDetonated && SanyaRemastered.Instance.Config.StopRespawnAfterDetonated)
					if (Coroutines.AirBombWait != 0)
						curText = curText.Replace("[CENTER_DOWN]", FormatStringForHud($"Aucun respawn après l'explosion du site, un bombardement vas être effectuer.", 5));
					else
						curText = curText.Replace("[CENTER_DOWN]", FormatStringForHud($"Aucun respawn après l'explosion du site.", 5));
				else if (RespawnTickets.Singleton.GetAvailableTickets(SpawnableTeamType.NineTailedFox) <= 0 && RespawnTickets.Singleton.GetAvailableTickets(SpawnableTeamType.ChaosInsurgency) <= 0)
					curText = curText.Replace("[CENTER_DOWN]", FormatStringForHud($"Aucun respawn. Il n'y a plus de tickets disponibles.", 5));
				else if (_respawnCounter == 0)//{(Respawn.NextKnownTeam == SpawnableTeamType.NineTailedFox ? "" : (Respawn.NextKnownTeam == SpawnableTeamType.ChaosInsurgency ? "":""))}
					curText = curText.Replace("[CENTER_DOWN]", FormatStringForHud($"Respawn en cours...", 5));
				else if (_respawnCounter != -1)
					curText = curText.Replace("[CENTER_DOWN]", FormatStringForHud($"Prochain respawn dans {_respawnCounter} seconde{(_respawnCounter <= 1 ? "" : "s")}.", 5));
				else
					curText = curText.Replace("[CENTER_DOWN]", FormatStringForHud(string.Empty, 5));
			}
			else
				curText = curText.Replace("[CENTER_DOWN]", FormatStringForHud(string.Empty, 5));

			//[BOTTOM]
			if (!string.IsNullOrEmpty(_hudBottomString))
				curText = curText.Replace("[BOTTOM]", FormatStringForHud(_hudBottomString, 2));
			else if (_player.Role == RoleType.Spectator) 
			{
				try
				{
					Player Spectate = Player.Get(_player.ReferenceHub.spectatorManager.CurrentSpectatedPlayer.gameObject);
					if (Spectate != null && Spectate.TryGetSessionVariable("NewRole", out Tuple<string, string> newrole))
					{
						curText = curText.Replace("[BOTTOM]", FormatStringForHud($"\n<b><color={Spectate.RoleColor.ToHex()}>{newrole.Item2}</color></b>", 2));
					}
					else
						curText = curText.Replace("[BOTTOM]", FormatStringForHud(string.Empty, 2));
				}
				finally 
				{
					curText = curText.Replace("[BOTTOM]", FormatStringForHud(string.Empty, 2));
				}
			}
			else if (!string.IsNullOrWhiteSpace(SanyaRemastered.Instance.Config.IsBeta))
				curText = curText.Replace("[BOTTOM]", FormatStringForHud(SanyaRemastered.Instance.Config.IsBeta, 2));
			else
				curText = curText.Replace("[BOTTOM]", FormatStringForHud(string.Empty, 2));

			_hudText = curText;
			_player.SendTextHintNotEffect(_hudText, 2);
		}

		private string FormatStringForHud(string text, int needNewLine)
		{
			int curNewLine = text.Count(x => x == '\n');
			for (int i = 0; i < needNewLine - curNewLine; i++)
				text += '\n';
			return text;
		}
	}
}

