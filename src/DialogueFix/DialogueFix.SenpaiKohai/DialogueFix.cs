using BepInEx;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Diagnostics.Eventing.Reader;
using UnityEngine.Video;

namespace DialogueFix {
    [BepInProcess("SenpaiKouhai")]
    public partial class DialogueFixPlugin : BaseUnityPlugin {
    }
    public static class Hooks {
        //Dunno if this is intended or if JSK just forgot that arrays are 0 indexed
        //Either way, it annoys me ¯\_(ツ)_/¯
        [HarmonyILManipulator, HarmonyPatch(typeof(F_Manager), "Update")]
        public static void FManagerUpdateManipulator(ILContext ctx) {
            var c = new ILCursor(ctx);
            for (int i = 0; i < 4; i++) {
                c.GotoNext(MoveType.After,
                    x => (x.MatchLdsfld(AccessTools.Field(typeof(Main_System), "Heroine1_GANSYA")) || x.MatchLdsfld(AccessTools.Field(typeof(Main_System), "Heroine2_GANSYA"))),
                    x => x.MatchLdcI4(3),
                    x => x.MatchBlt(out ILLabel _),
                    x => x.MatchLdcI4(1)
                    );
                c.Index--;
                c.Remove();
                c.Emit(OpCodes.Ldc_I4_0);
            }
        }

        static bool _nakadashi1 = false;
        static bool _nakadashi2 = false;
        static bool _resdashi1 = false;
        static bool _resdashi2 = false;

        [HarmonyILManipulator, HarmonyPatch(typeof(H_Manager), "Update")]
        public static void HManagerUpdateManipulator(ILContext ctx) {            
            var boolArray = new string[4] { "_nakadashi1", "_nakadashi2", "_resdashi1", "_resdashi2" };
            var varArray = new string[4] { "Heroine1_NAKADASHI", "Heroine2_NAKADASHI", "Heroine1_RESDASHI", "Heroine2_RESDASHI" };
            for (int i = 0; i < 4; i++) {
                var c = new ILCursor(ctx);
                c.GotoNext(MoveType.After,
                    x => x.MatchLdsfld(AccessTools.Field(typeof(Main_System), varArray[i])),
                    x => x.MatchLdcI4(3),
                    x => x.MatchBlt(out ILLabel _),
                    x => x.MatchLdcI4(1)
                    );
                c.Index--;
                c.Remove();
                c.Emit(OpCodes.Ldc_I4_0);
                c.Emit(OpCodes.Stsfld, AccessTools.Field(typeof(Hooks), boolArray[i]));
                c.Emit(OpCodes.Ldc_I4_0);
            }
        }

        [HarmonyPrefix, HarmonyPatch(typeof(Main_System), nameof(Main_System.Demo_Ikou))]
        public static void DemoIkouPrefix(ref int[] __state) {
            __state = new int[4];
            if (_nakadashi1) {
                __state[0] = Main_System.Heroine1_NAKADASHI;
                Main_System.Heroine1_NAKADASHI = 2;
            }
            if (_nakadashi2) {
                __state[1] = Main_System.Heroine2_NAKADASHI;
                Main_System.Heroine2_NAKADASHI = 2;
            }
            if (_resdashi1) {
                __state[2] = Main_System.Heroine1_RESDASHI;
                Main_System.Heroine1_RESDASHI = 2;
            }
            if (_resdashi2) {
                __state[3] = Main_System.Heroine2_RESDASHI;
                Main_System.Heroine2_RESDASHI = 2;
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Main_System), nameof(Main_System.Demo_Ikou))]
        public static void DemoIkouPostfix(ref int[] __state) {           
            if (_nakadashi1) {              
                Main_System.Heroine1_NAKADASHI = __state[0];
            }
            if (_nakadashi2) {               
                Main_System.Heroine2_NAKADASHI = __state[1];
            }
            if (_resdashi1) {                
                Main_System.Heroine1_RESDASHI = __state[2];
            }
            if (_resdashi2) {               
                Main_System.Heroine2_RESDASHI = __state[3];
            }
        }
    }
}
