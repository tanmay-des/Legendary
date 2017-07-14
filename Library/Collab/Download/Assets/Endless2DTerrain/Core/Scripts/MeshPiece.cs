using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


namespace Endless2DTerrain
{
    public class MeshPiece
    {
        private Settings settings { get; set; }

        public MeshPiece(VertexGenerator vg, Plane planeType, Settings s)
        {
            this.vg = vg;
            PlaneType = planeType;
            settings = s;
        }

 
        public enum Plane
        {
            Front,
            Top,
            Detail
        }

        public VertexGenerator vg { get; set; }

        public Plane PlaneType { get; set; }

        //A reference to the created mesh object
        public GameObject MeshObject { get; set; }

        //Top verts
        public List<Vector3> KeyTopVerticies { get; set; }
        public List<Vector3> AllTopVerticies { get; set; }

        //Bottom verts
        public List<Vector3> KeyBottomVerticies { get; set; }
        public List<Vector3> AllBottomVerticies { get; set; }

        //All verts in mesh plane
        public List<Vector3> PlaneVerticies { get; set; }

        //All verts in plane (after rotation, if any)
        public List<Vector3> RotatedPlaneVerticies { get; set; }

        //Reference to the components we need to build our mesh
        private MeshFilter meshFilter { get; set; }
        private MeshRenderer meshRenderer { get; set; }
        private MeshCollider meshCollider { get; set; }
		private PolygonCollider2D polyCollider { get; set; }

        public Mesh mesh { get; set; }

        //For easy access when positioning the mesh
        public Vector3 BottomRightCorner
        {
            get
            {
                return RotatedPlaneVerticies[RotatedPlaneVerticies.Count - 2];
            }
        }

        public Vector3 TopRightCorner
        {
            get
            {
                return RotatedPlaneVerticies[RotatedPlaneVerticies.Count - 1];
            }
        }

        public Vector3 BottomLeftCorner
        {
            get
            {
                return RotatedPlaneVerticies[0];
            }
        }



        public Vector3 TopLeftCorner
        {
            get
            {
                return RotatedPlaneVerticies[1];
            }
        }

   
		
        //Always just start with the normal plane verticies.  If the standard mesh was rotated, the top mesh will be as well
		public Vector3 StartTopMesh{
			get{        
				return PlaneVerticies[1];		
			}
		}



        /// <summary>
        /// Generate key verticies.  This will update the terrain rules, which is why we have to pass in the plane type. 
        /// We currently only want to update our repeating point location for the front plane
        /// </summary>
        /// <param name="planeType"></param>
        public void PopulateKeyVerticies(Plane planeType)
        {
            bool updateRepeatingPointLocation = (planeType == Plane.Front);
            KeyTopVerticies = vg.GenerateKeyVerticies(updateRepeatingPointLocation);
        }

        public void MoveMesh(Vector3 move, Plane planeType)
        {
            TransformHelpers th = new TransformHelpers();

            //Update all the verticies
            KeyTopVerticies = th.MoveStartVertex(KeyTopVerticies, move, false, planeType);
            KeyBottomVerticies = th.MoveStartVertex(KeyBottomVerticies, move, false, planeType);
            AllTopVerticies = th.MoveStartVertex(AllTopVerticies, move, false, planeType);
            AllBottomVerticies = th.MoveStartVertex(AllBottomVerticies, move, false, planeType);
            PlaneVerticies = th.MoveStartVertex(PlaneVerticies, move, false, planeType);
            RotatedPlaneVerticies = th.MoveStartVertex(RotatedPlaneVerticies, move, false, planeType);

            //Now clear and update the mesh
            CreateMesh();
        }
		
		public void Create(Vector3 origin, float angle, List<Vector3> keyVerticies){
            //Update all our verticies
            SetVerticies(origin, angle, keyVerticies);
            CreateMesh();
		}

        private void CreateMesh(){
            if (BelowMinVerts()){return;}
		
       
            if (mesh == null){       
                InstantiateMeshObject();
            }else{
                mesh.Clear();
                AddMeshComponents();
            }

            //Set verts, uvs and triangles for the mesh from the rotated plane verts (these are populated even if there was no rotation)
            mesh.vertices = RotatedPlaneVerticies.ToArray();
            mesh.triangles = GetMeshTriangles(RotatedPlaneVerticies).ToArray();
            mesh.uv = GetUVMapping(RotatedPlaneVerticies).ToArray();

            if (PlaneType == Plane.Front && settings.MainMaterial !=null)
            {
                meshRenderer.GetComponent<Renderer>().sharedMaterial = settings.MainMaterial;
                meshRenderer.GetComponent<Renderer>().sharedMaterial.renderQueue = RenderQueue.FrontPlane;
               
            }
            if (PlaneType == Plane.Detail && settings.DetailMaterial !=null)
            {
                meshRenderer.GetComponent<Renderer>().sharedMaterial = settings.DetailMaterial;
                meshRenderer.GetComponent<Renderer>().sharedMaterial.renderQueue = RenderQueue.DetailPlane;
              
            }
            if (PlaneType == Plane.Top && settings.DrawTopMeshRenderer && settings.TopMaterial !=null)
            {
                meshRenderer.GetComponent<Renderer>().sharedMaterial = settings.TopMaterial;
            }


            //Add collider to the top plane
            AddCollider();

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
        }

