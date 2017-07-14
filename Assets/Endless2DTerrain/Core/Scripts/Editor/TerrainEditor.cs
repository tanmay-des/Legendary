using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Endless2DTerrain;
using System;

[CustomEditor(typeof(TerrainDisplayer))]
public class TerrainEditor : Editor {
	private SerializedObject terrainDisplayer;

    private SerializedProperty PreviewLength;
	
	//Front plane
	private SerializedProperty MainMaterial;
	private SerializedProperty MainMaterialXTiling;
    private SerializedProperty MainMaterialYTiling;
    private SerializedProperty MainMaterialRotation;
	private SerializedProperty MainPlaneHeight;
    private SerializedProperty MainPlaneFollowTerrainCurve;
	
	//Top plane
	private SerializedProperty TopMaterial;
	private SerializedProperty TopMaterialXTiling;
    private SerializedProperty TopMaterialYTiling;
    private SerializedProperty TopMaterialRotation;
	private SerializedProperty TopPlaneHeight;
    public SerializedProperty DrawTopMeshCollider;
    public SerializedProperty DrawTopMeshRenderer;

    //Detail plane
    public SerializedProperty DetailMaterial;
    public SerializedProperty DetailMaterialXTiling;
    public SerializedProperty DetailMaterialYTiling;
    public SerializedProperty DetailMaterialRotation;
    public SerializedProperty DetailPlaneHeight;
    public SerializedProperty DrawDetailMeshRenderer;
    private SerializedProperty DetailPlaneFollowTerrainCurve;

	//Terrain rules
	private SerializedProperty Rules;

    //Prefab rules
    private SerializedProperty PrefabRules;
	
	private bool frontPlaneFoldout = true;
	private bool topPlaneFoldout = true;
    private bool detailPlaneFoldout = true;
	private bool aboutFoldout = false;
	
	
	private bool terrainGenerationRules = true;
    private bool prefabGenerationRules = true;
	
	private List<bool> RulesExpanded;
    private List<bool> PrefabRulesExpanded;

    private static GUIContent addButton = new GUIContent("Add Rule +", "Add Terrain Generation Rule");
	private static GUIContent removeButton = new GUIContent("Delete", "Delete Terrain Generation Rule");
	private static GUIContent upButton = new GUIContent("Up", "Move Rule Up One");
	private static GUIContent downButton = new GUIContent("Down", "Move Rule Down One");

    private static GUIContent addPrefabButton = new GUIContent("Add Prefab +", "Add Prefab Generation Rule");
    private static GUIContent removePrefabButton = new GUIContent("Delete", "Delete Prefab Generation Rule");


