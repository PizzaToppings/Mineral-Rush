using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshGeneration {

    // this script loops through certains points on the map. These points are the corners of (big) triangle shaped areas.  
    // It checks which other points are part of that triangle, and [...] does something?

    // gets the triangleData
       public static TriangleData Generate(float[,] noise, float heightMultiplier)
       {
           int width = noise.GetLength(0);
           int length = noise.GetLength(1);

           TriangleData _mesh = new TriangleData(width, length);
           int pointNr = 0; // The number of the point on the map that will be used to create a traingle

           for (int w = 0; w < width; w++)
           {
               for (int l = 0; l < length; l++)
               {
                   _mesh.vertices[pointNr] = new Vector3(l, noise[l, w] * heightMultiplier, w);  // location of the vertices (same as the waypoints)
                   _mesh.uvMap[pointNr] = new Vector2(l / (float)length, w / (float)width); // the location for the UVmap (percantage based)
                
                   if (w < width - 1 && l < length - 1) // loop through all points, except the right edge and bottom edge, as they will not be used
                   {
                       _mesh.CreateTriangle(pointNr, pointNr + width + 1, pointNr + width);
                       _mesh.CreateTriangle(pointNr + width + 1, pointNr, pointNr + 1); 
                   }
                   pointNr++;
               }
           }
           return _mesh;
       } 
}

// sets the traingle data
public class TriangleData 
{
    public Vector3[] vertices; // the corner of the triangles.
    public int[] triangles; // these are kinda like polygons: they are triangles which contain certain data for height and color. theyre only much bigger
    public Vector2[] uvMap; // the texture coordinates. Allows to add the colors.

    int cornerNr; // the number of the triangle corners

    public TriangleData(int width, int length)
    {
        vertices = new Vector3[width * length];
        uvMap = new Vector2[width * length];
        // calculation for triangles: the size of the map minus the edges, because they dont have more corners next to them. multiplied by 6, because each point creates 2 triangles with 3 corners
        triangles = new int[(width - 1) * (length - 1) * 6];  
    }

    // creates the triangles similar to how polygons work. looks at the corner points clockwise, and sets these three corners as the corners of one triangle
    public void CreateTriangle(int a, int b, int c)
    {
        triangles[cornerNr] = a;
        triangles[(cornerNr + 1)] = b;
        triangles[(cornerNr + 2)] = c;
        cornerNr += 3;
    }

    // adds all data to the mesh
    public Mesh CreateMesh()
    {
        Mesh _mesh = new Mesh();
        _mesh.vertices = vertices;
        _mesh.triangles = triangles;
        _mesh.uv = uvMap;
        _mesh.RecalculateNormals();
        return _mesh;
    }
}

