using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace Endless2DTerrain
{  

    public class VertexGenerator
    {

        public Settings settings { get; set; }

        public VertexGenerator(Settings s)
        {
            TerrainRules = s.Rules;
            settings = s;
            if (TerrainRules == null || TerrainRules.Count == 0)
            {
                throw new System.Exception("Terrain rules required to generate vertices");
            }

            //Move to the first rule
            CurrentTerrainRule = TerrainRules[0];

            //Initialize some other values
            RuleStartLocation = 0;
            CurrentLocation = 0;
            TotalDistanceTraveled = 0;

            LoopRules = true;
        }

        //Our terrain generation rules
        public TerrainRule CurrentTerrainRule { get; set; }
        public List<TerrainRule> TerrainRules { get; set; }

        //If this is true, when we reach the end of the last rule in the list start over with the first
        public bool LoopRules { get; set; }

        //Track where we currently are, how far we have traveled overall, and how far since the beginning of this rule so we know when to 
        //break to the next rule
        public float CurrentLocation { get; set; }
        public float RuleStartLocation { get; set; }

        public float TotalDistanceTraveled { get; set; }        
        public float DistanceTraveledSinceRuleStart
        {
            get
            {
                return TotalDistanceTraveled - RuleStartLocation;
            }
        }

        //For repeated generation...need to know if we want to start the next curve at the bottom or the top
        private bool RepeatingPointsAtTop { get; set; }


        //Determine if we should move to the next rule or not
        public bool MoveToNextTerrainRule()
        {
            if (CurrentTerrainRule == null)
            {
                return false;
            }

            if (CurrentTerrainRule.SelectedTerrainLength == TerrainRule.TerrainLength.Fixed)
            {
                if (DistanceTraveledSinceRuleStart > CurrentTerrainRule.RuleLength)
                {
                    return true;
                }
            }
            return false;
        }

        //Need to track this here, because we may have moved on to the next rule (in key vert generation) before this is used (in calc vert generation)
        private float CalculatedVertexSpacing { get; set; }

        //Move directly to a given rule
        public void MoveToRuleAtIndex(int index)
        {
            //Make sure we have a valid index   
            if (index > (TerrainRules.Count -1) || index < 0){
                return;
            }
            CurrentTerrainRule = TerrainRules[index];
            RuleStartLocation = CurrentLocation;
            RepeatingPointsAtTop = false;
        }


        //Decide which terrain rule to apply
        public void UpdateTerrainRule()
        {
            if (MoveToNextTerrainRule())
            {
                //If we reach the end but we are looping rules, go back to the first one
                TerrainRule lastTerrainRule = TerrainRules[TerrainRules.Count - 1];
                if (CurrentTerrainRule == lastTerrainRule)
                {
                    if (LoopRules)
                    {
                        CurrentTerrainRule = TerrainRules[0];

                        //Track where our rule is starting
                        RuleStartLocation = CurrentLocation;
                    }
                    else
                    {
                        CurrentTerrainRule = null;
                    }
                }
                else
                {
                    CurrentTerrainRule = TerrainRules[GetCurrentTerrainRuleIndex() + 1];

                    //Track where our rule is starting
                    RuleStartLocation = CurrentLocation;
                }

                //Reset this
                RepeatingPointsAtTop = false;
            }

        }


        public int GetCurrentTerrainRuleIndex()
        {
            if (CurrentTerrainRule != null)
            {
                for (int i = 0; i < TerrainRules.Count; i++)
                {
                    if (TerrainRules[i] == CurrentTerrainRule)
                    {
                        return i;
                    }
                }
            }

            //Can't find it or the current terrain rule is null?
            return -1;     
        }

        public List<Vector3> GenerateKeyVerticies(bool updateRepeatingPointLocation)
        {
            List<Vector3> verticies = new List<Vector3>();


            //Make sure we are on the correct rule
            UpdateTerrainRule();

            if (CurrentTerrainRule == null) { return null; }

            //Determine where we will start and end vertex generation
            float startLocation = CurrentLocation;
            float endLocation = CurrentTerrainRule.MeshLength + startLocation;


            //Loop until we've generated enough vertices to reach the desired mesh length (or we hit a new rule)
            while (CurrentLocation < endLocation && !MoveToNextTerrainRule())
            {

                float keyVertexStepSize = GetKeyVertexStepSize();

                //Random terrain generation
                if (CurrentTerrainRule.SelectedTerrainStyle == TerrainRule.TerrainStyle.Random)
                {
                    float y = GetKeyVertexRandomHeight();
                    float x = CurrentLocation + keyVertexStepSize;
                    verticies.Add(new Vector3(x, y, settings.OriginalStartPoint.z));
                }

                //Repeated terrain generation
                if (CurrentTerrainRule.SelectedTerrainStyle == TerrainRule.TerrainStyle.Repeated && updateRepeatingPointLocation)
                {

                

                    float yMin = CurrentTerrainRule.MinimumKeyVertexHeight;
                    float yMax = CurrentTerrainRule.MaximumKeyVertexHeight;
                    float x = CurrentLocation + keyVertexStepSize;

                    //Determine if we want to start the curve at the top or the bottom
                    if (RepeatingPointsAtTop)
                    {
                        verticies.Add(new Vector3(x, yMax, settings.OriginalStartPoint.z));
                    }
                    else
                    {
                        verticies.Add(new Vector3(x, yMin, settings.OriginalStartPoint.z));
                    }

                    //Flip after every iteration
                    RepeatingPointsAtTop = !RepeatingPointsAtTop;

                  
                }

                //Update our current location and our total distance generated
                CurrentLocation += keyVertexStepSize;
                TotalDistanceTraveled += keyVertexStepSize;


                //Don't toggle our points begin location if we are ending the mesh after this iteration.  If we ended at the stop we want to start at the top, and vice versa
                if (!(CurrentLocation < endLocation) && 
                    CurrentTerrainRule.SelectedTerrainStyle == TerrainRule.TerrainStyle.Repeated &&                   
                    updateRepeatingPointLocation)
                {
                  
                    RepeatingPointsAtTop = !RepeatingPointsAtTop;
                }
            }
            
            //Store before switching to the next rule so calculated verts will use the proper spacing for this current rule
            CalculatedVertexSpacing = CurrentTerrainRule.CalculatedVertexSpacing;

            //Update again after we finish generating vertices, so if we are on a new rule we'll switch to the correct
            //angle immediately on the next cycle
            UpdateTerrainRule();
        

            return verticies;
        }



        public List<Vector3> GenerateCalculatedVertices(List<Vector3> keyVertices)
        {

            List<Vector3> allVerticies = new List<Vector3>();
            TransformHelpers th = new TransformHelpers();

            //Start by inserting after the first key point
            int insertIndex = 1;

            for (int i = 0; i < keyVertices.Count - 1; i++)
            {  

                //Add the key vert
                allVerticies.Add(keyVertices[i]);

                Vector3 currentVertex = keyVertices[i];
                Vector3 nextVertex = keyVertices[i + 1];

                float x0 = currentVertex.x;
                float x1 = nextVertex.x;

                float y0 = currentVertex.y;
                float y1 = nextVertex.y;
               

                //How many segments between our two key points
                int totalSegments = Mathf.CeilToInt(Mathf.Ceil(x1 - x0) / CalculatedVertexSpacing);

                //The width of each of these segments		
                float segmentWidth = (x1 - x0) / totalSegments;


                for (int j = 1; j < totalSegments; j++)
                {
                    float newX = x0 + j * segmentWidth;

                    //Calculate our new y value by cosine interpolation
                    float mu = (float)j / (float)totalSegments;
                    float newY = th.CosineInterpolate(y0, y1, mu);

                    Vector3 newVert = new Vector3(newX, newY, settings.OriginalStartPoint.z);
                    allVerticies.Insert(insertIndex, newVert);

                    //Move to the next calculated point
                    insertIndex += 1;
                }

                //Jump over the key point and move on to the next calculated point
                insertIndex += 1;

                if (i == keyVertices.Count - 2)
                {
                    allVerticies.Add(nextVertex);
                }
            }

     

            return allVerticies;
        }

        //Get a point x distance below the lowest y vertex in the set of verticies
        public List<Vector3> GenerateFlatLowerBoundVerticies(List<Vector3> topVerticies, float distanceBelowLowestYVert)
        {
            float minY = topVerticies.Select(t => t.y).Min();
            float bottomY = minY - distanceBelowLowestYVert;

            List<Vector3> lowerVerticies = new List<Vector3>();
            for (int i = 0; i < topVerticies.Count; i++)
            {
                Vector3 topVertex = topVerticies[i];
                lowerVerticies.Add(new Vector3(topVertex.x, bottomY, topVertex.z));
            }
            return lowerVerticies;
        }

        //Add the verts in a consistent way so I know the triangle generation will be the same
        public List<Vector3> GetPlaneVerticies(List<Vector3> lowerVerticies, List<Vector3> topVerticies)
        {
            List<Vector3> planeVerticies = new List<Vector3>();
            for (int i = 0; i < topVerticies.Count; i++)
            {
                planeVerticies.Add(lowerVerticies[i]);
                planeVerticies.Add(topVerticies[i]);
            }
            return planeVerticies;
        }



        public float GetKeyVertexRandomHeight()
        {
            return Random.Range(CurrentTerrainRule.MinimumKeyVertexHeight, CurrentTerrainRule.MaximumKeyVertexHeight);
        }

        public float GetKeyVertexStepSize()
        {
            float stepSize = 1;
            //Return either a fixed or random spacing step size
            if (CurrentTerrainRule.MinimumKeyVertexSpacing == CurrentTerrainRule.MaximumKeyVertexSpacing)
            {
                stepSize = CurrentTerrainRule.MinimumKeyVertexSpacing;
            }
            else
            {
                stepSize = Random.Range(CurrentTerrainRule.MinimumKeyVertexSpacing, CurrentTerrainRule.MaximumKeyVertexSpacing);
            }
            return stepSize;
        }

    }
}
