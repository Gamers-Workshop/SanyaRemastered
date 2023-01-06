using CustomPlayerEffects;
using Exiled.API.Extensions;
using Exiled.API.Features;
using PlayerStatsSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SanyaRemastered
{
	public class Scp914 : StatusEffectBase, IHealablePlayerEffect
	{
		public bool IsHealable(ItemType it) => it == ItemType.SCP500;

		int TimeBetweenTicks;

        public override void Enabled()
		{
			TimeBetweenTicks = 1;
			TimeLeft = TimeBetweenTicks;
		}
		public override void OnEffectUpdate()
		{
			TimeLeft -= Time.deltaTime;
			if (TimeLeft > 0f)
			{
				return;
			}

			TimeLeft = TimeBetweenTicks;
			if (Intensity == 255)
			{
				Hub.playerStats.DealDamage(new CustomReasonDamageHandler("SCP-914"));
				Hub.GetComponent<SanyaRemasteredComponent>().AddHudCenterDownText("Vous êtes mort d'un arret cardiaque", 20);
			}
			Intensity++;
		}
	}
}