		private Vector2 Vector3to2(Vector3 point){
			return new Vector2 (point.x, point.y);
		}

        public void AddCollider()
        {
			if (PlaneType == Plane.Top && settings.DrawTopMeshCollider) {
				var colliderMesh = new Mesh ();
				colliderMesh.vertices = mesh.vertices;
				colliderMesh.triangles = mesh.triangles;
				colliderMesh.RecalculateBounds ();

				meshCollider.sharedMesh = colliderMesh;
				meshCollider.smoothSphereCollisions = true;
			} else if (PlaneType == Plane.Front) {
				Vector2[] Points = new Vector2[mesh.triangles.Length];
				for (int i = 0; i < mesh.triangles.Length; i++) {
					Points [i] = Vector3to2 (mesh.vertices [mesh.triangles [i]]);				
				}
				polyCollider.points = Points;
			}
        }

        public void CreateCorner(List<Vector3> keyTopVerticies, List<Vector3> keyBottomVerticies)
        {
            //Update all our verticies
            SetCornerVerticies(keyTopVerticies, keyBottomVerticies);
            CreateMesh();
        }

        public void Create(Vector3 origin, float angle)
        {
			Create(origin, angle, null);
        }
		
		private bool BelowMinVerts(){
            if (KeyTopVerticies == null ||
                (KeyTopVerticies.Count < 2) ||
				(vg.CurrentTerrainRule == null)){
                return true;
            }
            return false;
		}


        private void SetCornerVerticies(List<Vector3> keyTopVerticies, List<Vector3> keyBottomVerticies)
        {
            KeyTopVerticies = keyTopVerticies;
            KeyBottomVerticies = keyBottomVerticies;

            AllTopVerticies = keyTopVerticies;
            AllBottomVerticies = keyBottomVerticies;

            PlaneVerticies = vg.GetPlaneVerticies(AllBottomVerticies, AllTopVerticies);
            RotatedPlaneVerticies = PlaneVerticies;

        }
       

        private void SetVerticies(Vector3 origin, float angle, List<Vector3> keyVerticies)
        {
			if (keyVerticies !=null){KeyTopVerticies = keyVerticies;}
			
			//Don't try and set the vertices if we're below the min we need for a plane
			if (BelowMinVerts()){return;}
            if (vg.CurrentTerrainRule == null) { return; }

          
            TransformHelpers th = new TransformHelpers();

            AllTopVerticies = vg.GenerateCalculatedVertices(KeyTopVerticies);

            //For the front plane, set a fixed lower boundary on the verts (bottom of mesh will be a straight line)
            if (PlaneType == Plane.Front)
            {               
                Vector3 firstBottomVertex = AllTopVerticies[0];
                Vector3 shift = new Vector3(firstBottomVertex.x, firstBottomVertex.y - settings.MainPlaneHeight, firstBottomVertex.z);

                AllBottomVerticies = th.MoveStartVertex(AllTopVerticies, AllTopVerticies[0], shift, true);
                KeyBottomVerticies = th.MoveStartVertex(KeyTopVerticies, AllTopVerticies[0], shift, true);

                //Test perp verts
                //Vector3 shift = new Vector3(firstBottomVertex.x, firstBottomVertex.y, firstBottomVertex.z);
                //AllBottomVerticies = th.GetPerpendicularOffset(AllTopVerticies, settings.MainPlaneHeight);
                //KeyBottomVerticies = th.GetPerpendicularOffset(KeyTopVerticies, settings.MainPlaneHeight);
            }

            if (PlaneType == Plane.Detail)
            {
                Vector3 firstBottomVertex = th.CopyVertex(AllTopVerticies[0]);
                Vector3 shift = new Vector3(firstBottomVertex.x, firstBottomVertex.y - settings.DetailPlaneHeight, firstBottomVertex.z);

                AllBottomVerticies = th.MoveStartVertex(AllTopVerticies,firstBottomVertex, shift, true);
                KeyBottomVerticies = th.MoveStartVertex(KeyTopVerticies, firstBottomVertex, shift, true);
            }

            //For the top of the mesh, shift the verticies in the z direction
            if (PlaneType == Plane.Top)
            {
               
                //The bottom verts are a copy of the top
                AllBottomVerticies = th.CopyList(AllTopVerticies);
                KeyBottomVerticies = th.CopyList(KeyTopVerticies);
				
				Vector3 firstBottomVertex = AllTopVerticies[0];

                //Then shift the top verts into the z plane
                AllTopVerticies = th.MoveStartVertex(AllTopVerticies, AllTopVerticies[0], new Vector3(firstBottomVertex.x, firstBottomVertex.y, firstBottomVertex.z + settings.TopPlaneHeight), false);
            }


            //Now stich top and bottom verticies together into a plane
            PlaneVerticies = vg.GetPlaneVerticies(AllBottomVerticies, AllTopVerticies);


            //For the top plane we have to move our point of origin based on the plane height
            if (PlaneType == Plane.Top)
            {
                origin = new Vector3(origin.x, origin.y, origin.z + settings.TopPlaneHeight);
            }

			
			//Now move the whole plane to the point of origin (usually where the last mesh ended)
            //Move relative to the vertex at index 1 (the top of the plane) instead of zero (the bottom of the plane) since we want to match the top of the meshes
            PlaneVerticies = th.MoveStartVertex(PlaneVerticies, PlaneVerticies[1], origin, false);

            //Store the rotated verticies so we know where the actual end point of the mesh is (and where to start the next one)
            //Create the mesh from the rotate verticies, but generate it from the non-rotated ones
            if (angle!=0)
            {
                RotatedPlaneVerticies = th.RotateVertices(PlaneVerticies, angle);	
			    RotatedPlaneVerticies = th.MoveStartVertex(RotatedPlaneVerticies, RotatedPlaneVerticies[1], origin, false);
            }
            else
            {
                RotatedPlaneVerticies = PlaneVerticies;
            }

           
        }

