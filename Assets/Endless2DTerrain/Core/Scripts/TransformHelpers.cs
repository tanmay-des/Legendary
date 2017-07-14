using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Endless2DTerrain
{
    public class TransformHelpers
    {
        public List<Vector3> CopyList(List<Vector3> verticies)
        {
            List<Vector3> copy = new List<Vector3>();
            for (int i = 0; i < verticies.Count; i++)
            {
                copy.Add(new Vector3(verticies[i].x, verticies[i].y, verticies[i].z));
            }		
            return copy;
        }

        public List<Vector3> MoveStartVertex(List<Vector3> verticies, Vector3 newStartVertex, bool copyVertices, MeshPiece.Plane planeType)
        {
            int startIndex = 0;
            if (planeType == MeshPiece.Plane.Front || planeType == MeshPiece.Plane.Detail)
            {
                startIndex = 1;
            }

            Vector3 originalStartVertex = verticies[startIndex];
            return MoveStartVertex(verticies, originalStartVertex, newStartVertex, copyVertices);
        }

        public Vector3 GetPointAlongLine(Vector3 point1, Vector3 point2, float magnitude)
        {
            Vector3 line = point2 - point1;
            line = line.normalized * magnitude;
            line = line + point1;
            return line;
        }

        public List<Vector3> GetPerpendicularOffset(List<Vector3> verticies, float offset)
        {
            List<Vector3> copy = new List<Vector3>();
           

            for (int i = 0; i < verticies.Count; i++)
            {
                Vector3 currentVertex = verticies[i];
                Vector3 previousVertex;

                if (i == 0)
                {
                    previousVertex = new Vector3(currentVertex.x - 1, currentVertex.y, currentVertex.z);
                }
                else
                {
                    previousVertex = verticies[i - 1];
                }
           
           
                Vector3 vectorTowardPlayer = new Vector3(currentVertex.x, currentVertex.y, currentVertex.z - 1);
					
				Vector3 a = currentVertex;
				Vector3 b = previousVertex;
				Vector3 c = vectorTowardPlayer;					
					
					
	            Vector3 side1 = b-a;
                Vector3 side2 = c-a;

                //Get a height of 1 times our offset size
	            Vector3 perpendicular = Vector3.Cross(side1, side2).normalized * offset;				
					
                //And get the perp vertex relative to the current vertex
				perpendicular = perpendicular + currentVertex;	
	            copy.Add(perpendicular);
            }

            return copy;
        }


        //Move a list of verticies to a new start location.  Keep all their positions relative to each other
        public List<Vector3> MoveStartVertex(List<Vector3> verticies, Vector3 originalStartVertex, Vector3 newStartVertex, bool copyVertices)
        {
            List<Vector3> copy = new List<Vector3>();

            //And how much we have to move by
            Vector3 moveVector = newStartVertex - originalStartVertex;

            //Now move every vector in our list proportial to our new location
            for (int i = 0; i < verticies.Count; i++)
            {
                Vector3 currentVertex = verticies[i];

                if (copyVertices)
                {
                    Vector3 newVertex = new Vector3(currentVertex.x, currentVertex.y, currentVertex.z);
                    copy.Add(newVertex + moveVector);
                }
                else
                {
                    verticies[i] = verticies[i] + moveVector;
                }

              
            }

            if (copyVertices)
            {
                return copy;
            }
            else
            {
                return verticies;
            }
         
        }

        //Rotate a vertex by a given angle
        public Vector3 RotateVertex(Vector3 vertex, float angle)
        {
            if (angle == 0)
            {
                return vertex;
            }

            var rotation = Quaternion.Euler(0, 0, angle);
            return rotation * vertex;
        }

        //Rotate a list of verticies by a given angle
        public List<Vector3> RotateVertices(List<Vector3> verticies, float angle)
        {
			List<Vector3> rotated = new List<Vector3>();
            for (int i = 0; i < verticies.Count(); i++)
            {
                Vector3 vertex = verticies[i];
                rotated.Add(RotateVertex(vertex, angle));
            }

            return rotated;
        }

        public Vector3 CopyVertex(Vector3 temp)
        {
            return new Vector3(temp.x, temp.y, temp.z);
        }


        //From http://paulbourke.net/miscellaneous/interpolation/
        //Interpolate the y values based off of the cosine function
        public float CosineInterpolate(float y1, float y2, float mu)
        {
            //Mu is between 0 and 1 - it is the relative position between the two y values
            float mu2;

            mu2 = (1 - Mathf.Cos(mu * Mathf.PI)) / 2;
            return (y1 * (1 - mu2) + y2 * mu2);
        }

      

    }
}
