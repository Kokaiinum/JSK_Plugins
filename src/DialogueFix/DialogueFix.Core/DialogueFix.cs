
using BepInEx;
using HarmonyLib;
using BepInEx.Configuration;
using BepInEx.Logging;
using System;

namespace DialogueFix {
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public partial class DialogueFixPlugin : BaseUnityPlugin {
        internal static ManualLogSource logger;

        internal static ConfigEntry<bool> PluginEnabled;

        internal static Harmony harmony;

        void Awake() {
            logger = Logger;
            PluginEnabled = Config.Bind("General", "Enabled", true, "Enables the plugin");
            PluginEnabled.Value = true;

            harmony = new Harmony(PluginInfo.PLUGIN_GUID);

            if (PluginEnabled.Value) harmony.PatchAll(typeof(Hooks)); //At time of writing all of the games use the same code for this but it's possible the classes/variable etc might get renamed in future games
                                                                      //so I think it's better to separate them now

            PluginEnabled.SettingChanged += enabledChanged;
            
        }

        void enabledChanged(object sender, EventArgs args) {
            if (PluginEnabled.Value) {              
            harmony.PatchAll(typeof(Hooks));                 
            } else {
                harmony.UnpatchSelf();
            }
        }
    }
}
