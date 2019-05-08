using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    public static class _ConvexHull
    {
        //Convex Hull Algorithm 1
        public static List<Vector2> JarvisMarch(HashSet<Vector2> points)
        {
            //Has to return a list and not hashset because the points have an order coming after each other
            List<Vector2> pointsOnHull = JarvisMarchAlgorithm.GetConvexHull(points);

            return pointsOnHull;
        }

        //Convex Hull Algorithm 2
        //public static List<Vector3> GrahamScan(List<Vector3> points)
        //{
        //    return null;
        //}
    }
}
