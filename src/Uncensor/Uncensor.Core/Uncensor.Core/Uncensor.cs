using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Uncensor {
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]    
    public partial class UncensorPlugin : BaseUnityPlugin {

        internal static ManualLogSource logger;

        internal static ConfigEntry<bool> PluginEnabled;

        void Awake() {
            logger = Logger;
            PluginEnabled = Config.Bind("General", "Enabled", true, "Enables the plugin");
            PluginEnabled.Value = true;


            //Only need to use this when making a plugin for a new game
            //CheckEntriesAreUnique();

            SceneManager.sceneLoaded += DestroyQuad;
        }

        //TODO: Fix this awful implementation
        //Maybe separate each set of GameObjects per scene and only iterate that set on the specific scene
        //Hook GameObject construtor maybe? Probably bad idea. Dunno if that even works when these are serialised assets from the editor, not from Ass-CSharp
        internal static void DestroyQuad(Scene s, LoadSceneMode lsm) {
            if (!PluginEnabled.Value) return;
            foreach (string rectangle in destroyRectangle) {
                var go = GameObject.Find(rectangle);
                if (go != null) {
                    go.SetActive(false);
                }
            }
        }



       
        ////The entries don't actually have to be unique, but if we're doing this awful iterate thing we should at least try to not repeat work
        //internal static void CheckEntriesAreUnique() {           

        //    var set = new HashSet<string>();
        //    var failSet = new Dictionary<string, int>();

        //    foreach(string entry in destroyRectangle) {
        //        if (!set.Add(entry)) {
        //            if (!failSet.ContainsKey(entry)) {
        //                failSet.Add(entry, 1);
        //            } else {
        //                failSet[entry]++;
        //            }
        //        }
        //    }

        //    if (failSet.Count > 0) {
        //        logger.LogMessage("Entries in Uncensor plugin are not unique! The following entries are repeated:");
        //        foreach (KeyValuePair<string, int> count in failSet) {
        //            logger.LogMessage(count.Key + " repeated " + count.Value + " times");
        //        }
        //    }
        //}
    }
}