        private void InstantiateMeshObject()
        {
            MeshObject = new GameObject("MeshPiece");
            AddMeshComponents();
        }

        private void AddMeshComponents()
        {
            
            if (meshFilter == null){meshFilter = MeshObject.AddComponent<MeshFilter>();}       

			if (PlaneType == Plane.Top) {
				if (settings.DrawTopMeshRenderer) {
					if (meshRenderer == null) {
						meshRenderer = MeshObject.AddComponent<MeshRenderer> ();
					}
				}
				if (settings.DrawTopMeshCollider) {
					if (meshCollider == null) {
						meshCollider = MeshObject.AddComponent<MeshCollider> ();
					}
				}
			} else if (PlaneType == Plane.Front) {
				if (meshRenderer == null) { meshRenderer = MeshObject.AddComponent<MeshRenderer>(); }
				if (polyCollider == null) { polyCollider = MeshObject.AddComponent<PolygonCollider2D> (); }
			}
            else
            {
                if (meshRenderer == null) { meshRenderer = MeshObject.AddComponent<MeshRenderer>(); }
                if (meshCollider == null) { meshCollider = MeshObject.AddComponent<MeshCollider>(); }
            }
			
			GameObject.DestroyImmediate(meshFilter.sharedMesh);
			meshFilter.sharedMesh  = new Mesh();
			mesh = meshFilter.sharedMesh;

           // mesh = meshFilter.mesh;
        }

        private List<int> GetMeshTriangles(List<Vector3> planeVerticies)
        {
            //Assume the mesh is a single plane
            List<int> triangles = new List<int>();
            for (int i = 0; i < planeVerticies.Count; i += 2)
            {

                //Don't worry about figuring this all out for now, just break if we're past the max index for the vertices
                if ((i + 3) > planeVerticies.Count)
                {
                    break;
                }

                triangles.Add(i);
                triangles.Add(i + 1);
                triangles.Add(i + 3);

                triangles.Add(i + 3);
                triangles.Add(i + 2);
                triangles.Add(i);

            }
            return triangles;
        }


        //http://www.freemathhelp.com/length-line-segment.html
        public float GetLengthOfLine(Vector2 lineStart, Vector2 lineEnd)
        {
            float x = Mathf.Abs(lineStart.x - lineEnd.x);
            float y = Mathf.Abs(lineStart.y - lineEnd.y);
            x = Mathf.Pow(x, 2);
            y = Mathf.Pow(y, 2);
            return Mathf.Sqrt(x + y);
        }


        private Color32[] GetColors(List<Vector3> planeVerticies)
        {
          
            Color32[] colors = new Color32[planeVerticies.Count()];
            for (int i = 0; i < planeVerticies.Count; i++)
            {
                colors[i] = Color.black;
            }

            return colors;
        }

