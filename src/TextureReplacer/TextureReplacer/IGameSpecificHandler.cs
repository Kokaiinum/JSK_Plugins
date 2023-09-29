using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TextureReplacer {
    internal interface IGameSpecificHandler { 
        void SwapStates(bool enabled);

        bool RefreshSprites();

        void DumpSprites();


        void RemoveSprite(Texture2D tex);

        void AddOrUpdateSprite(Texture2D tex);        
    }
}
