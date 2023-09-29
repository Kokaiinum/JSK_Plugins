using Shared;
using System;
using BepInEx;
using System.IO;
using HarmonyLib;
using UnityEngine;
using MonoMod.Cil;
using System.Linq;
using UnityEngine.UI;
using BepInEx.Logging;
using BepInEx.Configuration;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Collections;
using static System.Net.Mime.MediaTypeNames;
using Image = UnityEngine.UI.Image;
using System.Collections.Concurrent;
using System.Reflection;
using System.CodeDom;

namespace TextureReplacer {
    [BepInPlugin("TextureReplacer", "JSK Texture Replacer", "1.0.0.0")]
    public class TextureReplacerPlugin : BaseUnityPlugin {

        internal static ManualLogSource logger;

        internal static ConfigEntry<bool> PluginEnabled;
        internal ConfigEntry<KeyboardShortcut> PluginEnabledHotkey;

        internal ConfigEntry<KeyboardShortcut> ReloadHotkey;

        internal ConfigEntry<string> CurrentDir;

        internal ConfigEntry<bool> WatchForFileChanges;

        internal ConfigEntry<bool> EnableDumping;
        internal ConfigEntry<KeyboardShortcut> DumpHotkey;

        internal static Dictionary<string, Texture2D> currentReplacement;
        internal Dictionary<string, Dictionary<string, Texture2D>> replacements;

        internal Dictionary<string, string> fileToTextureMap;

        internal readonly string texDir = Paths.PluginPath + $"\\ReplacementTextures";
        internal static readonly string dumpDir = Paths.PluginPath + $"\\DumpedTextures";

        internal static List<SpriteRenderer> currentSpriteRenderers;
        internal static Dictionary<SpriteRenderer, Sprite> originalRendererSprites;
        internal static Dictionary<SpriteRenderer, Sprite> replacementRendererSprites;

        internal static List<Image> currentImages;
        internal static Dictionary<Image, Sprite> originalImageSprites;
        internal static Dictionary<Image, Sprite> replacementImageSprites;

        internal FileSystemWatcher fileSystemWatcher;

        internal List<string> updatedTextures;
        internal List<string> deletedTextures;

        internal static HashSet<string> dumpedCache;

        internal static bool dumpSprites = false;

        internal readonly string[] supportedFormats = { ".png", ".dds" };

        internal IGameSpecificHandler gameSpecificHandler;
        internal bool callGSH = false;

