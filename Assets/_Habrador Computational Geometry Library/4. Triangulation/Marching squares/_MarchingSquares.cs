using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Habrador_Computational_Geometry.Marching_Squares;

namespace Habrador_Computational_Geometry
{
    //Based on Procedural Cave Generation (E02. Marching Squares): https://www.youtube.com/watch?v=yOgIncKp0BE
    //and Coding in the Cabana 5: Marching Squares https://www.youtube.com/watch?v=0ZONMNUKTfU
    public static class MarchingSquares
    {
        //For the mesh
        public static List<MyVector2> vertices;

        public static List<int> triangles;


        //The map consists of 0 or 1, where 1 means solid (or active)
        //squareSize is how big each square in the grid is 
        //The map will be centered in 2d space around (x = 0, z = 0)
        public static SquareGrid GenerateMesh(int[,] map, float squareSize)
        {
            //Validate input
            if (map == null)
            {
                Debug.LogError("The Marching Squares Map doesnt exist");

                return null;
            }

            //We need at least 4 nodes (1 square) to create a mesh
            if (map.GetLength(0) <= 1 || map.GetLength(1) <= 1)
            {
                Debug.LogError("The Marching Squares Map is too small");

                return null;
            }



            //Create the data grid to make it easier to generate the mesh
            //Each square has 4 corners, which are either solid or empty, 
            //which is what the Marching Squares needs to generate a mesh at this square
            SquareGrid squareGrid = new SquareGrid(map, squareSize);

            //Init the mesh
            vertices = new List<MyVector2>();

            triangles = new List<int>();

            //Loop through and triangulate each square
            int xLength = squareGrid.squares.GetLength(0);
            int zLength = squareGrid.squares.GetLength(1);

            for (int x = 0; x < xLength; x++)
            {
                for (int z = 0; z < zLength; z++)
                {
                    Square thisSquare = squareGrid.squares[x, z];

                    TriangulateSquare(thisSquare);
                }
            }

            //Assign the vertices and triangles to the grid
            squareGrid.triangles = new List<int>(triangles);

            squareGrid.vertices = new List<MyVector2>(vertices);

            return squareGrid;
        }



        //Triangulate a single square with Marching Squares
        //Each square has 4 corners, so we have 16 different possible meshes we can use to triangulate this square
        private static void TriangulateSquare(Square s)
        {
            switch (s.configuration)
            {
                //0 corners are active = no mesh
                case 0:
                    break;

                //1 corner is active
                case 1:
                    //BL is active
                    MeshFromPoints(s.B, s.BL, s.L);
                    break;
                case 2:
                    MeshFromPoints(s.BR, s.B, s.R);
                    break;
                case 4:
                    MeshFromPoints(s.TR, s.R, s.T);
                    break;
                case 8:
                    MeshFromPoints(s.T, s.L, s.TL);
                    break;

                //2 corners are active
                case 3:
                    //BR and BL are active
                    MeshFromPoints(s.R, s.BR, s.BL, s.L);
                    break;
                case 6:
                    MeshFromPoints(s.BR, s.B, s.T, s.TR);
                    break;
                case 9:
                    MeshFromPoints(s.B, s.BL, s.TL, s.T);
                    break;
                case 12:
                    MeshFromPoints(s.R, s.L, s.TL, s.TR);
                    break;
                case 5:
                    //BL and TR are active
                    MeshFromPoints(s.R, s.B, s.BL, s.L, s.T, s.TR);
                    break;
                case 10:
                    MeshFromPoints(s.R, s.BR, s.B, s.L, s.TL, s.T);
                    break;

                //3 corners are active
                case 7:
                    MeshFromPoints(s.T, s.TR, s.BR, s.BL, s.L);
                    break;
                case 11:
                    MeshFromPoints(s.R, s.BR, s.BL, s.TL, s.T);
                    break;
                case 13:
                    MeshFromPoints(s.R, s.B, s.BL, s.TL, s.TR);
                    break;
                case 14:
                    MeshFromPoints(s.TR, s.BR, s.B, s.L, s.TL);
                    break;

                //4 corners are active
                //Notice that we are ignoring the middle-vertices between the corners
                case 15:
                    MeshFromPoints(s.TR, s.BR, s.BL, s.TL);
                    break;
            }
        }



        //Triangulate unknown number of points
        //The "points" are points on the convex hull of the triangles belongin to this square
        //so this is the same idea as when we triangulate a convex hull
        private static void MeshFromPoints(params Node[] points)
        {
            //Make sure each point becomes a vertex in the mesh
            AssignVertices(points);

            //Create the triangles of the final mesh
            if (points.Length >= 3)
            {
                CreateTriangle(points[0], points[1], points[2]);
            }
            if (points.Length >= 4)
            {
                CreateTriangle(points[0], points[2], points[3]);
            }
            if (points.Length >= 5)
            {
                CreateTriangle(points[0], points[3], points[4]);
            }
            if (points.Length >= 6)
            {
                CreateTriangle(points[0], points[4], points[5]);
            }
        }



        //Each node has a vertexIndex which we can use to avoid duplicates
        private static void AssignVertices(Node[] points)
        {
            //Loop through all points on the convex hull
            for (int i = 0; i < points.Length; i++)
            {
                //This point is not a member of the mesh so assign it 
                if (points[i].vertexIndex == -1)
                {
                    //Add it
                    vertices.Add(points[i].pos);

                    points[i].vertexIndex = vertices.Count - 1;
                }
            }
        }



        //Create a triangle based on three nodes
        private static void CreateTriangle(Node a, Node b, Node c)
        {
            triangles.Add(a.vertexIndex);
            triangles.Add(b.vertexIndex);
            triangles.Add(c.vertexIndex);
        }
    }
}
