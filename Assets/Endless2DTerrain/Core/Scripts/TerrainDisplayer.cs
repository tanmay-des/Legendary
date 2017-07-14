using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Endless2DTerrain;

public class TerrainDisplayer : MonoBehaviour {

    //Length of preview in design view
    public float PreviewLength;

    //Our list of terrain generation and prefab generation rules
    public List<TerrainRule> Rules;
    public List<PrefabRule> PrefabRules;

    //The front plane material, tiling, etc.
	public Material MainMaterial;
    public float MainMaterialXTiling;
    public float MainMaterialYTiling;
    public float MainMaterialRotation;
    public float MainPlaneHeight;
    public bool MainPlaneFollowTerrainCurve;

    //Top plane settings
    public Material TopMaterial;
    public float TopMaterialXTiling;
    public float TopMaterialYTiling;
    public float TopMaterialRotation;
    public bool DrawTopMeshCollider;
    public bool DrawTopMeshRenderer;
    public float TopPlaneHeight;

    //Detail plane settings
    public Material DetailMaterial;
    public float DetailMaterialXTiling;
    public float DetailMaterialYTiling;
    public float DetailMaterialRotation;
    public bool DrawDetailMeshRenderer;
    public float DetailPlaneHeight;
    public bool DetailPlaneFollowTerrainCurve;
    public Vector3 DetailPlaneOffset;

    //Width for corner mesh where two planes with different angles meet
    public float CornerMeshWidth;
  
    private Vector3 OriginalStartPoint { get; set; }
    float currentX { get; set; }

    private bool coroutineRunning = false;

    //References to the terrain and prefab managers
    public TerrainManager TerrainManager { get; set; }
    public PrefabManager PrefabManager { get; set; }

    void Awake()
    {       
        Setup();
    }

    public void Setup()
    {
        if (Rules != null)
        {
            Settings s = new Settings();
            s.Rules = Rules;
            s.PrefabRules = PrefabRules;

            s.MainMaterial = MainMaterial;
            s.MainMaterialXTiling = MainMaterialXTiling;
            s.MainMaterialYTiling = MainMaterialYTiling;
            s.MainMaterialRotation = MainMaterialRotation;

            s.TopMaterial = TopMaterial;
            s.TopMaterialXTiling = TopMaterialXTiling;
            s.TopMaterialYTiling = TopMaterialYTiling;
            s.TopMaterialRotation = TopMaterialRotation;
            s.DrawTopMeshCollider = DrawTopMeshCollider;
            s.DrawTopMeshRenderer = DrawTopMeshRenderer;

            s.DetailMaterial = DetailMaterial;
            s.DetailMaterialXTiling = DetailMaterialXTiling;
            s.DetailMaterialYTiling = DetailMaterialYTiling;
            s.DetailMaterialRotation = DetailMaterialRotation;
            s.DrawDetailMeshRenderer = DrawDetailMeshRenderer;

            s.MainPlaneHeight = MainPlaneHeight;
            s.TopPlaneHeight = TopPlaneHeight;
            s.DetailPlaneHeight = DetailPlaneHeight;

            s.CornerMeshWidth = CornerMeshWidth;

            s.OriginalStartPoint = this.transform.position;
            s.DetailPlaneOffset = new Vector3(0,.1f,-.2f);

            s.MainPlaneFollowTerrainCurve = MainPlaneFollowTerrainCurve;
            s.DetailPlaneFollowTerrainCurve = DetailPlaneFollowTerrainCurve;

            s.ParentGameObjectName = this.name;
            s.terrainDisplayer = this;


            TerrainManager = new TerrainManager(s);
            PrefabManager = new PrefabManager(s);

         

            Cleanup();

        }
    }

    //Remove the terrain and prefab managers and reset what we need to for the rules.  This is called when we switch between edit and play modes
    public void Cleanup()
    {
        if (TerrainManager != null)
        {
            TerrainManager.RemoveTerrainObject();
        }
        if (PrefabManager != null)
        {
            PrefabManager.RemovePrefabObject();
        }

        for (int i = 0; i < PrefabRules.Count; i++)
        {
            PrefabRules[i].CurrentLocation = Vector3.zero;
            PrefabRules[i].LastPrefabLocation = Vector3.zero;
        }
    }

    // Use this for initialization
    void Start()
    {
        //Generate the initial terrain to avoid slowdown once we start
        Shader.WarmupAllShaders();
    }

    // Update is called once per frame
    void Update()
    {
        if (!coroutineRunning)
        {
            StartCoroutine(GenerateTerrainCoroutine(100));	
        }
      	
    }


    private IEnumerator GenerateTerrainCoroutine(float leadAmount)
    {
        coroutineRunning = true;

        GenerateTerrain(leadAmount);

        //No need to run this every frame...just run it every so often
        yield return new WaitForSeconds(.2f);

        coroutineRunning = false;

  

     
	}

    public void GenerateTerrain(float leadAmount)
    {
        //Track the right and left sides of the screen so we know how much terrain to generate
        Vector3 rightSide = Camera.main.ViewportToWorldPoint(new Vector3(1, 0, -Camera.main.transform.position.z));
        Vector3 leftSide = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, -Camera.main.transform.position.z));
        float endX = rightSide.x + leadAmount;

        while (TerrainManager.VertexGen.CurrentTerrainRule != null && TerrainManager.GetFarthestX() < endX)
        {
            TerrainManager.Generate(endX);   
            //Update our prefabs with the current terrain info       
            PrefabManager.PlacePrefabs(TerrainManager);

            TerrainManager.Cleanup(leftSide.x - leadAmount);
            PrefabManager.Cleanup(leftSide.x - leadAmount);
        }

        //Only need this when we are no longer generating any new terrain but still need to do the final object cleanup
        if (TerrainManager.VertexGen.CurrentTerrainRule == null && TerrainManager.Pool != null && TerrainManager.Pool.TerrainPieces.Count > 0)
        {
            TerrainManager.Cleanup(leftSide.x);
            PrefabManager.Cleanup(leftSide.x);
        }

       


    }
	
}
