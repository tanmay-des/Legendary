using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Endless2DTerrain
{
    public class TerrainPiece
    {
        private Settings settings { get; set; }

        public TerrainPiece(Settings s)
        {
            settings = s;
            MeshPieces = new List<MeshPiece>();
        }

        //Just for reference
        public float TerrainAngle { get; set; }


        //The parent terrain piece
        public GameObject TerrainObject { get; set; }

        //The multiple meshes the terrain is made of
        public List<MeshPiece> MeshPieces { get; set; }

        //Get the front mesh plane in this terrain piece
        public MeshPiece FrontMesh
        {
            get
            {
                if (MeshPieces == null || MeshPieces.Count == 0)
                {
                    return null;
                }
                return MeshPieces.Where(mp => mp.PlaneType == MeshPiece.Plane.Front).FirstOrDefault();
            }
        }

        public MeshPiece DetailMesh
        {
            get
            {
                if (MeshPieces == null || MeshPieces.Count == 0)
                {
                    return null;
                }
                return MeshPieces.Where(mp => mp.PlaneType == MeshPiece.Plane.Detail).FirstOrDefault();
            }
        }

        //Used for piecing together the meshes, not for generating the verticies.  Return the top vertex of the current front plane, so we know what point to
        //match with the top of the next mesh
        public Vector3 NextTerrainOrigin
        {
            get
            { 
                //Get the front mesh
                MeshPiece frontMesh = FrontMesh;
                if (frontMesh == null) { return settings.OriginalStartPoint; }

                //The last vertex is a top vertex.  Get the last top one so we know where to start the next mesh
                Vector3 lastTopVertex = frontMesh.TopRightCorner;
                return lastTopVertex;
            }
        }




        public void Create(VertexGenerator vg, Vector3 origin)
        {

            //Track the angle we are currenlty on          
            TerrainAngle = vg.CurrentTerrainRule.Angle;

            //Create the front mesh and populate our key verts for the front plane
            MeshPiece mp = new MeshPiece(vg, MeshPiece.Plane.Front, settings);
            mp.PopulateKeyVerticies(MeshPiece.Plane.Front);            
            
            //Now create the mesh
            mp.Create(origin, TerrainAngle);
          
			
			//The first mesh could be null if we are below the minimum verticies we need to create a plane
			if (mp.MeshObject != null){
				
                //Create a placeholder object for our mesh pieces
				InstantiateTerrainObject();	
			
                //And add the front plane mesh piece to our list of meshes in the terrain piece
			  	MeshPieces.Add(mp);


                if (settings.DrawDetailMeshRenderer)
                {
                    MeshPiece mpDetail = new MeshPiece(vg, MeshPiece.Plane.Detail, settings);
                    mpDetail.Create(origin + settings.DetailPlaneOffset, TerrainAngle, mp.KeyTopVerticies);
                    MeshPieces.Add(mpDetail);
                }



                if (settings.DrawTopMeshCollider || settings.DrawTopMeshRenderer)
                {
                    MeshPiece mpTop = new MeshPiece(vg, MeshPiece.Plane.Top, settings);
                    mpTop.Create(mp.StartTopMesh, TerrainAngle, mp.KeyTopVerticies);
                    MeshPieces.Add(mpTop);
                }
      
                  
				
				
	            //Just to tidy up the heirarchy
	            ParentMeshesToTerrainObject();
					
			}
        }

        public void CreateCorner(VertexGenerator vg, TerrainPiece previousTerrain, TerrainPiece currentTerrain)
        {

            //Our plane is made up of the last top and bottom verts of the previous mesh, and the first top and bottom verts of the current mesh
            List<Vector3> topVerticies = GetCornerVerts(previousTerrain, currentTerrain, MeshPiece.Plane.Front, true);
            List<Vector3> bottomVerticies = GetCornerVerts(previousTerrain, currentTerrain, MeshPiece.Plane.Front, false);

            //Create our front mesh piece
            MeshPiece meshPiece = new MeshPiece(vg, MeshPiece.Plane.Front, settings);
            meshPiece.CreateCorner(topVerticies, bottomVerticies);

            //The first mesh could be null if we are below the minimum verticies we need to create a plane
            if (meshPiece.MeshObject != null)
            {
                TransformHelpers th = new TransformHelpers();

                //Now we've created the front of our mesh
                InstantiateTerrainObject();
                MeshPieces.Add(meshPiece);

                //Add detail mesh

                if (settings.DrawDetailMeshRenderer)
                {
                    MeshPiece meshPieceDetail = new MeshPiece(vg, MeshPiece.Plane.Detail, settings);
                    topVerticies = GetCornerVerts(previousTerrain, currentTerrain, MeshPiece.Plane.Detail, true);
                    bottomVerticies = GetCornerVerts(previousTerrain, currentTerrain, MeshPiece.Plane.Detail, false);
                    meshPieceDetail.CreateCorner(topVerticies, bottomVerticies);
                    MeshPieces.Add(meshPieceDetail);
                }            


                if (settings.DrawTopMeshCollider || settings.DrawTopMeshRenderer)
                {
                    //Create the verticies for the top of our mesh, and add that too                  
                    MeshPiece meshPieceTop = new MeshPiece(vg, MeshPiece.Plane.Top, settings);

                    //Use the top verts as our bottom z row
                    bottomVerticies = th.CopyList(topVerticies);
                    Vector3 firstBottomVertex = topVerticies[0];

                    //Then shift the top verts into the z plane
                    topVerticies = th.MoveStartVertex(topVerticies, firstBottomVertex, new Vector3(firstBottomVertex.x, firstBottomVertex.y, firstBottomVertex.z + settings.TopPlaneHeight), false);
                    meshPieceTop.CreateCorner(topVerticies, bottomVerticies);
                    MeshPieces.Add(meshPieceTop);
                }



                //Just to tidy up the heirarchy
                ParentMeshesToTerrainObject();

            }
        }

        public List<Vector3> GetCornerVerts(TerrainPiece previousTerrain, TerrainPiece currentTerrain, MeshPiece.Plane planeType, bool topVerts)
        {


            MeshPiece lastMesh = null;
            MeshPiece currentMesh = null;

            List<Vector3> verts = new List<Vector3>();

            if (planeType == MeshPiece.Plane.Front)
            {
                lastMesh = previousTerrain.FrontMesh;
                currentMesh = currentTerrain.FrontMesh;
            }
            else if (planeType == MeshPiece.Plane.Detail)
            {
                lastMesh = previousTerrain.DetailMesh;
                currentMesh = currentTerrain.DetailMesh;
            }

            if (topVerts)
            {
                verts.Add(lastMesh.TopRightCorner);
                verts.Add(currentMesh.TopLeftCorner);
            }
            else
            {
                verts.Add(lastMesh.BottomRightCorner);
                verts.Add(currentMesh.BottomLeftCorner);
            }

            return verts;
        }

        private void InstantiateTerrainObject()
        {
            //This is just a placeholder for all the mesh pieces
            TerrainObject = new GameObject("TerrainPiece");
        }

        private void ParentMeshesToTerrainObject()
        {
            for (int i = 0; i < MeshPieces.Count; i++)
            {
                MeshPieces[i].MeshObject.transform.parent = TerrainObject.transform;
            }
        }

    }


   
}
