using BepInEx;

namespace Uncensor {

    [BepInProcess("MGBuster")]
    public partial class UncensorPlugin : BaseUnityPlugin {

        readonly static string[] destroyRectangle = new string[]{ "MAINSCENE/Quad", "MAINSCENE1/Quad", "MAINSCENE2/Quad", "MAINSCENE2/Quad (1)", "MAINSCENE3/Quad", "MAINSCENE4/Quad", "MAINSCENE4/Quad (1)",
            "_MAOH_MAIN_/Quad", "_MAOH_MAIN_/Quad (1)", "DEMOSCENE/DEMOSCENE2/Quad", "DEMOSCENE/DEMOSCENE2/Quad (1)", "DEMOSCENE/Quad (1)", "DEMOSCENE/Quad", "___MAOHSAMA___/Quad" };
    }
}
