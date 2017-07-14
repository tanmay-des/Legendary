using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Endless2DTerrain
{
    public class PrefabPool
    {

        public PrefabPool()
        {
            Prefabs = new List<PrefabQueue>();
        }


        public List<PrefabQueue> Prefabs { get; set; }

        public GameObject Add(GameObject prefabToClone, Vector3 position, float angle, string type, bool matchGroundAngle)
        {
     
            PrefabQueue prefabToAdd = Prefabs.Where(t=>t.PrefabType == type && t.IsSpawned == false).FirstOrDefault();
            
            if (prefabToAdd == null){
                //Let's create a new one
                prefabToAdd = new PrefabQueue();
                GameObject prefab = (GameObject)GameObject.Instantiate(prefabToClone, position, new Quaternion());
                prefab.name = prefabToClone.name;
                if (angle != 0 && matchGroundAngle){
                     prefab.transform.localEulerAngles = new Vector3(0, 0, angle);
                }
                prefabToAdd.PrefabType = type;
                prefabToAdd.Prefab = prefab;
                prefabToAdd.IsSpawned = true;
                prefabToAdd.Prefab.SetActive(true);
                Prefabs.Add(prefabToAdd);
            }else{ 
                //Just update one from the queue
                prefabToAdd.Prefab.transform.position = position;
                if (angle != 0 && matchGroundAngle){
                     prefabToAdd.Prefab.transform.localEulerAngles = new Vector3(0, 0, angle);
                }
                prefabToAdd.IsSpawned =true;
                prefabToAdd.Prefab.SetActive(true);
            }
            return prefabToAdd.Prefab;
        }

        public void Remove(GameObject prefab)
        {
            var prefabToRemove = Prefabs.Where(p => p.Prefab == prefab).FirstOrDefault();        
            prefabToRemove.IsSpawned = false;
            prefabToRemove.Prefab.SetActive(false);
      
     
        }

        public class PrefabQueue
        {
            public GameObject Prefab { get; set; }
            public string PrefabType { get; set; }
            public bool IsSpawned { get; set; }
        }
    }
}
