using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Endless2DTerrain
{
    public class PrefabManager
    {
       
        public TerrainManager terrainManager { get; set; }
        public PrefabPool Pool { get; set; }
        private TransformHelpers th { get; set; }
        private Settings settings { get; set; }
        public const string ManagerName = "Prefab Manager";

        private GameObject PrefabManagerObject { get; set; }

        public PrefabManager(Settings s)
        {
            th = new TransformHelpers();
            Pool = new PrefabPool();
            settings = s;
            InstantiatePrefabManagerObject();
        
        }

    
        public void PlacePrefabs(TerrainManager tm)
        {

            terrainManager = tm;
            InstantiatePrefabManagerObject();

            List<PrefabQueue> prefabsToAdd = new List<PrefabQueue>();
           
            
            for (int i = 0; i < terrainManager.AllFrontTopVerticies.Count(); i++)
            {
                Vector3 current = terrainManager.AllFrontTopVerticies[i];

                for (int j = 0; j < settings.PrefabRules.Count(); j++)
                {
                    PrefabRule rule = settings.PrefabRules[j];

                    //Can't do anything without a prefab
                    if (rule.PrefabToClone == null) { break; }

                    //If we haven't started yet, set our initial values
                    if (rule.LastPrefabLocation == Vector3.zero){
                       
                        rule.LastPrefabLocation = current;
                    }

                    rule.CurrentLocation = current;

                    //Save it because it is randomized and changes every time
                    float repeatDistance = rule.RepeatDistance;

                    if (rule.AddPrefab(repeatDistance))
                    {
                        

                        //Find the location of the first prefab
                        float nextXLocation = rule.NextPrefabXLocation(repeatDistance);
                        Vector3 nextLocation = FindLocationAlongTerrain(nextXLocation);
                        float angle = FindSlopeAngle(nextLocation.x);

                        //Store a list of the prefabs to add.  Only add them if every prefab in this ruleset can be added.
                        //If they can't, add them at the start of the next mesh
                        bool addAllPrefabs = true;
                        prefabsToAdd.Clear();
                        prefabsToAdd.Add(new PrefabQueue() { location = nextLocation, angle = angle });

                     
                        

                        if (rule.GroupSize > 1)
                        {
                            float increase = 0;
                            for (int k = 1; k < rule.GroupSize; k++)
                            {
                                //Find the location of the next prefab in this group
                                increase = increase + rule.GroupSpacing;
                                nextLocation = FindLocationAlongTerrain(nextXLocation + increase);

                                //We can't place all prefabs.  Break out
                                if (nextLocation == Vector3.zero)
                                {
                                    addAllPrefabs = false;
                                    break;
                                }
                                else
                                {
                                    //Store the location of these prefabs as well
                                    angle = FindSlopeAngle(nextXLocation + increase);
                                    prefabsToAdd.Add(new PrefabQueue() { location = nextLocation, angle = angle });
                                }

                            }
                        }

                        //Can we add all the prefabs?  Then go ahead and instatiate them
                        if (addAllPrefabs)
                        {
                            for (int k = 0; k < prefabsToAdd.Count(); k++)
                            {
                                PrefabQueue pq = prefabsToAdd[k];

                                //Determine if this prefab is allowed to be placed on this terrain rule
                                var currentRule = tm.VertexGen.CurrentTerrainRule;
                                bool allowedForThisTerrainRule = currentRule.AllowedPrefabs.Where(ap => ap.Allowed && ap.Index == j).Any();
                                bool meetsDistanceReqs = true;

                                //Determine if this prefab is within the distance rules
                                if (rule.UseMinDistance)
                                {
                                    if (pq.location.x < rule.MinDistance) { meetsDistanceReqs = false; }
                                }
                                if (rule.UseMaxDistance)
                                {
                                    if (pq.location.x > rule.MaxDistance) { meetsDistanceReqs = false; }
                                }


                                //Only add if it is within the slope limits
                                if (pq.angle >= rule.MinSlope && pq.angle <= rule.MaxSlope && allowedForThisTerrainRule && meetsDistanceReqs)
                                {
                                    rule.InstantiatePrefab(pq.location, PrefabManagerObject, Pool, pq.angle);
                                    rule.LastPrefabLocation = pq.location;
                                }
                                else
                                {
                                    //Just update this so we can keep placing prefabs, but don't actually create the prefab
                                    rule.LastPrefabLocation = pq.location;
                                }

                 
                            }
                        }
                    }
                }
          
            }

        }

        public Vector3 FindLocationAlongTerrain(float location)
        {
            Vector3 low = Vector3.zero;
            Vector3 high = Vector3.zero;

            

            //Find the verticies below and above the given location
            for (int i = 0; i < terrainManager.AllFrontTopVerticies.Count(); i++)
            {
                Vector3 current = terrainManager.AllFrontTopVerticies[i];

                //Exact match, return early
                if (current.x == location)
                {
                    return current;
                }

                if (current.x < location)
                {
                    low = current;
                }
                else
                {
                    high = current;
                    break;
                }
            }

            if (low == Vector3.zero || high == Vector3.zero)
            {
                return Vector3.zero;
            }

            Vector3 newLocation = th.GetPointAlongLine(low, high, (location - low.x));

            return newLocation;
        }

        public float FindSlopeAngle(float location)
        {

            Vector3 low = Vector3.zero;      
            Vector3 high = Vector3.zero;


            //Find the verticies below and above the given location
            for (int i = 0; i < terrainManager.AllFrontTopVerticies.Count(); i++)
            {
                Vector3 current = terrainManager.AllFrontTopVerticies[i];

                if (current.x < location)
                {
                    low = current;
                }
                else if (current.x >= location)
                {
                    high = current;
                    break;
                }
            }

        
            float rise = high.y - low.y;
            float run = high.x - low.x;
            float angle = Mathf.Atan2(rise, run) * 180 / Mathf.PI;
          
            return angle;
        }


        public void Cleanup(float beginX)
        {

            List<GameObject> prefabsToRemove = new List<GameObject>();
			
			if (Pool == null){return;}

            for (int i = 0; i < Pool.Prefabs.Count(); i++)
            {
                GameObject prefab = Pool.Prefabs[i].Prefab;           
                if (prefab.transform.position.x < beginX)
                {
                    prefabsToRemove.Add(prefab);
                }
            }

            for (int i = 0; i < prefabsToRemove.Count(); i++)
            {
                Pool.Remove(prefabsToRemove[i]);
            }
        }


        public void RemovePrefabObject()
        {
            var obj = GameObject.Find(ManagerName);
            if (obj != null){
                GameObject.DestroyImmediate(obj);
            }
            
        }


        private void InstantiatePrefabManagerObject()
        {
           
            //This is just a placeholder for all the mesh pieces
            if (!GameObject.Find(ManagerName))
            {
                PrefabManagerObject = new GameObject(ManagerName);
                PrefabManagerObject.transform.parent = settings.terrainDisplayer.transform;
            }
        }

        private struct PrefabQueue
        {
            public Vector3 location;
            public float angle;
        }

    }
}