	void OnEnable () {
		terrainDisplayer = new SerializedObject(target);

        PreviewLength = terrainDisplayer.FindProperty("PreviewLength");
		
		//Load all the values from the inspector into the serialized property editor objects
		MainMaterial = terrainDisplayer.FindProperty("MainMaterial");
		MainMaterialXTiling = terrainDisplayer.FindProperty("MainMaterialXTiling");
		MainMaterialYTiling = terrainDisplayer.FindProperty("MainMaterialYTiling");
		MainMaterialRotation = terrainDisplayer.FindProperty("MainMaterialRotation");
		MainPlaneHeight = terrainDisplayer.FindProperty("MainPlaneHeight");
        MainPlaneFollowTerrainCurve = terrainDisplayer.FindProperty("MainPlaneFollowTerrainCurve");
		
		TopMaterial = terrainDisplayer.FindProperty("TopMaterial");
		TopMaterialXTiling = terrainDisplayer.FindProperty("TopMaterialXTiling");
		TopMaterialYTiling = terrainDisplayer.FindProperty("TopMaterialYTiling");
		TopMaterialRotation = terrainDisplayer.FindProperty("TopMaterialRotation");
		TopPlaneHeight = terrainDisplayer.FindProperty("TopPlaneHeight");
        DrawTopMeshCollider = terrainDisplayer.FindProperty("DrawTopMeshCollider");
        DrawTopMeshRenderer = terrainDisplayer.FindProperty("DrawTopMeshRenderer");

        DetailMaterial = terrainDisplayer.FindProperty("DetailMaterial");
        DetailMaterialXTiling = terrainDisplayer.FindProperty("DetailMaterialXTiling");
        DetailMaterialYTiling = terrainDisplayer.FindProperty("DetailMaterialYTiling");
        DetailMaterialRotation = terrainDisplayer.FindProperty("DetailMaterialRotation");
        DetailPlaneHeight = terrainDisplayer.FindProperty("DetailPlaneHeight");
        DrawDetailMeshRenderer = terrainDisplayer.FindProperty("DrawDetailMeshRenderer");
        DetailPlaneFollowTerrainCurve = terrainDisplayer.FindProperty("DetailPlaneFollowTerrainCurve");
		
		Rules = terrainDisplayer.FindProperty("Rules");
        PrefabRules = terrainDisplayer.FindProperty("PrefabRules");
		
		RulesExpanded = new List<bool>();
        PrefabRulesExpanded = new List<bool>();
	
	}
	
	
	public override void OnInspectorGUI () {		
		terrainDisplayer.Update();


       
        EditorGUILayout.Slider(PreviewLength, 100, 1000, new GUIContent("Preview Length", "Length of terrain generated in design view."));
        if (PreviewLength.floatValue < 100)
        {
            PreviewLength.floatValue = 100;
        }


			
		frontPlaneFoldout = EditorGUILayout.Foldout(frontPlaneFoldout, "Front");
		if (frontPlaneFoldout){		
			EditorGUILayout.BeginHorizontal();
				EditorGUILayout.Space();
				EditorGUILayout.BeginVertical();				
					EditorGUILayout.PropertyField(MainMaterial, new GUIContent("Material", "The material that will be rendered on this plane of the mesh."));
					EditorGUILayout.PropertyField(MainPlaneHeight, new GUIContent("Height", "The height of this plane of the mesh."));
					EditorGUILayout.PropertyField(MainMaterialXTiling, new GUIContent("UV X Tiling", "UV tiling for the x plane of the mesh."));
                    EditorGUILayout.PropertyField(MainMaterialYTiling, new GUIContent("UV Y Tiling", "UV tiling for the y plane of the mesh."));		
					EditorGUILayout.PropertyField(MainMaterialRotation, new GUIContent("UV Rotation", "Rotation of the UV tiling for the mesh."));
                    EditorGUILayout.PropertyField(MainPlaneFollowTerrainCurve, new GUIContent("UV Follow Terrain Curve", "Experimental - textures are bent to follow the curve of the terrain."));

                    if (MainPlaneHeight.floatValue < 1) { MainPlaneHeight.floatValue = 1; }
                    if (MainMaterialXTiling.floatValue < 1) { MainMaterialXTiling.floatValue = 1; }
                    if (MainMaterialYTiling.floatValue < 1) { MainMaterialYTiling.floatValue = 1; }
            
            EditorGUILayout.EndVertical();	
			EditorGUILayout.EndHorizontal();	
		}
		
		
		


  
		topPlaneFoldout = EditorGUILayout.Foldout(topPlaneFoldout, "Top");		
		if (topPlaneFoldout){				
			EditorGUILayout.BeginHorizontal();
				EditorGUILayout.Space();
				EditorGUILayout.BeginVertical();

                //Only draw the top plane if the checkbox is checked
                DrawTopMeshRenderer.boolValue = EditorGUILayout.Toggle(new GUIContent("Draw Top Mesh Renderer", "Draw top mesh renderer if players can see the top of the mesh."), DrawTopMeshRenderer.boolValue);
                DrawTopMeshCollider.boolValue = EditorGUILayout.Toggle(new GUIContent("Draw Top Mesh Collider", "Draw the top mesh collider to keep from falling through the mesh."), DrawTopMeshCollider.boolValue);

                if (DrawTopMeshCollider.boolValue || DrawTopMeshRenderer.boolValue)
                {

                    EditorGUILayout.PropertyField(TopMaterial, new GUIContent("Material", "The material that will be rendered on this plane of the mesh."));
                    EditorGUILayout.PropertyField(TopPlaneHeight, new GUIContent("Height", "The height of this plane of the mesh."));
                    EditorGUILayout.PropertyField(TopMaterialXTiling, new GUIContent("UV X Tiling", "UV tiling for the x plane of the mesh."));
                    EditorGUILayout.PropertyField(TopMaterialYTiling, new GUIContent("UV Y Tiling", "UV tiling for the y plane of the mesh."));
                    EditorGUILayout.PropertyField(TopMaterialRotation, new GUIContent("UV Rotation", "Rotation of the UV tiling for the mesh."));

                    if (TopPlaneHeight.floatValue < 1) { TopPlaneHeight.floatValue = 1; }
                    if (TopMaterialXTiling.floatValue < 1) { TopMaterialXTiling.floatValue = 1; }
                    if (TopMaterialYTiling.floatValue < 1) { TopMaterialYTiling.floatValue = 1; }
                }

				EditorGUILayout.EndVertical();	
			EditorGUILayout.EndHorizontal();
		}	



 
        detailPlaneFoldout = EditorGUILayout.Foldout(detailPlaneFoldout, "Detail");
        if (detailPlaneFoldout)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical();



            //Only draw the detail plane if we are rendering it
            DrawDetailMeshRenderer.boolValue = EditorGUILayout.Toggle(new GUIContent("Draw Detail Mesh Renderer", "Check if you want to add a detail mesh on top of the front mesh"), DrawDetailMeshRenderer.boolValue);
            if (DrawDetailMeshRenderer.boolValue)
            {
                EditorGUILayout.PropertyField(DetailMaterial, new GUIContent("Material", "The material that will be rendered on this plane of the mesh."));
                EditorGUILayout.PropertyField(DetailPlaneHeight, new GUIContent("Height", "The height of this plane of the mesh."));
                EditorGUILayout.PropertyField(DetailMaterialXTiling, new GUIContent("UV X Tiling", "UV tiling for the x plane of the mesh."));
                EditorGUILayout.PropertyField(DetailMaterialYTiling, new GUIContent("UV Y Tiling", "UV tiling for the y plane of the mesh."));
                EditorGUILayout.PropertyField(DetailMaterialRotation, new GUIContent("UV Rotation", "Rotation of the UV tiling for the mesh."));
                EditorGUILayout.PropertyField(DetailPlaneFollowTerrainCurve, new GUIContent("UV Follow Terrain Curve", "Experimental - textures are bent to follow the curve of the terrain."));

                if (DetailPlaneHeight.floatValue < 1) { DetailPlaneHeight.floatValue = 1; }
                if (DetailMaterialXTiling.floatValue < 1) { DetailMaterialXTiling.floatValue = 1; }
                if (DetailMaterialYTiling.floatValue < 1) { DetailMaterialYTiling.floatValue = 1; }
            }

             
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }


		
		EditorGUILayout.HelpBox("Enter terrain generation rules.  At least one is required.", MessageType.None);
		
		
		//Group all the terrain generation rules under one foldout
		terrainGenerationRules = EditorGUILayout.Foldout(terrainGenerationRules, "Terrain Generation Rules");


		
		if (GUILayout.Button(addButton)){
			Rules.arraySize +=1;	
		}
		
