using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Endless2DTerrain
{
    public class TerrainPool
    {
        public List<TerrainPiece> TerrainPieces { get; set; }

        public TerrainPool()
        {
            TerrainPieces = new List<TerrainPiece>();
        }

        public void Add(TerrainPiece tp)
        {
            TerrainPieces.Add(tp);
        }

        public void Remove(TerrainPiece tp)
        {
            TerrainPieces.Remove(tp);
            GameObject.DestroyImmediate(tp.TerrainObject);
        }
    }
}