        void Awake() {
            logger = Logger;

            gameSpecificHandler = GameSpecificHandlerGenerator.GetHandler(UnityEngine.Application.productName);

            PluginEnabled = Config.Bind("General", "Enabled", true, new ConfigDescription("Enables the plugin.", null, new ConfigurationManagerAttributes { Order = 9 }));
            PluginEnabled.SettingChanged += SpriteRendererAndImageEventHandler;

            PluginEnabledHotkey = Config.Bind("General", "Enabled Toggle Hotkey", new KeyboardShortcut(KeyCode.E), new ConfigDescription("Hotkey for enabling or disabling the entire plugin.", null, new ConfigurationManagerAttributes { Order = 8 }));

            ReloadHotkey = Config.Bind("General", "Reload Hotkey", new KeyboardShortcut(KeyCode.R), new ConfigDescription("Hotkey for manually reloading ALL texture folders. Will probably lag your game a little.", null, new ConfigurationManagerAttributes { Order = 9 }));

            WatchForFileChanges = Config.Bind("Advanced", "Watch For File Changes", true, new ConfigDescription("If the plugin should watch for file changes and update them live.", null, new ConfigurationManagerAttributes { IsAdvanced = true, Order = 12 }));
            WatchForFileChanges.SettingChanged += OnFSWSettingChanged;

            EnableDumping = Config.Bind("Advanced", "Enable Dumping", true, new ConfigDescription("Enable dumping of textures.\nPLEASE NOTE: Runtime texture dumping may not dump textures that are 100% accurate to the ones stored in the assets!!!\nIf you have any doubt that the textures are of sufficient quality please extract the assets with a tool like AssetStudio instead!", null, new ConfigurationManagerAttributes { IsAdvanced = true, Order = 10 }));
            EnableDumping.SettingChanged += (object o, EventArgs e) => {
                if (!(e is SettingChangedEventArgs sC)) return;
                if (!(bool)sC.ChangedSetting.BoxedValue) dumpSprites = false;
            };

            DumpHotkey = Config.Bind("Advanced", "Dump Hotkey", new KeyboardShortcut(KeyCode.D), new ConfigDescription("Hotkey to dump textures. Will continuously dump newly loaded textures until toggled off again. Only does anything if dumping is enabled.", null, new ConfigurationManagerAttributes { IsAdvanced = true, Order = 11 }));

            updatedTextures = new List<string>();
            deletedTextures = new List<string>();

            fileToTextureMap = new Dictionary<string, string>();

            currentSpriteRenderers = new List<SpriteRenderer>();
            originalRendererSprites = new Dictionary<SpriteRenderer, Sprite>();
            replacementRendererSprites = new Dictionary<SpriteRenderer, Sprite>();

            currentImages = new List<Image>();
            originalImageSprites = new Dictionary<Image, Sprite>();
            replacementImageSprites = new Dictionary<Image, Sprite>();

            replacements = new Dictionary<string, Dictionary<string, Texture2D>>();

            if (!Directory.Exists(texDir)) Directory.CreateDirectory(texDir);

            ParseAllReplacements();
            SceneManager.sceneLoaded += SpriteRendererSceneHandler;

            //Harmony.CreateAndPatchAll(typeof(Hooks));
            //if ()
            Hooks.InstallHooks();
        }

        void Update() {
            if (PluginEnabledHotkey.Value.IsDown()) PluginEnabled.Value = !PluginEnabled.Value;

            if (!PluginEnabled.Value) return;

            if (EnableDumping.Value == true && DumpHotkey.Value.IsDown()) {
                dumpSprites = !dumpSprites;
                var d = dumpSprites ? "enabled" : "disabled";
                logger.LogMessage($"Dumping {d}");
                if (dumpedCache == null) {
                    dumpedCache = new HashSet<string>();
                    if (!Directory.Exists(dumpDir)) Directory.CreateDirectory(dumpDir);
                    var prevCache = Directory.GetFiles(dumpDir);
                    foreach (string file in prevCache) {
                        dumpedCache.Add(Path.GetFileNameWithoutExtension(file));
                    }
                    DumpRendererSprites();
                    if (gameSpecificHandler != null) gameSpecificHandler.DumpSprites();
                }
            }

            if (Input.GetKeyDown(KeyCode.T)) {
                var sprites = Resources.FindObjectsOfTypeAll<Sprite>().Where(x => x != null && x.texture != null && x.texture.height > 0 && x.texture.width > 0).ToArray();
                logger.LogMessage($"currenntly {sprites.Length} sprites");
                foreach (Sprite sprite in sprites) {
                    DumpSprite(sprite.texture);
                }

            }

            if (ReloadHotkey.Value.IsDown()) {
                deletedTextures.Clear();
                updatedTextures.Clear();
                ParseAllReplacements();
                RefreshRendererSpritesAndImages();
            }

            if (deletedTextures.Count > 0) {
                foreach (string deleted in deletedTextures) {
                    if (fileToTextureMap.TryGetValue(deleted, out string texture)) {
                        RemoveSprite(currentReplacement[texture]);
                        if (callGSH) gameSpecificHandler.RemoveSprite(currentReplacement[texture]);
                        currentReplacement.Remove(texture);
                        fileToTextureMap.Remove(deleted);
                    }
                }
                deletedTextures.Clear();
            }

            if (updatedTextures.Count > 0) {
                foreach (var texture in updatedTextures) {
                    if (!File.Exists(texture)) continue;
                    var tex = ParseReplacement(currentReplacement, texture);
                    var wasFound = AddOrUpdateSprite(tex);
                    if ((!wasFound) && callGSH) gameSpecificHandler.AddOrUpdateSprite(tex);
                }
                updatedTextures.Clear();
            }
        }

