using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SereCore;
using UnityEngine;

namespace RandomizerMod
{
    public static class Sprites
    {
        private static Dictionary<string, Sprite> _sprites;

        public static void Load()
        {
            _sprites = ResourceHelper.GetSprites("RandomizerMod.Resources.");
        }

        public static Sprite GetSprite(string name)
        {
            if (_sprites != null && _sprites.TryGetValue(name, out Sprite sprite))
            {
                return sprite;
            }
            return null;
        }
    }
}
