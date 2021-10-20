using CustomPlayerEffects;
using Exiled.API.Features;
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
		public int TimeBeforeContain = 25;
		public string CassieAnnounceContain;
		public List<Door> doors = new List<Door>();

		private void Start()
		{
			_plugin = SanyaRemastered.Instance;
			_player = Player.Get(gameObject);
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

			if (doors.Count == doors.Where(x=>x.Base.GetExactState() == 0f).Count())
            {
				if (TimeBeforeContain <= 0)
                {
					foreach (Door door in doors)
						door.Base.ServerChangeLock(DoorLockReason.SpecialDoorFeature, true);
					try { Methods.SpawnDummyModel(_player.Position, default, _player.Role, _player.Scale); } catch (Exception) { Log.Error("Cant spawn dummy"); }
					Cassie.GlitchyMessage(CassieAnnounceContain, 0.05f, 0.05f);
					_player.SetRole(RoleType.Spectator);
				}
			}
			else
				Destroy(this);
			TimeBeforeContain--;
		}
	}
}
