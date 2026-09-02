using System;
using UnityEngine;

namespace SmashFest.Levels
{

    [Serializable]
    public class LevelObjectData
    {
        public string id;
        public Vector3 position;
        public Vector3 rotation;
    }

  
    [Serializable]
    public class LevelData
    {
        public int levelNumber;
        public int ballCount;
        public string platformId;
        public LevelObjectData[] objects;
    }
}