        void ParseAllReplacements() {
            currentReplacement = null;
            replacements.Clear();
            var dirs = Directory.GetDirectories(texDir).Where(x => (Directory.GetFiles(x, "*.png").Length > 0)).Select(y => Path.GetFileName(y)).ToArray();
            if (dirs.Length == 0) {
                logger.LogMessage("No replacement texture folders found!");
                return;
            }
            foreach (var dir in dirs) {
                var dic = new Dictionary<string, Texture2D>();
                ParseReplacements(dic, dir);
                replacements.Add(dir, dic);
            }

            var cuDiDes = new ConfigDescription("Folder to pull replacement textures from.", new AcceptableValueList<string>(dirs));
            CurrentDir = Config.Bind("General", "Current Directory", string.Empty, cuDiDes);
            CurrentDir.SettingChanged += OnDirChange;

            OnDirChange(this, EventArgs.Empty);
        }

        void ParseReplacements(Dictionary<string, Texture2D> dic, string dir) {
            dic.Clear();
            var files = Directory.GetFiles(Path.Combine(texDir, dir), "*.png");
            foreach (var file in files) {
                _ = ParseReplacement(dic, file);
            }
        }

        Texture2D ParseReplacement(Dictionary<string, Texture2D> dic, string file) {
            //var key = Path.GetFileNameWithoutExtension(file);
            var data = File.ReadAllBytes(file);
            var tex = new Texture2D(2, 2, TextureFormat.DXT1, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.name = Path.GetFileNameWithoutExtension(file).Split('#')[0];
            tex.anisoLevel = 1;
            tex.mipMapBias = 0;
            tex.LoadImage(data);
            var key = tex.UniqueName();
            if (dic.TryGetValue(key, out var oldTex)) Destroy(oldTex);
            dic[key] = tex;
            fileToTextureMap[file] = key;
            return tex;
        }

        void OnDirChange(object sender, EventArgs e) {
            currentReplacement = replacements[CurrentDir.Value];
            SwapRendererSpritesAndImages(false);
            RefreshRendererSpritesAndImages();
            if (PluginEnabled.Value) SwapRendererSpritesAndImages(true);
            if (WatchForFileChanges.Value) InitFSW();

        }

        void InitFSW() {
            var tarPath = Path.Combine(texDir, CurrentDir.Value);
            if (fileSystemWatcher != null) {
                fileSystemWatcher.Path = tarPath;
                return;
            }
            fileSystemWatcher = new FileSystemWatcher(tarPath);
            fileSystemWatcher.EnableRaisingEvents = true;
            fileSystemWatcher.Changed += OnFileChangedOrCreated;
            fileSystemWatcher.Created += OnFileChangedOrCreated;
            fileSystemWatcher.Deleted += OnFileDeleted;
            fileSystemWatcher.Renamed += OnFileRenamed;
        }

        void OnFSWSettingChanged(object sender, EventArgs e) {
            if (e is SettingChangedEventArgs sE) {
                if ((bool)sE.ChangedSetting.BoxedValue == true) {
                    InitFSW();
                } else {
                    fileSystemWatcher.Dispose();
                    fileSystemWatcher = null;
                }
            }
        }

        void OnFileChangedOrCreated(object sender, FileSystemEventArgs e) {
            if (supportedFormats.Contains(Path.GetExtension(e.FullPath))) updatedTextures.Add(e.FullPath);
        }

        void OnFileDeleted(object sender, FileSystemEventArgs e) {
            if (supportedFormats.Contains(Path.GetExtension(e.FullPath))) deletedTextures.Add(e.FullPath);
        }

        void OnFileRenamed(object sender, FileSystemEventArgs e) {
            if (e is RenamedEventArgs rEvArgs && supportedFormats.Contains(Path.GetExtension(rEvArgs.FullPath))) {
                deletedTextures.Add(rEvArgs.OldFullPath);
                updatedTextures.Add(rEvArgs.FullPath);
            }
        }

        static internal void DumpSprite(Texture2D t) {
            if (!Directory.Exists(dumpDir)) Directory.CreateDirectory(dumpDir);
            RenderTexture temp;
            temp = RenderTexture.GetTemporary(t.width, t.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.sRGB);
            //maybe GL.Clear(true, true, Color.clear) here?
            Graphics.Blit(t, temp);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = temp;
            Texture2D texture = new Texture2D(t.width, t.height);
            texture.ReadPixels(new Rect(0, 0, temp.width, temp.height), 0, 0);
            texture.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(temp);

            //texture.GetRawTextureData();  ??

            File.WriteAllBytes(Path.Combine(dumpDir, (t.UniqueName() + ".png")), texture.EncodeToPNG());
            //File.WriteAllBytes(Path.Combine(dumpDir, (t.UniqueName() + ".dat")), t.GetRawTextureData());
            Destroy(texture);

        }

        internal void SpriteRendererAndImageEventHandler(object o, EventArgs e) {
            SwapRendererSpritesAndImages(PluginEnabled.Value);
            if (callGSH) gameSpecificHandler.SwapStates(PluginEnabled.Value);
        }

        internal void SpriteRendererSceneHandler(Scene scene, LoadSceneMode mode) {           
            currentSpriteRenderers = Resources.FindObjectsOfTypeAll<SpriteRenderer>().ToList();
            currentImages = Resources.FindObjectsOfTypeAll<CanvasRenderer>().Select(x => x.gameObject).Where(x => x.GetComponent<Image> != null).Select(x => x.GetComponent<Image>()).ToList();
            RefreshRendererSpritesAndImages();
            if (gameSpecificHandler != null) callGSH = gameSpecificHandler.RefreshSprites();
            if (PluginEnabled.Value) {
                SwapRendererSpritesAndImages(PluginEnabled.Value);
                if (callGSH) gameSpecificHandler.SwapStates(PluginEnabled.Value);
            }

            ;
            
            //var image = new Image();
        }
        //TODO: consolidate directory into <Behaviour, Texture> instead
        //might require special casing per behaviour type by still probably less cruft?
        internal void RefreshRendererSpritesAndImages() {
            originalRendererSprites.Clear();
            originalImageSprites.Clear();
            replacementRendererSprites.Clear();
            replacementImageSprites.Clear();
            foreach (SpriteRenderer spriteRenderer in currentSpriteRenderers) {
                if (spriteRenderer?.sprite?.texture != null) {
                    originalRendererSprites[spriteRenderer] = spriteRenderer.sprite;
                    var name = spriteRenderer.sprite.texture.UniqueName();
                    if (dumpSprites && dumpedCache.Add(name)) DumpSprite(spriteRenderer.sprite.texture);
                    if (currentReplacement.TryGetValueSafe(name, out Texture2D tex)) {
                        CreateNewSpriteAndSetRelationship(spriteRenderer, tex);
                    }
                }
            }
            foreach (Image image in currentImages) {
                if (image?.sprite != null) {
                    originalImageSprites[image] = image.sprite;
                    var name = image.sprite.texture.UniqueName();
                    if (dumpSprites && dumpedCache.Add(name)) DumpSprite(image.sprite.texture);
                    if (currentReplacement.TryGetValueSafe(name, out Texture2D tex)) {
                        CreateNewSpriteAndSetRelationship(image, tex);
                    }
                }
            }
        }

    

        internal void DumpRendererSprites() {
            foreach (Sprite sprite in originalRendererSprites.Values) {
                var name = sprite?.texture?.UniqueName();
                if (dumpSprites && name != null && dumpedCache.Add(name)) DumpSprite(sprite.texture);
            }
            foreach (Sprite sprite in originalImageSprites.Values) {
                var name = sprite?.texture?.UniqueName();
                if (dumpSprites && name != null && dumpedCache.Add(name)) DumpSprite(sprite.texture);
            }
        }

        internal void CreateNewSpriteAndSetRelationship(SpriteRenderer sR, Texture2D tex) {
            replacementRendererSprites[sR] = sR.sprite.ReplaceTexture(tex);
        }

        internal void CreateNewSpriteAndSetRelationship(Image i, Texture2D tex) {
            replacementImageSprites[i] = i.sprite.ReplaceTexture(tex);
        }

        internal static void SwapRendererSpritesAndImages(bool replace) {
            if (!(replacementRendererSprites == null || replacementRendererSprites.Values.Count == 0)) {
                var dic = replace ? replacementRendererSprites : originalRendererSprites;
                foreach (SpriteRenderer sr in replacementRendererSprites.Keys) sr.sprite = dic[sr];
            }
            if (!(replacementImageSprites == null || replacementImageSprites.Values.Count == 0)) {
                var dic = replace ? replacementImageSprites : originalImageSprites;
                foreach (Image i in replacementImageSprites.Keys) i.sprite = dic[i];
            }
        }

        internal void RemoveSprite(Texture2D tex) {
            SpriteRenderer srToRemove = null;
            Image imageToRemove = null;
            foreach (var sr in replacementRendererSprites.Keys) {
                if (replacementRendererSprites[sr].texture.UniqueName() == tex.UniqueName()) {
                    sr.sprite = originalRendererSprites[sr];
                    srToRemove = sr;
                }
            }
            if (srToRemove != null) {
                replacementRendererSprites.Remove(srToRemove);
                return;
            }
            foreach (var i in replacementImageSprites.Keys) {
                if (replacementImageSprites[i].texture.UniqueName() == tex.UniqueName()) {
                    i.sprite = originalImageSprites[i];
                    imageToRemove = i;
                }
            }
            if (imageToRemove != null) replacementImageSprites.Remove(imageToRemove);

        }

        internal bool AddOrUpdateSprite(Texture2D tex) {
            bool found = false;
            foreach (var sr in currentSpriteRenderers) {
                if (sr?.sprite?.texture?.UniqueName() == tex.UniqueName()) {
                    CreateNewSpriteAndSetRelationship(sr, tex);
                    found = true;
                }
            }
            foreach (var i in currentImages) {
                if (i?.sprite?.texture?.UniqueName() == tex.UniqueName()) {
                    CreateNewSpriteAndSetRelationship(i, tex);
                    found = true;
                }
            }
            SwapRendererSpritesAndImages(PluginEnabled.Value);
            return found;
        }
    }

