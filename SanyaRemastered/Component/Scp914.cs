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
	public class Scp914 : PlayerEffect , IHealablePlayerEffect , IDisplayablePlayerEffect
	{
		public bool GetSpectatorText(out string s)
		{
			s = $"SCP-914 ({Intensity})";
			return true;
		}
		public bool IsHealable(ItemType it)
		{
			return it == ItemType.SCP500;
		}
		public override void Enabled()
		{
			TimeBetweenTicks = 1;
			TimeLeft = TimeBetweenTicks;
		}
		public override void OnUpdate()
		{
			TimeLeft -= Time.deltaTime;
			if (TimeLeft > 0f)
			{
				return;
			}
			Log.Info($"Scp914 Hurt {TimeBetweenTicks}");
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