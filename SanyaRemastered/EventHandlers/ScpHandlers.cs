using CustomPlayerEffects;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using Exiled.Events.EventArgs.Scp049;
using Exiled.Events.EventArgs.Scp079;
using Exiled.Events.EventArgs.Scp096;
using Exiled.Events.EventArgs.Scp173;
using Exiled.Events.EventArgs.Scp914;
using Hazards;
using InventorySystem.Items.Usables.Scp330;
using MapEditorReborn.Commands.ModifyingCommands.Position;
using Mirror;
using PlayerRoles;
using PlayerStatsSystem;
using RelativePositioning;
using SanyaRemastered.Data;
using Scp914;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SanyaRemastered.EventHandlers
{
    public class ScpHandlers
    {
        public ScpHandlers(SanyaRemastered plugin) => this.plugin = plugin;
        internal readonly SanyaRemastered plugin;

        public void OnAddingTarget(AddingTargetEventArgs ev)
        {
            if (ev.Target.IsGodModeEnabled)
                ev.IsAllowed = false;
        }
    }
}
