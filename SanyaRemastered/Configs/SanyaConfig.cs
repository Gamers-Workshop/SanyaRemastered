using Exiled.API.Features;
using Exiled.API.Interfaces;
using PlayerRoles;
using SanyaRemastered.Functions;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;

namespace SanyaRemastered.Configs
{
    public sealed class Config : IConfig
    {
        public Config()
        {
            DataDirectory = Path.Combine(Paths.Plugins, "SanyaRemastered");
        }
        [Description("Activation du SanyaRemastered")]
        public bool IsEnabled { get; set; }
        public bool Debug { get; set; }
        public string IsBeta { get; set; } = string.Empty;

        [Description("Ram Testing")]
        public bool RamInfo { get; set; } = false;

        [Description("Ram Restart if to much ram (Go)")]
        public double RamRestartNoPlayer { get; set; } = -1;
        public double RamRestartWithPlayer { get; set; } = -1;

        [Description("Affiche une étoile pour tous les joueurs (comme le setnick)")]
        public bool AllStar { get; set; } = false;
        [Description("\n  # Serveur Config\n  # Localisation des données des joueurs")]
        public string DataDirectory { get; private set; } = string.Empty;
        public string AudioSoundAirBomb { get; private set; } = "/home/scp/.config/EXILED/Configs/AudioAPI/Siren.ogg";

        [Description("Hud Activé")]
        public bool ExHudEnabled { get; set; } = false;

        [Description("NukeCap peut étre refermer")]
        public bool Nukecapclose { get; set; } = false;

        [Description("Game config")]
        public bool IntercomInformation { get; set; } = false;
        public bool IntercomBrokenOnBlackout { get; set; } = false;
        public bool CloseDoorsOnNukecancel { get; set; } = false;
        public int OutsidezoneTerminationTimeAfterNuke { get; set; } = -1;
        [Description("Generator Config")]
        public bool GeneratorUnlockOpen { get; set; } = false;
        public bool GeneratorFinishLock { get; set; } = false;
        public bool GeneratorActivatingClose { get; set; } = false;

        [Description("\n  # Human Balanced")]
        public bool StopRespawnAfterDetonated { get; set; } = false;
        public bool ItemShootMove { get; set; } = false;
        public bool OpenDoorOnShoot { get; set; } = false;
        public bool GodmodeAfterEndround { get; set; } = false;
        [Description("Hitmark Add")]
        public bool HitmarkGrenade { get; set; } = false;
        public bool HitmarkKilled { get; set; } = false;

        public string GetConfigs()
        {
            string returned = "\n";

            PropertyInfo[] infoArray = typeof(Config).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo info in infoArray)
            {
                if (info.PropertyType.IsList())
                {
                    returned += $"{info.Name}:\n";
                    if (info.GetValue(this) is IEnumerable list)
                        foreach (object i in list) returned += $"{i}\n";
                }
                else if (info.PropertyType.IsDictionary())
                {
                    returned += $"{info.Name}: ";

                    object obj = info.GetValue(this);

                    IDictionary dict = (IDictionary)obj;

                    PropertyInfo key = obj.GetType().GetProperty("Keys");
                    PropertyInfo value = obj.GetType().GetProperty("Values");
                    object keyObj = key.GetValue(obj, null);
                    object valueObj = value.GetValue(obj, null);
                    IEnumerable keyEnum = keyObj as IEnumerable;

                    foreach (var i in dict.Keys)
                    {
                        returned += $"[{i}:{dict[i]}]";
                    }

                    returned += "\n";
                }
                else
                {
                    returned += $"{info.Name}: {info.GetValue(this)}\n";
                }
            }

            FieldInfo[] fieldInfos = typeof(Config).GetFields(BindingFlags.Public | BindingFlags.Instance);

            foreach (FieldInfo info in fieldInfos)
            {
                if (info.FieldType.IsList())
                {
                    returned += $"{info.Name}:\n";
                    if (info.GetValue(this) is IEnumerable list)
                        foreach (object i in list) returned += $"{i}\n";
                }
                else if (info.FieldType.IsDictionary())
                {
                    returned += $"{info.Name}: ";

                    object obj = info.GetValue(this);

                    IDictionary dict = (IDictionary)obj;

                    PropertyInfo key = obj.GetType().GetProperty("Keys");
                    PropertyInfo value = obj.GetType().GetProperty("Values");
                    object keyObj = key.GetValue(obj, null);
                    object valueObj = value.GetValue(obj, null);
                    IEnumerable keyEnum = keyObj as IEnumerable;

                    foreach (object i in dict.Keys)
                    {
                        if (dict[i].GetType().IsList())
                        {
                            returned += $"[{i}:";
                            if (dict[i] is IEnumerable list)
                                foreach (var x in list) returned += $"{x},";
                            returned += "]";
                        }
                        else
                        {
                            returned += $"[{i}:{dict[i]}]";
                        }
                    }

                    returned += "\n";
                }
                else
                {
                    returned += $"{info.Name}: {info.GetValue(this)}\n";
                }
            }

            return returned;
        }

    }
}