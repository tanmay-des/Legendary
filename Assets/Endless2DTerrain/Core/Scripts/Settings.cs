using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Endless2DTerrain{
    public class Settings {
        public Material MainMaterial { get; set; }
        public float MainMaterialXTiling { get; set; }
        public float MainMaterialYTiling { get; set; }
        public float MainMaterialRotation { get; set; }

        public Material TopMaterial { get; set; }
        public float TopMaterialXTiling { get; set; }
        public float TopMaterialYTiling { get; set; }
        public float TopMaterialRotation { get; set; }
        public bool DrawTopMeshCollider { get; set; }
        public bool DrawTopMeshRenderer { get; set; }

        public Material DetailMaterial { get; set; }
        public float DetailMaterialXTiling { get; set; }
        public float DetailMaterialYTiling { get; set; }
        public float DetailMaterialRotation { get; set; }
        public bool DrawDetailMeshRenderer { get; set; }

        public float MainPlaneHeight { get; set; }
        public float TopPlaneHeight { get; set; }
        public float DetailPlaneHeight { get; set; }

        public bool MainPlaneFollowTerrainCurve { get; set; }
        public bool DetailPlaneFollowTerrainCurve { get; set; }

        public float CornerMeshWidth { get; set; }
        public Vector3 DetailPlaneOffset { get; set; }

        public List<TerrainRule> Rules { get; set; }
        public List<PrefabRule> PrefabRules { get; set; }
        public Vector3 OriginalStartPoint { get; set; }
        public string ParentGameObjectName { get; set; }

        public TerrainDisplayer terrainDisplayer { get; set; }
    }
}

