using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Endless2DTerrain
{
    [Serializable]
    public class TerrainRule
    {
   
        public enum TerrainStyle
        {
            Repeated = 0,
            Random = 1
        }

        public enum TerrainLength
        {
            Infinite = 0,        
            Fixed = 1      
        }

        //Determine what pattern we want to use to generate the terrain
        public TerrainStyle SelectedTerrainStyle;

        //Can only have one infinite rule.  Set beginning no end must be the last rule
        public TerrainLength SelectedTerrainLength;

        //Determine the top and bottom y values that will be used for terrain generation
        public float MinimumKeyVertexHeight;
        public float MaximumKeyVertexHeight;

        //The minimum and maximum values between key points in the terrain
        public float MinimumKeyVertexSpacing;
        public float MaximumKeyVertexSpacing;

        //How far apart do we space our calculated verticies?
        public float CalculatedVertexSpacing;
   
        //Only applies if this is a fixed rule type.  How long to run the rule
        public float RuleLength;

        //How long is the mesh (provided it is not broken by hitting a new rule)
        public float MeshLength;

        //Are the points generated along an angle (default is 0)
        public float Angle;

        //Prefabs that are allowed to generate on this rule
        public bool ExpandedRules;
        public List<AllowedPrefabRule> AllowedPrefabs;

        [Serializable]
        public class AllowedPrefabRule
        {
            public string Name;
            public int Index;
            public bool Allowed;
        }

    }
}