        private List<Vector2> GetUVMapping(List<Vector3> planeVerticies)
        {

            TransformHelpers th = new TransformHelpers();
			
			float textureHeight = 1;
			float textureWidth = 1;


            float xTiling = 1f;
            float yTiling = 1f;

            //The uv tiling also has to factor in the height and width of the textures we are using
			if (settings.MainMaterial!= null && settings.MainMaterial.mainTexture != null){	        
	             textureHeight = settings.MainMaterial.mainTexture.height;
	             textureWidth = settings.MainMaterial.mainTexture.width;
			}
    
            if (settings.TopMaterial !=null && settings.TopMaterial.mainTexture != null)
            {
                textureHeight = settings.TopMaterial.mainTexture.height;
                textureWidth = settings.TopMaterial.mainTexture.width;
            }

            if (settings.DetailMaterial != null && settings.DetailMaterial.mainTexture != null)
            {
                textureHeight = settings.DetailMaterial.mainTexture.height;
                textureWidth = settings.DetailMaterial.mainTexture.width;
            }

            //Set our tiling depending on the plane
            if (PlaneType == Plane.Front)
            {
                xTiling = settings.MainMaterialXTiling;
                yTiling = settings.MainMaterialYTiling;
            }

            if (PlaneType == Plane.Top)
            {         
                xTiling = settings.TopMaterialXTiling;
                yTiling = settings.TopMaterialYTiling;
            }

            if (PlaneType == Plane.Detail)
            {
                xTiling = settings.DetailMaterialXTiling;
                yTiling = settings.DetailMaterialYTiling;
            }


            //Tile the texture as needed
            textureWidth = textureWidth / xTiling;
            textureHeight = textureHeight / yTiling;

            //Track how far along we are on the top texture mapping
            float currentTopTextureX = 0;


            List<Vector2> uvs = new List<Vector2>();
            for (int i = 0; i < planeVerticies.Count; i++)
            {
                Vector3 vertex = planeVerticies[i];
                Vector3 previousVertex = Vector3.zero;
                if (i > 0)
                {
                    previousVertex = planeVerticies[i - 1];
                }
               

                //Our standard uv mapping is just our point in space divided by the width and the height of our texture (assuming an x/y plane)
                float xMapping = vertex.x / textureWidth;
                float yMapping = vertex.y / textureHeight;

             
                if (PlaneType == Plane.Top)
                {
                    //We have to factor in the rise in y (from the lowest point in the list to our current point), as well as the x movement across
                    //in our uv mapping.  This is because this is not a flat plane

                    //The first time through, set our current x position based off the vertex
                    if (currentTopTextureX == 0) { currentTopTextureX = vertex.x; }              
                    xMapping = (currentTopTextureX) / textureWidth;

                    //After that, increment it by the length of the line between the two points (which includes the movement in the x and y plane) in the uv mapping
                    if (previousVertex != Vector3.zero)
                    {
                        float length = GetLengthOfLine(vertex, previousVertex);
                        currentTopTextureX += length;
                        xMapping = (currentTopTextureX) / textureWidth;
                    }

                    //For the y mapping, since we are in the z plane divide the texture height by z instead of y.
                    yMapping = vertex.z / textureHeight;
                }


                //If we want the uv mapping to follow the curve of the plane instead, set it here
                if (PlaneType == Plane.Front && settings.MainPlaneFollowTerrainCurve)
                {
                    if (i % 2 == 0)
                    {
                        yMapping = -settings.MainPlaneHeight / textureHeight;
                    }
                    else
                    {
                        yMapping = 1;
                    }
                }

                if (PlaneType == Plane.Detail && settings.DetailPlaneFollowTerrainCurve)
                {
                    if (i % 2 == 0)
                    {
                        yMapping = -settings.DetailPlaneHeight / textureHeight;
                    }
                    else
                    {
                        yMapping = 1;
                    }
                }

                //Finally set the actual uv mapping
                Vector2 uv = new Vector2(xMapping, yMapping);


                //Now set the rotation of the uv mapping
                if (settings.MainMaterialRotation != 0 && PlaneType == Plane.Front)
                {
                    uv = th.RotateVertex(uv, settings.MainMaterialRotation);
                }

                if (settings.DetailMaterialRotation != 0 && PlaneType == Plane.Detail)
                {
                    uv = th.RotateVertex(uv, settings.DetailMaterialRotation);
                }


                if (settings.TopMaterialRotation != 0 && PlaneType == Plane.Top)
                {
                    uv = th.RotateVertex(uv, settings.TopMaterialRotation);
                }

                uvs.Add(uv);
            }

            return uvs;

        }



    }



}
