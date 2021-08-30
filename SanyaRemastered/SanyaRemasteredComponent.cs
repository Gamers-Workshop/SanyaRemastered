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

namespace SanyaRemastered
{
	public class SanyaRemasteredComponent : MonoBehaviour
	{

		public static readonly HashSet<Player> _scplists = new HashSet<Player>();
		private static Vector3 _espaceArea = new Vector3(177.5f, 985.0f, 29.0f);

		public bool DisableHud = false;

		private SanyaRemastered _plugin;
		private Player _player;
		private string CustomText = string.Empty;
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

		private void Start()
		{
			_plugin = SanyaRemastered.Instance;
			_player = Player.Get(gameObject);
			_espaceArea = new Vector3(177.5f, 985.0f, 29.0f);
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

			CheckTraitor();
			//CheckOnDecal();

			//SoundVolume();
			UpdateRespawnCounter();
			UpdateScpLists();
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

		private void CheckTraitor()
		{
			if (_plugin.Config.TraitorChancePercent <= 0) return;

			if (_player.Team != Team.MTF && _player.Team != Team.CHI) return;
			if (!_player.IsCuffed) return;
			if (Vector3.Distance(_player.Position, _espaceArea) > Escape.radius) return;

			if (UnityEngine.Random.Range(0, 100) >= _plugin.Config.TraitorChancePercent)
			{
				switch (_player.Team)
				{
					case Team.MTF:
						_player.SetRole(RoleType.ChaosConscript);
						break;
					case Team.CHI:
						_player.SetRole(RoleType.NtfPrivate);
						break;
				}
			}
			else
				_player.SetRole(RoleType.Spectator);
		}
		private void CheckOnDecal()
		{
			if (_plugin.Handlers.DecalList.Count != 0 || !_plugin.Config.Coroding106 || _player.Team == (Team.SCP | Team.RIP) ) return;
			foreach (Vector3 Decal in _plugin.Handlers.DecalList)
				if (Vector3.Distance(_player.Position, Decal) < 1)
				{
					_player.ReferenceHub.playerEffectsController.EnableEffect<Disabled>(1f);
					_player.ReferenceHub.playerEffectsController.EnableEffect<SinkHole>(1f);
					_player.ReferenceHub.playerEffectsController.EnableEffect<Corroding>(1f);
				}
		}

		private void SoundVolume()
		{
			if (!(_timer > 1f) || !_player.IsAlive) return;
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

		private void UpdateExHud()
		{
			if (DisableHud || !_plugin.Config.ExHudEnabled || !(_timer > 1f)) return;
			string curText = _hudTemplate;
			//[LEFT_UP]
			string info = null;
			if (_player.IsMuted && _player.IsVoiceChatting)
				info += $"<b>Vous avez été mute</b> ";
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
					list += "SCP\n";
					int Scp0492 = 0;
					foreach (var scp in _scplists)
						if (scp.Role == RoleType.Scp079)
							list += $"{scp.ReferenceHub.characterClassManager.CurRole.fullName}:Tier{scp.ReferenceHub.scp079PlayerScript.Lvl + 1}\n";
						else if (scp.Role == RoleType.Scp0492)
							Scp0492++;
						else
							list += $"{scp.ReferenceHub.characterClassManager.CurRole.fullName}:{scp.GetHealthAmountPercent()}%\n";
					if (Scp0492 > 0)
						list += $"Scp049-2:{Scp0492}\n";
					list.TrimEnd('\n');
				}
				if (_player.Role == RoleType.Scp096 && SanyaRemastered.Instance.Config.ExHudScp096 && _player.CurrentScp is PlayableScps.Scp096 Scp096 && Scp096._targets.Count() != 0)
				{
					var TargetList = Scp096._targets.OrderBy(x => Vector3.Distance(_player.Position, x.gameObject.transform.position));
					list += $"Target : {TargetList.Count()}\n";
					list += $"Distance : {(int)Vector3.Distance(_player.Position, TargetList.First().gameObject.transform.position)}m\n";
					foreach (Room room in Map.Rooms.Where(r => r.Players.Where(p => TargetList.Contains(p.ReferenceHub)).Count() != 0))
						list += $"{room.Type} : {room.Players.Where(x => TargetList.Contains(x.ReferenceHub)).Count()}\n";
					list.TrimEnd('\n');
				}
				curText = curText.Replace("[LIST]", FormatStringForHud(list, 7));
			}
			else
				curText = curText.Replace("[LIST]", FormatStringForHud(string.Empty, 7));

			//[CENTER_UP]
			curText = curText.Replace("[CENTER_UP]", FormatStringForHud(string.Empty, 6));

			//[CENTER]
			if (_player.Role == RoleType.Scp079 && _player.Zone == Exiled.API.Enums.ZoneType.HeavyContainment && SanyaRemastered.Instance.Config.ExHudScp079Moreinfo)
            {
				string InfoGen = null;
				foreach (Scp079Generator gen in Recontainer079.AllGenerators.Where(x=>x.Activating).OrderBy(x => x.Network_syncTime))
                {
					InfoGen += $"<color=#ffff00>({Map.FindParentRoom(gen.gameObject).Type}){Mathf.FloorToInt(gen.Network_syncTime) / 60:00} : {Mathf.FloorToInt(gen.Network_syncTime) % 60:00}</color>\n";
				}
				curText = curText.Replace("[CENTER]", FormatStringForHud(InfoGen, 6));
			}
			curText = curText.Replace("[CENTER]", FormatStringForHud(string.Empty, 6));

			//[CENTER_DOWN]
			if (!string.IsNullOrEmpty(_hudCenterDownString))
				curText = curText.Replace("[CENTER_DOWN]", FormatStringForHud(_hudCenterDownString, 5));
			else if (_player.Role == RoleType.Spectator)
			{
				if (Coroutines.isActuallyBombGoing)
					curText = curText.Replace("[CENTER_DOWN]", FormatStringForHud($"Aucun Respawn tant que le bombardement est activé.", 5));
				else if (Coroutines.AirBombWait != 0 && Coroutines.AirBombWait < 60)
					curText = curText.Replace("[CENTER_DOWN]", FormatStringForHud($"Aucun Respawn. Un bombardement est prévu sur le site dans {Coroutines.AirBombWait} seconde{(Coroutines.AirBombWait <= 1 ? "" : "s")} !", 5));
				else if (Warhead.IsDetonated && SanyaRemastered.Instance.Config.StopRespawnAfterDetonated)
					if (Coroutines.AirBombWait != 0)
						curText = curText.Replace("[CENTER_DOWN]", FormatStringForHud($"Aucun Respawn après l'explosion du site, un bombardement vas être effectuer.", 5));
					else
						curText = curText.Replace("[CENTER_DOWN]", FormatStringForHud($"Aucun Respawn après l'explosion du site.", 5));
				else if (RespawnTickets.Singleton.GetAvailableTickets(SpawnableTeamType.NineTailedFox) <= 0 && RespawnTickets.Singleton.GetAvailableTickets(SpawnableTeamType.ChaosInsurgency) <= 0)
					curText = curText.Replace("[CENTER_DOWN]", FormatStringForHud($"Aucun Respawn. Il n'y a plus de tickets disponibles.", 5));
				else if (_respawnCounter == 0)//{(Respawn.NextKnownTeam == SpawnableTeamType.NineTailedFox ? "" : (Respawn.NextKnownTeam == SpawnableTeamType.ChaosInsurgency ? "":""))}
					curText = curText.Replace("[CENTER_DOWN]", FormatStringForHud($"Respawn en cours...", 5));
				else if (_respawnCounter != -1)
					curText = curText.Replace("[CENTER_DOWN]", FormatStringForHud($"Prochain Respawn dans {_respawnCounter} seconde{(_respawnCounter <= 1 ? "" : "s")}.", 5));
				else
					curText = curText.Replace("[CENTER_DOWN]", FormatStringForHud(string.Empty, 5));
			}
			else
				curText = curText.Replace("[CENTER_DOWN]", FormatStringForHud(string.Empty, 5));

			//[BOTTOM]
			if (!string.IsNullOrEmpty(_hudBottomString))
				curText = curText.Replace("[BOTTOM]", FormatStringForHud(_hudBottomString, 2));

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

