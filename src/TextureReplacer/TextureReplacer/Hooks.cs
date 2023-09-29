using HarmonyLib;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace TextureReplacer {
    public static class Hooks {
     

        static internal void InstallHooks() {
            var harmony = Harmony.CreateAndPatchAll(typeof(Hooks));
        }

        [HarmonyILManipulator, HarmonyPatch(typeof(Anima2D.SpriteMeshInstance), "spriteTexture", MethodType.Getter)]
        public static void SpriteTextureManipulator(ILContext ctx) {
            var c = new ILCursor(ctx);

            c.GotoNext(x => x.MatchCallvirt(AccessTools.Method(typeof(Sprite), "get_texture")));
            c.Index++;
            c.EmitDelegate<Func<Texture2D, Texture2D>>((t) => {
                var name = t.UniqueName();
                if (TextureReplacerPlugin.dumpSprites && TextureReplacerPlugin.dumpedCache.Add(name)) TextureReplacerPlugin.DumpSprite(t);
                if (!TextureReplacerPlugin.PluginEnabled.Value || TextureReplacerPlugin.currentReplacement == null || !TextureReplacerPlugin.currentReplacement.ContainsKey(name)) return t;
                return TextureReplacerPlugin.currentReplacement[name];
            });
        }//TODO: Investigate if NativeDetour can actually be used here to deteour the SR sprite property? Dunno if this works for prefabbed SRs. XUAT seems to make it work somehow though
    }              
}
