using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CocosNG.Core;
using CocosNG.Core.Primitives;
using CocosNG.Core.Content;

namespace CocosNG.Core.Nodes.Sprites
{
    public class CCSpriteSheetCache
    {
        private static CCSpriteSheetCache _instance;

        private Dictionary<string, CCSpriteSheet> _spriteSheets = new Dictionary<string, CCSpriteSheet>(); 

        public static CCSpriteSheetCache Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new CCSpriteSheetCache();
                }
                return _instance;
            }
        }

        public static void DestroyInstance()
        {
            _instance = null;
        }


        public CCSpriteSheet AddSpriteSheet(string fileName)
        {
            CCSpriteSheet result;
            if (!_spriteSheets.TryGetValue(fileName, out result))
            {
                result = new CCSpriteSheet(fileName);
                _spriteSheets.Add(fileName, result);
            }
            return result;
        }

        public CCSpriteSheetCache AddSpriteSheetAnd(string fileName)
        {
            AddSpriteSheet(fileName);
            return this;
        }

        public CCSpriteSheet AddSpriteSheet(string fileName, string textureFileName)
        {
            CCSpriteSheet result;
            if (!_spriteSheets.TryGetValue(fileName, out result))
            {
                result = new CCSpriteSheet(fileName, textureFileName);
                _spriteSheets.Add(fileName, result);
            }
            return result;
        }

        public CCSpriteSheetCache AddSpriteSheetAnd(string fileName, string textureFileName)
        {
            AddSpriteSheet(fileName, textureFileName);
            return this;
        }

        public CCSpriteSheet AddSpriteSheet(string fileName, CCTexture2D texture)
        {
            CCSpriteSheet result;
            if (!_spriteSheets.TryGetValue(fileName, out result))
            {
                result = new CCSpriteSheet(fileName, texture);
                _spriteSheets.Add(fileName, result);
            }
            return result;
        }

        public CCSpriteSheetCache AddSpriteSheetAnd(string fileName, CCTexture2D texture)
        {
            AddSpriteSheet(fileName, texture);
            return this;
        }

        public CCSpriteSheet AddSpriteSheet(Stream stream, CCTexture2D texture, string name)
        {
            CCSpriteSheet result;
            if (!_spriteSheets.TryGetValue(name, out result))
            {
                result = new CCSpriteSheet(name, texture);
                _spriteSheets.Add(name, result);
            }
            return result;
        }

        public CCSpriteSheetCache AddSpriteSheetAnd(Stream stream, CCTexture2D texture, string name)
        {
            AddSpriteSheet(stream, texture, name);
            return this;
        }

        public CCSpriteSheet AddSpriteSheet(PlistDictionary dictionary, CCTexture2D texture, string name)
        {
            CCSpriteSheet result;
            if (!_spriteSheets.TryGetValue(name, out result))
            {
                result = new CCSpriteSheet(name, texture);
                _spriteSheets.Add(name, result);
            }
            return result;
        }

        public CCSpriteSheetCache AddSpriteSheetAnd(PlistDictionary dictionary, CCTexture2D texture, string name)
        {
            AddSpriteSheet(dictionary, texture, name);
            return this;
        }

        public CCSpriteSheet SpriteSheetForKey(string name)
        {
            CCSpriteSheet result = null;
            if (!_spriteSheets.TryGetValue(name, out result))
            {
                CCLog.Log("SpriteSheet of key {0} is not exist.", name);
            }
            return result;
        }

        public void RemoveAll()
        {
            _spriteSheets.Clear();
        }

        public CCSpriteSheetCache RemoveAllAnd()
        {
            RemoveAll();
            return this;
        }

        public void RemoveUnused()
        {
            if (_spriteSheets.Count > 0)
            {
                var tmp = new Dictionary<string, WeakReference>();

                foreach (var pair in _spriteSheets)
                {
                    tmp.Add(pair.Key, new WeakReference(pair.Value));
                }

                _spriteSheets.Clear();

                GC.Collect();

                foreach (var pair in tmp)
                {
                    if (pair.Value.IsAlive)
                    {
                        _spriteSheets.Add(pair.Key, (CCSpriteSheet)pair.Value.Target);
                    }
                }
            }
        }

        public CCSpriteSheetCache RemoveUnusedAnd()
        {
            RemoveUnused();
            return this;
        }

        public void Remove(CCSpriteSheet spriteSheet)
        {
            if (spriteSheet == null)
            {
                return;
            }

            string key = null;

            foreach (var pair in _spriteSheets)
            {
                if (pair.Value == spriteSheet)
                {
                    key = pair.Key;
                    break;
                }
            }

            if (key != null)
            {
                _spriteSheets.Remove(key);
            }
        }

        public CCSpriteSheetCache RemoveAnd(CCSpriteSheet spriteSheet)
        {
            Remove(spriteSheet);
            return this;
        }

        public void Remove(string name)
        {
            if (name == null)
            {
                return;
            }
            _spriteSheets.Remove(name);
        }

        public CCSpriteSheetCache RemoveAnd(string name)
        {
            Remove(name);
            return this;
        }

    }
}