		if (terrainGenerationRules){				
			EditorGUILayout.BeginHorizontal();
		
			EditorGUILayout.Space();
			EditorGUILayout.BeginVertical();
				
		
			//Loop through the terrain generation rules
			for (int i =0; i<Rules.arraySize; i++){
			
				SerializedProperty terrainRule = Rules.GetArrayElementAtIndex(i);
				
				//Get the properites of each terrain rule
				SerializedProperty selectedTerrainStyle = terrainRule.FindPropertyRelative("SelectedTerrainStyle");
				SerializedProperty selectedTerrainLength = terrainRule.FindPropertyRelative("SelectedTerrainLength");
		        SerializedProperty minimumKeyVertexHeight = terrainRule.FindPropertyRelative("MinimumKeyVertexHeight");
		        SerializedProperty maximumKeyVertexHeight = terrainRule.FindPropertyRelative("MaximumKeyVertexHeight");      
		        SerializedProperty minimumKeyVertexSpacing = terrainRule.FindPropertyRelative("MinimumKeyVertexSpacing");
		        SerializedProperty maximumKeyVertexSpacing = terrainRule.FindPropertyRelative("MaximumKeyVertexSpacing");    
		        SerializedProperty calculatedVertexSpacing = terrainRule.FindPropertyRelative("CalculatedVertexSpacing"); 
		        SerializedProperty ruleLength = terrainRule.FindPropertyRelative("RuleLength");
				SerializedProperty meshLength = terrainRule.FindPropertyRelative("MeshLength");        
		        SerializedProperty angle = terrainRule.FindPropertyRelative("Angle");
                SerializedProperty expandedRules = terrainRule.FindPropertyRelative("ExpandedRules");

      							
				//Determine if the rule is expanded or collapsed
				if (RulesExpanded.Count <= i){
					RulesExpanded.Add (true);			
				}
				
				RulesExpanded[i] = EditorGUILayout.Foldout(RulesExpanded[i], "Rule " + (i+1).ToString());				
			
				
				if (RulesExpanded[i]){
					EditorGUILayout.BeginHorizontal();
		
					EditorGUILayout.Space();
					EditorGUILayout.BeginVertical();
					
					//Delete this element if the remove button is clicked
					bool deleted = false;	
					EditorGUILayout.BeginHorizontal();
					if (GUILayout.Button(removeButton, GUILayout.Width(50))){
						Rules.DeleteArrayElementAtIndex(i);	
						deleted = true;
					}
					
					if(i!=0){
						if (GUILayout.Button(upButton, GUILayout.Width(50))){						
							Rules.MoveArrayElement(i, i-1);
						}
					}
		
					if (i!=Rules.arraySize - 1){
						if (GUILayout.Button(downButton, GUILayout.Width(50))){						
							Rules.MoveArrayElement(i, i+1);
						}
					}
					
				
					EditorGUILayout.EndHorizontal();
		
					
					//Don't layout anything else if we deleted the element
					if (!deleted){
						//Now allow the users to edit the rule
						EditorGUILayout.PropertyField(selectedTerrainStyle, new GUIContent("Terrain Style", "Repeated = Consistent hills of the same height." + System.Environment.NewLine + "Random = Randomly generated hills of different heights."));	
						EditorGUILayout.PropertyField(selectedTerrainLength, new GUIContent("Terrain Length", "Fixed = Terrain will only be generated up the rule length number of units. " + System.Environment.NewLine + "Infinite = Terrain will be generated forever."));
	
						EditorGUILayout.PropertyField(minimumKeyVertexHeight, new GUIContent("Min Terrain Height", "Low point for generated mesh verticies."));
						EditorGUILayout.PropertyField(maximumKeyVertexHeight, new GUIContent("Max Terrain Height", "High point for generated mesh verticies."));
						EditorGUILayout.PropertyField(minimumKeyVertexSpacing, new GUIContent("Min Key Vertex Spacing", "The minimum distance apart your key vertices will be placed."));
						EditorGUILayout.PropertyField(maximumKeyVertexSpacing, new GUIContent("Max Key Vertex Spacing", "The maximum distance apart your key verticies will be placed."));
						EditorGUILayout.PropertyField(calculatedVertexSpacing, new GUIContent("Calculated Vertex Spacing", "How far apart the verticies between key verticies will be placed."));
						
						
	
						if (selectedTerrainLength.intValue == (int)Endless2DTerrain.TerrainRule.TerrainLength.Fixed){
							EditorGUILayout.PropertyField(ruleLength, new GUIContent("Rule Length", "How long terrain will be generated for this rule."));
							
							//Make sure the mesh length doesn't pass our rule length
							if (meshLength.floatValue > ruleLength.floatValue){
                                ruleLength.floatValue = meshLength.floatValue;
							}						
					
							
						}
						
						EditorGUILayout.PropertyField(meshLength, new GUIContent("Mesh Length", "How many units in length the the meshes will be for the rule."));
						EditorGUILayout.PropertyField(angle, new GUIContent("Angle", "The angle your generated terrain will be set at."));

                        //Determine what prefabs can be seen on this rule, provided we have prefab rules
                        if (PrefabRules.arraySize > 0)
                        {
                            expandedRules.boolValue = EditorGUILayout.Foldout(expandedRules.boolValue, "Prefabs Allowed On This Rule");
                     
                            for (int j = 0; j < PrefabRules.arraySize; j++)
                            {

                                //Add all the prefab rules first
                                SerializedProperty allowedPrefab = PrefabRules.GetArrayElementAtIndex(j);
                                SerializedProperty allowedPrefabToClone = allowedPrefab.FindPropertyRelative("PrefabToClone");
                                SerializedProperty allowedPrefabs = terrainRule.FindPropertyRelative("AllowedPrefabs");

                                //Delete any values past the existing one
                                if (allowedPrefabs.arraySize > PrefabRules.arraySize)
                                {
                                    for (int del = PrefabRules.arraySize - 1; del < allowedPrefabs.arraySize; del++)
                                    {
                                        allowedPrefabs.DeleteArrayElementAtIndex(del);
                                    }
                                }


                                //Is there something at this index?  If not, add it
                                if (j >= allowedPrefabs.arraySize)
                                {
                                    allowedPrefabs.InsertArrayElementAtIndex(j);
                                }

                                //Get our values
                                SerializedProperty allowedPrefabRule = allowedPrefabs.GetArrayElementAtIndex(j);
                                SerializedProperty name = allowedPrefabRule.FindPropertyRelative("Name");
                                SerializedProperty index = allowedPrefabRule.FindPropertyRelative("Index");
                                SerializedProperty allowed = allowedPrefabRule.FindPropertyRelative("Allowed");

                                //And set them if they are not set
                                if (String.IsNullOrEmpty(name.stringValue))
                                {
                                    allowed.boolValue = true;
                                }


                                //Don't bother showing this if they haven't set a value for the prefab
                                if (allowedPrefabToClone.objectReferenceValue != null)
                                {
                                    name.stringValue = allowedPrefabToClone.objectReferenceValue.name + " - Prefab Rule " + (index.intValue + 1).ToString();
                                    index.intValue = j;

                                    if (expandedRules.boolValue)
                                    {
                                        EditorGUILayout.BeginHorizontal();
                                        EditorGUILayout.Space();
                                        EditorGUILayout.LabelField(new GUIContent(name.stringValue));
                                        allowed.boolValue = EditorGUILayout.Toggle(allowed.boolValue);
                                        EditorGUILayout.EndHorizontal();
                                    }
                          

                                }
                            }
                          
                        }
                       

          

		
						//Set some defaults for the values			
						if (meshLength.floatValue <10){
							meshLength.floatValue = 10;	
						}
						
			
						
						angle.floatValue = Mathf.Clamp(angle.floatValue, -80, 80);
						
						//Make sure we have enough verts to make a mesh
						if (maximumKeyVertexSpacing.floatValue * 2f > meshLength.floatValue){
                            meshLength.floatValue = maximumKeyVertexSpacing.floatValue * 2f;						
						}
						
						if (minimumKeyVertexSpacing.floatValue > maximumKeyVertexSpacing.floatValue){
                            maximumKeyVertexSpacing.floatValue = minimumKeyVertexSpacing.floatValue;	
						}
						
						if (calculatedVertexSpacing.floatValue < 0.1f){
							calculatedVertexSpacing.floatValue = 0.1f;	
						}
						
						if (minimumKeyVertexSpacing.floatValue <=0){
							minimumKeyVertexSpacing.floatValue = 1;	
						}
						if (maximumKeyVertexSpacing.floatValue <=0){
							maximumKeyVertexSpacing.floatValue = 1;	
						}
						
					}
							
					EditorGUILayout.EndVertical();				
					EditorGUILayout.EndHorizontal();			
					
				}
			}
		
			EditorGUILayout.EndVertical();	
			EditorGUILayout.EndHorizontal();
		}		


