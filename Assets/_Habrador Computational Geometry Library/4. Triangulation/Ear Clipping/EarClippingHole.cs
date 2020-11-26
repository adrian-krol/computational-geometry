using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Habrador_Computational_Geometry
{
    //Methods related to making holes in Ear Clipping algorithm
    public static class EarClippingHole
    {
        //Merge holes with hull so we get one big list of vertices we can triangulate
        public static List<MyVector2> MergeHolesWithHull(List<MyVector2> verticesHull, List<List<MyVector2>> allHoleVertices)
        {
            //Validate
            if (allHoleVertices == null || allHoleVertices.Count == 0)
            {
                return null;
            }


            //Change data structure
            Polygon hull = new Polygon(verticesHull);

            List<Polygon> holes = new List<Polygon>();

            foreach (List<MyVector2> hole in allHoleVertices)
            {
                //Validate data
                if (hole == null || hole.Count <= 2)
                {
                    Debug.Log("The hole doesn't have enough vertices");

                    continue;
                }

                Polygon connectedVerts = new Polygon(hole);

                holes.Add(connectedVerts);
            }


            //Sort the holes by their max x-value coordinate, from highest to lowest
            holes = holes.OrderByDescending(o => o.maxX_Vert.x).ToList();


            //Merge the holes with the hull so we get a hull with seams that we can triangulate like a hull without holes
            foreach (Polygon hole in holes)
            {
                MergeHoleWithHull(hull, hole);
            }


            return hull.vertices;
        }



        //Merge a single hole with the hull
        //Basic idea is to find a vertex in the hole that can also see a vertex in the hull
        //Connect these vertices with two edges, and the hole is now a part of the hull with an invisible seam
        //between the hole and the hull
        private static void MergeHoleWithHull(Polygon hull, Polygon hole)
        {
            //Step 1. Find the vertex in the hole which has the maximum x-value
            //Has already been done when we created the data structure


            //Step 2. Form a line going from this vertex towards (in x-direction) to a position outside of the hull
            MyVector2 lineStart = hole.maxX_Vert;
            //Just add some value so we know we are outside
            MyVector2 lineEnd = new MyVector2(hull.maxX_Vert.x + 0.1f, hole.maxX_Vert.y);


            //Step 3. Find a vertex on the hull which is visible to the point on the hole with max x pos
            //The first and second point on the hull is defined as edge 0, and so on...
            int closestEdge = -1;

            MyVector2 visibleVertex;

            FindVisibleVertexOnHUll(hull, hole, lineStart, lineEnd, out closestEdge, out visibleVertex);

            //This means we couldn't find a closest edge
            if (closestEdge == -1)
            {
                Debug.Log("Couldn't find a closest edge to hole");

                return;
            }


            //Step 4. Modify the hull vertices so we get an edge from the hull to the hole, around the hole, and back to the hull

            //First reconfigure the hole list to start at the vertex with the largest x pos
            //[a, b, c, d, e] and c is the one with the largest x pos, we get:
            //[a, b, c, d, e, a, b]
            //[c, d, e, a, b]
            //We also need two extra vertices, one from the hole and one from the hull
            //If p is the visible vertex, then we get
            //[c, d, e, a, b, c, p]

            //This is maybe more efficient if we turn the hole list into a queue?

            //Add to back of list
            for (int i = 0; i < hole.maxX_ListPos; i++)
            {
                hole.vertices.Add(hole.vertices[i]);
            }

            //Remove those we added to the back of the list
            hole.vertices.RemoveRange(0, hole.maxX_ListPos);

            //Add the two extra vertices we need
            hole.vertices.Add(hole.vertices[0]);
            hole.vertices.Add(visibleVertex);


            //Merge the hole with the hull
            List<MyVector2> verticesHull = hull.vertices;

            //Find where we should insert the hole
            int hull_VisibleVertex_ListPos = hull.GetListPos(visibleVertex);

            if (hull_VisibleVertex_ListPos == -1)
            {
                Debug.Log("Cant find corresponding pos in list");

                return;
            }

            verticesHull.InsertRange(hull_VisibleVertex_ListPos + 1, hole.vertices);

            //Debug.Log($"Number of vertices on the hull after adding a hole: {verticesHull.Count}");
        }



        //Find a vertex on the hull that should be visible from the hole
        private static void FindVisibleVertexOnHUll(Polygon hull, Polygon hole, MyVector2 lineStart, MyVector2 lineEnd, out int closestEdge, out MyVector2 visibleVertex)
        {
            //The first and second point on the hull is defined as edge 0, and so on...
            closestEdge = -1;
            //The vertex that should be visible to the hole (which is the max of the line that's intersecting with the line)
            visibleVertex = new MyVector2(-1f, -1f);


            //Do line-line intersection to find intersectionVertex where the line is intersecting with the line
            MyVector2 intersectionVertex = new MyVector2(-1f, -1f);

            float minDistanceSqr = Mathf.Infinity;

            List<MyVector2> verticesHull = hull.vertices;

            for (int i = 0; i < verticesHull.Count; i++)
            {
                MyVector2 p1_hull = verticesHull[i];
                MyVector2 p2_hull = verticesHull[MathUtility.ClampListIndex(i + 1, verticesHull.Count)];

                //We dont need to check this line if its to the left of the point on the hole
                //If so they cant intersect
                if (p1_hull.x < hole.maxX_Vert.x && p2_hull.x < hole.maxX_Vert.x)
                {
                    continue;
                }

                bool isIntersecting = _Intersections.LineLine(lineStart, lineEnd, p1_hull, p2_hull, true);

                //Here we can maybe add a check if any of the vertices is on the line

                if (isIntersecting)
                {
                    intersectionVertex = _Intersections.GetLineLineIntersectionPoint(lineStart, lineEnd, p1_hull, p2_hull);

                    float distanceSqr = MyVector2.SqrDistance(lineStart, intersectionVertex);

                    if (distanceSqr < minDistanceSqr)
                    {
                        closestEdge = i;
                        minDistanceSqr = distanceSqr;
                    }
                }
            }

            //This means we couldn't find a closest edge
            if (closestEdge == -1)
            {
                Debug.Log("Couldn't find a closest edge to hole");

                return;
            }


            //Find visibleVertex
            //The closest edge has two vertices. Pick the one with the highest x-value, which is the vertex
            //that should be visible from the hole
            MyVector2 p1 = hull.vertices[closestEdge];
            MyVector2 p2 = hull.vertices[MathUtility.ClampListIndex(closestEdge + 1, hull.vertices.Count)];

            visibleVertex = p1;

            if (p2.x > visibleVertex.x)
            {
                visibleVertex = p2;
            }



            //But the hull may still intersect with this edge between the point on the hole and the point on the hull, 
            //so the point on the hull might not be visible
            //So we might have to find a new point which is visible
            FindActualVisibleVertexOnHull(hull, hole, intersectionVertex, ref visibleVertex);
        }



        //The hull may still intersect with this edge between the point on the hole and the point on the hull, 
        //so the point on the hull might not be visible, so we should try to find a better point
        private static void FindActualVisibleVertexOnHull(Polygon hull, Polygon hole, MyVector2 intersectionVertex, ref MyVector2 visibleVertex)
        {
            //Form a triangle
            Triangle2 t = new Triangle2(hole.maxX_Vert, intersectionVertex, visibleVertex);

            //According to litterature, we check if an reflect vertices are within this triangle
            //If so, one of them will be visible
            List<MyVector2> reflectVertices = FindReflectVertices(hull, hole);


            float minAngle = Mathf.Infinity;

            float minDistSqr = Mathf.Infinity;

            foreach (MyVector2 v in reflectVertices)
            {
                if (_Intersections.PointTriangle(t, v, includeBorder: true))
                {
                    float angle = MathUtility.AngleBetween(intersectionVertex - hole.maxX_Vert, v - hole.maxX_Vert);

                    //Debug.DrawLine(v.ToVector3(1f), hole.maxX_Vert.ToVector3(1f), Color.blue, 2f);

                    //Debug.DrawLine(intersectionVertex.ToVector3(1f), hole.maxX_Vert.ToVector3(1f), Color.black, 2f);

                    //TestAlgorithmsHelpMethods.DebugDrawCircle(v.ToVector3(1f), 0.3f, Color.blue);

                    //Debug.Log(angle * Mathf.Rad2Deg);

                    if (angle < minAngle)
                    {
                        minAngle = angle;

                        visibleVertex = v;

                        //We also need to calculate this in case a future point has the same angle
                        minDistSqr = MyVector2.SqrDistance(v, hole.maxX_Vert);

                        //Debug.Log(minDistanceSqr);

                        //TestAlgorithmsHelpMethods.DebugDrawCircle(v.ToVector3(1f), 0.3f, Color.green);
                    }
                    //If the angle is the same, then pick the vertex which is the closest to the point on the hull
                    else if (Mathf.Abs(angle - minAngle) < MathUtility.EPSILON)
                    {
                        float distSqr = MyVector2.SqrDistance(v, hole.maxX_Vert);


                        //Debug.Log(minDistanceSqr);


                        if (distSqr < minDistSqr)
                        {
                            visibleVertex = v;

                            minDistSqr = distSqr;

                            //TestAlgorithmsHelpMethods.DebugDrawCircle(v.ToVector3(1f), 0.3f, Color.red);

                            //Debug.Log(distSqr);
                        }

                        //Debug.Log("Hello");
                    }
                }
            }

            Debug.DrawLine(visibleVertex.ToVector3(1f), hole.maxX_Vert.ToVector3(1f), Color.red, 2f);

            TestAlgorithmsHelpMethods.DebugDrawCircle(visibleVertex.ToVector3(1f), 0.3f, Color.red);
        }




        //Find reflect vertices
        private static List<MyVector2> FindReflectVertices(Polygon hull, Polygon hole)
        {
            List<MyVector2> reflectVertices = new List<MyVector2>();


            List<MyVector2> verticesHull = hull.vertices;

            for (int i = 0; i < verticesHull.Count; i++)
            {
                MyVector2 p = verticesHull[i];

                //We dont need to check this vertex if its to the left of the point on the hull
                //because that vertex can't be within the triangle
                if (p.x < hole.maxX_Vert.x)
                {
                    continue;
                }

                MyVector2 p_prev = verticesHull[MathUtility.ClampListIndex(i - 1, verticesHull.Count)];

                MyVector2 p_next = verticesHull[MathUtility.ClampListIndex(i + 1, verticesHull.Count)];

                //Here we have to ignore colinear points, which need to be reflect when triangulating, but are giving an error here
                if (!EarClipping.IsVertexConvex(p_prev, p, p_next, isColinearPointsConcave: false))
                {
                    reflectVertices.Add(p);
                }
            }


            return reflectVertices;
        }
    }
}
