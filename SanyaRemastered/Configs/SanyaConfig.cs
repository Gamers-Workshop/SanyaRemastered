using Exiled.API.Features;
using Exiled.API.Interfaces;
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
        public string IsBeta { get; set; } = string.Empty;


        [Description("WIP")]
        public bool IsDebugged { get; set; } = false;
        public bool Coroding106 { get; set; } = false;
        public bool ExplodingGrenadeTesla { get; set; } = false;
        public bool GateClosingAuto { get; set; } = false;

        public string BoxMessageOnJoin { get; set; } = string.Empty;

        [Description("Ram Testing")]
        public bool RamInfo { get; set; } = false;
        [Description("Ram Restart if to much ram (Go)")]
        public double RamRestartNoPlayer { get; set; } = -1;
        public double RamRestartWithPlayer { get; set; } = -1;


        [Description("RandomRespawnPosPercent")]
        public int RandomRespawnPosPercent { get; set; } = -1;
        [Description("\n  # Serveur Config\n  # Localisation des données des joueurs")]
        public string DataDirectory { get; private set; } = string.Empty;

        [Description("Hud Activé")]
        public bool ExHudEnabled { get; set; } = false;

        [Description("Hud Scp-079 auras plus d'info")]
        public bool ExHudScp079Moreinfo { get; set; } = false;
        [Description("Hud Scp-096 auras plus d'info")]
        public bool ExHudScp096 { get; set; } = false;

        [Description("Disable Player lists")]
        public bool DisablePlayerLists { get; set; } = false;
        [Description("NukeCap peut étre refermer")]
        public bool Nukecapclose { get; set; } = false;
        [Description("Tesla Config")]
        public float TeslaRange { get; set; } = 5.5f;
        public bool TeslaDestroyName { get; set; } = false;
        public bool TeslaNoTriggerRadioPlayer { get; set; } = false;

        [Description("Ajout de porte sur la map")]
        public bool AddDoorsOnSurface { get; set; } = false;
        public bool EditObjectsOnSurface { get; set; } = false;
        [Description("Game config")]

        public bool IntercomInformation { get; set; } = false;
        public bool IntercomBrokenOnBlackout { get; set; } = false;
        public bool CloseDoorsOnNukecancel { get; set; } = false;
        public bool FemurBreakerCanBeUsedWithNo106 { get; set; } = false;
        public int OutsidezoneTerminationTimeAfterNuke { get; set; } = -1;
        [Description("Désactivé la microhid pour les rôle")]
        public List<RoleType> MicroHidNotActive { get; set; } = new() {RoleType.ClassD, RoleType.Scientist };

        [Description("Generator Config")]
        public bool GeneratorUnlockOpen { get; set; } = false;
        public bool GeneratorFinishLock { get; set; } = false;
        public bool GeneratorActivatingClose { get; set; } = false;

        [Description("\n  # Human Balanced")]
        public bool StopRespawnAfterDetonated { get; set; } = false;
        public bool Item_shoot_move { get; set; } = false;
        public bool Grenade_shoot_fuse { get; set; } = false;
        public bool OpenDoorOnShoot { get; set; } = false;
        public bool GrenadeChainSametiming { get; set; } = false;
        public bool GodmodeAfterEndround { get; set; } = false;
        public float PainEffectStart { get; set; } = -1;

        [Description("Donne un effect d'assourdissement quand on est proche de l'explosion d'une grenade")]
        public bool GrenadeEffect { get; set; } = false;

        [Description("Stamina Add \n  # Stamina effect ajoute un leger ralentissement quand la personne n'as pas de stamina")]
        public bool StaminaEffect { get; set; } = false;
        public float StaminaLostJump { get; set; } = 0.05f;
        [Description("Ascenceur Attente")]
        public float WaitForUseLift { get; set; } = 0;

        [Description("\n  # SCP Balanced " +
            "Permet au SCP de .contain")]
        public bool ContainCommand { get; set; } = false;

        [Description("Le cadavre n'apparait pas quand on se fait tuer par")]
        public bool Scp106RemoveRagdoll { get; set; } = false;
        public bool Scp096RemoveRagdoll { get; set; } = false;
        [Description("Scp106 est ralentie")]
        public bool Scp106slow { get; set; } = false;
        [Description("Scp939 est ralentie")]
        public bool Scp939slow { get; set; } = false;
        [Description("Effect sur SCP-049-2")]
        public bool Scp0492effect { get; set; } = false;

        [Description("SCP-914 = Effect")]
        public bool Scp914Effect { get; set; } = false;

        [Description("Hitmark Add")]
        public bool HitmarkGrenade { get; set; } = false;
        public bool HitmarkKilled { get; set; } = false;

        [Description("SCP-939 Patches")]

        public float Scp939Size { get; set; } = 1f;
        [Description("RP")]
        public bool Scp096Real { get; set; } = false;
        public bool Scp049Real { get; set; } = false;
        public bool Scp173Real { get; set; } = false;
        public bool ScpTakeFallDamage { get; set; } = false;
        public List<string> ScpFallDamage { get; set; } = new()
        {
            "Scp049",
            "Scp0492",
            "Scp93989",
            "Scp93953"
        };

        [Description("Dégats Usp")]
        public float UspDamageMultiplierHuman { get; set; } = 1f;
        public float UspDamageMultiplierScp { get; set; } = 1f;
        [Description("Armor Protect on SCP-939")]
        public int Scp939EffectiveArmor { get; set; } = -1;


        [Description("Recovery Amount")]
        public Dictionary<string, int> ScpRecoveryAmount { get; set; } = new()
        {
            {"Scp049", 0},
            {"Scp0492", 0},
            {"Scp096", 0},
            {"Scp106", 0},
            {"Scp173", 0},
            {"Scp939", 0}
        };
        [Description("Multiplicateur de dégats")]
        public Dictionary<RoleType, float> ScpDamageMultiplicator { get; set; } = new()
        {
            {RoleType.Scp049, 1f},
            {RoleType.Scp0492, 1f},
            {RoleType.Scp096, 1f},
            {RoleType.Scp173, 1f},
            {RoleType.Scp93953, 1f},
            {RoleType.Scp93989, 1f},
        };
        [Description("Ne comprends pas la MicroHid Ni la Tesla")]
        public float Scp106DamageMultiplicator { get; set; } = 1f;
        public float Scp106GrenadeMultiplicator { get; set; } = 1f;

        [Description("SCP Activé les mode étendue")]
        public bool Scp079ExtendEnabled { get; set; } = false;

        [Description("SCP-079 Config Plugin \n" +
            "# Pour désactivé une capacité Scp079ExtendLevel = 6\n" +
            "# Tp a la caméra du générateur activé le plus proche")]
        public int Scp079ExtendLevelFindGeneratorActive { get; set; } = 1;
        public float Scp079ExtendCostFindGeneratorActive { get; set; } = 10f;
        [Description("DoorBeep")]

        public int Scp079ExtendLevelFindscp { get; set; } = 1;
        public float Scp079ExtendCostFindscp { get; set; } = 10f;
        [Description("DoorBeep")]
        public int Scp079ExtendLevelDoorbeep { get; set; } = 1;
        public float Scp079ExtendCostDoorbeep { get; set; } = 5f;
        [Description("Blackout Intercom")]
        public int Scp079ExtendBlackoutIntercom { get; set; } = 1;
        public float Scp079ExtendCostBlackoutIntercom { get; set; } = 5f;

        [Description("SCP-079 GAS Config")]
        public List<string> GazBlacklistRooms { get; set; } = new();
        public int GasDuration { get; set; } = 60;
        public int GasTimerWait { get; set; } = 60;
        public int GasExpGain { get; set; } = 10;
        public float Scp079ExCostGaz { get; set; } = 150;
        public int Scp079ExLevelGaz { get; set; } = 4;
        public int GasWaitingTime { get; set; } = 60;

        [Description("SCP-079 Config")]
        public Dictionary<string, float> Scp079ManaCost { get; set; } = new()
        {
            {"Camera Switch",                   1f },
            {"Door Lock",                       4f },
            {"Door Lock Start",                 5f },
            {"Door Lock Minimum",              10f },
            {"Door Interaction DEFAULT",        5f },
            {"Door Interaction CONT_LVL_1",    50f },
            {"Door Interaction CONT_LVL_2",    40f },
            {"Door Interaction CONT_LVL_3",   110f },
            {"Door Interaction ARMORY_LVL_1",  50f },
            {"Door Interaction ARMORY_LVL_2",  60f },
            {"Door Interaction ARMORY_LVL_3",  70f },
            {"Door Interaction EXIT_ACC",      60f },
            {"Door Interaction INCOM_ACC",     30f },
            {"Door Interaction CHCKPOINT_ACC", 10f },
            {"Room Lockdown",                  60f },
            {"Tesla Gate Burst",               50f },
            {"Elevator Teleport",              30f },
            {"Elevator Use",                   10f },
            {"Speaker Start",                  10f },
            {"Speaker Update",                0.8f }
        };
        [Description("SCP Can't interact Now")]
        public bool ScpCantInteract { get; set; } = false;
        public Dictionary<string, List<RoleType>> ScpCantInteractList { get; set; } = new()
        {
            {"Use914",                   new List<RoleType>{RoleType.Scp173,RoleType.Scp106,RoleType.Scp096,RoleType.Scp049,RoleType.Scp0492, RoleType.Scp93953, RoleType.Scp93989, RoleType.Scp079 } },
            {"Contain106",               new List<RoleType>{RoleType.Scp173,RoleType.Scp106,RoleType.Scp096,RoleType.Scp049,RoleType.Scp0492, RoleType.Scp93953, RoleType.Scp93989, RoleType.Scp079 } },
            {"DetonateWarhead",          new List<RoleType>{RoleType.Scp173,RoleType.Scp106,RoleType.Scp096,RoleType.Scp049,RoleType.Scp0492, RoleType.Scp93953, RoleType.Scp93989, RoleType.Scp079 } },
            {"AlphaWarheadButton",       new List<RoleType>{RoleType.Scp173,RoleType.Scp106,RoleType.Scp096,RoleType.Scp049,RoleType.Scp0492, RoleType.Scp93953, RoleType.Scp93989, RoleType.Scp079 } },
            {"UseElevator",              new List<RoleType>{RoleType.Scp173,RoleType.Scp106,RoleType.Scp096,RoleType.Scp049,RoleType.Scp0492, RoleType.Scp93953, RoleType.Scp93989, RoleType.Scp079 } },
            {"UseGenerator",             new List<RoleType>{RoleType.Scp173,RoleType.Scp106,RoleType.Scp096,RoleType.Scp049,RoleType.Scp0492, RoleType.Scp93953, RoleType.Scp93989, RoleType.Scp079 } },
            {"UseLocker",                new List<RoleType>{RoleType.Scp173,RoleType.Scp106,RoleType.Scp096,RoleType.Scp049,RoleType.Scp0492, RoleType.Scp93953, RoleType.Scp93989, RoleType.Scp079 } },
            {"UseAlphaWarheadPanel",     new List<RoleType>{RoleType.Scp173,RoleType.Scp106,RoleType.Scp096,RoleType.Scp049,RoleType.Scp0492, RoleType.Scp93953, RoleType.Scp93989, RoleType.Scp079 } },
            {"DoorInteractOpen",         new List<RoleType>{RoleType.Scp173,RoleType.Scp106,RoleType.Scp096,RoleType.Scp049,RoleType.Scp0492, RoleType.Scp93953, RoleType.Scp93989, RoleType.Scp079 } },
            {"DoorInteractClose",        new List<RoleType>{RoleType.Scp173,RoleType.Scp106,RoleType.Scp096,RoleType.Scp049,RoleType.Scp0492, RoleType.Scp93953, RoleType.Scp93989, RoleType.Scp079 } },
        };

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