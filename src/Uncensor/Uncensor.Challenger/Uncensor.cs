using BepInEx;
using BepInEx.Logging;
using System.Runtime.InteropServices;

namespace Uncensor {

    [BepInProcess("Tougioh")]
    public partial class UncensorPlugin : BaseUnityPlugin {

        readonly static string[] destroyRectangle = new string[]{ "MAINSCENE/Quad", "MAINSCENE2/Quad (1)", "MAINSCENE3/Quad (1)", "MAINSCENE4/Quad (1)", // general
                                                                  "MAINSCENE/Otoko01/OTOKO_BASE/Hara/Quad (2)", "MAINSCENE2/_OTOKO2_/BASE/Body/Inkei/Quad (3)", "MAINSCENE3/Otoko01 (1)/OTOKO_BASE/Hara/Quad (2)", "MAINSCENE4/Otoko01 (1)/OTOKO_BASE/Hara/Quad (2)", //missionary
                                                                  "MAINSCENE/Otoko01/OTOKO_BASE/Hara/Inkei/Quad (1)", "MAINSCENE/Otoko01/OTOKO_BASE/Hara/Tama/Quad (2)", "MAINSCENE3/Otoko01/OTOKO_BASE/Hara/Quad (2)", "MAINSCENE3/Otoko01/OTOKO_BASE/Hara/Inkei/Quad (1)","MAINSCENE3/_OTOKO2_/BASE/Body/Inkei/Quad (3)", "MAINSCENE4/Otoko01/OTOKO_BASE/Hara/Quad (2)", "MAINSCENE4/Otoko01/OTOKO_BASE/Hara/Inkei/Quad (1)", //doggy
                                                                  "MAINSCENE1/Quad (1)", "MAINSCENE1/_OTOKO2_/BASE/Body/Inkei/Quad (3)", "MAINSCENE3/Quad", "MAINSCENE3/_OTOKO2_ (1)/BASE/Body/Inkei/Quad (3)", "MAINSCENE4/_OTOKO2_ (2)/BASE/Body/Inkei/Quad (3)", //counters
                                                                  "MAINSCENE1/Quad", "MAINSCENE1/Otoko01/OTOKO_BASE/Hara/Quad (2)", "MAINSCENE1/Otoko01/OTOKO_BASE/Hara/Inkei/Quad (1)", "MAINSCENE2/Quad", "MAINSCENE2/Otoko01/OTOKO_BASE/Hara/Quad (2)", "MAINSCENE2/Otoko01/OTOKO_BASE/Hara/Inkei/Quad (1)", //cowgirl
                                                                  "MAINSCENE4/Quad", //fainted
                                                                  "MAINSCENE4/Otoko01 (1)/OTOKO_BASE/Hara/Inkei/Quad (1)", "MAINSCENE2/Otoko01/OTOKO_BASE/Hara/Tama/Quad (2)", "MAINSCENE3/Otoko01/OTOKO_BASE/Hara/Tama/Quad (2)", "MAINSCENE4/Otoko01 (1)/OTOKO_BASE/Hara/Tama/Quad (2)", //throws
                                                                  "MAINSCENE2/_OTOKO2_ (1)/BASE/Body/Inkei/Quad (3)",//mounted blowjob
                                                                  "_MAOH_MAIN_/Quad", "_MAOH_MAIN_/Otoko01/OTOKO_BASE/Hara/Inkei/Quad (1)", "_MAOH_MAIN_/Otoko01/OTOKO_BASE/Hara/Quad (2)", //fight
                                                                  "DEMOSCENE/Quad (1)", "DEMOSCENE 1/Quad (1)", "MAINSCENE1/_OTOKO2_ (2)/BASE/Body/Inkei/Quad (3)", "MAINSCENE2/_OTOKO2_ (3)/BASE/Body/Inkei/Quad (3)", "MAINSCENE2/_OTOKO2_ (2)/BASE/Body/Inkei/Quad (3)", "DEMOSCENE/Otoko01 (1)/OTOKO_BASE/Hara/Quad (2)",   //endings
                                                                  "___MAIN___/Quad", "___MAIN___/Otoko01/OTOKO_BASE/Hara/Quad (2)","___MAIN___/Otoko02/OTOKO_BASE/Hara/Quad (2)"};


    }
}
