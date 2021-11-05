﻿using CustomPlayerEffects;
using Exiled.API.Extensions;
using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SanyaRemastered
{
	public class Scp914Effect : MonoBehaviour
	{

		private SanyaRemastered _plugin;
		private Player _player;
		private float _timer = 0f;
		public int TimerBeforeDeath;

		private void Start()
		{
			_plugin = SanyaRemastered.Instance;
			_player = Player.Get(gameObject);
			_player.EnableEffect<Scp207>();
			_player.ChangeEffectIntensity<Scp207>(4);

		}
		private void OnDestroy()
		{
			_player.ChangeRunningSpeed(default);
			_player.ChangeWalkingSpeed(default);
		}

		private void FixedUpdate()
		{
			if (!_plugin.Config.IsEnabled) return;

			_timer += Time.deltaTime;

			IsGoingToDeath();

			if (_timer > 1f)
				_timer = 0f;
		}

		public void IsGoingToDeath()
		{
			if (!(_timer > 1f)) return;
			TimerBeforeDeath--;
			if (TimerBeforeDeath < 0)
            {
				_player.ReferenceHub.playerStats.HurtPlayer(new PlayerStats.HitInfo(914914, "SCP-914", DamageTypes.Scp207, 0, true), _player.ReferenceHub.gameObject);
				_player.ReferenceHub.GetComponent<SanyaRemasteredComponent>().AddHudCenterDownText("Vous êtes mort d'un arret cardiaque", 20);
			}
		}
	}
}

