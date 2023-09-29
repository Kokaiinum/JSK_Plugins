using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace TextureReplacer {
    internal class BtnIkouHandler : IGameSpecificHandler {


        internal static List<Btn_Ikou> btnList = new List<Btn_Ikou>();
        internal static Dictionary<Btn_Ikou, Dictionary<string, Sprite>> originalButtonToSpriteMap = new();
        internal static Dictionary<Btn_Ikou, Dictionary<string, Sprite>> replacementButtonToSpriteMap = new();

        internal static string[] btnFieldNames = new string[] { "sprite_01", "sprite_02", "sprite_03", "buttonImage" };
       

        public void AddOrUpdateSprite(Texture2D tex) {
            foreach (var btn in btnList) {
                for (var i = 0; i < btnFieldNames.Length; i++) {
                    var cSprite = btn.GetSprite(i);
                    if (cSprite?.texture.UniqueName() == tex.UniqueName()) {
                        CreateNewSpriteAndSetRelationship(tex, btn, btnFieldNames[i], cSprite);
                    }
                }
            }
            RefreshSprites();
        }

        public void CreateNewSpriteAndSetRelationship(Texture2D tex, Btn_Ikou btn, string name, Sprite original) {
            if (!replacementButtonToSpriteMap.ContainsKey(btn)) replacementButtonToSpriteMap[btn] = new Dictionary<string, Sprite>();
            replacementButtonToSpriteMap[btn][name] = original.ReplaceTexture(tex);

        }

        public void DumpSprites() {
            foreach(Btn_Ikou btn in btnList) {
                for (var i = 0; i < btnFieldNames.Length; i++) {
                    var sprite = btn.GetSprite(i);
                    var name = sprite.texture.UniqueName();
                    if (TextureReplacerPlugin.dumpSprites && name != null && TextureReplacerPlugin.dumpedCache.Add(name)) TextureReplacerPlugin.DumpSprite(sprite.texture);
                }
            }
        }


        
        public bool RefreshSprites() {
            btnList.Clear();
            originalButtonToSpriteMap.Clear();
            replacementButtonToSpriteMap.Clear();
            btnList = Resources.FindObjectsOfTypeAll<Btn_Ikou>().ToList();
            if (btnList.Count <= 0) return false;
            foreach (var btn in btnList) {
                var newDic = new Dictionary<string, Sprite>();
                for (int i = 0; i < btnFieldNames.Length; i++) {
                    var cSprite = btn.GetSprite(i);
                    newDic[btnFieldNames[i]] = cSprite;
                    var name = cSprite?.texture.UniqueName();
                    if (TextureReplacerPlugin.dumpSprites && TextureReplacerPlugin.dumpedCache.Add(name)) TextureReplacerPlugin.DumpSprite(cSprite.texture);
                    if (TextureReplacerPlugin.currentReplacement.TryGetValueSafe(name, out Texture2D tex)) {
                        CreateNewSpriteAndSetRelationship(tex, btn, btnFieldNames[i], cSprite);
                    }
                }
                originalButtonToSpriteMap[btn] = newDic;
            }
            return true;
        }

        public void RemoveSprite(Texture2D tex) {
            foreach (var btn in replacementButtonToSpriteMap.Keys) {
                string nameToRemove = null;
                foreach (var name in replacementButtonToSpriteMap[btn].Keys) {
                    if (replacementButtonToSpriteMap[btn]?[name]?.texture.UniqueName() == tex.UniqueName()) {
                        var originalSprite = originalButtonToSpriteMap[btn][name];
                        btn.SetSprite(Array.IndexOf(btnFieldNames, name), originalSprite);
                        nameToRemove = name;
                    }
                    if (nameToRemove != null) replacementButtonToSpriteMap[btn].Remove(nameToRemove);
                }                
            }           
        }

        public void SwapStates(bool enabled) {
            if (btnList == null || btnList.Count == 0) return;
            if (replacementButtonToSpriteMap == null || replacementButtonToSpriteMap.Count == 0) return;


            foreach (Btn_Ikou btn in replacementButtonToSpriteMap.Keys) {
                for (int i = 0; i < btnFieldNames.Length; i++) {
                    var dic = enabled ? replacementButtonToSpriteMap : originalButtonToSpriteMap;
                    var fieldName = btnFieldNames[i];
                    if (dic[btn].TryGetValue(fieldName, out Sprite sprite)) {
                        btn.SetSprite(i, sprite);
                    }
                }
            }
        }
    }

    static class PapaGALExtensions {

        public static Sprite GetSprite(this Btn_Ikou btn, int index) {
            if (index < 3) {
                return (Sprite)AccessTools.Field(btn.GetType(), BtnIkouHandler.btnFieldNames[index])?.GetValue(btn);
            } else {
                return btn.GetImage()?.sprite;
            }
        }

        public static Image GetImage(this Btn_Ikou btn) {
            return ((Image)AccessTools.Field(btn.GetType(), "buttonImage")?.GetValue(btn));
        }


        public static void SetSprite(this Btn_Ikou btn, int index, Sprite sprite) {
            if (index < 3) {
                AccessTools.Field(btn.GetType(), BtnIkouHandler.btnFieldNames[index]).SetValue(btn, sprite);
            } else {
                btn.GetImage().sprite = sprite;
            }
        }
   
    }
}
