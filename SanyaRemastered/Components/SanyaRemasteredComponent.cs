using System;
using System.Linq;
using UnityEngine;
using Respawning;
using Exiled.API.Features;
using System.Text;
using SanyaRemastered.Data;
using SanyaRemastered.Functions;
using System.Collections.Generic;
using SanyaRemastered;
using Targeting;
using CustomPlayerEffects;
using Exiled.API.Extensions;
using System.Runtime.CompilerServices;
using HarmonyLib;
using MapGeneration.Distributors;
using Exiled.API.Enums;
using Exiled.API.Features.Roles;
using PlayerRoles;

namespace SanyaRemastered
{
	public class SanyaRemasteredComponent : MonoBehaviour
	{
		public bool DisableHud = false;

		private SanyaRemastered _plugin;
		private Player _player;
		private readonly string _hudTemplate = "<line-height=95%><voffset=8.5em><align=left><size=50%><alpha=#44>([STATS])<alpha=#ff></size></align>\n<align=right>[LIST]</align><align=center>[CENTER_UP][CENTER][CENTER_DOWN][BOTTOM]";
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

        //StringBuilder
        readonly StringBuilder list = new();
		//public float soundvolume = -1f;
		private void Start()
		{
			_plugin = SanyaRemastered.Instance;
			_player = Player.Get(gameObject);
		}

		private void FixedUpdate()
		{
			if (!_plugin.Config.IsEnabled) return;

			_timer += Time.deltaTime;

			UpdateTimers();

			UpdateRespawnCounter();
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
			/*
			if(!RoundSummary.RoundInProgress() || Warhead.IsDetonated || _player.Role.Team is not Team.Dead) return;
			if(RespawnManager.CurrentSequence() == RespawnManager.RespawnSequencePhase.RespawnCooldown)
				_respawnCounter = (int)Math.Truncate(RespawnManager.Singleton._timeForNextSequence - RespawnManager.Singleton._stopwatch.Elapsed.TotalSeconds);
			else
				_respawnCounter = 0;*/
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
            if (_player.Role is FpcRole fpcRole && fpcRole.IsInvisible)
				info += $"<b>vous êtes invisible</b> ";
			curText = curText.Replace("([STATS])", info);

			//[LIST]
			curText = curText.Replace("[LIST]", FormatStringForHud(string.Empty, 7));

			//[CENTER_UP]
			curText = curText.Replace("[CENTER_UP]", FormatStringForHud(string.Empty, 6));

			//[CENTER]
			curText = curText.Replace("[CENTER]", FormatStringForHud(string.Empty, 6));

			
			//[CENTER_DOWN]
			if (!string.IsNullOrEmpty(_hudCenterDownString))
				curText = curText.Replace("[CENTER_DOWN]", FormatStringForHud(_hudCenterDownString, 5));
			else if (_player.Role.Type is RoleTypeId.Spectator or RoleTypeId.Overwatch)
			{
				if (Coroutines.IsActuallyBombGoing)
					curText = curText.Replace("[CENTER_DOWN]", FormatStringForHud($"Aucun respawn tant que le bombardement est activé.", 5));
				else if (Coroutines.AirBombWait is not 0 && Coroutines.AirBombWait < 60)
					curText = curText.Replace("[CENTER_DOWN]", FormatStringForHud($"Aucun respawn. Un bombardement est prévu sur le site dans {Coroutines.AirBombWait} seconde{(Coroutines.AirBombWait <= 1 ? "" : "s")} !", 5));
				else if (Warhead.IsDetonated && SanyaRemastered.Instance.Config.StopRespawnAfterDetonated)
					if (Coroutines.AirBombWait is not 0)
						curText = curText.Replace("[CENTER_DOWN]", FormatStringForHud($"Aucun respawn après l'explosion du site, un bombardement vas être effectuer.", 5));
					else
						curText = curText.Replace("[CENTER_DOWN]", FormatStringForHud($"Aucun respawn après l'explosion du site.", 5));
				else if (_respawnCounter is 0)//{(Respawn.NextKnownTeam == SpawnableTeamType.NineTailedFox ? "" : (Respawn.NextKnownTeam == SpawnableTeamType.ChaosInsurgency ? "":""))}
					curText = curText.Replace("[CENTER_DOWN]", FormatStringForHud($"Respawn en cours...", 5));
				else if (_respawnCounter is not -1)
					curText = curText.Replace("[CENTER_DOWN]", FormatStringForHud($"Prochain respawn dans {_respawnCounter} seconde{(_respawnCounter <= 1 ? "" : "s")}.", 5));
				else
					curText = curText.Replace("[CENTER_DOWN]", FormatStringForHud(string.Empty, 5));
			}
			else
				curText = curText.Replace("[CENTER_DOWN]", FormatStringForHud(string.Empty, 5));

			//[BOTTOM]
			if (!string.IsNullOrEmpty(_hudBottomString))
				curText = curText.Replace("[BOTTOM]", FormatStringForHud(_hudBottomString, 2));
			else if (_player.Role.Is(out SpectatorRole spectator))
			{
				try
				{
					Player Spectate = spectator.SpectatedPlayer;
					if (Spectate is not null && Spectate.TryGetSessionVariable("NewRole", out Tuple<string, string> newrole))
					{
						curText = curText.Replace("[BOTTOM]", FormatStringForHud($"\n<b><color={Spectate.Role.Color.ToHex()}>{newrole.Item2}</color></b>", 2));
					}
					else
						curText = curText.Replace("[BOTTOM]", FormatStringForHud(string.Empty, 2));
				}
				catch
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

