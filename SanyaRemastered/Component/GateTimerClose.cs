using CustomPlayerEffects;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SanyaRemastered
{
#pragma warning disable IDE0051
	public class GateTimerClose :  BasicDoor
	{

		private DoorVariant _door;
		private float _timer = 0f;
		private readonly int _timerBeforeClose = 20;
		public int _timeBeforeClosing = -1;

		new private void Start()
		{
			_door = gameObject.GetComponent<DoorVariant>();
		}
		public override void Update()
		{
			base.Update();

			_timer += Time.deltaTime;
			ClosingGate();

			if (_timer > 1f)
				_timer = 0f;
		}

		public void ClosingGate()
		{
			if (!(_timer > 1f)) return;
			if (_door.TargetState)
			{
				if (_timeBeforeClosing == -1)
				{
					_timeBeforeClosing = _timerBeforeClose;
				}
				if (_timeBeforeClosing == 0)
				{
					Door.Get(_door).IsOpen = false;
					_timeBeforeClosing = -1;
				}

				_timeBeforeClosing--;
				return;
			}
			if (_timeBeforeClosing == -1)
				return;

			_timeBeforeClosing = -1;
			return;
		}
	}
}


