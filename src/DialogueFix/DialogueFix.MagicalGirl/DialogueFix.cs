using BepInEx;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace DialogueFix {
    [BepInProcess("MGBuster")]
    public partial class DialogueFixPlugin : BaseUnityPlugin {
    }
    public static class Hooks {
        //Dunno if this is intended or if JSK just forgot that arrays are 0 indexed
        //Either way, it annoys me ¯\_(ツ)_/¯
        [HarmonyILManipulator, HarmonyPatch(typeof(F_Manager), "Update")]
        public static void FManagerUpdateManipulator(ILContext ctx) {
            var c = new ILCursor(ctx);
            for (int i = 0; i < 3; i++) {
                c.GotoNext(MoveType.After,
                    x => x.MatchLdsfld(AccessTools.Field(typeof(Main_System), "Heroine1_SYASEI")),
                    x => x.MatchLdcI4(3),
                    x => x.MatchBlt(out ILLabel _),
                    x => x.MatchLdcI4(1)
                    );
                c.Index--;
                c.Remove();
                c.Emit(OpCodes.Ldc_I4_0);
            }
        }

        [HarmonyILManipulator, HarmonyPatch(typeof(End_Manager), "Update")]
        public static void EndManagerUpdateManipulator(ILContext ctx) {
            var c = new ILCursor(ctx);
            c.GotoNext(MoveType.After,
                x => x.MatchLdsfld(AccessTools.Field(typeof(Main_System), "Heroine1_NAKADASHI")),
                x => x.MatchLdcI4(3),
                x => x.MatchBlt(out ILLabel _),
                x => x.MatchLdcI4(1)
                );
            c.Index--;
            c.Remove();
            c.Emit(OpCodes.Ldc_I4_0);
        }


        internal static bool _nakadashi = false;

        //TODO: This is read for endings, so fix that
        [HarmonyILManipulator, HarmonyPatch(typeof(H_Manager), "Update")]
        public static void HManagerUpdateManipulator(ILContext ctx) {
            var c = new ILCursor(ctx);

            c.GotoNext(MoveType.After,
                x => x.MatchLdsfld(AccessTools.Field(typeof(Main_System), "Heroine1_NAKADASHI")),
                x => x.MatchLdcI4(3),
                x => x.MatchBlt(out ILLabel _),
                x => x.MatchLdcI4(1)
                );
            c.Index--;
            c.Remove();
            c.Emit(OpCodes.Ldc_I4_1);
            c.Emit(OpCodes.Stsfld, AccessTools.Field(typeof(Hooks), "_nakadashi"));
            c.Emit(OpCodes.Ldc_I4_0);
        }

        [HarmonyPrefix, HarmonyPatch(typeof(Main_System), nameof(Main_System.Demo_Ikou))]
        public static void DemoIkouPrefix(ref int __state) {
            if (!_nakadashi) return;

            __state = Main_System.Heroine1_NAKADASHI;
            Main_System.Heroine1_NAKADASHI = 2;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Main_System), nameof(Main_System.Demo_Ikou))]
        public static void DemoIkouPostfix(ref int __state) {
            if (!_nakadashi) return;

            Main_System.Heroine1_NAKADASHI = __state;
        }
    }
}
