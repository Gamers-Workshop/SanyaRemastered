using CustomPlayerEffects;
using Exiled.API.Features;
using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using SanyaRemastered.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SanyaRemastered
{
    public class ContainScpComponent : MonoBehaviour
    {

		private SanyaRemastered _plugin;
		private Player _player;
		private float _timer = 0f;
		private Room _room = null;
		public int TimeBeforeContain = 25;
		public string CassieAnnounceContain;
		public List<Door> doors = new();

		private void Start()
		{
			_plugin = SanyaRemastered.Instance;
			_player = Player.Get(gameObject);
			_room = _player.CurrentRoom;
		}

		private void FixedUpdate()
		{
			if (!_plugin.Config.IsEnabled) return;

			_timer += Time.deltaTime;

			ContainSCP();

			if (_timer > 1f)
				_timer = 0f;
		}

		public void ContainSCP()
        {
			if (!(_timer > 1f)) return;
			_player.ReferenceHub.GetComponent<SanyaRemasteredComponent>().AddHudCenterDownText($"Vous allez être reconfiné dans {TimeBeforeContain} seconde{(TimeBeforeContain <= 1 ? "" : "s")}", 2);

			if (doors.Count == doors.Where(x=>x.Base.GetExactState() == 0f && (!x.Base.TryGetComponent(out BreakableDoor breakableDoor) || !breakableDoor.IsDestroyed)).Count() && _room == _player.CurrentRoom)
            {
				if (TimeBeforeContain <= 0)
                {
					foreach (Door door in doors)
                    {
						door.Base.ServerChangeLock(DoorLockReason.SpecialDoorFeature, true);
						if (door.Base is BreakableDoor dr)
						{
							dr._ignoredDamageSources &= DoorDamageType.Scp096;
							dr._ignoredDamageSources &= DoorDamageType.ServerCommand;
							dr._ignoredDamageSources &= DoorDamageType.Grenade;
							dr._ignoredDamageSources &= DoorDamageType.Weapon;
							dr._ignoredDamageSources &= DoorDamageType.None;
						}
					}
					Cassie.GlitchyMessage(CassieAnnounceContain, 0.05f, 0.05f);

					_player.SetRole(RoleType.Spectator);
					Destroy(this);
				}
			}
			else
				Destroy(this);
			TimeBeforeContain--;
		}
	}
}
