using UnityEditor;
using UnityEngine;
using System.Collections;

public class EndlessMenu : MonoBehaviour {	
	[MenuItem ("Component/Endless 2D Terrain/Create Terrain")]
	static void CreateTerrain () {
        if (!GameObject.Find("Endless 2D Terrain"))
        {
			 var temp = Resources.Load("Prefabs/Endless 2D Terrain", typeof(GameObject));	
             GameObject prefab = GameObject.Instantiate(temp) as GameObject;
             prefab.name = "Endless 2D Terrain";
             var td = prefab.GetComponent<TerrainDisplayer>();
             td.Setup();
             td.GenerateTerrain(100);
		   
	         
        }  
	}


}
