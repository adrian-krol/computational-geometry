using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Habrador_Computational_Geometry;
using System.Linq;

public class MeshSimplificationController : MonoBehaviour
{
    public MeshFilter meshFilterToSimplify;

    public MeshFilter meshFilterToShowSimplifiedMesh;


    public void SimplifyMesh()
    {
        //Has to be sharedMesh if we are using Editor tools
        Mesh meshToSimplify = meshFilterToSimplify.sharedMesh;


        //
        // Change data structure and normalize
        //

        //Mesh -> MyMesh
        MyMesh myMeshToSimplify = new MyMesh(meshToSimplify); 

        //Normalize to 0-1
        Normalizer3 normalizer = new Normalizer3(myMeshToSimplify.vertices);

        //We only need to normalize the vertices
        myMeshToSimplify.vertices = normalizer.Normalize(myMeshToSimplify.vertices);



        //
        // Simplify
        //

        System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();

        timer.Start();

        MyMesh mySimplifiedMesh = MeshSimplificationQEM.SimplifyByMergingEdges(myMeshToSimplify);

        timer.Stop();

        Debug.Log($"It took {timer.ElapsedMilliseconds / 1000f} seconds to simplify the mesh");



        //
        // Change data structure and un-normalize
        //

        //Un-Normalize
        mySimplifiedMesh.vertices = normalizer.UnNormalize(mySimplifiedMesh.vertices);

        //Convert to global space
        Transform trans = meshFilterToSimplify.transform;

        mySimplifiedMesh.vertices = mySimplifiedMesh.vertices.Select(x => trans.TransformPoint(x.ToVector3()).ToMyVector3()).ToList();

        //Convert to mesh
        Mesh unitySimplifiedMesh = mySimplifiedMesh.ConvertToUnityMesh(generateNormals: true, meshName: "simplified mesh");

        //Attach to new game object
        meshFilterToShowSimplifiedMesh.mesh = unitySimplifiedMesh;
    }
}
