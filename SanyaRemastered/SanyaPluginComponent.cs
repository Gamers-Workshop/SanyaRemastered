using System;
using System.Linq;
using UnityEngine;
using Mirror;
using Mirror.LiteNetLib4Mirror;
using Respawning;
using Exiled.API.Features;
using Exiled.API.Extensions;

using SanyaPlugin.Data;
using SanyaPlugin.Functions;
using System.Collections.Generic;
using System.Threading;
using GameCore;

namespace SanyaPlugin
{
	public class SanyaPluginComponent : MonoBehaviour
	{
		public static readonly HashSet<Player> _scplists = new HashSet<Player>();

		public bool DisableHud = false;

		private SanyaPlugin _plugin;
		private Player _player;
		private Vector3 _espaceArea;
		private string _hudTemplate = "<align=left><voffset=38em><size=50%>([STATS])\n</size></align><align=right>[LIST]</align><align=center>[CENTER_UP][CENTER][CENTER_DOWN][BOTTOM]</align></voffset>";
		private float _timer = 0f;
		private int _respawnCounter = -1;
		private string _hudText = string.Empty;
		private string _hudCenterDownString = string.Empty;
		private float _hudCenterDownTime = -1f;
		private float _hudCenterDownTimer = 0f;

		private void Start()
		{
			_plugin = SanyaPlugin.Instance;
			_player = Player.Get(gameObject);
			_espaceArea = new Vector3(177.5f, 985.0f, 29.0f);
		}

		private void FixedUpdate()
		{
			if (!_plugin.Config.IsEnabled) return;

			_timer += Time.deltaTime;

			UpdateTimers();

			CheckTraitor();
			CheckVoiceChatting();
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

		public void ClearHudCenterDownText(string text, ulong timer)
		{
			_hudCenterDownTime = -1f;
		}

		public void UpdateTimers()
		{
			if (_hudCenterDownTimer < _hudCenterDownTime)
				_hudCenterDownTimer += Time.deltaTime;
			else
				_hudCenterDownString = string.Empty;
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
						_player.SetRole(RoleType.ChaosInsurgency);
						break;
					case Team.CHI:
						_player.SetRole(RoleType.NtfCadet);
						break;
				}
			}
			else
				_player.SetRole(RoleType.Spectator);
		}

		private void CheckVoiceChatting()
		{
			if (_plugin.Config.Scp939CanSeeVoiceChatting == 0) return;

			if (_player.IsHuman()
				&& _player.GameObject.TryGetComponent(out Radio radio)
				&& (radio.isVoiceChatting || radio.isTransmitting))
				_player.ReferenceHub.footstepSync._visionController.MakeNoise(25f);
		}

		private void UpdateRespawnCounter()
		{
			if (!RoundSummary.RoundInProgress() || Warhead.IsDetonated || _player.Role != RoleType.Spectator || _timer < 1f) return;

			_respawnCounter = (int)Math.Truncate(RespawnManager.CurrentSequence() == RespawnManager.RespawnSequencePhase.RespawnCooldown ? RespawnManager.Singleton._timeForNextSequence - RespawnManager.Singleton._stopwatch.Elapsed.TotalSeconds : 0);
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
			if (DisableHud || !_plugin.Config.ExHudEnabled) return;

			string curText = _hudTemplate;
			//[LEFT_UP]
			if (_player.IsMuted && _player.GameObject.TryGetComponent(out Radio radio) && (radio.isVoiceChatting || radio.isTransmitting)) 
				curText = _hudTemplate.Replace("[STATS]",$"Vous avez été mute");
			curText = curText.Replace("([STATS])", string.Empty);
			//[LIST]
			if (_player.Team == Team.SCP)
			{
				string List = string.Empty;
				if (_player.Role == RoleType.Scp079 && SanyaPlugin.Instance.Config.ExHudScp079Moreinfo)
				{
					foreach (var scp in _scplists)
						if (scp.Role == RoleType.Scp079)
							List += $"<u>{scp.ReferenceHub.characterClassManager.CurRole.fullName}:Tier{scp.ReferenceHub.scp079PlayerScript.curLvl + 1}</u>\n";
						else
							List += $"{scp.ReferenceHub.characterClassManager.CurRole.fullName}:{scp.GetHealthAmountPercent()}%\n";
					List.TrimEnd('\n');
				}
				curText = curText.Replace("[LIST]", FormatStringForHud(List, 6));
			}
			else
				curText = curText.Replace("[LIST]", FormatStringForHud(string.Empty, 6));
			
			//[CENTER_UP]
			if (_player.Role == RoleType.Scp079 && SanyaPlugin.Instance.Config.Scp079ExtendEnabled)
				curText = curText.Replace("[CENTER_UP]", FormatStringForHud(_player.ReferenceHub.animationController.curAnim == 1 ? "Extend:Enabled" : "Extend:Disabled", 6));
			else if (_player.Role == RoleType.Scp106 && SanyaPlugin.Instance.Config.Scp106WalkthroughCooldown != -1)
				if (SanyaPlugin.Instance.Handlers.last106walkthrough.Elapsed.TotalSeconds > _plugin.Config.Scp106WalkthroughCooldown || _player.IsBypassModeEnabled)
					curText = curText.Replace("[CENTER_UP]", FormatStringForHud($"Extend:Ready", 6));
				else
					curText = curText.Replace("[CENTER_UP]", FormatStringForHud($"Extend:Charging({_plugin.Config.Scp106WalkthroughCooldown - (int)SanyaPlugin.Instance.Handlers.last106walkthrough.Elapsed.TotalSeconds}s left)", 6));
			else
				curText = curText.Replace("[CENTER_UP]", FormatStringForHud(string.Empty, 6));

			//[CENTER]
			curText = curText.Replace("[CENTER]", FormatStringForHud(string.Empty, 6));

			//[CENTER_DOWN]
			if (_player.Team == Team.RIP)
			{ 
				if (_respawnCounter == 0)
					curText = curText.Replace("[CENTER_DOWN]", FormatStringForHud($"Respawn en cours", 6));
				else
					curText = curText.Replace("[CENTER_DOWN]", FormatStringForHud($"Prochain Respawn dans {_respawnCounter} secondes", 6));
				if (!string.IsNullOrEmpty(_hudCenterDownString))
					curText = curText.Replace("[CENTER_DOWN]", FormatStringForHud(_hudCenterDownString, 6));
			}
			else if(!string.IsNullOrEmpty(_hudCenterDownString))
				curText = curText.Replace("[CENTER_DOWN]", FormatStringForHud(_hudCenterDownString, 6));
			else
				curText = curText.Replace("[CENTER_DOWN]", FormatStringForHud(string.Empty, 6));

			//[BOTTOM]
			curText = curText.Replace("[BOTTOM]", FormatStringForHud(string.Empty, 6));

			if (_hudText != curText || RoundSummary.roundTime > 0)
			{
				_hudText = curText;
				_player.SendTextHintNotEffect(_hudText, 2);
			}
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

