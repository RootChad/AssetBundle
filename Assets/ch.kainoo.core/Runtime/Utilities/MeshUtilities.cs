using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace ch.kainoo.core.utilities
{

    public static class MeshUtilities
    {
        public static Mesh CreateCube(string name, Vector3 center, Vector3 size)
        {
            Mesh mesh = new Mesh();
            mesh.name = name;

            // Positions (8 points)
            Vector3[] c = new Vector3[] { 
                new Vector3(center.x - size.x, center.y - size.y, center.z + size.z),
                new Vector3(center.x + size.x, center.y - size.y, center.z + size.z),
                new Vector3(center.x + size.x, center.y - size.y, center.z - size.z),
                new Vector3(center.x - size.x, center.y - size.y, center.z - size.z),
                new Vector3(center.x - size.x, center.y + size.y, center.z + size.z),
                new Vector3(center.x + size.x, center.y + size.y, center.z + size.z),
                new Vector3(center.x + size.x, center.y + size.y, center.z - size.z),
                new Vector3(center.x - size.x, center.y + size.y, center.z - size.z),
            };

            // Vertices (24 vertices)
            // There's deduplication because of triangles and normals.
            Vector3[] vertices = new Vector3[]
            {
                c[0], c[1], c[2], c[3], // Bottom
	            c[7], c[4], c[0], c[3], // Left
	            c[4], c[5], c[1], c[0], // Front
	            c[6], c[7], c[3], c[2], // Back
	            c[5], c[6], c[2], c[1], // Right
	            c[7], c[6], c[5], c[4]  // Top
            };


            // Triangles
            int[] triangles = new int[]
            {
                3, 1, 0,        3, 2, 1,        // Bottom	
	            7, 5, 4,        7, 6, 5,        // Left
	            11, 9, 8,       11, 10, 9,      // Front
	            15, 13, 12,     15, 14, 13,     // Back
	            19, 17, 16,     19, 18, 17,	    // Right
	            23, 21, 20,     23, 22, 21,     // Top
            };


            // UVs for each vertex (24 pieces)
            Vector2 uv00 = new Vector2(0f, 0f);
            Vector2 uv10 = new Vector2(1f, 0f);
            Vector2 uv01 = new Vector2(0f, 1f);
            Vector2 uv11 = new Vector2(1f, 1f);

            Vector2[] uvs = new Vector2[]
            {
                uv11, uv01, uv00, uv10, // Bottom
	            uv11, uv01, uv00, uv10, // Left
	            uv11, uv01, uv00, uv10, // Front
	            uv11, uv01, uv00, uv10, // Back	        
	            uv11, uv01, uv00, uv10, // Right 
	            uv11, uv01, uv00, uv10  // Top
            };

            // Normals
            Vector3 up = Vector3.up;
            Vector3 down = Vector3.down;
            Vector3 forward = Vector3.forward;
            Vector3 back = Vector3.back;
            Vector3 left = Vector3.left;
            Vector3 right = Vector3.right;

            Vector3[] normals = new Vector3[]
            {
                down, down, down, down,             // Bottom
	            left, left, left, left,             // Left
	            forward, forward, forward, forward,	// Front
	            back, back, back, back,             // Back
	            right, right, right, right,         // Right
	            up, up, up, up                      // Top
            };


            // Output
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.normals = normals;
            mesh.uv = uvs;
            mesh.Optimize();

       //     mesh.vertices = new Vector3[]
       //     {
       //         new Vector3(center.x - size.x, center.y + size.y, center.z - size.z),
       //         new Vector3(center.x - size.x, center.y - size.y, center.z - size.z),
       //         new Vector3(center.x + size.x, center.y + size.y, center.z - size.z),
       //         new Vector3(center.x + size.x, center.y - size.y, center.z - size.z),

       //         new Vector3(center.x - size.x, center.y - size.y, center.z + size.z),
       //         new Vector3(center.x + size.x, center.y - size.y, center.z + size.z),
       //         new Vector3(center.x - size.x, center.y + size.y, center.z + size.z),
       //         new Vector3(center.x + size.x, center.y + size.y, center.z + size.z),

       //         new Vector3(center.x - size.x, center.y + size.y, center.z - size.z),
       //         new Vector3(center.x + size.x, center.y + size.y, center.z - size.z),

       //         new Vector3(center.x - size.x, center.y + size.y, center.z - size.z),
       //         new Vector3(center.x - size.x, center.y + size.y, center.z + size.z),

       //         new Vector3(center.x + size.x, center.y + size.y, center.z - size.z),
       //         new Vector3(center.x + size.x, center.y + size.y, center.z + size.z),
       //     };
       //     mesh.triangles = new int[]
       //     {
       //         0,  2,  1,  // front
			    //1,  2,  3,
       //         4,  5,  6,  // back
			    //5,  7,  6,
       //         6,  7,  8,  // top
			    //7,  9,  8,
       //         1,  3,  4,  // bottom
			    //3,  5,  4,
       //         1, 11, 10,  // left
			    //1,  4, 11,
       //         3, 12,  5,  // right
			    //5, 12, 13,
       //     };
       //     mesh.uv = new Vector2[]
       //     {
       //         new Vector2(0, 0.66f),
       //         new Vector2(0.25f, 0.66f),
       //         new Vector2(0, 0.33f),
       //         new Vector2(0.25f, 0.33f),

       //         new Vector2(0.5f, 0.66f),
       //         new Vector2(0.5f, 0.33f),
       //         new Vector2(0.75f, 0.66f),
       //         new Vector2(0.75f, 0.33f),

       //         new Vector2(1, 0.66f),
       //         new Vector2(1, 0.33f),

       //         new Vector2(0.25f, 1),
       //         new Vector2(0.5f, 1),

       //         new Vector2(0.25f, 0),
       //         new Vector2(0.5f, 0),
       //     };
       //     mesh.Optimize();
       //     mesh.RecalculateNormals();

            return mesh;
        }
    }

}