    internal static class Util {

        //http://szudzik.com/ElegantPairing.pdf
        public static int ElegantPair(int x, int y) {
            if (y > x) return (y * y) + x;
            return (x * x) + x + y;
        }

        public static string UniqueName(this Texture2D texture) {
            return texture.name + '#' + ElegantPair(texture.width, texture.height);
        }

        public static bool TryGetValueSafe<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey key, out TValue value) {
            if (dic == null || key == null) {
                value = default;
                return false;
            }
            return dic.TryGetValue(key, out value);
        }

        public static Sprite ReplaceTexture(this Sprite sprite, Texture2D texture) {
            var tv = new Vector2((sprite.pivot.x / sprite.rect.width), sprite.pivot.y / (sprite.rect.height)); ; //Why is this required? Why does sprite.pivot return the RESULT of the transformation instead of the parameters? why did they make it like this?
            var temp = Sprite.Create(texture, sprite.rect, tv);
            temp.name = sprite.name;
            return temp;
        }

        //public static Sprite GetImageSprite(this Image image) {
        //    return (Sprite) AccessTools.Field(typeof(Image), "m_Sprite").GetValue(image);
        //}

        //public static void SetImageSprite(this Image image, Sprite sprite) {
        //    AccessTools.Field(typeof(Image), "m_Sprite").SetValue(image, sprite);
        //}
    }

    internal static class GameSpecificHandlerGenerator {
        internal static IGameSpecificHandler GetHandler(string productName) {
            switch (productName) {
                case "PapaGAL":
                case "WakaraseMaohsama":
                        return new BtnIkouHandler();
                default: return null;
            }

        }

    }
}