        //Prefab generation rules
        //***************************//
        EditorGUILayout.HelpBox("Enter prefab generation rules.", MessageType.None);


        //Group all the terrain generation rules under one foldout
        prefabGenerationRules = EditorGUILayout.Foldout(prefabGenerationRules, "Prefab Generation Rules");



        if (GUILayout.Button(addPrefabButton))
        {
            PrefabRules.arraySize += 1;
        }

        if (prefabGenerationRules)
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical();


            //Loop through the terrain generation rules
            for (int i = 0; i < PrefabRules.arraySize; i++)
            {

                SerializedProperty prefabRule = PrefabRules.GetArrayElementAtIndex(i);

                //Get the properites of each terrain rule
                SerializedProperty prefabToClone = prefabRule.FindPropertyRelative("PrefabToClone");
                SerializedProperty offset = prefabRule.FindPropertyRelative("Offset");
                SerializedProperty minRepeatDistance = prefabRule.FindPropertyRelative("MinRepeatDistance");
                SerializedProperty maxRepeatDistance = prefabRule.FindPropertyRelative("MaxRepeatDistance");
                SerializedProperty minGroupSize = prefabRule.FindPropertyRelative("MinGroupSize");
                SerializedProperty maxGroupSize = prefabRule.FindPropertyRelative("MaxGroupSize");
                SerializedProperty minGroupSpacing = prefabRule.FindPropertyRelative("MinGroupSpacing");
                SerializedProperty maxGroupSpacing = prefabRule.FindPropertyRelative("MaxGroupSpacing");
                SerializedProperty minSlope = prefabRule.FindPropertyRelative("MinSlope");
                SerializedProperty maxSlope = prefabRule.FindPropertyRelative("MaxSlope");
                SerializedProperty matchGroundAngle = prefabRule.FindPropertyRelative("MatchGroundAngle");

                SerializedProperty useMinDistance = prefabRule.FindPropertyRelative("UseMinDistance");
                SerializedProperty minDistance = prefabRule.FindPropertyRelative("MinDistance");
                SerializedProperty useMaxDistance = prefabRule.FindPropertyRelative("UseMaxDistance");
                SerializedProperty maxDistance = prefabRule.FindPropertyRelative("MaxDistance");




                //Determine if the rule is expanded or collapsed
                if (PrefabRulesExpanded.Count <= i)
                {
                    PrefabRulesExpanded.Add(true);
                }

                PrefabRulesExpanded[i] = EditorGUILayout.Foldout(PrefabRulesExpanded[i], "Prefab Rule " + (i + 1).ToString());


                if (PrefabRulesExpanded[i])
                {
                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.Space();
                    EditorGUILayout.BeginVertical();

                    //Delete this element if the remove button is clicked
                    bool ruleDeleted = false;
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button(removePrefabButton, GUILayout.Width(50)))
                    {
                        PrefabRules.DeleteArrayElementAtIndex(i);
                        ruleDeleted = true;
                    }

                  


                    EditorGUILayout.EndHorizontal();


                    //Don't layout anything else if we deleted the element
                    if (!ruleDeleted)
                    {
                        //Now allow the users to edit the rule
                        EditorGUILayout.PropertyField(prefabToClone, new GUIContent("Prefab To Clone", "The prefab you want to clone."));
                        EditorGUILayout.PropertyField(offset, new GUIContent("Offset", "The amount the prefab will be offset fron the default placement."), true);
                        EditorGUILayout.PropertyField(minRepeatDistance, new GUIContent("Min Repeat Distance", "Minimum distance between prefab placement."));                        
                        EditorGUILayout.PropertyField(maxRepeatDistance, new GUIContent("Max Repeat Distance", "Maximum distance between prefab placement."));
                        EditorGUILayout.PropertyField(minGroupSize, new GUIContent("Min Group Size", "Minimum group size for prefabs - used if you want more than one prefab generated at a time."));
                        EditorGUILayout.PropertyField(maxGroupSize, new GUIContent("Max Group Size", "Maximum group size for prefabs - used if you want more than one prefab generated at a time."));
                        EditorGUILayout.PropertyField(minGroupSpacing, new GUIContent("Min Group Spacing", "The minimum spacing between the prefabs in your group."));
                        EditorGUILayout.PropertyField(maxGroupSpacing, new GUIContent("Max Group Spacing", "The maximum spacing between the prefabs in your group."));
                        EditorGUILayout.PropertyField(minSlope, new GUIContent("Min Slope Placement", "Prefabs will only be generated on slopes if the slopoe is greater than this angle."));
                        EditorGUILayout.PropertyField(maxSlope, new GUIContent("Max Slope Placement", "Prefabs will only be generated on slopes if the slope is less than this angle."));
                        EditorGUILayout.PropertyField(matchGroundAngle, new GUIContent("Match Ground Angle", "Rotate the prefabs to match the current slope of the ground."));

                        //Set min and max distances
                        useMinDistance.boolValue = EditorGUILayout.Toggle("Use Min Distance", useMinDistance.boolValue);
                        if (useMinDistance.boolValue)
                        {
                            EditorGUILayout.PropertyField(minDistance, new GUIContent("Min Distance"));
                        }

                        useMaxDistance.boolValue = EditorGUILayout.Toggle("Use Max Distance", useMaxDistance.boolValue);                    
                        if (useMaxDistance.boolValue){
                            EditorGUILayout.PropertyField(maxDistance, new GUIContent("Max Distance"));
                        }

                        if (minDistance.floatValue < 0) { minDistance.floatValue = 0; }
                        if (maxDistance.floatValue < 0) { maxDistance.floatValue = 0; }

                        if (minRepeatDistance.floatValue > maxRepeatDistance.floatValue) { maxRepeatDistance.floatValue = minRepeatDistance.floatValue; }
                        if (minGroupSize.intValue > maxGroupSize.intValue) { maxGroupSize.intValue = minGroupSize.intValue; }
                        if (minGroupSpacing.floatValue > maxGroupSpacing.floatValue) { maxGroupSpacing.floatValue = minGroupSpacing.floatValue; }
                        if (minGroupSpacing.floatValue < 1)
                        {
                            minGroupSpacing.floatValue = 1;
                        }
                        if (maxGroupSpacing.floatValue < 1)
                        {
                            maxGroupSpacing.floatValue = 1;
                        }
                        if (minRepeatDistance.floatValue < 1)
                        {
                            minRepeatDistance.floatValue = 1;
                        }
                        
                        //Set some default if these aren't set
                        if (maxSlope.floatValue == 0 && minSlope.floatValue == 0)
                        {
                            maxSlope.floatValue = 90f;
                            minSlope.floatValue = -90f;
                        }
		
                    

                    }

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();

                }
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }		
		
		

		
		


		//Now update all the modified properties
		terrainDisplayer.ApplyModifiedProperties();
		
		
		aboutFoldout = EditorGUILayout.Foldout(aboutFoldout, " About Endless - 2D Terrain Generator");			
		if (aboutFoldout)
		{		
			
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.Space();
			EditorGUILayout.BeginVertical();	
			
			GUILayout.Label(
				"Created by Neil Meredith.\n" +
				"Thank you for your purchase!.");			
			if (GUILayout.Button("Rate this package! (Unity Asset Store)"))
			{
				Application.OpenURL("https://www.assetstore.unity3d.com/#/content/12324");
			}		
		
			
        	EditorGUILayout.EndVertical();	
			EditorGUILayout.EndHorizontal();	
			
			
			
			
		}


        if (GUI.changed)
        {       
            TerrainDisplayer td = target as TerrainDisplayer;        
            td.Setup();
            td.GenerateTerrain(PreviewLength.floatValue);
        }
	}

 

}
